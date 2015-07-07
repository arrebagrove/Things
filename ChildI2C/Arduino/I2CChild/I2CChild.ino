#include <Wire.h>
#include <Servo.h>

Servo ServoOne;

void setup()
{
	pinMode(12, OUTPUT);
	ServoOne.attach(9);
  
	Wire.begin(4); //Arduino child number
	Wire.onReceive(receiveEvent); //I2C on receive event (RPI2 write)
	Wire.onRequest(requestEvent); //I2C on request event (RPI2 read)
	
	Serial.begin(9600); //Debug
}

void loop() 
{ 
  delay(1000);  // waits for the servo to get there 
} 

String laststamp = "";
String message = "";

void receiveEvent(int count)
{
  message += DecodeUri();
  int STXidx= message.indexOf(0x02);
  int ETXidx= message.indexOf(0x03);
  int CKSTXidx= message.lastIndexOf(0x02);
  int CKETXidx= message.lastIndexOf(0x03);
  
  if (ETXidx > -1) //finished
  {
	  if (STXidx > -1 && ETXidx == CKETXidx && STXidx == CKSTXidx)
	  {
		  ProcessCommand(message);
		  message = "";
	  }
	  else
	  {
		  //corrupt
		  message = "";
	  }
  }
}

String answer = "";
void ProcessCommand(String uri)
{
  laststamp = GetParameterValue("dt", uri);
  Serial.println(uri);
  String command = GetCommand(uri);
  
	if (command == "sio")
	{
		int port = GetParameterValue("pt", uri).toInt();
		String state = GetParameterValue("st", uri);
		ProcessGpio(port, state == "on");
	}
	else if (command == "svo")
	{
		int port = GetParameterValue("pt", uri).toInt();
		int angle = GetParameterValue("an", uri).toInt();
        Serial.println(angle);
		ServoOne.write(angle);
	}
	else if (command = "gio")
	{
		int port = GetParameterValue("pt", uri).toInt();
		if (digitalRead(port))
		{
			answer = "on";
		}
		else
		{
			answer = "off";
		}
	}
}

String DecodeUri()
{
	String ret = "";
	while (1 < Wire.available())
	{
    ret += (char)Wire.read();
	}
	ret += (char)Wire.read();

	return ret;
}

String GetCommand(String message)
{
  return message.substring(1, message.indexOf("?")); //removes STX (start text)
}

String GetParameterValue(String parameter, String message)
{
  int position = message.indexOf(parameter) + parameter.length() + 1;

  int end = message.indexOf("&",position);

  if(end == -1)
  {
    end = message.length() -1 ; //removes ETX (end text)
    //Serial.println(end);
  }
  return message.substring(position, end);
}

void ProcessGpio(int number,bool status)
{
	digitalWrite(number, status);
}

void requestEvent()
{
	if (answer != "")
	{
		Wire.write(answer.c_str());
		answer = "";
	}
	else
	{
		Wire.write(laststamp.c_str());
	}
}
