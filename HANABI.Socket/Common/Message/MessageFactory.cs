using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Common.Enums;
using static Common.Utility;

namespace Common.Message
{
    /// <summary>
    /// 一个简单的静态工厂类，主要用来初始化报文信息，原本想设计Message接口，后面觉得没有必要，就没这么做
    /// 此类纯属用来构建和检查一个完整的Message实例
    /// </summary>
    public static class MessageFactory
    {
        /// <summary>
        /// 从一个报文中获取到具体的报文实例
        /// </summary>
        /// <param name="data">接收到的byte数据</param>
        /// <returns></returns>
        public static MessageContent InitMessageFromData(ref byte[] data, out byte[] leftMessage)
        {
            var textContent = Encoding.UTF8.GetString(data);
            leftMessage = null;

            MessageContent message = new MessageContent();

            StringReader stringReader = new StringReader(textContent);

            string line = "";
            string headerline = "";

            // 因为这个函数主要是为了验证头部的完整性，从而读取出报文的总长度，数据部分长度
            // 定义三个变量，分别代表报头是否开始，是否有效，是否结束，三个条件缺一不可
            // 否则将报文归纳为剩余报文，放到之后的报文前面一起处理
            bool headerBegin = false;
            bool headerValid = false;
            bool headerEnd = false;

            MessageType messageType = MessageType.None;

            string gid = "";
            int contentLength = 0;
            string ipAddress = "";
            string encoding = "";

            // 开始解析头部
            while (line != null)
            {
                // 读到当前行
                line = stringReader.ReadLine();

                if (line != null)
                {
                    if (headerBegin == false)
                    {
                        // 如果还没检测到固定头，则通过这个函数检查并给消息种类赋值
                        headerBegin = CheakIfHeaderLineAndGetType(line, ref messageType);

                        if (messageType == MessageType.None)
                        {
                            break;
                        }

                        if (messageType == MessageType.HeartBeat)
                        {
                            headerValid = true;
                            headerline += line + "\r\n";
                            headerEnd = true;
                            break;
                        }
                    }
                    else
                    {
                        // 请求头读完，退出循环
                        if (headerValid && line == "")
                        {
                            headerline += line + "\r\n";
                            headerEnd = true;
                            break;
                        }

                        var lineSplit = line.Split(':');

                        if (lineSplit.Length >= 2)
                        {
                            headerValid = true;
                            // 如果是GID
                            if (gid == "" && lineSplit[0].ToUpper() == "GID")
                            {
                                gid = lineSplit[1].Trim();
                            }

                            if (encoding == "" && lineSplit[0].ToUpper() == "TRANSFER-ENCODING")
                            {
                                encoding = lineSplit[1].Trim();
                                // 如果是分包报文
                                if (encoding.ToUpper().Trim() == "CHUNKED")
                                {
                                    message.IsChunked = true;
                                }
                            }

                            if (contentLength == 0 && lineSplit[0].ToUpper() == "CONTENT-LENGTH")
                            {
                                int.TryParse(lineSplit[1].Trim(), out contentLength);
                            }

                            if (ipAddress == "" && lineSplit[0].ToUpper() == "IP")
                            {
                                ipAddress = lineSplit[1].Trim() + (lineSplit.Length > 2 ? ":" + lineSplit[2].Trim() : "");
                            }

                        }
                        else
                        {
                            contentLength = 0;
                            gid = "";
                            ipAddress = "";
                            headerValid = false;
                            headerBegin = false;
                            encoding = "";
                        }
                    }
                    if (headerBegin)
                    {
                        headerline += line + "\r\n";
                    }
                    else
                    {
                        headerline = "";
                    }
                }
            }

            // 当前数据是错的，舍弃掉所有数据，等待之后的数据
            if (messageType == MessageType.None)
            {
                leftMessage = null;
                return null;
            }

            // 头还没读完，用之后的数据来处理
            if (headerBegin && !headerEnd)
            {
                leftMessage = data;
                return null;
            }

            line = stringReader.ReadLine();

            if (headerEnd == true && line == null && data.Last() != 10)
            {
                leftMessage = data;
                return null;
            }

            // 如果当前是分包数据，一直读到最后一个包，若包完整，给Content-Length赋值
            if (message.IsChunked)
            {
                var readLength = Encoding.UTF8.GetBytes(headerline).Length;
                var readFinish = false;

                while (readFinish == false)
                {
                    var dataleft = data.Skip(readLength).ToArray();
                    StringReader reader = new StringReader(Encoding.UTF8.GetString(dataleft));

                    line = reader.ReadLine();

                    if (line != null)
                    {
                        var lengthIsValid = int.TryParse(line, System.Globalization.NumberStyles.HexNumber, null, out var result);
                        // 如果长度有效，
                        if (lengthIsValid)
                        {
                            if (result != 0)
                            {
                                readLength += Encoding.UTF8.GetBytes(line + "\r\n").Length + result + Encoding.UTF8.GetBytes("\r\n").Length;
                                if (data.Length <= readLength)
                                {
                                    leftMessage = data;
                                    return null;
                                }
                            }
                            else
                            {
                                readLength += Encoding.UTF8.GetBytes(line + "\r\n\r\n").Length;
                                if (data.Length >= readLength)
                                {
                                    readFinish = true;
                                }
                            }
                        }
                        else
                        {
                            leftMessage = null;
                            return null;
                        }
                    }
                    else
                    {
                        leftMessage = data;
                        return null;
                    }
                }

                contentLength = readLength - Encoding.UTF8.GetBytes(headerline).Length;
            }

            // 当头完整时，根据这个头的信息，获取报文实体
            if (headerBegin && headerValid && headerEnd)
            {
                message = GetMessageByInfo(messageType, ref data, headerline,
                                           contentLength, gid, ipAddress, ref leftMessage);
            }

            return message;
        }

        /// <summary>
        /// 根据报文的各种信息初始化一个报文实例
        /// </summary>
        /// <param name="messageType">报文种类</param>
        /// <param name="data">从报头开始的数据</param>
        /// <param name="headerline">请求头文本内容</param>
        /// <param name="contentLength">请求行中标注的数据长度</param>
        /// <param name="gid">项目号</param>
        /// <param name="ipAddress">IP地址(包含Port)</param>
        /// <param name="leftMessage">剩余的数据</param>
        /// <returns>报文实例，如果当前报文格式不正确，则返回null</returns>
        private static MessageContent GetMessageByInfo(MessageType messageType, ref byte[] data, string headerline, int contentLength, string gid, string ipAddress, ref byte[] leftMessage)
        {
            var message = new MessageContent();

            message.Type = messageType;

            // 算出报头长度
            var headerLength = Encoding.UTF8.GetBytes(headerline).Length;

            message.MessageLength = headerLength + contentLength;

            // 如果不是HTTP报文，还会在报文最后加上回车换行两个符号，所以总长度要加2
            if (messageType != MessageType.Http && messageType != MessageType.HeartBeat && contentLength != 0)
            {
                message.MessageLength += 2;
            }

            // 当前数据长度不够
            if (message.MessageLength > data.Length)
            {
                leftMessage = data;
                return null;
            }

            byte[] contentData = null;

            // 对于具体的报文种类，要具体处理
            switch (messageType)
            {
                case MessageType.Login:

                    if (gid == "")
                    {
                        message = null;
                    }
                    else
                    {
                        message.GID = gid;
                    }

                    break;

                case MessageType.Client:
                case MessageType.Server:
                    // 必须携带IP地址和端口号
                    var destIPAndPort = GetEndPointFromString(ipAddress);

                    if (destIPAndPort.Invalid)
                    {
                        message = null;
                    }
                    else
                    {
                        message.IPAndPort = destIPAndPort;
                    }

                    break;

                case MessageType.HeartBeat:
                case MessageType.Http:
                    message.GID = gid;
                    break;
                default:
                    message = null;
                    break;
            }

            if (message != null)
            {
                if (messageType == MessageType.Http)
                {
                    message.DataLength = message.MessageLength;
                    // 添加要转发的报文内容
                    contentData = new byte[message.MessageLength];

                    Array.Copy(data, contentData, message.MessageLength);
                    message.Data = contentData;
                }
                else
                {
                    message.DataLength = contentLength;
                    // 添加要转发的报文内容
                    contentData = new byte[contentLength];

                    Array.Copy(data, headerLength, contentData, 0, contentLength);
                    message.Data = contentData;
                }
                // 剩的数据里面可能含有效的报文也可能没有，
                data = data.Skip(message.MessageLength).ToArray();
            }

            return message;
        }

        /// <summary>
        /// 判断当前行是否为请求行，并且获取报文种类
        /// </summary>
        /// <param name="line"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        private static bool CheakIfHeaderLineAndGetType(string line, ref MessageType messageType)
        {
            bool headerBegin = false;
            messageType = MessageType.None;

            // 判定报文种类
            if (line.EndsWith("HTTP/1.0") || line.EndsWith("HTTP/1.1") || line.StartsWith("HTTP/1.0") || line.StartsWith("HTTP/1.1"))
            {
                headerBegin = true;
                messageType = MessageType.Http;
            }
            else
            {
                switch (line)
                {
                    case StaticResources.ClientHeaderLine:
                        headerBegin = true;
                        messageType = MessageType.Client;
                        break;

                    case StaticResources.ServerHeaderLine:
                        headerBegin = true;
                        messageType = MessageType.Server;
                        break;

                    case StaticResources.HeartBeat:
                        headerBegin = true;
                        messageType = MessageType.HeartBeat;
                        break;

                    case StaticResources.LoginHeaderLine:
                        headerBegin = true;
                        messageType = MessageType.Login;
                        break;
                }
            }

            return headerBegin;
        }

    }
}
