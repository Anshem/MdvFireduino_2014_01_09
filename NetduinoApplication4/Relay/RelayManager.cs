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
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Microsoft.SPOT.Hardware;
//This class control the relays
namespace NetduinoApplication4
{
    /// <summary>
    ///  D0 --> If it's heat.
    ///  D3 --> If it's cold.
    ///  D2 --> If it's under attack.
    /// </summary>
    public class RelayManager
    {
        private OutputPort cold;
        private OutputPort heat;
        private OutputPort defense;
        private OutputPort highDefense;
        
        /// <summary>
        ///  This method returns false if cold is activate
        /// </summary>
        public Boolean setActivateCold()
        {
            return activateCold();
        }
        /// <summary>
        ///  This method returns false if Heat is activate
        /// </summary>
        public Boolean setActivateHeat()
        {
            return activateHeat();
        }
        /// <summary>
        ///  This method returns false if defense is activate
        /// </summary>
        public Boolean setActivateDefense()
        {
            return activateDefense();
        }
        /// <summary>
        ///  This method returns false if HighDefense is activate
        /// </summary>
        public Boolean setActivateHighDefense()
        {
            return activateHighDefense();
        }
        /// <summary>
        ///  This method returns true if cold is deactivate
        /// </summary>
        public Boolean setDeactivateCold()
        {
            return deactivateCold();
        }
        /// <summary>
        ///  This method returns true if Heat is deactivate
        /// </summary>
        public Boolean setDeactivateHeat()
        {
            return deactivateHeat();
        }
        /// <summary>
        ///  This method returns true if Defense is deactivate
        /// </summary>
        public Boolean setDeactivateDefense()
        {
            return deactivateDefense();
        }
        /// <summary>
        ///  This method returns true if HighDefense is deactivate
        /// </summary>
        public Boolean setDeactivateHighDefense()
        {
            return deactivateHighDefense();
        }
        /// <summary>
        ///  Class Constructor
        /// </summary>
        public RelayManager()
        {
          //this.analogInput = new SecretLabs.NETMF.Hardware.AnalogInput(Pins.GPIO_PIN_A5);
            this.cold = new OutputPort(Pins.GPIO_PIN_D0, true);
            this.heat = new OutputPort(Pins.GPIO_PIN_D3, true);
            this.defense = new OutputPort(Pins.GPIO_PIN_D2, true);
            this.highDefense = new OutputPort(Pins.GPIO_PIN_D1, true);
        }
        /********/
        private Boolean deactivateCold()
        {
            if (cold.Read() == false)
            {
                cold.Write(true);
                return cold.Read();
            }
            return cold.Read();
        }
        private Boolean deactivateHeat()
        {
            if (!heat.Read())
            {
                heat.Write(true);
                return heat.Read();
            }
            return heat.Read();
        }
        private Boolean deactivateDefense()
        {
            if (!defense.Read())
            {
                defense.Write(true);
                return defense.Read();
            }
            return defense.Read();
        }
        private Boolean deactivateHighDefense()
        {
            if (!highDefense.Read())
            {
                highDefense.Write(true);
                return highDefense.Read();
            }
            return false;
        }
        /********/
        private Boolean activateCold()
        {
            if (cold.Read())
            {
                cold.Write(false);
            }
            return cold.Read();
        }
        private Boolean activateHeat()
        {
            if (heat.Read())
            {
                heat.Write(false);
            }
            return heat.Read();
        }
        private Boolean activateDefense()
        {
            if (defense.Read())
            {
                defense.Write(false);
            }
            return defense.Read();
        }
        private Boolean activateHighDefense()
        {
            if (highDefense.Read())
            {
                highDefense.Write(false);
            }
            return highDefense.Read();
        }


    }
}
