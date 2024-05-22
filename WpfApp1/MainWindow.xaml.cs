using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DEFAULT_SALT = "0000";
        const int DEFAULT_PASS_LENGTH = 3;
        const int DEFAULT_THREADS = 4;

        int[] i_pass = Enumerable.Range(33, 94).ToArray(); //{ 33..126 };
        string salt = "";
        byte[] passchar;
        byte[] hash;

        public MainWindow()
        {
            InitializeComponent();

            passchar = new byte[i_pass.Length];
            //Array.Copy(i_pass, passchar, i_pass.Length);
            passchar = i_pass.Select(i => (byte)i).ToArray();
            button2.IsEnabled = false;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            button2.IsEnabled = true;
            update_salt();

            //byte[] pass = new byte[n];
            //new Random().NextBytes(pass);

            Passgen passgen = new Passgen(salt, passchar, getMaxPassLength());
            string strpass = passgen.generate_password();
            label1.Content = strpass;

            hash = passgen.compute_hash(strpass);
            string strhash = System.Text.Encoding.UTF8.GetString(hash);
            label2.Content = strhash;

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, "generated_passwords.txt"), true))
            {
                outputFile.WriteLine(strhash);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            update_salt();
            labelRes1.Content = ((int)Math.Pow(passchar.Length, getMaxPassLength())).ToString();
            Cracker cracker = new Cracker(passchar, getMaxPassLength(), hash, salt, getThreads());

            var sw = new Stopwatch();
            sw.Start();
            labelRes2.Content = cracker.tryToCrack().Result;
            //cracker.hello();
            sw.Stop();
            labelRes3.Content = sw.ElapsedMilliseconds.ToString() + " milliseconds";
        }
    }
}