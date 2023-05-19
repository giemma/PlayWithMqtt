#include <ESP8266WiFi.h>
#include <PubSubClient.h>

const char* ssid = "NOMERETE";
const char* password = "PASSWORDRETE";

const char* device_identifier = "device1";
const char* mqtt_ip = "192.168.99.145";
const int mqtt_port = 1883;

const char* mqtt_user = "";
const char* mqtt_password = "";



WiFiClient espClient;
PubSubClient client(espClient);

void setup() {
  Serial.begin(115200); 
  pinMode(0, OUTPUT);

  digitalWrite(0, HIGH);

  WiFi.begin(ssid, password);
  Serial.println("Connecting");
  while(WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.print("Connected to WiFi network with IP Address: ");
  Serial.println(WiFi.localIP());

  client.setClient(espClient);
  client.setServer(mqtt_ip, mqtt_port);
  client.setCallback(callback);

  String in_topic ="esp8266/" + String(device_identifier) + "/fromserver/";
  client.subscribe(in_topic.c_str());

  digitalWrite(0, LOW);
}

int packetNumber=0;
unsigned long lastSent=0;

void loop() {

  if (!client.connected()) {
    reconnect();
    String in_topic ="esp8266/" + String(device_identifier) + "/fromserver/";
    client.subscribe(in_topic.c_str());
  }
  client.loop();
    
  if(millis() - lastSent>5000){
    packetNumber++;
    String topic ="esp8266/" + String(device_identifier) + "/toserver/";
    //String p = "OK ";
    //String payload = p + packetNumber;
    String payload = "OK";
    client.publish(topic.c_str(), payload.c_str(), false);
    lastSent=millis();
  }  
  
}

void reconnect() {  
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");   
    if (client.connect(device_identifier, mqtt_user, mqtt_password)) {
      Serial.println("connected");
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      delay(5000);
    }
  }
}

void callback(char* topic, byte* payload, unsigned int length) {
  String topicStr = topic; 

  Serial.println(topicStr);

  payload[length] = 0;
  String recv_payload = String(( char *) payload);

  if(recv_payload == "ON"){
    digitalWrite(0, HIGH);
    Serial.println("On");
  }else if(recv_payload == "OFF"){
    digitalWrite(0, LOW);
    Serial.println("OFF");
  }else{
    Serial.println("UNKNOWN:");
    Serial.println(recv_payload);
    
    for (int i = 0; i < 3; i++) {
      digitalWrite(0, HIGH);
      delay(200);
      digitalWrite(0, LOW);
    }
  }
  
  Serial.println();
}