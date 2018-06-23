using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anyBaseControl
{
    class GcodeHandler
    {



        // global data for the loaded GCODE
        private static string[] gcodeFileData;

        private static bool gcodeLoaded = false;
        private static Int32 gcodeParseIndex = 0;
        private static bool gcodeFileDataFinished = true;

        private static int gcodeFilePercent = 0;


        public static bool GcodeLoaded { get => gcodeLoaded; set => gcodeLoaded = value; }
        public static bool GcodeFileDataFinished { get => gcodeFileDataFinished; }
        public static int GcodeFilePercent { get => gcodeFilePercent; }

        public static void LoadGcodeFile(string fileName)
        {
            gcodeFileData = File.ReadAllLines(fileName);
            gcodeLoaded = true;
            gcodeParseIndex = 0;
            gcodeFileDataFinished = false;
        }


        public static string GetNextGcodeBlock()
        {
            string ret = "";
            if (gcodeParseIndex < gcodeFileData.Length - 1)
            {
                ret = gcodeFileData[gcodeParseIndex];
            }

            if (gcodeParseIndex < gcodeFileData.Length - 1)
            {
                gcodeParseIndex++;
            }
            else
            {
                gcodeFileDataFinished = true;
            };

            gcodeFilePercent = 100 * gcodeParseIndex / gcodeFileData.Length;

            return ret;
        }
    }
}
