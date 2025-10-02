using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using ClassIsland.Shared.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using static ClassIsland.Core.Abstractions.Services.ILessonsService;
using ClassIsland.Shared.Models.Notification;
using System.Windows.Documents;
using System.Windows;
using System.IO;
using System.Formats.Tar;
using NotificationRequest = ClassIsland.Core.Models.Notification.NotificationRequest;


namespace PluginWithNotificationProviders.Services.NotificationProviders;

[NotificationProviderInfo("A0C99B32-EFA4-4843-ADF6-54DE7C6FCD56", "SecRandom抽选结果", "显示SecRandom的抽选结果")]
public class SecRandomNotification : NotificationProviderBase
{
    public ILessonsService LessonsService { get; }
    private string _lastProcessedMessageId = string.Empty;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public SecRandomNotification(ILessonsService lessonsService)
    {
        LessonsService = lessonsService; 
        LessonsService.PreMainTimerTicked += LessonsServiceOnOnTicked; 
    }

    private void LessonsServiceOnOnTicked(object? sender, EventArgs e)
    {
        // 发送课程状态消息
        SendClassStatusMessage();
        
        // 接收并处理消息
        string result = Path.GetTempPath();
        string signalFile = result + "\\SecRandom_unread";
        string messageFile = result + "\\SecRandom_message_sent.json";
        
        if (File.Exists(signalFile))
        {
            // 如果消息文件存在，优先处理JSON消息
            if (File.Exists(messageFile))
            {
                try
                {
                    string jsonContent = File.ReadAllText(messageFile, Encoding.UTF8);
                    
                    // 立即删除信号文件，表示已处理（与Python实现一致）
                    try
                    {
                        File.Delete(signalFile);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"删除信号文件失败: {ex.Message}");
                    }
                    
                    // 删除消息文件，与Python实现保持一致
                    try
                    {
                        File.Delete(messageFile);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"删除消息文件失败: {ex.Message}");
                    }
                    
                    // 解析JSON文件并获取消息ID
                    string messageId = "";
                    using (JsonDocument document = JsonDocument.Parse(jsonContent))
                    {
                        JsonElement root = document.RootElement;
                        
                        // 获取消息ID（如果存在）
                        if (root.TryGetProperty("id", out JsonElement idElement))
                        {
                            if (idElement.ValueKind == JsonValueKind.String)
                            {
                                messageId = idElement.GetString() ?? "";
                            }
                            else
                            {
                                messageId = idElement.ToString();
                            }
                        }
                        
                        // 检查是否已经处理过此消息（仅当消息ID不为空时）
                        if (!string.IsNullOrEmpty(messageId) && messageId == _lastProcessedMessageId)
                        {
                            // 已经处理过此消息，直接返回
                            return;
                        }
                        
                        // 获取消息类型
                        string msgType = "unknown";
                        if (root.TryGetProperty("type", out JsonElement typeElement))
                        {
                            if (typeElement.ValueKind == JsonValueKind.String)
                            {
                                msgType = typeElement.GetString() ?? "unknown";
                            }
                            else if (typeElement.ValueKind == JsonValueKind.Number)
                            {
                                msgType = typeElement.GetInt32().ToString();
                            }
                            else
                            {
                                msgType = typeElement.ToString();
                            }
                        }
                        
                        // 根据消息类型处理
                        if (msgType == "selection_result")
                        {
                            // 处理抽选结果
                            string name = "未知";
                            if (root.TryGetProperty("name", out JsonElement nameElement))
                            {
                                if (nameElement.ValueKind == JsonValueKind.String)
                                {
                                    name = nameElement.GetString() ?? "未知";
                                }
                                else
                                {
                                    name = nameElement.ToString();
                                }
                            }
                            int display_time = 3; // 默认显示时间为3秒
                            if (root.TryGetProperty("display_time", out JsonElement displayTimeElement))
                            {
                                if (displayTimeElement.ValueKind == JsonValueKind.String)
                                {
                                    if (int.TryParse(displayTimeElement.GetString(), out int parsedDisplayTime))
                                    {
                                        display_time = parsedDisplayTime;
                                    }
                                }
                                else if (displayTimeElement.ValueKind == JsonValueKind.Number)
                                {
                                    display_time = displayTimeElement.GetInt32();
                                }
                            }   
                            
                            ShowNotification(new NotificationRequest()
                            {
                                MaskContent = NotificationContent.CreateTwoIconsMask($"抽选结果：恭喜 {name}!", factory: x => {
                                    // 这里的参数 x 就是创建好的提醒内容，可以在这里对提醒内容进行定制。
                                    x.Duration = TimeSpan.FromSeconds(display_time);
                                })
                            });
                        }
                        else if (msgType == "reward_result")
                        {
                            // 处理抽奖结果
                            string reward = "未知奖品";
                            if (root.TryGetProperty("reward", out JsonElement rewardElement))
                            {
                                if (rewardElement.ValueKind == JsonValueKind.String)
                                {
                                    reward = rewardElement.GetString() ?? "未知奖品";
                                }
                                else
                                {
                                    reward = rewardElement.ToString();
                                }
                            }
                            int display_time = 3; // 默认显示时间为3秒
                            if (root.TryGetProperty("display_time", out JsonElement displayTimeElement))
                            {
                                if (displayTimeElement.ValueKind == JsonValueKind.String)
                                {
                                    if (int.TryParse(displayTimeElement.GetString(), out int parsedDisplayTime))
                                    {
                                        display_time = parsedDisplayTime;
                                    }
                                }
                                else if (displayTimeElement.ValueKind == JsonValueKind.Number)
                                {
                                    display_time = displayTimeElement.GetInt32();
                                }
                            }   
                            
                            ShowNotification(new NotificationRequest()
                            {
                                MaskContent = NotificationContent.CreateTwoIconsMask($"抽奖结果：恭喜获得 {reward}!", factory: x => {
                                    // 这里的参数 x 就是创建好的提醒内容，可以在这里对提醒内容进行定制。
                                    x.Duration = TimeSpan.FromSeconds(display_time);
                                })
                            });
                        }
                        else if (msgType == "class_status")
                        {
                            // 跳过处理课程状态消息，这是我们自己发送的
                            return;
                        }
                        else
                        {
                            // 未知消息类型，尝试显示原始数据
                            string res = "未知结果";
                            if (root.TryGetProperty("result", out JsonElement resultElement))
                            {
                                if (resultElement.ValueKind == JsonValueKind.String)
                                {
                                    res = resultElement.GetString() ?? "未知结果";
                                }
                                else
                                {
                                    res = resultElement.ToString();
                                }
                            }
                            else if (root.TryGetProperty("name", out JsonElement nameElement))
                            {
                                if (nameElement.ValueKind == JsonValueKind.String)
                                {
                                    res = nameElement.GetString() ?? "未知结果";
                                }
                                else
                                {
                                    res = nameElement.ToString();
                                }
                            }
                            else if (root.TryGetProperty("selected", out JsonElement selectedElement))
                            {
                                if (selectedElement.ValueKind == JsonValueKind.String)
                                {
                                    res = selectedElement.GetString() ?? "未知结果";
                                }
                                else
                                {
                                    res = selectedElement.ToString();
                                }
                            }
                            int display_time = 3; // 默认显示时间为3秒
                            if (root.TryGetProperty("display_time", out JsonElement displayTimeElement))
                            {
                                if (displayTimeElement.ValueKind == JsonValueKind.String)
                                {
                                    if (int.TryParse(displayTimeElement.GetString(), out int parsedDisplayTime))
                                    {
                                        display_time = parsedDisplayTime;
                                    }
                                }
                                else if (displayTimeElement.ValueKind == JsonValueKind.Number)
                                {
                                    display_time = displayTimeElement.GetInt32();
                                }
                            }   
                            
                            ShowNotification(new NotificationRequest()
                            {
                                MaskContent = NotificationContent.CreateTwoIconsMask($"收到消息：{msgType}"),
                                OverlayContent = NotificationContent.CreateSimpleTextContent($"未知类型的消息: {res}", factory: x => {
                                    // 这里的参数 x 就是创建好的提醒内容，可以在这里对提醒内容进行定制。
                                    x.Duration = TimeSpan.FromSeconds(display_time);
                                })
                            });
                        }
                        
                        // 仅当消息ID不为空时更新已处理的消息ID
                        if (!string.IsNullOrEmpty(messageId))
                        {
                            _lastProcessedMessageId = messageId;
                        }
                    }
                }
                catch (JsonException)
                {
                    // 如果JSON解析失败，显示错误信息
                    ShowNotification(new NotificationRequest()
                    {
                        MaskContent = NotificationContent.CreateTwoIconsMask("抽选结果"),
                        OverlayContent = NotificationContent.CreateSimpleTextContent("无法解析抽选结果", factory: x => {
                            x.Duration = TimeSpan.FromSeconds(3);
                        })
                    });
                    
                    // 确保信号文件已被删除
                    try
                    {
                        if (File.Exists(signalFile))
                        {
                            File.Delete(signalFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"删除信号文件失败: {ex.Message}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 发送课程状态消息
    /// </summary>
    private void SendClassStatusMessage()
    {
        try
        {
            string tempPath = Path.GetTempPath();
            string messageFile = Path.Combine(tempPath, "SecRandom_message_received.json");
            string unreadFile = Path.Combine(tempPath, "SecRandom_unread_received");
            
            // 获取当前课程状态信息
            // 课程服务具有以下属性：
            string currentSubject = LessonsService.CurrentSubject?.Name ?? "无";     // Subject? 当前所处时间点的科目。如果没有加载课表，则为 null。
            string nextSubject = LessonsService.NextClassSubject?.Name ?? "无";       // Subject 下一节课（下一个上课类型的时间点）的科目。
            string currentState = LessonsService.CurrentState.ToString();                    // TimeState 当前时间点状态。
            bool isClassPlanEnabled = LessonsService.IsClassPlanEnabled;                     // bool 是否启用课表。
            bool isClassPlanLoaded = LessonsService.IsClassPlanLoaded;                       // bool 是否已加载课表。
            bool isLessonConfirmed = LessonsService.IsLessonConfirmed;                       // bool 是否已确定当前时间点。
            double onClassLeftTime = LessonsService.OnClassLeftTime.TotalSeconds;           // TimeSpan 距离上课剩余时间（秒）。
            double onBreakingTimeLeft = LessonsService.OnBreakingTimeLeftTime.TotalSeconds; // TimeSpan 距下课剩余时间（秒）。
            
            // 创建课程状态消息
            var classStatus = new
            {
                id = Guid.NewGuid().ToString("N"),                                          // string 唯一消息ID
                type = "class_status",                                                      // string 消息类型，标识为课程状态消息
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),                   // string ISO 8601格式的时间戳
                data = new
                {
                    current_subject = currentSubject,             // string 当前所处时间点的科目名称
                    next_subject = nextSubject,                   // string 下一节课的科目名称
                    current_state = currentState,                 // string 当前时间点状态的字符串表示
                    is_class_plan_enabled = isClassPlanEnabled,   // bool 是否启用课表
                    is_class_plan_loaded = isClassPlanLoaded,     // bool 是否已加载课表
                    is_lesson_confirmed = isLessonConfirmed,      // bool 是否已确定当前时间点
                    on_class_left_time = onClassLeftTime,         // double 距离上课剩余时间（秒）
                    on_breaking_time_left = onBreakingTimeLeft    // double 距下课剩余时间（秒）
                }
            };
            
            // 序列化为JSON
            string jsonContent = JsonSerializer.Serialize(classStatus, _jsonSerializerOptions);
            
            // 写入文件
            File.WriteAllText(messageFile, jsonContent, Encoding.UTF8);
            
            // 创建未读标记文件
            File.WriteAllText(unreadFile, "", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            // 记录错误，但不影响主要功能
            System.Diagnostics.Debug.WriteLine($"发送课程状态消息失败: {ex.Message}");
        }
    }
}