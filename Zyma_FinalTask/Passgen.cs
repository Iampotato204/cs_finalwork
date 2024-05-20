using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Zyma_FinalTask
{
    internal class Passgen
    {
        string salt;
        byte[] passchar;
        int passLength;

        public Passgen() { }
        public Passgen(string salt, byte[] passchar, int passLength) {
            this.salt = salt;
            this.passchar = passchar;
            this.passLength = passLength;
        }
        public byte[] compute_hash(string password)
        {
            return new SHA256Managed().ComputeHash(password.ToCharArray().Select(c => (byte)c).ToArray());
        }
        public byte[] compute_hash(byte[] password)
        {
            return new SHA256Managed().ComputeHash(password);
        }
        public string generate_password()
        {
            int n = new Random().Next(2, passLength+1);
            byte[] pass = new byte[n];// + salt_length];
            Random rand;
            //foreach (byte b in pass)
            for (int i = 0; i<n; i++)
            {
                rand = new Random();
                pass[i] = passchar[rand.Next(0, passchar.Length)];
            }
            /*for (int i = 0; i<salt_length; i++) 
            {
                pass[n+i] = (byte)(salt.ToCharArray()[i]);
            }*/

            return (System.Text.Encoding.UTF8.GetString(pass) + salt);
        }
    }
}
