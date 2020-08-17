using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Common.Enums;
using static Common.Message.MessageContent;

namespace Common
{
    public static class Utility
    {
        /// <summary> 
        /// MD5 加密字符串 
        /// </summary> 
        /// <param name="rawPass">源字符串</param> 
        /// <returns>加密后字符串</returns> 
        public static string MD5Encoding(string rawPass)
        {
            // 创建MD5类的默认实例：MD5CryptoServiceProvider 
            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hs)
            {
                // 以十六进制格式格式化 
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 从形如IP:端口号的字符串中返回一个EndPoint实例
        /// </summary>
        /// <param name="ipAndPort"></param>
        /// <returns></returns>
        public static MessageIPAndPort GetEndPointFromString(string ipAndPort)
        {
            MessageIPAndPort result = new MessageIPAndPort();

            var split = ipAndPort.Split(':');

            if (split.Length != 2)
            {
                result.Invalid = true;
            }
            else
            {
                IPAddress.TryParse(split[0], out var address);
                int.TryParse(split[1], out var port);
                if (address != null && (port > 0 && port <= 65535))
                {
                    result.IP = address.ToString();
                    result.Port = port;
                    result.Invalid = false;
                }
                else
                {
                    result.Invalid = true;
                }
            }
            return result;
        }


    }
}
