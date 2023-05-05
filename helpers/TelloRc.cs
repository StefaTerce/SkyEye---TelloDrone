using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tellocs;

namespace demoTello.helpers
{
    internal class TelloRc
    {
        private static class TelloRC
        {
            public static TelloCmd Tello { get; set; }
            public static int Speed { get; set; } = 33;

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

            /// <summary>
            /// Flip the drone forward.
            /// </summary>
            public static bool FlipForward()
            {
                return Tello.SendCommand("flip f");
            }

            /// <summary>
            /// Flip the drone backward.
            /// </summary>
            public static bool FlipBackward()
            {
                return Tello.SendCommand("flip b");
            }

            /// <summary>
            /// Flip the drone left.
            /// </summary>
            public static bool FlipLeft()
            {
                return Tello.SendCommand("flip l");
            }

            /// <summary>
            /// Flip the drone right.
            /// </summary>
            public static bool FlipRight()
            {
                return Tello.SendCommand("flip r");
            }
        }

    }
}
