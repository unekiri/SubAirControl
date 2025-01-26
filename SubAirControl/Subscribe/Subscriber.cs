using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubAirControl.Subscribe
{
    // メッセージ購読クラス
    public class Subscriber
    {
        public readonly IMqttClient _client;
        public TaskCompletionSource<bool> MessageReceivedCompletionSource { get; private set; }

        public Subscriber()
        {
            var factory = new MqttFactory();
            this._client = factory.CreateMqttClient();
        }

        public async Task ConnectToBroker(string brokerAddress, int port, CancellationToken cts)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(brokerAddress, port)
                .Build();
            var retry = 0;

            // メッセージ受信ハンドラ
            this._client.ApplicationMessageReceivedAsync += e =>
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

            while (!this._client.IsConnected && retry < 5)
            {
                try
                {
                    Console.WriteLine($"Attempting to connect to broker... (Attempt {retry + 1})");
                    await this._client.ConnectAsync(options, cts);
                    Console.WriteLine("Connected to MQTT broker.");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation was cancelled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection failed: {ex.Message}. Retrying in 1 second...");
                    await Task.Delay(TimeSpan.FromSeconds(1), cts);
                }
                retry++;
            }

            if (!this._client.IsConnected)
            {
                Console.WriteLine("Failed to connect to the MQTT broker after 5 attempts.");
            }
        }

        // MQTTブローカーのトピックから購読する
        public async Task SubscribeAndWaitForMessage(string topic, CancellationToken cts)
        {
            try
            {
                // トピックにサブスクライブ
                await this._client.SubscribeAsync(new MqttTopicFilterBuilder()
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
