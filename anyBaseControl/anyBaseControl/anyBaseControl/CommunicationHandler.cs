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
            Console.AppendText(text);

            //Console.SelectionStart = Console.Text.Length;
            //Console.SelectionLength = 0;
            //Console.ScrollToCaret();
            if(Console.Lines.Length>100)
            {
                Console.Lines = Console.Lines.Skip(Console.Lines.Length - 100).ToArray();
            }
        }

        public int ProcessIncomingData(string text)
        {
            int ret = 0;
            char[] splitChars = { '\r','\n' };
            IncomingData += text;

            // clean string and replace every pair of "\r\n" with a simple "\r\n"
            string cleanData = text.Replace("\r\n", "\r");

            // split sequence after the "\r" symbol
            string[] sequence = IncomingData.Split(splitChars,2,StringSplitOptions.RemoveEmptyEntries);

            if (sequence.Length > 1)
            {
                IncomingData = sequence[1];
                //this.ConsoleWrite("---"+sequence[0]);
                //this.ConsoleWrite("---"+sequence[1]);
            }
            else if (sequence.Length == 1)
            {
                //this.ConsoleWrite("+++" + sequence[0]);

                if (sequence[0].Equals("ok"))
                {
                    IncomingData = "";
                    ret++;
                }
            }

            //this.ConsoleWrite("+++" + ret);

            //this.ConsoleWrite(sequence.Length.ToString());

            System.Console.Write(sequence);

            //foreach (string item in sequence)
            //{
                //byte[] asciiBytes = Encoding.ASCII.GetBytes(item);
                //string ss = "";
                //foreach (byte b in asciiBytes)
                //{
                //    ss = ss + "." + b.ToString();
                //}
                
            //}

            return ret;
        }

        public bool ConfirmationReceived()
        {
            return true;
        }
    }
}
