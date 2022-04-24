import random
import time
import  sys
from  Adafruit_IO import  MQTTClient
import json
import serial.tools.list_ports

AIO_FEED_ID_LED = "btt-led"
AIO_FEED_ID_FAN = "btt-fan"
AIO_USERNAME = "duyvu1109"
AIO_KEY = "aio_APje81ttogFqZ1rbz5Ld8ZqkBUzO"
bbc_port = "COM10"
mess = ""
if len(bbc_port) > 0:
    ser = serial.Serial(port=bbc_port, baudrate=115200)

light = 0
temp = 0
cmd = 0

def processData(data):
    global light
    global temp
    global cmd
    data = data.replace("!", "")
    data = data.replace("#", "")
    splitData = data.split(":")
    print(splitData)
    if (splitData[1]=='LIGHT'):
        light = int(splitData[2])
        client.publish("bbc-light", light)
    elif(splitData[1]=='TEMP'):
        temp = int(splitData[2])
        client.publish("bbc-temp", temp)
    elif(splitData[1]=='FAN'):
        if(cmd==0):
            cmd=1
            client.publish("btt-fan", "ON")
        elif(cmd==1):
            cmd=0
            client.publish("btt-fan", "OFF")
        if len(bbc_port) > 0:
            ser.write((str(cmd) + "#").encode())
    elif(splitData[1]=='LED'):
        if(cmd==2):
            cmd=3
            client.publish("btt-led", "ON")
        elif(cmd==3):
            cmd=2
            client.publish("btt-led", "OFF")
        if len(bbc_port) > 0:
            ser.write((str(cmd) + "#").encode())
    


def  connected(client):
    print("Ket noi thanh cong...")
    client.subscribe(AIO_FEED_ID_LED)
    client.subscribe(AIO_FEED_ID_FAN)
def  subscribe(client , userdata , mid , granted_qos):
    print("Subscribe thanh cong...")

def  disconnected(client):
    print("Ngat ket noi...")
    sys.exit (1)

def message(client , feed_id , payload):
    print("Nhan du lieu: " + feed_id + " " + payload)
    if(feed_id=="btt-fan"):
        client.publish("btt-fan", payload)
        if(payload=="OFF"):
            cmd = 0
        else: 
            cmd = 1
    elif(feed_id =="btt-led"):
        client.publish("btt-led", payload)
        if(payload=="OFF"):
            cmd = 2
        else: 
            cmd = 3
    if len(bbc_port) > 0:
        ser.write((str(cmd) + "#").encode())
        
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
                mess = mess[end+1:]



client = MQTTClient(AIO_USERNAME , AIO_KEY)
client.on_connect = connected
client.on_disconnect = disconnected
client.on_message = message
client.on_subscribe = subscribe
client.connect()
client.loop_background()

while True:
    # value = random.randint(0, 100)
    # value1 = random.randint(0, 100)
    # print("Cap nhat temp:", value)
    # print("Cap nhat light:", value1)
    # client.publish("bbc-temp", value)
    # client.publish("bbc-light", value1)
    if len(bbc_port) >  0:
        readSerial()
    time.sleep(2)