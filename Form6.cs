using demoTello.helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tellocs;
using Drone;

namespace demoTello
{
    public partial class Form6 : Form
    {

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



        public Form6(TelloCmd tello)
        {
            TelloRC.Tello = tello;
            InitializeComponent();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            TelloRC.Tello.FlipForward();    
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            TelloRC.Tello.FlipLeft();
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            TelloRC.Tello.FlipRight();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            TelloRC.Tello.FlipBack();
        }
    }
}
