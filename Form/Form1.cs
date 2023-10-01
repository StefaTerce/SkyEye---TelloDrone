using MjpegProcessor;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using tellocs;
using Application = System.Windows.Forms.Application;
using System.Reflection;
using System.Linq;
using System.Threading;
using Drone;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Diagnostics;
using SharpDX.DirectInput;
using System.Collections.Generic;
using static demoTello.Form5;
using demoTello.helpers;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using SharpDX;
using static demoTello.Form1;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace demoTello
{

    public partial class Form1 : Form
    {
        

        private int step = 0;
        private Form1 _cmdForm;

        private TelloCmd _tello;

        private int[] _rcChannels = { 0, 0, 0, 0 };     // 4 channels
        private bool _updateStatus;

        private bool _videoStreamingWhenConnected;
        private int myHandler;
        string result2 = "";


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



        string error = "";

        public Form1()
        {
            InitializeComponent();

            tab_misc.Multiline = true;
            tab_misc.Appearance = TabAppearance.Buttons;
            tab_misc.ItemSize = new System.Drawing.Size(0, 1);
            tab_misc.SizeMode = TabSizeMode.Fixed;
            tab_misc.TabStop = false;

            funzioni.checkbox = checkBox1;

            funzioni.buttonStart = btnStart;

            funzioni.buttonTakeOff = btn_takeoff;

            funzioni.buttonLand = btn_land;

            _tello = new TelloCmd();
            _tello.CommandCallback += (cmd) =>
            {
                System.Windows.Forms.ListBox listBox = commands;
                listBox.Items.Add($"{listBox.Items.Count} {cmd}");
                listBox.SelectedIndex = listBox.Items.Count - 1;
            };
            _tello.CommandResultCallback += (cmd, result) =>
            {
                if (!string.IsNullOrEmpty(result)) result = result.Trim();
                int index = commands.Items.Count - 1;
                string lastCmd = commands.Items[index].ToString();
                if (lastCmd.StartsWith($"{index} {cmd}"))
                {
                    commands.Items[index] = $"{lastCmd} => {result}";
                    result2 = result;
                }
                else
                {
                    _tello.CommandCallback($"{cmd} => {result}");
                    result2 = result;
                }
                error = result2;
            };

            _tello.DefaultPhotoFolder = string.Empty;
            _tello.DefaultVideoFolder = string.Empty;
            _tello.MJPEGserverPath = "MJPEGServer.exe";
            _tello.DebugMode = false;
            _tello.UseTelloSpeedForRCControl = true;
            // whether to start video streaming/recording after connect
            _videoStreamingWhenConnected = false;

            _updateStatus = true;
            UpdateStatusAsync();
            
            TelloRC.Tello = _tello;  
        }

        private void btnStart_Click_1(object sender, EventArgs e)
        {
            if (_tello.Connected)
            {
                _tello.Disconnect();
                btnStart.Text = "CONNECT";
            }
            else
            {
                _tello.Connect();
                checkBox1.Checked = _videoStreamingWhenConnected;
                btnStart.TextAlign = ContentAlignment.MiddleRight;
                btnStart.Text = "DISCONNECT";             
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //form2Funzioni.audioMessage("takeoff.wav");
            btn_takeoff.Visible = false;
            btn_land.Visible = true;

            if (_tello.Connected)
            {
                bool ok = _tello.Takeoff();
                btn_takeoff.Enabled = false;
                btn_land.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            btn_land.Visible = false;
            btn_takeoff.Visible = true;

            _tello.Land();
            btn_takeoff.Enabled = true;
            btn_land.Enabled = false;
        }

        private async void UpdateStatusAsync()
        {
            while (_updateStatus)
            {
                try
                {
                    DisplayStatus();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
                await Task.Run(async () =>
                {
                    //int delay = (int)(DeviceSettings.Instance.MonitorRefreshRate * 1000);
                    await Task.Delay(500);   // in milliseconds
                }).ConfigureAwait(true);
            }
        }

        private void DisplayStatus()
        {
            if (_tello.Connected)
            {
                btnStart.BackColor = Color.Green;

                if (_tello.Flying) statusLabel.Text = "Flying";
                else if (_tello.VideoStreaming) statusLabel.Text = "Connected";
                /*
                heightLabel.Content = _tello.DroneState<string>("h");
                batteryLabel.Content = _tello.DroneState<string>("bat");
                flightTimeLabel.Content = _tello.DroneState<string>("time");
                tofLabel.Content = _tello.DroneState<string>("tof");
                temperatureLabel.Content = (_tello.DroneState<int>("templ") + _tello.DroneState<int>("temph")) / 2;
                */
            }
            else
            {
                btnStart.BackColor = Color.LightGray;
                statusLabel.Text = "Not Connected";

            }
        }

        public async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (_tello.Connected == false)
            {
                MessageBox.Show("Connect the drone first!", "Window title", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                if (checkBox1.Checked == true)
                {
                    Process[] processes = Process.GetProcessesByName("Python"); // get all processes with name "Streaming"
                    foreach (Process localProcess in processes)
                    {
                        localProcess.Kill();
                    }

                    _tello.StartOrStopVideoStreaming();

                    int port = 12345;  // Use the same port as in the Python script
                    IPAddress localAddr = IPAddress.Parse("127.0.0.1");  // Use the same IP address as in the Python script

                    TcpListener listener = new TcpListener(localAddr, port);
                    listener.Start();

                    string currentDirectory = Environment.CurrentDirectory;
                    currentDirectory = currentDirectory.Substring(0, currentDirectory.Length - 9);
                    currentDirectory = Path.Combine(currentDirectory + "Streaming\\Model.py");
                    Console.WriteLine("Current Directory: " + currentDirectory);

                    // Create a ProcessStartInfo object to configure the CMD process
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    // Create a new process
                    Process process = new Process { StartInfo = psi };

                    // Start the CMD process
                    process.Start();

                    // Send the Python script command to the CMD process
                    process.StandardInput.WriteLine($"python {currentDirectory}");

                    // Close the input stream to signal the end of the command
                    process.StandardInput.Close();

                    //// Read the output and error streams (optional)
                    //string output = process.StandardOutput.ReadToEnd();
                    //string error = process.StandardError.ReadToEnd();

                    //// Print the output and error (optional)
                    //Console.WriteLine("Output:");
                    //Console.WriteLine(output);

                    //Console.WriteLine("Error:");
                    //Console.WriteLine(error);

                    Console.WriteLine("Waiting for Python to send data...");
                    try
                    {
                        while (true)
                        {
                            using (TcpClient client = listener.AcceptTcpClient())
                            using (NetworkStream stream = client.GetStream())
                            {
                                StringBuilder messageBuilder = new StringBuilder();
                                byte[] buffer = new byte[1024]; 

                                int bytesRead;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                    messageBuilder.Append(dataReceived);

                                    // Check if a complete message (terminated with '\n') has been received
                                    string[] messages = messageBuilder.ToString().Split('\n');
                                    foreach (string message in messages)
                                    {
                                        if (!string.IsNullOrEmpty(message))
                                        {
                                            Console.WriteLine($"Received: {message}");
                                            //if(message == "green_circle")
                                            //    _tello.Takeoff();

                                            //if (message == "red_circle")
                                            //    _tello.Land();
                                        }
                                        await Task.Delay(500);
                                    }
                                    messageBuilder.Clear();
                                }
                            }
                            await Task.Delay(500);
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message);
                    }
                }
                if (checkBox1.Checked == false)
                {
                    Process[] processes = Process.GetProcessesByName("Python"); // get all processes with name "Python"
                    foreach (Process localProcess in processes)
                    {
                        localProcess.Kill();
                    }
                }
            }
        }


        private Point originalLocation;

        private void Form1_Load(object sender, EventArgs e)
        {
            originalLocation = this.Location;
            this.Anchor = AnchorStyles.Top;
        }



        private void commandi_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            OpenFormOnce();
            void OpenFormOnce()
            {

                if (Application.OpenForms.OfType<Form>().Any(f => f.Name == "Form4"))
                {

                }
                else
                {

                    Form4 form4 = new Form4();
                    form4.Show();
                }
            }
        }


        public static void changeLanguage(string language)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
        }


        private bool isCapturing = false; // flag per indicare se si sta acquisendo



        public class funzioni
        {
            public static System.Windows.Forms.CheckBox padBox; 

            public static System.Windows.Forms.CheckBox checkbox;

            public static System.Windows.Forms.Button buttonStart;

            public static System.Windows.Forms.Button buttonTakeOff;

            public static System.Windows.Forms.Button buttonLand;
            public static void changeStreamingCheckBox(bool value)
            {
                checkbox.Checked = value;
            }

            public static void connectClick()
            {
                buttonStart.PerformClick();
            }

            public static void takeOff()
            {
                buttonTakeOff.PerformClick();
            }

            public static void land()
            {
                buttonLand.PerformClick();
            }

        }



        //------------------------------------
        //TUTTI I COMANDI DA TASTIERA DEL DRONE
        //------------------------------------

        private bool fPressed = false;    
        private void btn_commands_Click(object sender, EventArgs e)
        {
            tab_misc.SelectedIndex = 0;
        }

        private void btn_output_Click(object sender, EventArgs e)
        {
            tab_misc.SelectedIndex = 1;
        }

        private void btn_map_Click(object sender, EventArgs e)
        {
            OpenFormOnce();
            void OpenFormOnce()
            {

                if (Application.OpenForms.OfType<Form>().Any(f => f.Name == "Form5"))
                {

                }
                else
                {
                    Form5 form5 = new Form5(_tello, result2);
                    form5.Show();
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_takeoff_Click(object sender, EventArgs e)
        {
            _tello.Takeoff();
        }

        private void btn_land_Click(object sender, EventArgs e)
        {
            _tello.Land();
        }


        //---------------------------------
        //PADBOXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //---------------------------------
        private async void PadBox_CheckedChanged(object sender, EventArgs e)
        {
            while (true)
            {
                await Task.Delay(100);
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
                            //await Task.Delay(100);
                        }
                    }
                }
                else if (PadBox.Checked == false)
                {
                    btn_sotto.PerformClick();
                    return;
                }
                
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
                        TelloRC.Channel["forward_backward_velocity"] = -(value / 1000);
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
                        TelloRC.Channel["forward_backward_velocity"] = value / 1000;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case "X":
                    if (value > 32767)
                    {
                        //Destra
                        TelloRC.Channel["left_right_velocity"] = value / 1000;
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
                        TelloRC.Channel["left_right_velocity"] = -(value / 1000);
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
                            funzioni.changeStreamingCheckBox(true);
                            isStreamingJoystik = true;
                        }
                        else
                        {
                            funzioni.changeStreamingCheckBox(false);
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
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = TelloRC.Speed;
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
                        //Rotate Left
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = -TelloRC.Speed;
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
                        funzioni.connectClick();
                    }
                    break;
                case "Buttons5":
                    if (value == 128)
                    {
                        //TakeOff
                        funzioni.takeOff();
                    }
                    break;
                case "Buttons6":
                    if (value == 128)
                    {
                        //Land
                        funzioni.land();
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


        //TATSIERA DRONEEEEEEEEEEEEEE
        private void btn_sopra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_sopra_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_sotto_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_avanti_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_indietro_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                    TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                    TelloRC.Channel["forward_backward_velocity"] = 0;
                    TelloRC.Channel["up_down_velocity"] = 0;
                    TelloRC.Channel["yaw_velocity"] = 0;
                    TelloRC.SendRCControl();
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_indietro_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sinistra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_sinistra_KeyUp(object sender, KeyEventArgs e)
        {
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_destra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_destra_KeyUp(object sender, KeyEventArgs e)
        {
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
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }


        private void tab_misc_KeyUp(object sender, KeyEventArgs e)
        {
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

        private void btn_sinistra_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_indietro_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_destra_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sopra_MouseDown(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = TelloRC.Speed;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sopra_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_indietro_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_sotto_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_destra_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
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

        private void btn_sinistra_MouseUp(object sender, MouseEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_orario_KeyUp(object sender, KeyEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_antiorario_KeyUp(object sender, KeyEventArgs e)
        {
            // _tello.Forward(20);
            TelloRC.Channel["left_right_velocity"] = 0;
            TelloRC.Channel["forward_backward_velocity"] = 0;
            TelloRC.Channel["up_down_velocity"] = 0;
            TelloRC.Channel["yaw_velocity"] = 0;
            TelloRC.SendRCControl();
        }

        private void btn_orario_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_antiorario_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (fPressed == true)
                    {
                        _tello.FlipForward();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.S:
                    if (fPressed == true)
                    {
                        _tello.FlipBack();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = 0;
                        TelloRC.Channel["forward_backward_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.A:
                    if (fPressed == true)
                    {
                        _tello.FlipLeft();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = -TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
                    break;
                case Keys.D:
                    if (fPressed == true)
                    {
                        _tello.FlipRight();
                    }
                    else
                    {
                        // _tello.Forward(20);
                        TelloRC.Channel["left_right_velocity"] = TelloRC.Speed;
                        TelloRC.Channel["forward_backward_velocity"] = 0;
                        TelloRC.Channel["up_down_velocity"] = 0;
                        TelloRC.Channel["yaw_velocity"] = 0;
                        TelloRC.SendRCControl();
                    }
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
                case Keys.T:
                    _tello.Takeoff();
                    break;
                case Keys.L:
                    _tello.Land();
                    break;
                case Keys.F:
                    fPressed = true;
                    break;
            }
        }

        private void btn_flip_Click(object sender, EventArgs e)
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

        private void btnVettore_Click(object sender, EventArgs e)
        {

        }
        //private void btn_startAutoPath_Click(object sender, EventArgs e)
        //{
        //    TelloRC.Tello.Takeoff();
        //    int x = 0;
        //    while (x <= num2 + 10)
        //    {
        //        int degree = Convert.ToInt32(istruzioniUfficiali[x].degrees);
        //        if (degree < 0)
        //        {
        //            degree = 360 + degree;
        //        }
        //        TelloRC.Tello.Clockwise(degree);
        //        Console.WriteLine(error);
        //        int distance = Convert.ToInt32(istruzioniUfficiali[x].distance);
        //        TelloRC.Tello.Fly("forward", distance * 100);
        //        Console.WriteLine(error);
        //        x++;
        //    }
        //    TelloRC.Tello.Land();
        //}

        //private void btn_clear_Click(object sender, EventArgs e)
        //{
        //    if (num > 0)
        //    {
        //        int x = 0;
        //        while (x < num)
        //        {
        //            istruzioni[x].distance = 0;
        //            istruzioni[x].degrees = 0;
        //            x++;
        //        }
        //        num = 0;
        //    }
        //    points.Clear();
        //    obstacles.Clear();
        //    pictureBox1.Invalidate();
        //}
    }
}
    


        