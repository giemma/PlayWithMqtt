import utime
import machine
import network
from lib.umqtt.simple import MQTTClient

SSID = "NOMERETE"
SSID_PASSWORD = "PASSWORD"
CLIENT_ID = "Raspberry Pi Pico W"
MQTT_BROKER = "192.168.99.145"
SUBSCRIBE_TOPIC="rasp/device1/fromserver/"

def blink():
    for x in range(5):
        led.on()
        utime.sleep_ms(100)
        led.off()
        utime.sleep_ms(100)
        
def do_connect_to_network():
    try:
        sta_if = network.WLAN(network.STA_IF)
        if not sta_if.isconnected():
            print('connecting to network...')
            sta_if.active(True)
            sta_if.connect(SSID, SSID_PASSWORD)
            while not sta_if.isconnected():
                print("Attempting to connect....")
                utime.sleep(1)
        print('Connected! Network config:', sta_if.ifconfig())
        return True
    except:
        return False
    
def sub_cb(topic, msg):
    #print((topic, msg))
    serialized = msg.decode('utf8', 'strict')
    
    if serialized == "ON":
        led.on()
    elif serialized == "OFF":
        led.off()
    else:
        print("Unknown command")
        print(serialized)
    
def do_connect_to_broker():
    global mqttClient,SUBSCRIBE_TOPIC
    try:        
        mqttClient = MQTTClient(CLIENT_ID, MQTT_BROKER, keepalive=60)
        mqttClient.set_callback(sub_cb)
        mqttClient.connect()
        
        print(SUBSCRIBE_TOPIC)
        mqttClient.subscribe(SUBSCRIBE_TOPIC)
        print(f"Connected to MQTT  Broker :: {MQTT_BROKER}, and waiting for callback function to be called!") 
        return True
    except:
        return False
    
def reset():
    print("Resetting...")
    utime.sleep_ms(1000)
    machine.reset()
    
#try:
led = machine.Pin(16,machine.Pin.OUT)
blink()

print("Connecting to your wifi...")
if do_connect_to_network():
    if do_connect_to_broker():
        led.off()
    #else:
        #reset()
#else:
    #reset()

led.off()
while True:
    mqttClient.check_msg()
    
#except:
#    reset()