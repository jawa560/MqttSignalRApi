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

// �K�[ SignalR �A��
builder.Services.AddSignalR();

var app = builder.Build();

// �]�w MQTT �Ȥ��
var mqttFactory = new MqttFactory();
var mqttClient = mqttFactory.CreateMqttClient();
var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithClientId("WebAPI_SignalR_Client")
    .WithTcpServer("127.0.0.1", 1883) // �ϥΤ��@�� MQTT ���A��
    .Build();

await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

// �q�\�ū׸�T���D�D
await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("temperature").Build(), CancellationToken.None);

// �B�z�����쪺�T��
mqttClient.ApplicationMessageReceivedAsync += async e =>
{
    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
    Console.WriteLine($"Received message: {message}");

    // �N�T���o�e���Ҧ� SignalR �Ȥ��
    await SendMessageToClientsAsync(message);
};

// �]�w SignalR
app.MapHub<TemperatureHub>("/temperatureHub");

// �]�w�R�A�ɮפ����n��
app.UseStaticFiles();

app.Run();

// �o�e�T�����Ҧ� SignalR �Ȥ��
async Task SendMessageToClientsAsync(string message)
{
    var hubContext = app.Services.GetRequiredService<IHubContext<TemperatureHub>>();
    await hubContext.Clients.All.SendAsync("ReceiveMessage", message);
}

// �w�q SignalR Hub
public class TemperatureHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
