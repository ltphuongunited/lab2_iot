using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace lab2
{
    public class ManagerUI : MonoBehaviour
    {
        // public static ManagerUI instance;
        public InputField brokerURI;
        public InputField username;
        public InputField password;

        public Text temp;
        public Text humi;
        public Toggle led;
        public Toggle pump;
        private bool ledCurr=true;
        private bool pumpCurr=true;

        public GameObject layer1;
        public GameObject layer2;

        void Start()
        {
            layer1.SetActive(true);
            layer2.SetActive(false);
            brokerURI.text = "mqttserver.tk";
            username.text = "bkiot";
            password.text = "12345678";
        }


        public void updateStatus(string temp, string humi)
        {
            this.temp.text = temp + "°C";
            this.humi.text = humi + "%";
        }

        public void login()
        {
            GetComponent<ManagerMqtt>().UpdateBeforeConnect();
        }
        public void switch_layer2() {
            layer2.SetActive(true);
            layer1.SetActive(false);
        }

        public void logout()
        {
            GetComponent<ManagerMqtt>().Disconnect();
            layer1.SetActive(true);
            layer2.SetActive(false);
        }

        public void changeLed()
        {
            if (led.isOn != ledCurr)
            {
                GetComponent<ManagerMqtt>().PublishConfigLed(led.isOn);
                ledCurr = led.isOn;
            }
        }

        public void changePump()
        {
            if (pump.isOn != pumpCurr)
            {
                GetComponent<ManagerMqtt>().PublishConfigPump(pump.isOn);
                pumpCurr = pump.isOn;
            }
        }

    }
}