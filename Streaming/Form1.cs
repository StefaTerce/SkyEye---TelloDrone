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
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Diagnostics;
using Drone;

namespace Streaming
{
    public partial class Form1 : Form
    {
        private MjpegDecoder mjpeg;

        private Mat imageopen;

        private TelloCmd _tello;   

        int step = 0;
        public Form1()
        {
            InitializeComponent();
            mjpeg = new MjpegDecoder();
            mjpeg.FrameReady += mjpeg_FrameReady;
            mjpeg.Error += mjpeg_Error;
            mjpeg.ParseStream(new Uri("http://127.0.0.1:9000")); //
        }
        private void mjpeg_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
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
            if (radioButton1.Checked == true)
            {
                step++;

                if (step == 20)
                {
                    imageopen = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmap);
                    Cv2.Resize(imageopen, imageopen, new OpenCvSharp.Size(image.Width / 2, image.Height / 2));


                    Cv2.GaussianBlur(imageopen, imageopen, new OpenCvSharp.Size(5, 5), 0);

                    Mat gray = new Mat();
                    Cv2.CvtColor(imageopen, gray, ColorConversionCodes.BGR2GRAY);

                    Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 15, 2);

                    CircleSegment[] circles = Cv2.HoughCircles(gray, HoughModes.Gradient, 1, 20, param1: 50, param2: 30, minRadius: 10, maxRadius: 0);

                    CircleSegment largestCircle = new CircleSegment(new Point2f(), 0);
                    double imageArea = image.Width * image.Height;

                    foreach (CircleSegment circle in circles)
                    {
                        double circleArea = Math.PI * circle.Radius * circle.Radius;
                        double circleCoverage = circleArea / imageArea;

                        if (circleCoverage >= 0.35 && (largestCircle == null || circle.Radius > largestCircle.Radius))
                        {
                            largestCircle = circle;
                        }
                    }

                    if (largestCircle.Radius > 0)
                    {
                        Cv2.Circle(imageopen, (int)largestCircle.Center.X, (int)largestCircle.Center.Y, (int)largestCircle.Radius, Scalar.Green, 2);

                        OpenCvSharp.Point center = new OpenCvSharp.Point((int)largestCircle.Center.X, (int)largestCircle.Center.Y);
                        Rect roi = new Rect(center.X, center.Y, 1, 1);
                        Mat roiMat = new Mat(imageopen, roi);
                        Scalar color = Cv2.Mean(roiMat);

                        image.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageopen);

                        Console.WriteLine($"Il cerchi piu grande {GetColorName(color)} occupa i 2/4 dell'immagine");

                        if (GetColorName(color) == "rosso")
                        {
                            //_tello.Land();
                            MessageBox.Show("Takeoff");
                        }
                        if (GetColorName(color) == "verde")
                        {
                            //_tello.Takeoff();
                            MessageBox.Show("Land");    
                        }
                    }
                    else
                    {
                        MessageBox.Show("no cerchio trovato che occupa i 2/4 dell'immagine");
                    }


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
        }
    }
}
