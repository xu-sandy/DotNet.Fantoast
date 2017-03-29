using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Core.Security
{
    /// <summary>
    /// 加密数据
    /// </summary>
    public class Encrypt
    {
        public static string GetMD5String(string sourceStr)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.Default.GetBytes(sourceStr);
            byte[] result = md5.ComputeHash(data);
            String retStr = "";
            for (int i = 0; i < result.Length; i++)
                retStr += result[i].ToString("x").PadLeft(2, '0');
            return retStr;
        }
    }
}
