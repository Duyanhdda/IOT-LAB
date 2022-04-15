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

using DG.Tweening;


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Newtonsoft.Json;
using M2MqttUnity.Button;
using Newtonsoft.Json.Linq;
using System.Linq;
/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace M2MqttUnity.Examples
{

    public class Status_Data
    {
        public string temperature { get; set; }
        public string humidity { get; set; }
    }
    public class Status_Device
    {
        public string device { get; set; }
        public bool status { get; set; }

    }
    /// <summary>
    /// Script for testing M2MQTT with a Unity UI
    /// </summary>
    public class M2MqttUnityTest : M2MqttUnityClient
    {
        //[Tooltip("Set this to true to perform a testing cycle automatically on startup")]
        //public bool autoTest = true; 
        //[Header("User Interface")]
        //public InputField consoleInputField;
        //public Toggle encryptedToggle;
        public InputField addressInputField;
        public InputField userInputField;
        public InputField pwdInputField;
        //public InputField portInputField;
        //public Button connectButton;
        //public Button disconnectButton;
        //public Button testPublishButton;
        //public Button clearButton;
        public string Topic;
        [SerializeField]
        private CanvasGroup _canvasLayer1;
        [SerializeField]  
        private CanvasGroup _canvasLayer2;
        [SerializeField] 
        private GameObject Btn_Quit;

        [SerializeField]
        private GameObject Btn_Out;

        public List<string> topics = new List<string>();

        [SerializeField]
        public Status_Data _status_data;

        public string Machine_Id;
        public string Topic_to_Subcribe = "";
        public string msg_received_from_topic = "";
        public Text BugMessage;
        public Text[] text_display = new Text[2];
        private List<string> eventMessages = new List<string>();
        private bool updateUI = false;
        public SwitchButton ledb;
        public SwitchButton pump;
        private Tween twenFade;
        //private void Awake()
        //{
        //    Topic_to_Subcribe = Topic + Machine_Id;
        //}

        public void Fade(CanvasGroup _canvas, float endValue, float duration, TweenCallback onFinish)
        {
            if (twenFade != null)
            {
                twenFade.Kill(false);
            }

            twenFade = _canvas.DOFade(endValue, duration);
            twenFade.onComplete += onFinish;
        }

        public void FadeIn(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 1f, duration, () =>
            {
                _canvas.interactable = true;
                _canvas.blocksRaycasts = true;
            });
        }

        public void FadeOut(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 0f, duration, () =>
            {
                _canvas.interactable = false;
                _canvas.blocksRaycasts = false;
            });
        }



        IEnumerator _IESwitchLayer()
        {
            if (_canvasLayer1.interactable == true)
            {
                FadeOut(_canvasLayer1, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_canvasLayer2, 0.25f);
            }
            else
            {
                FadeOut(_canvasLayer2, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_canvasLayer1, 0.25f);
            }
        }

        IEnumerator _IESwitchLayer1()
        {
            if (_canvasLayer2.interactable == true)
            {
                FadeOut(_canvasLayer2, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_canvasLayer1, 0.25f);
            }
            else
            {
                FadeOut(_canvasLayer1, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_canvasLayer2, 0.25f);
            }
        }
        public void SwitchLayer()
        {
            StartCoroutine(_IESwitchLayer());
        }
        public void SwitchLayer1()
        {
            OnApplicationQuit();
            addressInputField.text = "";
            userInputField.text = "";
            pwdInputField.text = "";
            StartCoroutine(_IESwitchLayer1());
        }
	    public void UpdateBeforeConnect(){
            if( addressInputField.text == "" || userInputField.text == ""  ){
                OnConnectionFailed("CONNECTION FAILED!");

            }
            else{
                this.brokerAddress = addressInputField.text;
                //this.brokerPort = (int) int.Parse(portInputField.text);
                this.mqttUserName = userInputField.text;
                this.mqttPassword = pwdInputField.text;
                this.Connect();
            }
	    }
        public void TestPublish()
        {
            // _status_data = new Status_Data();
            // _status_data.temperature = "31.0";
            // _status_data.humidity = "70.0";
            // string msg_config = JsonConvert.SerializeObject(_status_data);
                string data = "{\"device\":\"PUMP\",\"status\":false}";
                client.Publish("/bkiot/1912562/pump", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("publish led config");   
                
        }


        //public void SetBrokerAddress(string brokerAddress)
        //{
        //    if (addressInputField && !updateUI)
        //    {
        //        this.brokerAddress = brokerAddress;
        //    }
        //}

        //public void SetBrokerPort(string brokerPort)
        //{
        //    if (portInputField && !updateUI)
        //    {
        //        int.TryParse(brokerPort, out this.brokerPort);
        //    }
        //}

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }


        //public void SetUiMessage(string msg)
        //{
        //    if (consoleInputField != null)
        //    {
        //        consoleInputField.text = msg;
        //        updateUI = true;
        //    }
        //}

        public void AddUiMessage(string msg)
        {
            //if (consoleInputField != null)
            //{
            //    consoleInputField.text += msg + "\n";
            //    updateUI = true;
            //}
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            //SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SwitchLayer();
            //SetUiMessage("Connected to broker on " + brokerAddress + "\n");

            //if (autoTest)
            //{
            //    TestPublish();
            //}
            SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {
            // if (Topic_to_Subcribe != "")
            // {
            //     client.Subscribe(new string[] { Topic_to_Subcribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            // }
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
            // client.Unsubscribe(new string[] { Topic_to_Subcribe });
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
            // AddUiMessage("CONNECTION FAILED! " + errorMessage);
            BugMessage.text =  errorMessage;

        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        private void UpdateUI()
        {
            //if (client == null)
            //{
            //    if (connectButton != null)
            //    {
            //        connectButton.interactable = true;
            //        disconnectButton.interactable = false;
            //        testPublishButton.interactable = false;
            //    }
            //}
            //else
            //{
            //    if (testPublishButton != null)
            //    {
            //        testPublishButton.interactable = client.IsConnected;
            //    }
            //    if (disconnectButton != null)
            //    {
            //        disconnectButton.interactable = client.IsConnected;
            //    }
            //    if (connectButton != null)
            //    {
            //        connectButton.interactable = !client.IsConnected;
            //    }
            //}
            //if (addressInputField != null && connectButton != null)
            //{
            //    addressInputField.interactable = connectButton.interactable;
            //    addressInputField.text = brokerAddress;
            //}
            //if (portInputField != null && connectButton != null)
            //{
            //    portInputField.interactable = connectButton.interactable;
            //    portInputField.text = brokerPort.ToString();
            //}
            //if (encryptedToggle != null && connectButton != null)
            //{
            //    encryptedToggle.interactable = connectButton.interactable;
            //    encryptedToggle.isOn = isEncrypted;
            //}
            //if (clearButton != null && connectButton != null)
            //{
            //    clearButton.interactable = connectButton.interactable;
            //}
            //updateUI = false;
        }

        protected override void Start()
        {
            //SetUiMessage("Ready.");
            Topic_to_Subcribe = Topic + Machine_Id;
            updateUI = true;
            base.Start();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            // TestPublish();
            BugMessage.text = "";
            string msg = System.Text.Encoding.UTF8.GetString(message);
            // Debug.Log("Received: " + msg);
            //StoreMessage(msg);
            if (topic == topics[0])
                ProcessMessageStatus(msg);
            if (topic == topics[1])
                ProcessMessageLed(msg);
            if (topic == topics[2])
                ProcessMessagePump(msg);
        }
        private string Convert(string msg){
            string a = "";
            for (int i = 0; i < msg.Length; i++) 
            {
                if(msg[i] == '.') break;
                a += msg[i];
            }
            return a;
        }
        private void ProcessMessageStatus(string msg)
        {
            Status_Data _status_data = JsonConvert.DeserializeObject<Status_Data>(msg);
            text_display[0].text = Convert(_status_data.temperature) + "°C";
            text_display[1].text = Convert(_status_data.humidity) + "%";
        }
        private void ProcessMessageLed(string msg)
        {
            Status_Device _status_data = JsonConvert.DeserializeObject<Status_Device>(msg);
            ledb.SetStatus(_status_data.status);
        }
        private void ProcessMessagePump(string msg)
        {
            Status_Device _status_data = JsonConvert.DeserializeObject<Status_Device>(msg);
            pump.SetStatus(_status_data.status);
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage("Received: " + msg);
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            //if (autoTest)
            //{
            //    autoConnect = true;
            //}
        }
        public void switchButtonPubData(){ 
            if(ledb.switchState == false){
                string data = "{\"device\":\"LED\",\"status\":true}";
                client.Publish("/bkiot/1912562/led", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("publish led true config");
            }
            else{
                string data = "{\"device\":\"LED\",\"status\":false}";
                client.Publish("/bkiot/1912562/led", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("publish led false config");                
            }
            ledb.SetStatus(!ledb.switchState);
        }
        public void switchButtonPubData1(){ 
            if(pump.switchState == false){
                string data = "{\"device\":\"PUMP\",\"status\":true}";
                client.Publish("/bkiot/1912562/pump", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("publish pump true config");
            }
            else{
                string data = "{\"device\":\"PUMP\",\"status\":false}";
                client.Publish("/bkiot/1912562/pump", System.Text.Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
                Debug.Log("publish led false config");                
            }  
            pump.SetStatus(!ledb.switchState);          
        }
    }
}
