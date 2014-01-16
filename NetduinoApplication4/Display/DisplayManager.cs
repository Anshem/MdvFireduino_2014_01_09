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
using Microsoft.SPOT;
using EmbeddedLab.NetduinoPlus.Day2.Display;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;


namespace NetduinoApplication4
{
    class DisplayManager 
    {
        private LCD lcd;
        public DisplayManager()
        {
            this.lcd = new LCD(
                Pins.GPIO_PIN_D9, // RS
                Pins.GPIO_PIN_D8,  // Enable
                Pins.GPIO_PIN_D7,  // D4
                Pins.GPIO_PIN_D6,  // D5
                Pins.GPIO_PIN_D5,  // D6
                Pins.GPIO_PIN_D4,  // D7
                16,                // Number of Columns 
                LCD.Operational.DoubleLIne, // LCD Row Format
                2,                 // Number of Rows in LCD
                LCD.Operational.Dot5x8);    // Dot Size of LCD
        }

        public void showDisplay1(String msg)
        {
            lcd.Show(msg);
        }

        public void showDisplay(String tmpReal, float rangMin, float rangMax, int combatTime, float inTime, bool combatEnd, bool stop)
        {
            lcd.ShowCursor = true;
            //lcd.Show("FireDuino", 200, true);
            //lcd.Show("   FireDuino   " + var);
            String var="  "+tmpReal+"C";
            String varRangMax = rangMax.ToString();
            String varRangMin = rangMin.ToString();
            
            if (varRangMax.Length == 2)
            {
                varRangMax += ".0";
            }

            if (varRangMax.Length ==1 )
            {
                varRangMax =" "+varRangMax;
                varRangMax += ".0";
            }

            if (varRangMax.Length > 4)
            {
                varRangMax = varRangMax.Substring(0,4);
            }

            if (varRangMin.Length == 2)
            {
                varRangMin += ".0";
            }

            if (varRangMin.Length == 1)
            {
                varRangMin = " " + varRangMin;
                varRangMin += ".0";
            }

            if (varRangMin.Length > 4)
            {
                varRangMin = varRangMin.Substring(0, 4);
            }
            if (!stop)
            {
            if (!combatEnd)
            {
                if (combatTime >= 100)
                {
                    if (inTime.ToString().Length==3)
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"        " + combatTime + "\"");
                    }
                    if (inTime.ToString().Length == 2)
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"         " + combatTime + "\"");
                    }
                    else
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"          " + combatTime + "\"");
                    }
                }
                else if (combatTime >= 10)
                {
                    if (inTime.ToString().Length == 3)
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"         " + combatTime + "\"");
                    }
                    if (inTime.ToString().Length == 2)
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"          " + combatTime + "\"");
                    }
                    else
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"           " + combatTime + "\"");
                    }
                }
                else
                {
                    if (inTime.ToString().Length == 3)
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"          " + combatTime + "\"");
                    }
                    if (inTime.ToString().Length == 2)
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"           " + combatTime + "\"");
                    }
                    else
                    {
                        lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"            " + combatTime + "\"");
                    }
                }
            }
            else
            {
                if (inTime.ToString().Length == 3)
                {
                    lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"    " + "Finished");
                }
                if (inTime.ToString().Length == 2)
                {
                    lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"     " + "Finished");
                }
                else
                {
                    lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"      " + "Finished");
                }
             }
            }else{
                if (inTime.ToString().Length == 3)
                {
                    lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"  " + " WARNING!!");
                }
                if (inTime.ToString().Length == 2)
                {
                    lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"   " + " WARNING!!");
                }
                else
                {
                    lcd.Show(varRangMin + "-" + varRangMax + var + inTime + "\"    " + " WARNING!!");
                }
            }
        }
        public void eraseDisplay()
        {
            lcd.ClearDisplay();
            lcd.ShowCursor = false;
        }

    }         
        
    
}
