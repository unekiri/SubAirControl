using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubAirControl.Helpers
{
    public class MqttConnectionHelper : IConnectionHelper
    {
        public async Task ConnectWithRetryAsync(IMqttClient client, MqttClientOptions options, int maxRetries = 5)
        {
            int retry = 0;
            while (!client.IsConnected && retry < maxRetries)
            {
                try
                {
                    Debug.WriteLine($"接続中... ({retry + 1}回目)");
                    await client.ConnectAsync(options, CancellationToken.None);
                    Debug.WriteLine("接続に成功しました。");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"接続に失敗しました。再接続を行います...: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                retry++;
            }
            if (!client.IsConnected)
            {
                Debug.WriteLine("接続に失敗しました。");
            }
        }
    }
}
