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
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoApplication4
{

    public class TMP36
    {
        private SecretLabs.NETMF.Hardware.AnalogInput analogInput;

        public TMP36()
        {
            this.analogInput = new SecretLabs.NETMF.Hardware.AnalogInput(Pins.GPIO_PIN_A5);
        }

        /// <summary>
        ///  This method returns Temperature in Celsius
        /// </summary>
           public float getTemperature(int interval)
           {
               return TMP36.getCTemp(analogInput, 10, interval, 3300);
           }

        // port is analog port  
        // sample is the number of samples to take  
        // interval is the time (ms) to wait between samples  
        // voltage is input voltage * 1000, default is 3.3v  
        public static float getVoltage(AnalogInput port, int sample, int interval, int voltage)
        {
            int reading = 0;
            for (int i = 0; i < sample; i++)
            {
                Thread.Sleep(interval);
                // Read the value on the AIO port  
                reading += (int)port.Read();
            }
            reading /= sample;
           // Debug.Print("Reading: " + reading + "");
            // Voltage at pin in milliVolts = (reading from ADC) * (3300/1024) 
            return (reading * (voltage / 1024F));
        }

        private static float getCTemp(AnalogInput port, int sample, int interval, int voltage)
        {
            // Temp in °C = [(Vout in mV) - 500] / 10 
            return ((getVoltage(port, sample, interval, voltage) - 500F) / 10F);
        }

        public static float getFTemp(AnalogInput port, int sample = 10, int interval = 100, int voltage = 3300)
        {
            // Convert to °F   
            return ((getCTemp(port, sample, interval, voltage) * (9F / 5F)) + 32F);
        }
    }  
}
