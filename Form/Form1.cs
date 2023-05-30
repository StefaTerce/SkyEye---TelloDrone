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

namespace demoTello
{

    public partial class Form1 : Form
    {

        private Form2 _cmdForm;

        private TelloCmd _tello;

        private int[] _rcChannels = { 0, 0, 0, 0 };     // 4 channels 
        private bool _updateStatus;

        private bool _videoStreamingWhenConnected;
        private int myHandler;
        string result2 = "";

        public Form1()
        {
            InitializeComponent();

            funzioni.checkbox = checkBox1;

            funzioni.buttonStart = btnStart;

            funzioni.buttonTakeOff = button1;

            funzioni.buttonLand = button2;

            _tello = new TelloCmd(); 
            _tello.CommandCallback += (cmd) => 
            {
                ListBox listBox = commandi;
                listBox.Items.Add($"{listBox.Items.Count} {cmd}");
                listBox.SelectedIndex = listBox.Items.Count - 1;
            };
            _tello.CommandResultCallback += (cmd, result) =>
            {
                if (!string.IsNullOrEmpty(result)) result = result.Trim();
                int index = commandi.Items.Count - 1;
                string lastCmd = commandi.Items[index].ToString();
                if (lastCmd.StartsWith($"{index} {cmd}"))
                {
                    commandi.Items[index] = $"{lastCmd} => {result}";
                    result2 = result;
                }
                else
                {
                    _tello.CommandCallback($"{cmd} => {result}");
                    result2 = result;
                }
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


        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            if (_tello.Connected)
            {
                _tello.Disconnect();
                btnStart.Text = "CONNECT";
                _cmdForm.Close();
                _cmdForm.Dispose();
                _cmdForm = null;
            }
            else
            {
                _tello.Connect();
                checkBox1.Checked = _videoStreamingWhenConnected;
                btnStart.Text = "DISCONNECT";
                _cmdForm = new Form2(_tello);

                _cmdForm.Show();
                _cmdForm.Focus();
                _cmdForm.Location = new System.Drawing.Point(448, 245);
            }
        }

        private void Takeoff_Click(object sender, EventArgs e)
        {
            //form2Funzioni.audioMessage("takeoff.wav");
            button1.Visible = false;
            button2.Visible = true;

            if (_tello.Connected)
            {
                bool ok = _tello.Takeoff();
                button1.Enabled = false;
                button2.Enabled = true;
            }
        }

        private void Land_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = true;

            _tello.Land();
            button1.Enabled = true;
            button2.Enabled = false;
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
                MessageBox.Show("Connect the drone first!",  "Error",  MessageBoxButtons.OK,  MessageBoxIcon.Asterisk);
            }
            if(_tello.Connected == true)
            {
                if (checkBox1.Checked == true)
                {
                    Process[] process = Process.GetProcessesByName("Streaming"); // get all processes with name "Streaming"
                    foreach (Process localProcess in process)
                    {
                        localProcess.Kill();
                    }

                    _tello.StartOrStopVideoStreaming(); 
                    string path = "";
                    string currentDirectory = Directory.GetCurrentDirectory(); 
                    path = currentDirectory.Substring(0, currentDirectory.Length - 9); 
                    Console.WriteLine(path);
                    path = Path.Combine(path + "Streaming\\bin\\Debug\\Streaming.exe");
                    Console.WriteLine(path);
                    Process myProcess = new Process();
                    myProcess.StartInfo.FileName = path;
                    myProcess.Start();

                    while (true)
                    {
                        await Task.Delay(500);
                        if (myProcess.HasExited)
                        {
                            Console.WriteLine("Il processo è stato chiuso.");
                            _tello.StartOrStopVideoStreaming(); 
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Il processo è ancora in esecuzione.");
                        }
                    }
                }
                if (checkBox1.Checked == false)
                {
                    Process[] process = Process.GetProcessesByName("Streaming"); // get all processes with name "Streaming"
                    foreach (Process localProcess in process)
                    {
                        localProcess.Kill();
                    }
                }
            }
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }



        private void commandi_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Settings_MouseClick(object sender, MouseEventArgs e)
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

        private void Map_Click(object sender, EventArgs e)
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
    }
}