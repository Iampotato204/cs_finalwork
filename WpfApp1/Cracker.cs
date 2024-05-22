using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Cracker_old
    {
        // all combinations are split in chunks of size ($threads * 1000), then each chunk is divided in $threads sectors for each thread to calculate 1000 at a time
        private const int threadChunkSize = 1000;
        private int chunkSize;
        byte[] passchar;
        byte[] hash;
        string salt;
        int threads;
        int max_pass_l;
        //int sampling_rate;

        public Cracker_old(byte[] passchar, int max_pass_l, byte[] hash, string salt, int threads)
        {
            this.passchar = passchar;
            this.max_pass_l = max_pass_l;
            this.hash = hash;
            this.salt = salt;
            this.threads = threads;

            // password combinations with default password char set(94) and 5 as a max length = 7,339,040,224
            // sampling rate (combinations for each thread to guess) = 1,834,760,056

            // upd1
            // because of that, default char was later changed to 62 elements 
            // current combinations:  916,132,832
            // current sampling rate: 229,033,208

            // upd2
            // nah, 94 is fine, i'll crank max length down to 4
            // current combinations:  78,074,896
            // current sampling rate: 19,518,724
            //this.sampling_rate = (int)(Math.Floor(Math.Pow(passchar.Length, max_pass_l) / threads));
            this.chunkSize = threadChunkSize * threads;
        }

        public byte[] toBaseX_readable(int num, int xbase)
        {
            int n = (int)(Math.Ceiling(Math.Log(num + 1, xbase)));
            byte[] result = new byte[n];
            int c = n - 1;
            do
            {
                byte tr = passchar[num % xbase];
                Debug.WriteLine("num: " + num + ", symbols: " + n + ", index: " + c + ", cur. ch: " + tr);
                result[c--] = tr; //base is always less than char/(byte) size
                num /= xbase;
            }
            while (num > 0);

            return result;
        }

        public byte[] toBaseX(int num, int xbase)
        {
            int n = (int)(Math.Ceiling(Math.Log(num + 1, xbase)));
            byte[] result = new byte[n--];

            do
            {
                result[n--] = passchar[num % xbase];
            }
            while ((num /= xbase) > 0);

            return result;
        }

        public async Task<string> tryToCrack()
        {
            //Task[] tasks = new Task[threads];
            List<Task<string>> tasks = new List<Task<string>>();
            byte[][] gen_pass = new byte[threads][];
            //List<string> temp_pass = new List<string>();

            //foreach (int i in new int[threads])

            //int chunks = (int)(Math.Ceiling(Math.Pow(passchar.Length, max_pass_l) / threadChunkSize));

            int combinations = (int)(Math.Pow(passchar.Length, max_pass_l));
            int chunks0 = combinations / chunkSize;

            string tempres;
            for (int k = 0; k < chunks0; k++)
            {
                for (int i = 0; i < threads; i++)
                {
                    Task<string> t = Task<string>.Run(() =>
                    {
                        return multithreaded_bruteforce(
                            k * chunkSize + threadChunkSize * i,
                            k * chunkSize + threadChunkSize * (i + 1) - 1); // -1 because max val is included in range
                    });
                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
                for (int i = 0; i < threads; i++)
                {
                    tempres = tasks[i].Result;
                    if (!(tempres == null))
                    {
                        return tempres;
                    }
                }
            }
            // if there was no pass in first xx..xx000 combinations:

            return multithreaded_bruteforce(
                combinations - (combinations % chunkSize),
                combinations).Result;
        }

        private bool checkPassword(byte[] temppass)
        {
            return (new Passgen().compute_hash(temppass).SequenceEqual(hash));
        }

        //private byte[] bruteforce(byte[] starting_positions, int maxVal) //recursively loops all the chars in range.
        //public void hello() { bruteforce(new byte[] { 1, 2, 3 }, 4); }
        private async Task<string> multithreaded_bruteforce(int starting_position, int maxVal)
        {
            byte[] generated;
            byte[] byteSalt = salt.ToCharArray().Select(c => (byte)c).ToArray();
            for (int i = starting_position; i <= maxVal; i++)
            {
                generated = toBaseX(i, passchar.Length);
                byte[] temp_pass = new byte[generated.Length + byteSalt.Length]; //generated.Select(g => g).Concat(salt.Select(s => s));
                generated.CopyTo(temp_pass, 0);
                byteSalt.CopyTo(temp_pass, generated.Length);

                Debug.WriteLine(System.Text.Encoding.UTF8.GetString(generated));

                //Console.WriteLine(System.Text.Encoding.UTF8.GetString(generated));
                if (checkPassword(temp_pass))
                {
                    return System.Text.Encoding.UTF8.GetString(temp_pass);
                }
            }
            return null;
        }
    }
}
