/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace lab2
{

    public class data_status
    {
        public string temp { get; set; }
        public string humi { get; set; }
    }

    public class config_data
    {
        public string device { get; set; }
        public string status { get; set; }
    }
    public class ManagerMqtt : M2MqttUnityClient
    {

        public List<string> topics = new List<string>();
        public string Topic_to_Subcribe = "";
        public string msg_received_from_topic_status = "";
        public string msg_received_from_topic_led = "";
        public string msg_received_from_topic_pump = "";
        // private List<string> eventMessages = new List<string>();
        public data_status status;
        public config_data led;
        public config_data pump;


        public void UpdateBeforeConnect()
        {
            this.brokerAddress = GetComponent<ManagerUI>().brokerURI.text;
            this.mqttUserName = GetComponent<ManagerUI>().username.text;
            this.mqttPassword = GetComponent<ManagerUI>().password.text;
            this.brokerPort = 1883;
            this.Connect();
        }
        public void TestPublish()
        {
            client.Publish(Topic_to_Subcribe, System.Text.Encoding.UTF8.GetBytes("Test message"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            Debug.Log("Test message published");
        }

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }


        protected override void OnConnecting()
        {
            try
            {
                base.OnConnecting();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        protected override void OnConnected()
        {
            GetComponent<ManagerUI>().switch_layer2();
            base.OnConnected();
            SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {

            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                }
            }
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Unsubscribe(new string[] { topic });
                }
            }

        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.Log("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            Debug.Log("CONNECTION LOST!");
            Disconnect();
        }



        protected override void Start()
        {
            base.Start();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            if (topic == topics[0])
            {
                Debug.Log("status");
                ProcessMessageStatus(msg);

            }
            if (topic == topics[1])
            {
                Debug.Log("led");
                ProcessMessageControlLed(msg);
            }
            if (topic == topics[2])
            {
                Debug.Log("pump");
                ProcessMessageControlPump(msg);
            }
        }

        private void ProcessMessageStatus(string msg)
        {
            status = JsonConvert.DeserializeObject<data_status>(msg);
            msg_received_from_topic_status = msg;
            GetComponent<ManagerUI>().updateStatus(status.temp, status.humi);
        }

        private void ProcessMessageControlLed(string msg)
        {
            led = JsonConvert.DeserializeObject<config_data>(msg);
            msg_received_from_topic_led = msg;
        }

        private void ProcessMessageControlPump(string msg)
        {
            pump = JsonConvert.DeserializeObject<config_data>(msg);
            msg_received_from_topic_pump = msg;
        }

        public void PublishConfigLed(bool on)
        {
            config_data ledData = new config_data();
            ledData.device = "LED";
            ledData.status = on? "ON" : "OFF";
            string msg_config = JsonConvert.SerializeObject(ledData);
            client.Publish(topics[1], System.Text.Encoding.UTF8.GetBytes(msg_config), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("Publish LED");
        }

        public void PublishConfigPump(bool on)
        {
            config_data pumpData = new config_data();
            pumpData.device = "PUMP";
            pumpData.status = on == true ? "ON" : "OFF";
            string msg_config = JsonConvert.SerializeObject(pumpData);
            client.Publish(topics[2], System.Text.Encoding.UTF8.GetBytes(msg_config), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("Publish PUMP");
        }


        private void OnDestroy()
        {
            Disconnect();
        }

    }


}
