using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Common.Enums;

namespace Common.Message
{
    public class MessageContent
    {
        /// <summary>
        /// 报文的种类
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 报文长度总长度(包括头)
        /// </summary>
        public int MessageLength { get; set; }

        /// <summary>
        /// 报文中的数据部分长度
        /// </summary>
        public int DataLength { get; set; }

        /// <summary>
        /// 报文数据(需要被转发的部分)
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public string GID { get; set; }

        /// <summary>
        /// 报文的IP和端口号(只有Client和Server报文里有这个属性)
        /// </summary>
        public MessageIPAndPort IPAndPort {get;set;}

        /// <summary>
        /// 要发送至的GID号
        /// </summary>
        public string SendToGID { get; set; }

        /// <summary>
        /// 是否分包报文
        /// </summary>
        public bool IsChunked { get; set; } = false;

        /// <summary>
        /// 报文的IP和端口号(只有Client和Server报文里有这个属性)
        /// </summary>
        public struct MessageIPAndPort
        {
            /// <summary>
            /// IP
            /// </summary>
            public string IP { get; set; }

            /// <summary>
            /// 端口号
            /// </summary>
            public int Port { get; set; }

            /// <summary>
            /// 是否无效
            /// </summary>
            public bool Invalid { get; set; }
        }

    }
}
