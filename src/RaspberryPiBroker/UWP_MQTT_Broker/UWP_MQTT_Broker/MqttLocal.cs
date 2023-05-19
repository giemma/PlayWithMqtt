using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UWP_MQTT_Broker
{
    public class MqttLocal
    {
        MqttFactory mqttFactory;
        IMqttClient mqttClient;

        public event EventHandler<string> Error;

        public MqttLocal()
        {
            mqttFactory = new MqttFactory();
        }
        public async Task Start(string localIp, int port)
        {
            try
            {
                mqttClient = mqttFactory.CreateMqttClient();

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithClientId("MQTT Local")
                    .WithTcpServer(localIp, port).Build();

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    return Task.CompletedTask;
                };

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex.Message);
            }
        }

        public async Task Publish(string topic, string message)
        {
            try
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex.Message);
            }

        }
    }
}
