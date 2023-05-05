using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace demoTello
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string pizzeta = default;
            StreamReader miofile;
            string currentDirectory = Environment.CurrentDirectory;
            string filePath = Path.Combine(currentDirectory, "Bro.txt");
            miofile = new StreamReader(filePath);
            pizzeta = miofile.ReadLine();
            if (pizzeta == "True")
            {
                Application.Run(new Form3());
                Task.Delay(1000).Wait();

            }
            miofile.Close();
            Application.Run(new Form1());
           



        }
    }
}
