using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// 添加 SignalR 服務
builder.Services.AddSignalR();

var app = builder.Build();

// 設定 MQTT 客戶端
var mqttFactory = new MqttFactory();
var mqttClient = mqttFactory.CreateMqttClient();
var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithClientId("WebAPI_SignalR_Client")
    .WithTcpServer("127.0.0.1", 1883) // 使用公共的 MQTT 伺服器
    .Build();

await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

// 訂閱溫度資訊的主題
await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("temperature").Build(), CancellationToken.None);

// 處理接收到的訊息
mqttClient.ApplicationMessageReceivedAsync += async e =>
{
    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
    Console.WriteLine($"Received message: {message}");

    // 將訊息發送給所有 SignalR 客戶端
    await SendMessageToClientsAsync(message);
};

// 設定 SignalR
app.MapHub<TemperatureHub>("/temperatureHub");

// 設定靜態檔案中介軟體
app.UseStaticFiles();

app.Run();

// 發送訊息給所有 SignalR 客戶端
async Task SendMessageToClientsAsync(string message)
{
    var hubContext = app.Services.GetRequiredService<IHubContext<TemperatureHub>>();
    await hubContext.Clients.All.SendAsync("ReceiveMessage", message);
}

// 定義 SignalR Hub
public class TemperatureHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
