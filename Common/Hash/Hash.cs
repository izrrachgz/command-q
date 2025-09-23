using System.Text;
using System.Security.Cryptography;

namespace Common.Hash
{
    public class Hash
    {
        public static string ComputeSHA256(string chain)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(chain));

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
