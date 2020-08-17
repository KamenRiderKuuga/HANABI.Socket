using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Enums
    {
        /// <summary>
        /// 报文种类
        /// </summary>
        public enum MessageType
        {
            /// <summary>
            /// 无类别
            /// </summary>
            None,
            /// <summary>
            /// HTTP报文
            /// </summary>
            Http,
            /// <summary>
            /// 来自客户端的报文
            /// </summary>
            Client,
            /// <summary>
            /// 来自服务器的报文
            /// </summary>
            Server,
            /// <summary>
            /// 登录请求
            /// </summary>
            Login,
            /// <summary>
            /// 心跳包
            /// </summary>
            HeartBeat,
            /// <summary>
            /// 分包报文
            /// </summary>
            Chunked
        }

        public enum MessageStatus
        {

            /// <summary>
            /// 读取完成
            /// </summary>
            Finished,

            /// <summary>
            /// 读取未未完成
            /// </summary>
            UnFinished,

            /// <summary>
            /// 头部读取未完成
            /// </summary>
            HeaderUnfinshed

        }

        /// <summary>
        /// 连接种类
        /// </summary>
        public enum SessionType
        {
            // <summary>
            /// 无类别
            /// </summary>
            None,
            /// <summary>
            /// HTTP
            /// </summary>
            Http,
            /// <summary>
            /// 客户端
            /// </summary>
            Client,
            /// <summary>
            /// 服务器
            /// </summary>
            Server
        }
    }
}
