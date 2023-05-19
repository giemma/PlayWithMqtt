using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UWP_MQTT_Broker
{
    public sealed partial class MainPage : Page
    {
        MQTTBroker MQTTBroker { get; set; }
        MqttLocal MqttLocal { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.Unloaded += MainPage_Unloaded;

            MQTTBroker = new MQTTBroker();
            MqttLocal = new MqttLocal();

            MQTTBroker.Error += MQTTBroker_Error;
            MQTTBroker.ServerStarted += MQTTBroker_ServerStarted;
            MQTTBroker.ClientConnected += MQTTBroker_ClientConnected;
            MQTTBroker.MessagePublished += MQTTBroker_MessagePublished;
            MQTTBroker.ClientSubscribed += MQTTBroker_ClientSubscribed;


            MqttLocal.Error += MqttLocal_Error;
            MQTTBroker.Start_Server();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (MQTTBroker != null)
                MQTTBroker.StopServer();
        }

        private async void MQTTBroker_ClientSubscribed(object sender, ClientSubscribed e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LstLog.Items.Insert(0, $"{e.ClientId} subscribed to topic '{e.Topic}'");
            });
        }



        private async void MQTTBroker_ServerStarted(object sender, ServerStarted e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LstLog.Items.Insert(0, $"Server started. IP is {e.Ip}, port {e.Port}");
                TxtBrokerIp.Text = e.Ip;
                TxtBrokerPort.Text = e.Port.ToString();
            });
        }
        private async void MQTTBroker_Error(object sender, string e)
        {
            MessageDialog messageDialog = new MessageDialog("[MqttBroker] " + e);
            await messageDialog.ShowAsync();
        }
        private async void MQTTBroker_ClientConnected(object sender, string e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LstLog.Items.Insert(0, $"Client '{e}' connected!");
            });
        }
        private async void MQTTBroker_MessagePublished(object sender, MessageReceived e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LstLog.Items.Insert(0, $"[{e.ClientId}][{e.Topic}] {e.Payload}");
            });
        }


        private async void MqttLocal_Error(object sender, string e)
        {
            MessageDialog messageDialog = new MessageDialog("[MqttLocal] " + e);
            await messageDialog.ShowAsync();
        }
        private async void BtnLocalConnect_Click(object sender, RoutedEventArgs e)
        {
            await MqttLocal.Start(TxtBrokerIp.Text, int.Parse(TxtBrokerPort.Text));
        }

        private async void BtnESP8266Send1_Click(object sender, RoutedEventArgs e)
        {
            await MqttLocal.Publish(TxtESP8266Topic.Text, TxtESP8266Payload1.Text);
        }

        private async void BtnESP8266Send2_Click(object sender, RoutedEventArgs e)
        {
            await MqttLocal.Publish(TxtESP8266Topic.Text, TxtESP8266Payload2.Text);
        }

        private async void BtnRaspSend1_Click(object sender, RoutedEventArgs e)
        {
            await MqttLocal.Publish(TxtRaspTopic.Text, TxtRaspPayload1.Text);
        }

        private async void BtnRaspSend2_Click(object sender, RoutedEventArgs e)
        {
            await MqttLocal.Publish(TxtRaspTopic.Text, TxtRaspPayload2.Text);
        }
    }
}
