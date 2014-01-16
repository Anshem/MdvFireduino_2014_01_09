/*----------------------------------------------------------------------------------+
|                                                                                   |
|           Copyright (c) 2014, Xhakal_Systems. All Rights Reserved.                |
|                                                                                   |
|           Limited permission is hereby granted to reproduce and modify this       |
|           copyrighted material provided that this notice is retained              |
|           in its entirety in any such reproduction or modification.               |
|                                                                                   |
|           Author: mTéllez                                                         |
|           First Version Date: 2014/01/16                                          |
|                                                                                   |
+-----------------------------------------------------------------------------------+
 */

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using HttpLibrary;

namespace NetduinoApplication4
{
    public class Program 
    {
        private static HttpServer Server;//server object
        //temperature collected by the sensor
        public static float temp;
        //public static String tiempo;
        //Rank of temperature
        public static float maxTemp, minTemp;
        //CombatTime --> Time of combat. time --> Remaining Time. inRange --> Time in Range.
        public static int combatTime, time, interval = 50;
        public static float inRangeAux, inRange;
        public static bool combatEnd = true;
        public static bool stop = false;
        public static bool flagDisplay = true;
        public static bool flagTimeNow=true;
        public static bool coldDeactivate;
       
        public static void Main()
        {
            Program program = new Program();
            Server = new HttpServer(80, 100, 256, @"\SD");//initialize server with sd card as a storage device
            Server.OnServerError += new OnErrorDelegate(Server_OnServerError);
            Server.Start();
            
            Debug.Print("IP ADDRESS OBTAINED : " + Server.ObtainedIp);
            // Unlimited thread for visualization
            Thread c = new Thread(program.temperature);
            c.Start();
            // Unlimited thread for visualization
            Thread t = new Thread(program.printDisplay);
            t.Start();
            // End of operation
            // clean 

        }
        /// <summary>
        /// This method is the controller of temperature
        /// </summary>
        /// <param name="combatTime">Combat Time selected</param>
        /// <param name="minTemperature"></param>
        /// <param name="maxTemperature"></param>
        /// <param name="refreshTime"></param>
        /// <returns></returns>
        public String controlador(int combatTime, float minTemperature, float maxTemperature, int value)
        {
            String sendResponse = "";
            interval = value;
            Debug.Print("Entro en el controlador");
            RelayManager sensor = new RelayManager();
            minTemp = minTemperature;
            maxTemp = maxTemperature;
            String minS = "";
            String maxS = "";
            int inRangeD=0;
            String inRangeS = "";
            time = countDown(combatTime, DateTime.Now.ToString());
            bool flag = true;
            bool comeToinrange = false;
            int baseInTime = 0;
            int actualInTime = 0;
            inRange = 0;
            String timeForZone = DateTime.Now.ToString();
            //Inicio del While
            while (time > 0 &&!stop)
            {   
                combatEnd = false;
                time = countDown(combatTime, DateTime.Now.ToString());
                if (minTemperature <= temp && maxTemperature >= temp)
                {
                    if(flag){
                        String toFinish = DateTime.Now.ToString();
                        int minTosec = Convert.ToInt32(toFinish.Substring(14, 2)) * 60;
                        int sec = Convert.ToInt32(toFinish.Substring(17, 2));
                        baseInTime = minTosec + sec;
                        flag=false;
                    }
                    comeToinrange = true;
                    coldDeactivate = sensor.setDeactivateCold();
                    sensor.setDeactivateHeat();
                }

                if(temp < minTemperature){
                    flag = true;
                    coldDeactivate = sensor.setDeactivateCold();
                    sensor.setActivateHeat();
                   if(comeToinrange){
                    String toFinish = DateTime.Now.ToString();
                    int minTosec = Convert.ToInt32(toFinish.Substring(14, 2)) * 60;
                    int sec = Convert.ToInt32(toFinish.Substring(17, 2));
                    actualInTime = minTosec + sec;
                    inRangeAux = actualInTime - baseInTime;
                    inRange += inRangeAux;
                    comeToinrange = false;
                   }
                }

                if (temp > maxTemperature)
                {
                    flag = true;
                    sensor.setDeactivateHeat();
                    coldDeactivate = sensor.setActivateCold();
                   if (comeToinrange)
                    {
                        String toFinish = DateTime.Now.ToString();
                        int minTosec = Convert.ToInt32(toFinish.Substring(14, 2)) * 60;
                        int sec = Convert.ToInt32(toFinish.Substring(17, 2));
                        actualInTime = minTosec + sec;
                        inRangeAux = actualInTime - baseInTime;
                        inRange += inRangeAux;
                        comeToinrange = false;
                    }
                }
                if (temp>=40)
                {
                    stop = true;
                    minS = minTemp.ToString("N1");
                    maxS = maxTemp.ToString("N1");
                    inRangeD = (int)inRange;
                    inRangeS = inRangeD.ToString();
                    if (inRangeS.Length == 1)
                    {
                        inRangeS = "00" + inRangeS;
                    }

                    else if (inRangeS.Length == 2)
                    {
                        inRangeS = "0" + inRangeS;
                    }
                    sendResponse = "m" + minS + "M" + maxS + "R" + inRangeS + "W";
                    return sendResponse;
                }
                
            }
            //fin del while
            //si viene de estar en rango actualizo el tiempo dentro de combate
            if (comeToinrange)
            {
                String toFinish = DateTime.Now.ToString();
                int minTosec = Convert.ToInt32(toFinish.Substring(14, 2)) * 60;
                int sec = Convert.ToInt32(toFinish.Substring(17, 2));
                actualInTime = minTosec + sec;
                 inRangeAux = actualInTime - baseInTime;
                inRange += inRangeAux;
                comeToinrange = false;
            }
            combatEnd = true;
            coldDeactivate = sensor.setDeactivateCold();
            sensor.setDeactivateDefense();
            sensor.setDeactivateHeat();
            sensor.setDeactivateHighDefense();
            minS = minTemp.ToString("N1");
            maxS = maxTemp.ToString("N1");
           
            inRangeD =(int) inRange;   
            inRangeS =inRangeD.ToString();

            if (inRangeS.Length ==1) {

                inRangeS = "00" + inRangeS;
            }

            else if (inRangeS.Length == 2)
            {

                inRangeS = "0" + inRangeS;
            }
            
            sendResponse = "m" + minS + "M" + maxS + "R" + inRangeS;
            flagTimeNow=true;    
            return sendResponse;
        }
        /// <summary>
        /// This method updates the sample temperature display and the web
        /// </summary>
        public void printDisplay()
        {
            DisplayManager display = new DisplayManager();
            display.showDisplay1("IP ADDRESS:     " + Server.ObtainedIp);
            Thread.Sleep(2000);
            while (true)
            {
                Thread.Sleep(300);
                display.showDisplay(temp.ToString().Substring(0,4), minTemp, maxTemp, time, inRange, combatEnd, stop);
            }
        }

        public void printDisplay1(String msg)
        {
            DisplayManager display = new DisplayManager();
            display.showDisplay1(msg);
        }

        public void temperature()
        {
            TMP36 tmp36 = new TMP36();
            while(true)
            {
                Thread.Sleep(20);
                temp = tmp36.getTemperature(interval);
            }
        }

        public int countDown(int combatTime2, String toFinish)
        {
            int minTosec = Convert.ToInt32(toFinish.Substring(14, 2))*60;
            int sec = Convert.ToInt32(toFinish.Substring(17, 2));
            int elapsedSeconds = minTosec + sec;
            if (flagTimeNow)
            {
                combatTime2 += elapsedSeconds;
                combatTime = combatTime2;
                flagTimeNow = false;
            }
            return combatTime-elapsedSeconds;
        }
        
        public void testCold()
        {
            RelayManager sensor = new RelayManager();
            sensor.setActivateCold();
            Thread.Sleep(3000);
            sensor.setDeactivateCold();
        }
        public void testWarm()
        {
            RelayManager sensor = new RelayManager();
            sensor.setActivateHeat();
            Thread.Sleep(3000);
            sensor.setDeactivateHeat();
        }

        public void abort()
        {
           stop = true;
        }

        static void Server_OnServerError(object sender, OnErrorEventArgs e)
        {
            Debug.Print(e.EventMessage);
        }
        
    }        
}     

