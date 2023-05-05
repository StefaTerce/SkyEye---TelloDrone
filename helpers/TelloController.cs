using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Drone
{
    public class TelloController : IDisposable
    {
        private const string TELLO_IP_ADDRESS = "192.168.10.1";
        private const int TELLO_PORT = 8889;

        private UdpClient client;

        public TelloController()
        {
            client = new UdpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse(TELLO_IP_ADDRESS), TELLO_PORT));
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private void SendCommand(string command)
        {
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            client.Send(commandBytes, commandBytes.Length);
        }

        public void Flip(TelloFlipDirection direction)
        {
            string command = $"flip {direction.ToString().ToLower()}";
            SendCommand(command);
        }
    }

    public enum TelloFlipDirection
    {
        Left,
        Right,
        Forward,
        Back,
        Backward
    }
}
