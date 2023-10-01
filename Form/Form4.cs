using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace demoTello
{
    public partial class Form4 : Form
    {
        private Button myButton;
        private Label myLabel;

        private ResourceManager _resourceManager;

        public Form4()
        {

            

            // Inizializza la label
            myLabel = new Label();
            myLabel.Text = "Testo della label";
            myLabel.Location = new System.Drawing.Point(50, 100);
            myLabel.Visible = false;

            // Aggiungi il pulsante e la label alla finestra
            this.Controls.Add(myButton);
            this.Controls.Add(myLabel);

            InitializeComponent();



            StreamReader miofile;
            string pizzeta = default;
            string currentDirectory = Environment.CurrentDirectory;
            string filePath = Path.Combine(currentDirectory, "Bro.txt");
            miofile = new StreamReader(filePath);
            pizzeta = miofile.ReadLine();
            if (pizzeta == "True")
            {
                miofile.Close();
                checkBox1.Checked = true;

            }
            if (pizzeta == "False")
            {
                miofile.Close();
                checkBox1.Checked = false;

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            StreamWriter miofile;
            string currentDirectory = Environment.CurrentDirectory;
            string filePath = Path.Combine(currentDirectory, "Bro.txt");
            miofile = new StreamWriter(filePath);
            if (checkBox1.Checked == true)
            {

                miofile.WriteLine("True");

            }
            if (checkBox1.Checked == false)
            {
                miofile.WriteLine("False");

            }
            miofile.Close();
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        private void ITALIANO_CheckedChanged(object sender, EventArgs e)
        {

        }
         
        //LINGUAAAAAAAAAAAAAAAAAAAAAAAAA
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    string language = "it";
                    Form1.changeLanguage(language);
                    Form1.changeLanguage(language);
                    Form3.changeLanguage(language);
                    Form4.changeLanguage(language);
                    Form5.changeLanguage(language);
                    break;
                case 1:
                    string language2 = "en";
                    Form1.changeLanguage(language2);
                    Form1.changeLanguage(language2);
                    Form3.changeLanguage(language2);
                    Form4.changeLanguage(language2);
                    Form5.changeLanguage(language2);
                    break;
                case 2:
                    string language3 = "de";
                    Form1.changeLanguage(language3);
                    Form1.changeLanguage(language3);
                    Form3.changeLanguage(language3);
                    Form4.changeLanguage(language3);
                    Form5.changeLanguage(language3);
                    break;
            }

            this.Controls.Clear();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Mostra/nascondi la label
            myLabel.Visible = !myLabel.Visible;
        }

        public static void changeLanguage(string language)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
        }
    }
}
