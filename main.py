

print("Xin chào ThingsBoard")
import paho.mqtt.client as mqttclient
import time
from selenium.webdriver.common.by import By
from selenium import webdriver
driver = webdriver.Chrome()
driver.get("https://www.where-am-i.co/")
BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883
THINGS_BOARD_ACCESS_TOKEN = "tpmf3c9NcxvIH3rq9FGk"


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    temp_data = {'value': True}
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setValue":
            temp_data['value'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    except:
        pass


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



# Cách 1
# GEO_IP_API_URL  = 'http://ip-api.com/json/'
#
# # Can be also site URL like this : 'google.com'
# IP_TO_SEARCH    = ip_address
#
# # Creating request object to GeoLocation API
# req             = urllib.request.Request(GEO_IP_API_URL+IP_TO_SEARCH)
# # Getting in response JSON
# response        = urllib.request.urlopen(req).read()
# # Loading JSON from text to object
# json_response   = json.loads(response.decode('utf-8'))
# latitude = json_response['lat']
# longtitude = json_response['lon']


# cách 2
def locate():
    element = driver.find_element(By.XPATH, '/html/body/main/div/div[2]/div[2]/div/table/tbody/tr[3]/td/span')
    data = str(element.text)
    latlon = data.split(', ')
    return latlon

while True:
    longtitude = locate()[1]
    latitude = locate()[0]
    collect_data = {'temperature': temp, 'humidity': humi, 'light': light_intesity
                    , 'longitude': longtitude, 'latitude': latitude}
    temp += 1
    humi += 1
    light_intesity += 1
    client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    time.sleep(5)