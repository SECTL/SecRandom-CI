using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SecRandom4Ci.Shared;

public static class SecRandomIpcSendUrl
{
    public static string DefaultIpcName = "SecRandom.secrandom";

    public static string NormalizeIpcName(string ipcName)
    {
        var value = (ipcName ?? string.Empty).Trim();
        if (value.StartsWith(@"\\.\pipe\", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring(@"\\.\pipe\".Length);
        }

        if (value.StartsWith("/tmp/", StringComparison.OrdinalIgnoreCase) &&
            value.EndsWith(".sock", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring("/tmp/".Length);
            value = value.Substring(0, value.Length - ".sock".Length);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            value = "ipc";
        }

        value = value.Replace(" ", "_").Replace("/", "_").Replace("\\", "_").Replace(":", "_");
        return value;
    }

    public static string GetIpcAddress(string ipcName)
    {
        var name = NormalizeIpcName(ipcName);
        if (OperatingSystem.IsWindows())
        {
            return $@"\\.\pipe\{name}";
        }

        return $"/tmp/{name}.sock";
    }

    public static TimeSpan ParseTimeout(string raw)
    {
        var text = (raw ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return TimeSpan.FromSeconds(5);
        }

        if (text.EndsWith("ms", StringComparison.OrdinalIgnoreCase))
        {
            var msPart = text.Substring(0, text.Length - 2).Trim();
            if (double.TryParse(msPart, out var ms) && ms > 0)
            {
                return TimeSpan.FromMilliseconds(ms);
            }

            return TimeSpan.FromSeconds(5);
        }

        if (double.TryParse(text, out var number) && number > 0)
        {
            if (!text.Contains('.') && number >= 1000)
            {
                return TimeSpan.FromMilliseconds(number);
            }

            return TimeSpan.FromSeconds(number);
        }

        return TimeSpan.FromSeconds(5);
    }

    public static async Task<byte[]?> ReadLineAsync(Stream stream, int maxBytes, CancellationToken ct)
    {
        var buffer = new byte[4096];
        var collected = new List<byte>(4096);

        while (collected.Count < maxBytes)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
            if (read <= 0)
            {
                break;
            }

            for (var i = 0; i < read; i++)
            {
                var b = buffer[i];
                collected.Add(b);
                if (b == (byte)'\n')
                {
                    return collected.ToArray();
                }

                if (collected.Count >= maxBytes)
                {
                    return collected.ToArray();
                }
            }
        }

        return collected.Count == 0 ? null : collected.ToArray();
    }

    public static async Task<JsonObject> SendUrlAsync(string url, string? ipcName = null, TimeSpan? timeout = null)
    {
        ipcName ??= DefaultIpcName;
        timeout ??= TimeSpan.FromSeconds(5);
        
        var address = GetIpcAddress(ipcName);
        var messageJson = JsonSerializer.Serialize(
            new Dictionary<string, object?>
            {
                ["type"] = "url",
                ["payload"] = new Dictionary<string, object?> { ["url"] = url }
            },
            new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
        );
        var requestBytes = Encoding.UTF8.GetBytes(messageJson + "\n");

        using var cts = new CancellationTokenSource(timeout.Value);
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var pipeName = NormalizeIpcName(ipcName);
                await using var pipe = new NamedPipeClientStream(
                    serverName: ".",
                    pipeName: pipeName,
                    direction: PipeDirection.InOut,
                    options: PipeOptions.Asynchronous
                );

                var connectTimeoutMs = (int)Math.Clamp(timeout.Value.TotalMilliseconds, 1, int.MaxValue);
                await pipe.ConnectAsync(connectTimeoutMs, cts.Token);
                await pipe.WriteAsync(requestBytes.AsMemory(0, requestBytes.Length), cts.Token);
                await pipe.FlushAsync(cts.Token);

                var responseLine = await ReadLineAsync(pipe, maxBytes: 1024 * 1024, cts.Token);
                if (responseLine is null || responseLine.Length == 0)
                {
                    return new JsonObject
                    {
                        ["success"] = false,
                        ["error"] = "empty_response"
                    };
                }

                var responseText = Encoding.UTF8.GetString(responseLine).Trim();
                var node = JsonNode.Parse(responseText) as JsonObject;
                return node ?? new JsonObject { ["success"] = false, ["error"] = "invalid_response" };
            }
            else
            {
                var endPoint = new UnixDomainSocketEndPoint(address);
                using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                var connectTask = socket.ConnectAsync(endPoint, cts.Token);
                await connectTask;

                await using var stream = new NetworkStream(socket, ownsSocket: true);
                await stream.WriteAsync(requestBytes.AsMemory(0, requestBytes.Length), cts.Token);
                await stream.FlushAsync(cts.Token);

                var responseLine = await ReadLineAsync(stream, maxBytes: 1024 * 1024, cts.Token);
                if (responseLine is null || responseLine.Length == 0)
                {
                    return new JsonObject
                    {
                        ["success"] = false,
                        ["error"] = "empty_response"
                    };
                }

                var responseText = Encoding.UTF8.GetString(responseLine).Trim();
                var node = JsonNode.Parse(responseText) as JsonObject;
                return node ?? new JsonObject { ["success"] = false, ["error"] = "invalid_response" };
            }
        }
        catch (Exception ex) when (
            ex is TimeoutException ||
            ex is OperationCanceledException ||
            ex is SocketException ||
            ex is IOException
        )
        {
            return new JsonObject
            {
                ["success"] = false,
                ["error"] = "ipc_not_found",
                ["detail"] = $"IPC 通道不存在或无法连接: {address}. 请确认 SecRandom 已运行且已启动 IPC 服务端。"
            };
        }
    }
}