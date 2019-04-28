#include <Servo.h>

Servo servo;

int mz_80=7;
int bant=10;
char gelenVeri;
signed int pos;


void setup() {

  Serial.begin(9600);

  servo.attach(6);
  pinMode(mz_80,INPUT);
  pinMode(bant,OUTPUT);
  pos=0;  
  servo.write(pos);

}

void loop() {
  
boolean sensordurum=digitalRead(mz_80);

   digitalWrite(bant,LOW);
  
  if(sensordurum==0)
  {
    digitalWrite(bant,HIGH);
    delay(1000);
    Serial.write("M");
    while(Serial.available()>0)
{
    gelenVeri = Serial.read();
    Serial.flush();

  switch(gelenVeri)
  {
    case 'R':
    digitalWrite(bant,LOW);
    
    
    break;
    
    
    case 'B':
    digitalWrite(bant,LOW);
    pos=41;
    servo.write(pos);
    delay(5000);
    pos=-41;
    servo.write(pos);
  
    break;
    case 'X':
    digitalWrite(bant,LOW);

    pos=55;
    servo.write(pos);
    delay(5000);
    pos=-55;
    servo.write(pos);
  
    break;
  }
  }
  }
  }












  
