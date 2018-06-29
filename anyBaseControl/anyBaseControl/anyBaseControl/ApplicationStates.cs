using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anyBaseControl
{
    class ApplicationStates
    {

        // define a list of states for the COM port connection
        public enum PortStates
        {
            PORT_OPENING,               // transition to opening the COM port
            PORT_WAITING_GRBL_HEADER,   // port opened, waiting for response from GRBL
            PORT_RUNNING_IDLE,          // port connected, but not file is being processed
            PORT_RUNNING_TRANSMIT,      // port connected, and a GCODE file is processed
            PORT_CLOSING,               // transition to port closing
            PORT_CLOSED                 // port is closed; initial state
        }

        // define a list of states for the COM handler; these are a substate 
        public enum TransmitStates
        {
            TX_IDLE,        // COM port is in PORT_RUNNING_TRANSMIT state, and a new transmission is possible
            TX_PENDING      // a new transmission is blocked until a confirmation ("ok") is received from COM
        }
    }
}
