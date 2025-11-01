using System.Text;
using System.Security.Cryptography;

namespace Common.Hash
{
    public class Hash
    {
        public static string ComputeSHA256(string chain)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(chain));

            StringBuilder sb = new();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        public static bool CompareSHA256(string known, string computed)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(known, computed) == 0;
        }
    }
}