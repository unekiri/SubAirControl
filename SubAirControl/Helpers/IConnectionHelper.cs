using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubAirControl.Helpers
{
    public interface IConnectionHelper
    {
        Task ConnectWithRetryAsync(IMqttClient client, MqttClientOptions options, int maxRetries = 5);
    }
}
