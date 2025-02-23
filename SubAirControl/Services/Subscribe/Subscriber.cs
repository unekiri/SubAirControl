using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using SubAirControl.Helpers;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubAirControl.Services.Subscribe
{
    // メッセージ購読クラス
    public class Subscriber
    {
        public readonly IMqttClient _client;
        private readonly IConnectionHelper _connectionHelper;
        public TaskCompletionSource<bool> MessageReceivedCompletionSource { get; private set; }

        public Subscriber(IConnectionHelper connectionHelper)
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _connectionHelper = connectionHelper;
        }

        public async Task ConnectToBroker(string brokerAddress, int port, CancellationToken cts)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(brokerAddress, port)
                .Build();
            var retry = 0;

            // メッセージ受信ハンドラ
            _client.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();

                // メッセージを受信したので TaskCompletionSource を完了させる
                MessageReceivedCompletionSource?.TrySetResult(true);
                return Task.CompletedTask;
            };

            await _connectionHelper.ConnectWithRetryAsync(_client, options);
        }

        // MQTTブローカーのトピックから購読する
        public async Task SubscribeAndWaitForMessage(string topic, CancellationToken cts)
        {
            try
            {
                // トピックにサブスクライブ
                await _client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .Build(), cts);

                while (!cts.IsCancellationRequested)
                {
                    // メッセージ受信を待つ準備をする
                    MessageReceivedCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                    // メッセージが来るまで待機
                    await MessageReceivedCompletionSource.Task;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error subscribing to topic {topic}: {e.Message}");
            }
        }
    }
}
