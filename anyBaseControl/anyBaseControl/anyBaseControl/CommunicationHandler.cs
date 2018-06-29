using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace anyBaseControl
{
    class CommunicationHandler
    {
        private TextBox Console;
        private string IncomingData = "";

        public CommunicationHandler(TextBox textBox)
        {
            Console = textBox;
        }

        public void ConsoleWrite(string text)
        {
            Console.Text += text + Environment.NewLine;

            Console.SelectionStart = Console.Text.Length;
            Console.SelectionLength = 0;
            Console.ScrollToCaret();
        }

        public int ProcessIncomingData(string text)
        {
            int ret = 0;

            IncomingData += text;

            // clean string and replace every pair of "\r\n" with a simple "\r\n"
            string cleanData = text.Replace("\r\n", "\r");

            // split sequence after the "\r" symbol
            string[] sequence = text.Split('\r');

            foreach (string item in sequence)
            {
                //byte[] asciiBytes = Encoding.ASCII.GetBytes(item);
                //string ss = "";
                //foreach (byte b in asciiBytes)
                //{
                //    ss = ss + "." + b.ToString();
                //}
                if(item.Equals("ok"))
                {
                    ret++;
                }
            }

            return ret;
        }

        public bool ConfirmationReceived()
        {
            return true;
        }
    }
}
