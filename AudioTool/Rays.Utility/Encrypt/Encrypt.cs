/*
 源码己托管:http://git.oschina.net/kuiyu/dotnetcodes
 */
using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace Rays.Utility
{
	/// <summary>
	/// 加密解密实用类。
	/// </summary>
	public class Encrypt
	{
		//密钥
		private static byte[] arrDESKey = new byte[] {42, 16, 93, 156, 78, 4, 218, 32};
		private static byte[] arrDESIV = new byte[] {55, 103, 246, 79, 36, 99, 167, 3};

		/// <summary>
		/// 加密。
		/// </summary>
		/// <param name="m_Need_Encode_String"></param>
		/// <returns></returns>
		public static string Encode(string m_Need_Encode_String)
		{
			if (m_Need_Encode_String == null)
			{
				throw new Exception("Error: \n源字符串为空！！");
			}
			DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
			MemoryStream objMemoryStream = new MemoryStream();
			CryptoStream objCryptoStream = new CryptoStream(objMemoryStream,objDES.CreateEncryptor(arrDESKey,arrDESIV),CryptoStreamMode.Write);
			StreamWriter objStreamWriter = new StreamWriter(objCryptoStream);
			objStreamWriter.Write(m_Need_Encode_String);
			objStreamWriter.Flush();
			objCryptoStream.FlushFinalBlock();
			objMemoryStream.Flush();
			return Convert.ToBase64String(objMemoryStream.GetBuffer(), 0, (int)objMemoryStream.Length);
		}

		/// <summary>
		/// 解密。
		/// </summary>
		/// <param name="m_Need_Encode_String"></param>
		/// <returns></returns>
		public static string Decode(string m_Need_Encode_String)
		{
			if (m_Need_Encode_String == null)
			{
				throw new Exception("Error: \n源字符串为空！！");
			}
			DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
			byte[] arrInput = Convert.FromBase64String(m_Need_Encode_String);
			MemoryStream objMemoryStream = new MemoryStream(arrInput);
			CryptoStream objCryptoStream = new CryptoStream(objMemoryStream,objDES.CreateDecryptor(arrDESKey,arrDESIV),CryptoStreamMode.Read);
			StreamReader  objStreamReader  = new StreamReader(objCryptoStream);
			return objStreamReader.ReadToEnd();
		}

         

        /// <summary>
        /// 32位MD5加密
        /// </summary>
        /// <param name="strText">要加密字符串</param>
        /// <returns></returns>
        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strText);
            bytes = md5.ComputeHash(bytes);
            md5.Clear();

            string ret = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                ret += bytes[i].ToString("x");
            }

            return ret;
        }
        /// <summary>
        /// 32位MD5加密，同System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Md5Hash(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }

        #region 获取API加密的字符串  
        public static string EncryptSign(string str)
        {
            string result = "";
            if (!string.IsNullOrEmpty(str))
            {
                byte[] str1 = Encoding.ASCII.GetBytes(str);
                int num = str1.Length / 3;
                byte[] str2 = new byte[num + 1];
                for (int i = 0; i < num; i++)
                {
                    str2[i] = (byte)((str1[i * 3] + str1[i * 3 + 1] + str1[i * 3 + 2]) / 3);
                }
                if (str1.Length % 3 == 1)
                {
                    str2[num] = str1[str1.Length - 1];
                }
                else if (str1.Length % 3 == 2)
                {
                    str2[num] = (byte)((str1[str1.Length - 2] + str1[str1.Length - 1]) / 2);
                }
                result = Encoding.ASCII.GetString(str2);
            }

            return result;
        }
        #endregion
    }
}
