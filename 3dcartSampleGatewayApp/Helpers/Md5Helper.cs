using System;
using System.Security.Cryptography;
using System.Text;

namespace _3dcartSampleGatewayApp.Helpers
{
    public class Md5Helper
    {
        public static string GetMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sBuilder = new StringBuilder();

                for (var i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        public static string GetMd5Hash(int input)
        {
            return GetMd5Hash(Convert.ToString(input));
        }

      
        public static bool VerifyMd5Hash(string input, string hash)
        {
            var hashOfInput = GetMd5Hash(input);

            var comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
                return true;
            else
                return false;
        }
    }
}