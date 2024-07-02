using System.Security.Cryptography;
using System.Text;

namespace RoboticArmSystem.Core.Utils
{
    public class EncryptTool
    {
        /// <summary>
        /// 將密碼傳入雜湊函數
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt32(string password)
        {
            string pwd = "";
            MD5 md5 = MD5.Create();

            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < s.Length; i++)
            {
                pwd = pwd + s[i].ToString("x");
            }
            return pwd;
        }
    }
}
