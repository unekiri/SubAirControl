using Microsoft.Extensions.Configuration;
using SubAirControl.Helpers;
using SubAirControl.Services.Subscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubAirControl.Services.Subscribe
{
    public class AtFirstExeSubscribe
    {
        public readonly IConfiguration _configuration;
        public readonly IConnectionHelper _connectionHelper;

        public AtFirstExeSubscribe(IConfiguration configuration, IConnectionHelper connectionHelper)
        {
            _configuration = configuration;
            _connectionHelper = connectionHelper;
        }

        public async Task Run(CancellationToken cts)
        {
            var address = _configuration["MqttSettings:Address"];
            var port = int.Parse(_configuration["MqttSettings:Port"]);
            var topic = _configuration["MqttSettings:Topic"];

            var subscribe = new Subscriber(_connectionHelper);
            // MQTTブローカーに接続
            await subscribe.ConnectToBroker(address, port, cts);

            // トピックの購読を行い、メッセージが受信されるまで待機
            await subscribe.SubscribeAndWaitForMessage(topic, cts);
        }
    }
}
