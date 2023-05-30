using demoTello.helpers;
using Drone;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using tellocs;
using SharpDX.DirectInput;
using System.Drawing.Text;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading;
using SharpDX;
using static demoTello.Form2;

namespace demoTello
{


    public partial class Form2 : Form
    {
   

        bool upDownValue = false;
        string upDownDirection = "nulla";
        bool isStreamingJoystik = false;
        private static class TelloRC
        {


            public static TelloCmd Tello { get; set; }
            public static int Speed { get; set; } = 100;



            public static Dictionary<string, int> Channel = new Dictionary<string, int>()
            {
                { "left_right_velocity", 0 },
                { "forward_backward_velocity",0 },
                { "up_down_velocity",0 },
                { "yaw_velocity",0 }
            };

            private static Dictionary<string, int> oldChannel = new Dictionary<string, int>()
            {
                { "left_right_velocity", 0 },
                { "forward_backward_velocity",0 },
                { "up_down_velocity",0 },
                { "yaw_velocity",0 }
            };

            /// <summary>
            /// Send RC control via four channels
            /// </summary>
            /// <param name="left_right_velocity">-100~100 (left/right)</param>
            /// <param name="forward_backward_velocity">-100~100 (backward/forward)</param>
            /// <param name="up_down_velocity">-100~100 (down/up)</param>
            /// <param name="yaw_velocity">-100~100 (yaw)</param>
            /// <param name="context">optional command context for logging/debugging</param>
            public static bool SendRCControl((string Key, int Value)[] setChannels = null)
            {
                if (!Tello.Connected)
                    return false;

                if (setChannels != null)
                    foreach (var channel in setChannels)
                    {
                        if (Channel.ContainsKey(channel.Key))
                            Channel[channel.Key] = channel.Value;
                    }

                if (Channel.Count == oldChannel.Count && !Channel.Except(oldChannel).Any())
                    return false;

                oldChannel = new Dictionary<string, int>(Channel);

                return Tello.SendRCControl(Channel["left_right_velocity"], Channel["forward_backward_velocity"], Channel["up_down_velocity"], Channel["yaw_velocity"], context: null);
            }
            // Funzione per eseguire il flip in avanti
            public static void FlipForward()
            {
                Tello.SendCommand("flip f");
            }

            // Funzione per eseguire il flip all'indietro
            public static void FlipBackward()
            {
                Tello.SendCommand("flip b");
            }

            // Funzione per eseguire il flip a destra
            public static void FlipRight()
            {
                Tello.SendCommand("flip r");
            }

            // Funzione per eseguire il flip a sinistra
            public static void FlipLeft()
            {
                Tello.SendCommand("flip l");
            }





        }
        

        private struct Istruzione
        {
            public double distance;
            public double degrees;
        }

        private List<Point> points = new List<Point>();
        private bool ostacoliMode = false;

        private Istruzione[] istruzioni = new Istruzione[100];
        private Istruzione[] istruzioniUfficiali = new Istruzione[100];

        int num = 0;

        [Serializable]
        class SerializableState
        {
            public List<Point> Points { get; }
            public List<Point> Obstacles { get; }
            public Image Image { get; }
        }

        public Form2(TelloCmd tello)
        {
            TelloRC.Tello = tello;
            InitializeComponent();
        }

        private List<Point> obstacles = new List<Point>();


        private void Form2_Load(object sender, System.EventArgs e)
        {
            //while (true)
            //{
            //    if (upDownValue == true)
            //    {
            //        switch (upDownDirection)
            //        {
            //            case "su":
            //                //su
            //                TelloRC.Channel["left_right_velocity"] = 0;
            //                TelloRC.Channel["forward_backward_velocity"] = 0;
            //                TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
            //                TelloRC.Channel["yaw_velocity"] = 0;
            //                TelloRC.SendRCControl();
            //                break;
            //            case "giu":
            //                //nulla
            //                TelloRC.Channel["left_right_velocity"] = 0;
            //                TelloRC.Channel["forward_backward_velocity"] = 0;
            //                TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
            //                TelloRC.Channel["yaw_velocity"] = 0;
            //                TelloRC.SendRCControl();
            //                break;
            //            default:
            //                //giu
            //                TelloRC.Channel["left_right_velocity"] = 0;
            //                TelloRC.Channel["forward_backward_velocity"] = 0;
            //                TelloRC.Channel["up_down_velocity"] = 0;
            //                TelloRC.Channel["yaw_velocity"] = 0;
            //                TelloRC.SendRCControl();
            //                Task.Delay(1000);
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        //nulla
            //        TelloRC.Channel["left_right_velocity"] = 0;
            //        TelloRC.Channel["forward_backward_velocity"] = 0;
            //        TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
            //        TelloRC.Channel["yaw_velocity"] = 0;
            //        TelloRC.SendRCControl();
            //    }
            //    Task.Delay(1000);
            //}
        }

        //    Azzera la velocita in ogni direzione
        //    TelloRC.Channel["left_right_velocity"] = 0;
        //    TelloRC.Channel["forward_backward_velocity"] = 0;
        //    TelloRC.Channel["up_down_velocity"] = 0;
        //    TelloRC.Channel["yaw_velocity"] = 0;
        //    TelloRC.SendRCControl();




        // F I X E D
        private void btn_sopra_MouseDown(object sender, MouseEventArgs e)
        {
            //TelloRC.Up(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Clockwise(-45);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Clockwise(-45);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sopra_MouseUp(object sender, MouseEventArgs e)
        {
            // reset;
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_destra_MouseDown(object sender, MouseEventArgs e)
        {
            //_tello.Right(20);
            TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_destra_MouseUp(object sender, MouseEventArgs e)
        {
            //_tello.Right(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_avanti_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_avanti_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sinistra_MouseDown(object sender, MouseEventArgs e)
        {
            //_tello.Left(20);
            TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sinistra_MouseUp(object sender, MouseEventArgs e)
        {
            //_tello.Left(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_indietro_MouseDown(object sender, MouseEventArgs e)
        {
            //_tello.Back(-20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_indietro_MouseUp(object sender, MouseEventArgs e)
        {
            //_tello.Back(-20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_orario_MouseDown(object sender, MouseEventArgs e)
        {
            //TelloRC.Clockwise(45);

            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
            TelloRC.SendRCControl();
        }

        private void btn_orario_MouseUp(object sender, MouseEventArgs e)
        {
            //TelloRC.Clockwise(45);

            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_antiorario_MouseDown(object sender, MouseEventArgs e)
        {
            //_tello.Clockwise(-45);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
            TelloRC.SendRCControl();
        }

        private void btn_antiorario_MouseUp(object sender, MouseEventArgs e)
        {
            //_tello.Clockwise(-45);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }


        private void btn_avanti_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.E:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Q:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.X:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Z:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
            }
        }

        private void btn_sinistra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.E:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Q:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.X:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Z:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
            }
        }

        private void btn_indietro_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.E:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Q:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.X:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Z:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
            }
        }

        private void btn_destra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.E:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Q:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.X:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Z:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
            }
        }

        private void btn_sopra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.E:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Q:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.X:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Z:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
            }
        }

        private void btn_sotto_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.E:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Q:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
                    TelloRC.SendRCControl();
                    break;
                case Keys.X:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.Z:
                    // _tello.Forward(20);
                    TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
            }
        }



        private void PadBox_CheckedChanged_1(object sender, EventArgs e)
        {

            if (PadBox.Checked == true)
            {
                // Initialize DirectInput
                var directInput = new DirectInput();

                // Find a Joystick Guid
                var joystickGuid = Guid.Empty;

                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                            DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

                // If Gamepad not found, look for a Joystick
                if (joystickGuid == Guid.Empty)
                    foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                            DeviceEnumerationFlags.AllDevices))
                        joystickGuid = deviceInstance.InstanceGuid;

                // If Joystick not found, throws an error
                if (joystickGuid == Guid.Empty)
                {
                    MessageBox.Show("Nessun joystick Trovato");
                    return;
                }

                // Instantiate the joystick
                var joystick = new Joystick(directInput, joystickGuid);

                MessageBox.Show("Gamepad Trovato" + joystickGuid);

                // Query all suported ForceFeedback effects
                var allEffects = joystick.GetEffects();
                foreach (var effectInfo in allEffects)
                    MessageBox.Show("Effect available {0}" + effectInfo.Name);

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                // Acquire the joystick
                joystick.Acquire();

                // Poll events from joystick
                while (true)
                {
                    if (PadBox.Checked == false)
                    {
                        btn_sotto.PerformClick();
                        return;
                    }
                    joystick.Poll();
                    var datas = joystick.GetBufferedData();
                    foreach (var state in datas)
                    {
                        if (PadBox.Checked == false)
                        {
                            return;
                        }
                        chiamaComandi(state.Offset.ToString(), state.Value);

                        Task.Delay(100);
                    }
                    Task.Delay(100);
                }
            }
            else if (PadBox.Checked == false)
            {
                btn_sotto.PerformClick();
                return;
            }
        }


        //private async void scrivi(string mex)
        //{
        //    await Task.Run(() => label1.Invoke(new Action(()=> { Console.WriteLine(mex);  })));// { label1.Text = mex; label1.Refresh(); })));

        //}
        void chiamaComandi(string offset, int value)

        {
            //Console.WriteLine($"{offset}-{value}");
            //this.Text = $"{offset}-{value}";

            //if (TelloRC.Tello.mock)  scrivi($"{offset}-{value}"); else return;
            switch (offset)
            {
                case "Y":
                    if (value > 32767)
                    {
                        //Indietro
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = -(value / 2000);
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else if (value == 32767)
                    {
                        //Nulla
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else
                    {
                        value = 65535 - value;
                        //Avanti
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = value / 2000;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case "X":
                    if (value > 32767)
                    {

                        //Destra
                        TelloRC.Channel["left_right_velocity"] = value / 2000;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else if (value == 32767)
                    {
                        //Nulla
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else
                    {
                        value = 65535 - value;
                        //Sinistra
                        TelloRC.Channel["left_right_velocity"] = -(value / 2000);
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case "Buttons3":
                    if (value == 128)
                    {
                        if (isStreamingJoystik == false)
                        {
                            Form1.funzioni.changeStreamingCheckBox(true);
                            isStreamingJoystik = true;
                        }
                        else
                        {
                            Form1.funzioni.changeStreamingCheckBox(false);
                            isStreamingJoystik = false;
                        }
                    }
                    break;
                case "Buttons2":
                    if (value == 128)
                    {
                        //Su
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 33;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else
                    {
                        //Nulla

                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case "Buttons1":
                    if (value == 128)
                    {
                        //Giù
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = -33;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else
                    {
                        //Nulla
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case "PointOfViewControllers0":
                    if (value == 0)
                    {
                        //Su
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 33;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else if (value == -1)
                    {
                        //Nulla
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    else if (value == 18000)
                        //Giù
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = -33;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case "Buttons4":
                    if (value == 128)
                    {
                        //Connect
                        Form1.funzioni.connectClick();
                    }
                    break;
                case "Buttons5":
                    if (value == 128)
                    {
                        //TakeOff
                        Form1.funzioni.takeOff();
                    }
                    break;
                case "Buttons6":
                    if (value == 128)
                    {
                        //Land
                        Form1.funzioni.land();
                    }
                    break;
                case "Buttons0":
                    if (value == 128)
                    {
                        if (PadBox.Checked == false)
                        {
                            PadBox.Checked = true;
                        }
                        else
                        {
                            PadBox.Checked = false;
                        }
                    }
                    break;
                case "Z":
                    if (value > 42767)
                    {
                        //Ruota a sinistra
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = -33;
                        TelloRC.SendRCControl();
                    }

                    if (value < 22767)
                    {
                        //Ruota a destra
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 33;
                        TelloRC.SendRCControl();
                    }
                    else
                    {
                        //Ruota a destra
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
            }
        }

        private void btn_avanti_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sinistra_Layout(object sender, LayoutEventArgs e)
        {

        }

        private void btn_sinistra_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_indietro_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_destra_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sopra_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_Click(object sender, EventArgs e)
        {

        }


        public static class form2Funzioni
        {
            public async static void audioMessage(string path)
            {
                using (var player = new System.Media.SoundPlayer(path))
                {
                    await Task.Run(() => { player.Load(); player.PlaySync(); });
                }
            }
        }


        public static void changeLanguage(string language)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFormOnce();
            void OpenFormOnce()
            {

                if (Application.OpenForms.OfType<Form>().Any(f => f.Name == "Form6"))
                {

                }
                else
                {

                    Form6 form = new Form6(TelloRC.Tello);
                    form.Show();
                }
            }

        }
    }  
}

