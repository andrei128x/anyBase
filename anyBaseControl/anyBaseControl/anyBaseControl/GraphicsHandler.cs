using OpenGL;
using System.Text.RegularExpressions;

namespace anyBaseControl
{
    class GraphicsHandler
    {
        // 
        private static float oldX = 0;
        private static float oldY = 0;
        private static float currentX = 0;
        private static float currentY = 0;

        // method called when Gl_Control's context is created
        public static void GraphicContextCreated()
        {
            // Here you can allocate resources or initialize state
            Gl.MatrixMode(MatrixMode.Projection);
            Gl.LoadIdentity();
            Gl.Ortho(0.0, 1.0f, 0.0, 1.0, 0.0, 1.0);

            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();

            Gl.Enable(EnableCap.Blend);
            Gl.Enable(EnableCap.LineSmooth);
            Gl.Enable(EnableCap.PolygonSmooth);
            Gl.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            Gl.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

            Gl.LineWidth(.05f);
            Gl.PointSize(.07f);

            //Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            Gl.ClearColor(.17f, .13f, .25f, .5f);
        }

        // method for drawing a line segment from the DataBuffer
        public static void LineSegment(float x1, float y1, float x2, float y2)
        {
            Gl.Color3(.79f, .19f, .39f);
            Gl.Begin(PrimitiveType.LineStrip);
            Gl.Vertex2(x1,y1);
            Gl.Vertex2(x2,y2);
            Gl.End();

            //Gl.Color3(.3f, .7f, .7f);
            //Gl.Begin(PrimitiveType.Points);
            //Gl.Vertex2(x1, y1);
            //Gl.End();
        }

        public static void DrawGCodeSegment(string command)
        {

            string coords = "[xXyYzZ][0-9.]*";
            Match m = Regex.Match(command, coords);
            while (m.Success)
            {
                System.Console.WriteLine("'{0}' found at position {1}", m.Value, m.Index);
                string parseStr = m.Value.ToString();
                if (parseStr.Contains("X") || parseStr.Contains("x"))
                {
                    currentX = float.Parse(parseStr.Substring(1, parseStr.Length-1));
                }

                if (parseStr.Contains("Y") || parseStr.Contains("y"))
                {
                    currentY = float.Parse(parseStr.Substring(1, parseStr.Length-1));
                }

                m = m.NextMatch();
            }

            LineSegment(oldX/40, oldY/40, currentX/40, currentY/40);
            oldX = currentX;
            oldY = currentY;
        }
    }
}
