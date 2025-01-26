using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubAirControl.Subscribe
{
    public class AtFirstExeSubscribe
    {
        public readonly IConfiguration _configuration;

        public AtFirstExeSubscribe(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task Run(CancellationToken cts)
        {
            var address = this._configuration["MqttSettings:Address"];
            var port = int.Parse(this._configuration["MqttSettings:Port"]);
            var topic = this._configuration["MqttSettings:Topic"];

            var subscribe = new Subscriber();
            // MQTTブローカーに接続
            await subscribe.ConnectToBroker(address, port, cts);

            // トピックの購読を行い、メッセージが受信されるまで待機
            await subscribe.SubscribeAndWaitForMessage(topic, cts);
        }
    }
}
