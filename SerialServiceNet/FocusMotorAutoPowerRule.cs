using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialServiceNet
{
    public class FocusMotorAutoPowerRule
    {
        public bool Enabled { get; set; } = true;

        public delegate void MotorControllerOn();

        public MotorControllerOn MotorEnable;

        public delegate void MotorControllerOff();

        public MotorControllerOff MotorDisable;

        /// <summary>
        /// Call this rule before motor start to work
        /// </summary>
        public void BeforeFocus()
        {
            if (Enabled)
            {
                MotorEnable();
            }
        }

        public void AfterFocus()
        {
            if (Enabled)
            {
                MotorDisable();
            }
        }
    }
}
