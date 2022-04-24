
import paho.mqtt.client as mqttclient
import time
import serial.tools.list_ports
print("Xin chào ThingsBoard")
mess = ""
bbc_port = "COM7"
if len(bbc_port) > 0:
    ser = serial.Serial(port=bbc_port, baudrate=115200)


def processData(data):
    data = data.replace("!", "")
    data = data.replace("#", "")
    splitData = data.split(":")
    print(splitData)


def readSerial():
    bytesToRead = ser.inWaiting()
    if (bytesToRead > 0):
        global mess
        mess = mess + ser.read(bytesToRead).decode("UTF-8")
        while ("#" in mess) and ("!" in mess):
            start = mess.find("!")
            end = mess.find("#")
            processData(mess[start:end + 1])
            if (end == len(mess)):
                mess = ""
            else:
                mess = mess[end + 1:]


BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883
THINGS_BOARD_ACCESS_TOKEN = "tpmf3c9NcxvIH3rq9FGk"


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")

cmd = 0
def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    temp_data = {'value': True}
    try:
        jsonobj = json.loads(message.payload)
        # if jsonobj['method'] == "setValue":
        if jsonobj['method'] == "setLED":
            temp_data['valueLED'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
        elif jsonobj['method'] == "setAIR":
            temp_data['valueAIR'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    except:
        pass
    if len(bbc_port) > 0:
        ser.write((str(cmd) + "#").encode())

def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Thingsboard connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")


client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(THINGS_BOARD_ACCESS_TOKEN)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message

temp = 30
humi = 50
light_intesity = 100
counter = 0
from requests import get
import json
import urllib.request

ip_address = get('https://api.ipify.org').content.decode('utf8')

GEO_IP_API_URL = 'http://ip-api.com/json/'

# Can be also site URL like this : 'google.com'
IP_TO_SEARCH = ip_address

# Creating request object to GeoLocation API
req = urllib.request.Request(GEO_IP_API_URL + IP_TO_SEARCH)
# Getting in response JSON
response = urllib.request.urlopen(req).read()
# Loading JSON from text to object
json_response = json.loads(response.decode('utf-8'))
latitude = json_response['lat']
longitude = json_response['lon']

while True:

    if len(bbc_port) > 0:
        readSerial()
    time.sleep(1)
