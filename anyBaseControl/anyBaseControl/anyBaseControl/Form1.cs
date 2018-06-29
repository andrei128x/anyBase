using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static anyBaseControl.ApplicationStates;

namespace anyBaseControl
{
    public partial class Form1 : Form
    {

        /* CONSTANTS */

        /* other INTERNAL variables */
        int count = 0;
        int recvSize;


        /* relevant VARIABLES */

        // default COM port connection state
        PortStates currentPortState = PortStates.PORT_CLOSED;

        // transmission state
        TransmitStates currentTxState = TransmitStates.TX_IDLE;

        // COM ports list, initially empty, but not null
        string[] portNames = { };

        // init the console handler;
        CommunicationHandler ComHandler;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ComHandler = new CommunicationHandler(textOutput);

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // state    -   PORT_CLOSED
            // type     -   permanent state
            // performs -   CLOSED -> OPENING transition
            if (currentPortState == PortStates.PORT_CLOSED)
            {
                currentPortState = PortStates.PORT_OPENING;
            }
            else if ((currentPortState == PortStates.PORT_RUNNING_IDLE) || (currentPortState == PortStates.PORT_RUNNING_TRANSMIT))
            {
                currentPortState = PortStates.PORT_CLOSING;
            }
        }

        private void timerBkgrTasks_Tick(object sender, EventArgs e)
        {
            // enumerate for new ports and update the list
            if (!SerialPort.GetPortNames().SequenceEqual(portNames))
            {
                portNames = SerialPort.GetPortNames();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(portNames);

                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }


            // state    -   PORT_OPENING
            // type     -   transitory to PORT_WAITING_GRBL_HEADER
            // performs -   opening COM connection and start of acquisition timer
            // error    -   return to PORT_CLOSED state
            if (currentPortState == PortStates.PORT_OPENING)
            {
                try
                {
                    serialPort.PortName = comboBox1.Text;
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                    timerComHandler.Enabled = true;
                    currentPortState = PortStates.PORT_WAITING_GRBL_HEADER;


                    btnConnect.Text = "Disconnect";
                    comboBox1.Enabled = false;
                    textInput.Enabled = true;
                }
                catch
                {
                    currentPortState = PortStates.PORT_CLOSING;
                }
            }


            // state    -   PORT_WAITING_GRBL_HEADER
            // type     -   transitory to PORT_RUNNING
            // performs -   opening COM connection and start of acquisition timer
            // error    -   return to PORT_CLOSED state
            if (currentPortState == PortStates.PORT_WAITING_GRBL_HEADER)
            {
                //currentPortState = PortStates.PORT_RUNNING_IDLE;
            }



            // state    -   PORT_RUNNING_IDLE
            // type     -   permanent state
            // performs -   updates different states in the system
            // error    -   none
            if (currentPortState == PortStates.PORT_RUNNING_IDLE)
            {
                if (GcodeHandler.GcodeLoaded)
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = true;
                }
            }

            // state    -   PORT_RUNNING_TRANSMIT
            // type     -   permanent state
            // performs -   updates different states in the system
            // error    -   none
            // hadled   -   handled in timerComHandler, not here


            // state    -   PORT_CLOSING
            // type     -   transitory state to PORT_CLOSED
            // performs -   closes the port and stops the acquisition timer
            if (currentPortState == PortStates.PORT_CLOSING)
            {
                serialPort.DiscardInBuffer();
                try
                {
                    serialPort.Close();
                }
                catch
                {
                    // do nothing, port goes into state PORT_CLOSED anyway
                    //Console.WriteLine("Error at closing port");
                }

                currentPortState = PortStates.PORT_CLOSED;
                btnConnect.Text = "Connect";

                textInput.Enabled = false;
                comboBox1.Enabled = true;

                // disable GCODE file processor buttons
                btnStart.Enabled = false;
                btnStop.Enabled = false;
            }

            // update graphics interface
            GraphicsHandler.SetDrawingColor1();
            GcodeHandler.RedrawFullPicture();

            toolStripStatusLabel1.Text = currentPortState.ToString();
        }

        private void timerComHandler_Tick(object sender, EventArgs e)
        {
            // keep track of incoming bytes
            int result = 0;
            int totalBytes = 0;

            string response;
            int confirmationsReceived = 1;

            // string to be sent via Serial
            string textOut = "";

            // state    -   PORT_RUNNING
            // type     -   permanent state
            // performs -   continuosly read COM port data and request redrawing of newly incoming data
            if( (currentPortState == PortStates.PORT_WAITING_GRBL_HEADER)  ||
                (currentPortState == PortStates.PORT_RUNNING_IDLE)  ||
                (currentPortState == PortStates.PORT_RUNNING_TRANSMIT))
            {
                byte[] localData = new byte[1024];
                totalBytes = serialPort.BytesToRead;
                int readBytes = totalBytes;

                if (serialPort.ReadBufferSize > 0)
                {
                    response = serialPort.ReadExisting();

                    // filter out empty responses
                    if (response != "")
                    {
                        ComHandler.ConsoleWrite(response);

                        // if "Grbl" substring has arrived, then we definitely are connected to a GRBL machine
                        if (response.Contains("Grbl"))
                        {
                            currentPortState = PortStates.PORT_RUNNING_IDLE;
                        }
                        else
                        {   // process normal incoming data
                            confirmationsReceived = ComHandler.ProcessIncomingData(response);
                        }

                        if (confirmationsReceived > 0)
                        {
                            currentTxState = TransmitStates.TX_IDLE;
                        }

                        //Console.Write(confirmationsReceived);
                        glControl1.Invalidate();
                    }
                }

                // hadle transmission of GCODE file
                if (currentPortState == PortStates.PORT_RUNNING_TRANSMIT)
                {
                    if ((!GcodeHandler.GcodeFileDataFinished) && (currentTxState == TransmitStates.TX_IDLE))
                    {
                        textOut = GcodeHandler.GetNextGcodeBlock();
                        ComHandler.ConsoleWrite(textOut);

                        serialPort.Write(textOut + "\r");
                        currentTxState = TransmitStates.TX_PENDING;
                        toolStripProgressBar1.Value = GcodeHandler.GcodeFilePercent;
                    }
                    else if(GcodeHandler.GcodeFileDataFinished)
                    {
                        currentPortState = PortStates.PORT_RUNNING_IDLE;
                        toolStripProgressBar1.Value = 100;
                    }

                    
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                // display loaded file path
                string gcodeFilePath = openFileDialog1.FileName.ToString();
                groupBox1.Text = "GCODE File [ " + gcodeFilePath + " ]";

                // load file content into the global variable
                GcodeHandler.LoadGcodeFile(gcodeFilePath);
            };
        }

        private void textInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                textOutput.Text += textInput.Text + (char)Keys.Return + (char)Keys.LineFeed;

                // line ending information found at : https://github.com/grbl/grbl/wiki/Using-Grbl
                serialPort.Write(textInput.Text + (char)Keys.Return);
                e.Handled = true;
                textInput.Clear();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (currentPortState == PortStates.PORT_RUNNING_IDLE)
            {
                currentPortState = PortStates.PORT_RUNNING_TRANSMIT;
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (currentPortState == PortStates.PORT_RUNNING_TRANSMIT)
            {
                currentPortState = PortStates.PORT_RUNNING_IDLE;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void glControl1_ContextCreated(object sender, OpenGL.GlControlEventArgs e)
        {
            GraphicsHandler.GraphicContextCreated();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        // method called when Render is requested via Invalidate
        private void glControl1_Render(object sender, OpenGL.GlControlEventArgs e)
        {
            Control senderControl = (Control)sender;
            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GraphicsHandler.SetDrawingColor1();
            GcodeHandler.RedrawFullPicture();
            GraphicsHandler.SetDrawingColor2();
            GcodeHandler.RedrawCompletedPicture();
        }
    }
}
