using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anyBaseControl
{
    class ApplicationStates
    {

        // define a list of possible application states for the COM port connection
        public enum PortStates
        {
            PORT_OPENING,
            PORT_WAITING_GRBL_HEADER,
            PORT_RUNNING_IDLE,
            PORT_RUNNING_TRANSMIT,
            PORT_CLOSING,
            PORT_CLOSED
        }
    }
}
