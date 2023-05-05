using MjpegProcessor;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using tellocs;
using OpenCvSharp;
using OpenCvSharp.Extensions;

using Application = System.Windows.Forms.Application;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace demoTello
{

    public partial class Form1 : Form
    {


        private Mat pizzeta;

        private int step = 0;
        private Form2 _cmdForm;

        private MjpegDecoder mjpeg;

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

            mjpeg = new MjpegDecoder();
            mjpeg.FrameReady += mjpeg_FrameReady;
            mjpeg.Error += mjpeg_Error;

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
            throw new NotImplementedException();
        }

        private void btnStart_click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(e.FrameBuffer))
            {
                bmp = new Bitmap(ms);
            }

            System.Drawing.Image newImg = (System.Drawing.Image)bmp.Clone();
            bmp.Dispose();

            Bitmap bmap = new Bitmap(newImg);
            image.Image = bmap;

            step++;

            if (step == 20)
            {
                pizzeta = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmap);
                Cv2.Resize(pizzeta, pizzeta, new OpenCvSharp.Size(image.Width / 2, image.Height / 2));


                Cv2.GaussianBlur(pizzeta, pizzeta, new OpenCvSharp.Size(5, 5), 0);

                Mat gray = new Mat();
                Cv2.CvtColor(pizzeta, gray, ColorConversionCodes.BGR2GRAY);

                Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 15, 2);

                CircleSegment[] circles = Cv2.HoughCircles(gray, HoughModes.Gradient, 1, 20, param1: 50, param2: 30, minRadius: 10, maxRadius: 0);

                CircleSegment largestCircle = new CircleSegment(new Point2f(), 0);
                double imageArea = image.Width * image.Height;

                foreach (CircleSegment circle in circles)
                {
                    double circleArea = Math.PI * circle.Radius * circle.Radius;
                    double circleCoverage = circleArea / imageArea;

                    if (circleCoverage >= 0.5 && (largestCircle == null || circle.Radius > largestCircle.Radius))
                    {
                        largestCircle = circle;
                    }
                }

                if (largestCircle.Radius > 0)
                {
                    Cv2.Circle(pizzeta, (int)largestCircle.Center.X, (int)largestCircle.Center.Y, (int)largestCircle.Radius, Scalar.Green, 2);

                    OpenCvSharp.Point center = new OpenCvSharp.Point((int)largestCircle.Center.X, (int)largestCircle.Center.Y);
                    Rect roi = new Rect(center.X, center.Y, 1, 1);
                    Mat roiMat = new Mat(pizzeta, roi);
                    Scalar color = Cv2.Mean(roiMat);

                    image.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(pizzeta);

                    MessageBox.Show($"Il cerchi piu grande {GetColorName(color)} occupa i 2/4 dell'immagine");

                    if (GetColorName(color) == "rosso")
                    {
                        button2.Visible = false;
                        button1.Visible = true;

                        _tello.Land();
                        button1.Enabled = true;
                        button2.Enabled = false;
                    }
                    if (GetColorName(color) == "verde")
                    {
                        button2.Visible = false;
                        button1.Visible = true;

                        _tello.Takeoff();
                        button1.Enabled = true;
                        button2.Enabled = false;
                    }
                }
                //else
                //{
                //    MessageBox.Show("no cerchio trovato che occupa i 2/4 dell'immagine");
                //}


                string GetColorName(Scalar color)
                {
                    if (color[2] > color[1] && color[2] > color[0])
                    {
                        return "rosso";
                    }
                    else if (color[1] > color[2] && color[1] > color[0])
                    {
                        return "verde";
                    }
                    else
                    {
                        return "unknown";
                    }
                }
            }
            if (step == 30)
            {
                step = 0;
            }
        }

        private void mjpeg_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void btnStart_Click_1(object sender, EventArgs e)
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
                _cmdForm.Location = new System.Drawing.Point(448,245);


            }

        }

        private void button1_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
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
                    // todo: debug exception
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

        public void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                _tello.StartOrStopVideoStreaming();
                mjpeg.ParseStream(new Uri("http://127.0.0.1:9000")); //
            }
            else
            {
                _tello.StartOrStopVideoStreaming();
                mjpeg.StopStream();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
                    
                    Form4 form = new Form4();
                    form.Show();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form4 form = new Form4();
                    form.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFormOnce();
            void OpenFormOnce()
            {

                if (Application.OpenForms.OfType<Form>().Any(f => f.Name == "Form5"))
                {

                }
                else
                {

                    Form5 banana = new Form5(_tello, result2);
                    banana.Show();
                }
            }
        }
        public static void changeLanguage(string language)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
        }

        
    }

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
