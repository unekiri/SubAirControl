using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubAirControl.Subscribe
{
    public class Subscriber
    {
        public IMqttClient Client { get; private set; }
        private TaskCompletionSource<bool> _messageReceivedCompletionSource;

        public Subscriber()
        {
            var factory = new MqttFactory();
            this.Client = factory.CreateMqttClient();

            // メッセージ受信ハンドラ
            this.Client.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();

                // メッセージを受信したので TaskCompletionSource を完了させる
                _messageReceivedCompletionSource?.TrySetResult(true);
                return Task.CompletedTask;
            };
        }

        public async Task Connect(string brokerAddress, int port)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(brokerAddress, port)
                .Build();

            await this.Client.ConnectAsync(options, CancellationToken.None);
            Console.WriteLine("Connected to MQTT broker.");
        }

        public async Task SubscribeAndWaitForMessage(string topic)
        {
            try
            {
                // トピックにサブスクライブ
                await this.Client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .Build());

                Console.WriteLine($"### SUBSCRIBED TO TOPIC: {topic} ###");

                // メッセージ受信を待つ準備をする
                _messageReceivedCompletionSource = new TaskCompletionSource<bool>();

                // メッセージが来るまで待機
                await _messageReceivedCompletionSource.Task;
                Console.WriteLine("Message received, continuing execution...");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error subscribing to topic {topic}: {e.Message}");
            }
        }
    }

    public class ExeSubscriber
    {
        public IConfiguration Configuration { get; private set; }

        public ExeSubscriber(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public async Task Run()
        {
            var address = this.Configuration["MqttSettings:Address"];
            var port = int.Parse(this.Configuration["MqttSettings:Port"]);
            var topic = this.Configuration["MqttSettings:Topic"];

            var subscriber = new Subscriber();

            // MQTTブローカーに接続
            await subscriber.Connect(address, port);

            // トピックの購読を行い、メッセージが受信されるまで待機
            await subscriber.SubscribeAndWaitForMessage(topic);
        }
    }

}
