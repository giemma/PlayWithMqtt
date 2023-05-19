using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;

namespace UWP_MQTT_Broker
{
    public class ServerStarted
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }

    public class MessageReceived
    {
        public string ClientId { get; set; }
        public string Topic { get; set; }
        public string Payload { get; set; }
    }

    public class ClientSubscribed
    {
        public string ClientId { get; set; }
        public string Topic { get; set; }
    }

    public class MQTTBroker
    {
        public event EventHandler<string> Error;
        public event EventHandler<ServerStarted> ServerStarted;
        public event EventHandler<string> ClientConnected;

        public event EventHandler<MessageReceived> MessagePublished;
        public event EventHandler<ClientSubscribed> ClientSubscribed;

        MqttServer mqttServer;
        public async Task Start_Server()
        {
            try
            {
                var mqttFactory = new MqttFactory();
                var mqttServerOptions = new MqttServerOptionsBuilder()
                    .WithDefaultEndpoint()
                    .Build();

                mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
                mqttServer.ValidatingConnectionAsync += e =>
                {

                    return Task.CompletedTask;
                };

                mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
                mqttServer.ClientSubscribedTopicAsync += MqttServer_ClientSubscribedTopicAsync;
                mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
                await mqttServer.StartAsync();

                string myIp = GetLocalIPAddress();

                ServerStarted?.Invoke(this, new ServerStarted
                {
                    Ip = myIp,
                    Port = mqttServerOptions.DefaultEndpointOptions.Port
                });
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex.Message);
            }
        }

        private async Task MqttServer_ClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs arg)
        {
            ClientSubscribed?.Invoke(this, new ClientSubscribed
            {
                ClientId = arg.ClientId,
                Topic = arg.TopicFilter.Topic
            });
        }

        private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            MessagePublished?.Invoke(this, new MessageReceived
            {
                ClientId = arg.ClientId,
                Topic = arg.ApplicationMessage?.Topic,
                Payload = arg.ApplicationMessage?.ConvertPayloadToString()
            });
        }

        public async void StopServer()
        {
            try
            {
                await mqttServer.StopAsync();
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex.Message);
            }
        }

        private async Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            ClientConnected?.Invoke(this, arg.ClientId);
        }





        public string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                return host.AddressList.Last(x => x.AddressFamily == AddressFamily.InterNetwork)?.ToString();

                throw new Exception("No network adapters with an IPv4 address in the system!");
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex.Message);
                return string.Empty;
            }
        }
    }
}
