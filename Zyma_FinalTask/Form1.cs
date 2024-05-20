using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Zyma_FinalTask
{
    public partial class Form1 : Form
    {
        const string DEFAULT_SALT = "0000";
        const int DEFAULT_PASS_LENGTH = 3;
        const int DEFAULT_THREADS = 4;

        int[] i_pass = Enumerable.Range(33, 94).ToArray(); //{ 33..126 };
        string salt = "";
        byte[] passchar;
        byte[] hash;
        //byte[] passchar = BitConverter.GetBytes((Enumerable.Range(33, 94).ToArray()));//{ 33..126 };
        
        public Form1()
        {
            InitializeComponent();

            passchar = new byte[i_pass.Length];
            //Array.Copy(i_pass, passchar, i_pass.Length);
            passchar = i_pass.Select(i => (byte)i).ToArray();
            button2.Enabled = false;
        }

        private string getSalt()
        {
            string salt;
            salt = textBox1.Text;
            if (salt.Length == 0)
            {
                salt = DEFAULT_SALT;
            }
            return salt;
        }
        private int getMaxPassLength()
        {
            int pass_l;
            try
            {
                pass_l = int.Parse(textBox2.Text);
            }
            catch
            {
                pass_l = DEFAULT_PASS_LENGTH;
            }
            return pass_l;
        }
        private int getThreads()
        {
            int threads;
            try
            {
                threads = int.Parse(textBox3.Text);
            }
            catch
            {
                threads = DEFAULT_THREADS;
            }
            return threads;
        }

        private void update_salt()
        {
            salt = getSalt();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            update_salt();

            //byte[] pass = new byte[n];
            //new Random().NextBytes(pass);

            Passgen passgen = new Passgen(salt, passchar, getMaxPassLength());
            string strpass = passgen.generate_password();
            label4.Text = strpass;

            hash = passgen.compute_hash(strpass);
            string strhash = System.Text.Encoding.UTF8.GetString(hash);
            label6.Text = strhash;

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "generated_passwords.txt"), true))
            {
                outputFile.WriteLine(strhash);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            update_salt();
            Cracker cracker = new Cracker(passchar, getMaxPassLength(), hash, salt, getThreads());

                var sw = new Stopwatch();
                sw.Start();
            label15.Text = cracker.tryToCrack().Result;
            //cracker.hello();
                sw.Stop();
            label16.Text = sw.ElapsedMilliseconds.ToString() + " milliseconds";
        }
    }
}
