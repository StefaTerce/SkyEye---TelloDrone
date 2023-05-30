using demoTello.helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using tellocs;
using Drone;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace demoTello
{

    public partial class Form5 : Form
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


        public struct Istruzione
        {
            public double distance;
            public double degrees;
        }

        private List<Point> points = new List<Point>();
        private bool ostacoliMode = false;

        private Istruzione[] istruzioni = new Istruzione[1000];
        private Istruzione[] istruzioniUfficiali = new Istruzione[1000];

        int num = 0;
        int num2 = 0;

        [Serializable]
        class SerializableState
        {
            public List<Point> Points { get; }
            public List<Point> Obstacles { get; }
            public Image Image { get; }

            public SerializableState(List<Point> points, List<Point> obstacles, Image image)
            {
                Points = points;
                Obstacles = obstacles;
                Image = image;
            }
        }
        string error = "";
        public Form5(TelloCmd tello, string result)
        {
            error= result;
            TelloRC.Tello = tello;
            InitializeComponent();
            pictureBox1.MouseClick += pictureBox1_MouseClick;
            comboBox1.Items.Add("Cerchi");
            comboBox1.Items.Add("Ostacoli");

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ostacoliMode = (comboBox1.SelectedItem.ToString() == "Ostacoli");
            pictureBox1.Invalidate();

        }

        private List<Point> obstacles = new List<Point>();

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (ostacoliMode)
            {

                obstacles.Add(e.Location);
                pictureBox1.Invalidate();
            }
            else
            {

                if (!HasCollision(e.Location))
                {
                    points.Add(e.Location);
                    pictureBox1.Invalidate();
                }
            }
        }

        private bool HasCollision(Point location)
        {

            foreach (Point obstacle in obstacles)
            {
                if (Distance(location, obstacle) < 50.0)
                {
                    return true;
                }
            }

            return false;
        }

        private int Distance(Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }


        double distanceOfficial;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            foreach (Point obstacle in obstacles)
            {
                e.Graphics.DrawLine(Pens.Red, obstacle.X - 25, obstacle.Y - 25, obstacle.X + 25, obstacle.Y + 25);
                e.Graphics.DrawLine(Pens.Red, obstacle.X + 25, obstacle.Y - 25, obstacle.X - 25, obstacle.Y + 25);
            }


            if (points.Count > 1)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Point start = points[i];
                    Point end = points[i + 1];

                    bool pathClear = true;
                    foreach (Point obstacle in obstacles)
                    {
                        if (LineIntersectsCircle(start, end, obstacle, 25))
                        {
                            pathClear = false;
                            break;
                        }
                    }
                    if (!pathClear)
                    {
                        points.Clear();
                        pictureBox1.Invalidate();
                        label1.Text = "Percorso passa per un ostacolo";
                        if (num > 0)
                        {
                            int x = 0;
                            while (x < num)
                            {
                                istruzioni[x].distance = 0;
                                istruzioni[x].degrees = 0;
                                x++;
                            }
                            num = 0;
                        }
                        break;
                    }
                    if (pathClear)
                    {
                        label1.Text = ".";
                        e.Graphics.DrawLine(Pens.Green, start, end);

                    }
                }
            }

            foreach (Point point in points)
            {
                if (!HasCollision(point))
                {
                    e.Graphics.FillEllipse(Brushes.Blue, point.X - 25, point.Y - 25, 50, 50);
                }
            }




            //if (points.Count > 1)
            //{
            //    double totalDistance = 0.0;
            //    for (int i = 0; i < points.Count - 1; i++)
            //    {
            //        Point start = points[i];
            //        Point end = points[i + 1];
            //        double distance = Distance(start, end) / 29.0;

            //        if (distance != 0.0)
            //        {
            //            if (i > 0)
            //            {
            //                istruzioni[i - 1].distance = distance;
            //            }
            //        }
            //        totalDistance += distance;
            //        if (distance != 0.0)
            //            e.Graphics.DrawString(distance.ToString("F2") + "m", this.Font, Brushes.Black, (start.X + end.X) / 2, (start.Y + end.Y) / 2);
            //    }
            //    e.Graphics.DrawString("Total distance: " + totalDistance.ToString("F2") + "m", this.Font, Brushes.Black, 10, pictureBox1.Height - 20);

            //    for (int i = 1; i < points.Count - 1; i = i + 2)
            //    {
            //        Point current = points[i];
            //        Point previous = points[i - 1];
            //        Point next = points[i + 1];

            //        double dx1 = current.X - previous.X;
            //        double dy1 = current.Y - previous.Y;
            //        double angle1 = Math.Atan2(dy1, dx1) * 180 / Math.PI;

            //        double dx2 = next.X - current.X;
            //        double dy2 = next.Y - current.Y;
            //        double angle2 = Math.Atan2(dy2, dx2) * 180 / Math.PI;

            //        // disegna i gradi tra i due punti

            //        double angle = -((angle2 - angle1) % 360);

            //        if (istruzioni[i - 1].distance != 0.0)
            //        {
            //            istruzioni[i - 1].degrees = angle;
            //            num++;
            //            refreshListBoxIstruzioni(ref num);
            //        }


            //        string angleText = $"{angle:N2} gradi";
            //        Point center = new Point((current.X + previous.X + next.X) / 3, (current.Y + previous.Y + next.Y) / 4);

            //        e.Graphics.DrawString(angleText, this.Font, Brushes.Black, center.X, center.Y);


            //    }
            //}



            if (points.Count > 1)
            {
                double totalDistance = 0.0;
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Point start = points[i];
                    Point end = points[i + 1];
                    double distance = Distance(start, end) / 29.0;

                    if (distance != 0.0)
                    {
                        if (i > 0)
                        {
                            istruzioni[i - 1].distance = distance;
                            distanceOfficial = distance;
                        }
                    }
                    totalDistance += distance;

                    if (distance != 0.0)
                    {
                        if (i > 0)
                        {
                            istruzioni[i - 1].distance = distance;
                        }
                    }
                    if (distance != 0.0)
                        e.Graphics.DrawString(distance.ToString("F2") + "m", this.Font, Brushes.Black, (start.X + end.X) / 2, (start.Y + end.Y) / 2);
                }
                e.Graphics.DrawString("Total distance: " + totalDistance.ToString("F2") + "m", this.Font, Brushes.Black, 10, pictureBox1.Height - 20);

                for (int i = 1; i < points.Count - 1; i = i + 2)
                {
                    Point current = points[i];
                    Point previous = (i > 1) ? points[i - 2] : points[i - 1];
                    Point next = (i < points.Count - 2) ? points[i + 2] : points[i + 1];

                    double dx1 = current.X - previous.X;
                    double dy1 = current.Y - previous.Y;
                    double angle1 = Math.Atan2(dy1, dx1) * 180 / Math.PI;

                    double dx2 = next.X - current.X;
                    double dy2 = next.Y - current.Y;
                    double angle2 = Math.Atan2(dy2, dx2) * 180 / Math.PI;

                    // calcola l'angolo tra i due cerchi basandosi sulla posizione del cerchio precedente
                    double angle = angle2 - angle1;

                    // aggiorna la posizione del cerchio precedente
                    previous = current;
                    if (istruzioni[i - 1].distance != 0.0)
                    {
                        istruzioni[i - 1].degrees = angle;
                        num++;
                        refreshListBoxIstruzioni(ref num);
                        txt_insDegree.Text = angle.ToString();
                        txt_insDistance.Text = distanceOfficial.ToString();
                        btn_createIstruction.PerformClick();
                    }

                    // disegna i gradi tra i due cerchi
                    string angleText = $"{angle:N2} gradi";
                    Point center = new Point((current.X + previous.X) / 2, (current.Y + previous.Y) / 2);

                    e.Graphics.DrawString(angleText, this.Font, Brushes.Black, center.X, center.Y);

                }
            }


        }
        private bool LineIntersectsCircle(Point start, Point end, Point circleCenter, int circleRadius)
        {

            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double a = dx * dx + dy * dy;
            double b = 2 * (dx * (start.X - circleCenter.X) + dy * (start.Y - circleCenter.Y));
            double c = (start.X - circleCenter.X) * (start.X - circleCenter.X) + (start.Y - circleCenter.Y) * (start.Y - circleCenter.Y) - circleRadius * circleRadius;
            double delta = b * b - 4 * a * c;
            if (delta >= 0)
            {
                double t1 = (-b - Math.Sqrt(delta)) / (2 * a);
                double t2 = (-b + Math.Sqrt(delta)) / (2 * a);
                if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1))
                {
                    return true;
                }
            }
            return false;
        }

        private void Form5_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (num > 0)
            {
                int x = 0;
                while (x < num)
                {
                    istruzioni[x].distance = 0;
                    istruzioni[x].degrees = 0;
                    x++;
                }
                num = 0;
            }
            points.Clear();
            obstacles.Clear();
            pictureBox1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.Text = ".";
            int x = 0;
            string k = default;
            string z = default;
            Random rnd = new Random();
            try
            {
                while (x < 5)
                {
                    int mIndex = rnd.Next(1, 1000);
                    z = mIndex.ToString();
                    k = label2.Text + z;
                    label2.Text = k;
                    x = x + 1;
                }
                string currentDirectory = Environment.CurrentDirectory;
                string filePath = Path.Combine(currentDirectory, $@"{label2.Text}.dat");
                string path = filePath;
                label2.Text = label2.Text + ".dat";
                SaveState(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore: " + ex.Message);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            try
            {
                var dialog = new OpenFileDialog();
                dialog.ShowDialog();
                string path = dialog.FileName;
                LoadState(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore: " + ex.Message);
            }
        }
        public void SaveState(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, new SerializableState(points, obstacles, pictureBox1.Image));
            }
        }
        public void LoadState(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                var serializableState = (SerializableState)formatter.Deserialize(fileStream);

                points = serializableState.Points;
                obstacles = serializableState.Obstacles;
                pictureBox1.Image = serializableState.Image;

                pictureBox1.Invalidate();
            }
        }


        private void btnVettore_Click(object sender, EventArgs e)
        {
            refreshListBoxIstruzioni(ref num);
        }


        public void separateIstructions(int pos,Istruzione[] arr)
        {
            
        }

        private void refreshListBoxIstruzioni(ref int num)
        {
            num2= 0;
            VECTOR.Items.Clear();
            if (num > 0)
            {
                int x = 0;
                while (x <= num)
                {
                    if (istruzioni[x].distance != 0.0)
                    {
                        if (istruzioni[x].distance > 5.00)
                        {
                            separateIstructions(x, istruzioni);
                        }
                        else
                        {
                            istruzioniUfficiali[x].distance = istruzioni[x].distance;
                        }


                        istruzioniUfficiali[x].distance = istruzioni[x].distance;

                        if (istruzioni[x].degrees < 0)
                        {
                            istruzioni[x].degrees = 360 + istruzioni[x].degrees;
                        }

                        istruzioniUfficiali[x].degrees = istruzioni[x].degrees;
                        string stringa = $"{x/2}: Angolo: {istruzioniUfficiali[x].degrees}, Distanza: {istruzioniUfficiali[x].distance}.";
                        VECTOR.Items.Add(stringa);
                        num2++;
                    }
                    x++;
                }
                num = x;
            }
            else
            {
                MessageBox.Show("Nessun Dato");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void parti_Click(object sender, EventArgs e)
        {
            TelloRC.Tello.Takeoff();
            int x = 0;
            while (x <= num2 + 10)
            {
                int degree = Convert.ToInt32(istruzioniUfficiali[x].degrees);
                if (degree < 0)
                {
                    degree = 360 + degree;
                }
                TelloRC.Tello.Clockwise(degree);
                Console.WriteLine(error);
                int distance = Convert.ToInt32(istruzioniUfficiali[x].distance);
                TelloRC.Tello.Fly("forward", distance * 100);
                Console.WriteLine(error);
                x++;
            }
            TelloRC.Tello.Land();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (num > 0)
            {
                int x = 0;
                while (x < num)
                {
                    istruzioni[x].distance = 0;
                    istruzioni[x].degrees = 0;
                    x++;
                }
                num = 0;
            }
            points.Clear();
            obstacles.Clear();
            pictureBox1.Invalidate();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            label2.Text = ".";
            int x = 0;
            string k = default;
            string z = default;
            Random rnd = new Random();
            try
            {
                while (x < 5)
                {
                    int mIndex = rnd.Next(1, 1000);
                    z = mIndex.ToString();
                    k = label2.Text + z;
                    label2.Text = k;
                    x = x + 1;
                }
                string currentDirectory = Environment.CurrentDirectory;
                string filePath = Path.Combine(currentDirectory, $@"{label2.Text}.dat");
                string path = filePath;
                label2.Text = label2.Text + ".dat";
                SaveState(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore: " + ex.Message);
            }
        }

        public static void changeLanguage(string language)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
        }

        private void btn_manualSet_Click(object sender, EventArgs e)
        {
            if (pnl_manualSet.Visible == true)
            {
                pnl_manualSet.Visible = false;
            }
            else
            {
                pnl_manualSet.Visible = true;
            }
        }

        private void btn_createIstruction_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txt_insDegree.Text, out double degree) == false || double.TryParse(txt_insDistance.Text, out double distance) == false || double.Parse(txt_insDistance.Text) <= 0.00)
                {
                MessageBox.Show("Dati Non Validi");
                }
            else
            {
                if (distance != 0.00)
                {
                    istruzioni[num].degrees = degree;
                    istruzioni[num].distance = distance;
                    num++;
                    refreshListBoxIstruzioni(ref num);
                }
                else
                {
                    MessageBox.Show("Inserisci Una Distanza Valida!");
                    return;
                }
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            pnl_manualSet.Visible = false;
        }

        int pos = -1;

        private void VECTOR_SelectedIndexChanged(object sender, EventArgs e)
        {
            pos = int.Parse(VECTOR.SelectedItem.ToString().Substring(0, 1));
        }

        private void btn_deleteIstruction_Click(object sender, EventArgs e)
        {
            if (pos != -1)
            {
                DialogResult choise = MessageBox.Show("Do you really want to delete this istruction?", "Delete Message", MessageBoxButtons.YesNo);
                if (choise == DialogResult.Yes)
                {
                    int y = pos;
                    while (y < num2)
                    {
                        istruzioniUfficiali[y] = istruzioniUfficiali[y + 1];
                        y++;
                    }
                    num2--;
                    refreshListBoxIstruzioni(ref num);
                }
            }
            pos = -1;
        }

       
    }

}
