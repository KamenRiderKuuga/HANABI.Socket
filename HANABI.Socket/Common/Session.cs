using Common.Message;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using static Common.Enums;
using static Common.Message.MessageContent;

namespace Common
{
    public class Session
    {
        public Socket Socket { get; set; }

        private string _gid = "";

        /// <summary>
        /// 项目编号
        /// </summary>
        public string Gid
        {
            get { return _gid; }
        }

        #region 在这里包装一些Sokcet类的方法，易于理解和书写

        public IAsyncResult BeginSend(byte[] data)
        {
            return Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallBack, this);
        }

        public MessageIPAndPort IPAndPort { get; set; }

        private static void SendCallBack(IAsyncResult AR)
        {
            Session currentSession = AR.AsyncState as Session;
            currentSession.Socket.EndSend(AR);

        }

        public IAsyncResult BeginReceive(AsyncCallback callback)
        {
            return Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, callback, this);
        }

        #endregion

        public bool IsChunk { get; set; }
        public bool IsChunkFinish { get; set; }

        /// <summary>
        /// 待处理的报文列表
        /// </summary>
        private List<MessageContent> _messagesList = new List<MessageContent>();

        public List<MessageContent> MessagesList { get { return _messagesList; } }

        /// <summary>
        /// 待转发的消息
        /// </summary>
        private List<MessageContent> _messagesBeSend = new List<MessageContent>();
        public List<MessageContent> MessagesBeSend { get { return _messagesBeSend; } }

        public SessionType SelfType { get; set; }

        public string SelfGID { get; set; } = "";

        private DateTime _heartBeatTime = DateTime.Now;
        public DateTime HeartBeatTime
        {
            get
            {
                return _heartBeatTime;
            }
        }

        /// <summary>
        /// 上次没接收完，余下的数据
        /// </summary>
        private byte[] _leftMessage = null;

        public void PlusData(IAsyncResult AR)
        {
            int received = 0;

            // 获取接收到的字节长度，并保存到一个byte数组
            if (IsBeClosed)
            {
                return;
            }

            received = Socket.EndReceive(AR);

            // 若获取到的长度为0，代表当前连接已经被关闭了
            if (received == 0)
            {
                IsBeClosed = true;
                return;
            }

            byte[] dataBuf = new byte[received];
            Array.Copy(Buffer, dataBuf, received);

            if (_leftMessage != null)
            {
                dataBuf = _leftMessage.Concat(dataBuf).ToArray();
            }

            MessageContent currentMessage = new MessageContent();

            // 从数据中不断读取报文，并实例化成Message，加入列表中
            while (currentMessage != null)
            {
                currentMessage = MessageFactory.InitMessageFromData(ref dataBuf, out _leftMessage);

                if (currentMessage != null)
                {
                    _messagesList.Add(currentMessage);
                }
            }

        }

        public bool IsBeClosed { get; set; } = false;

        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        public void HandleData()
        {

            if (_messagesList.Count > 0 && SelfType == SessionType.Server)
            {
                foreach (var message in _messagesList)
                {
                    ServerResponseMessage(message);
                }
            }

            // 遍历一次之后，数据都被清空
            _messagesList.Clear();
        }

        /// <summary>
        /// 响应HTTP请求
        /// </summary>
        private void ServerResponseMessage(MessageContent message)
        {
            var ipAndPort = Socket.RemoteEndPoint as IPEndPoint;

            switch (message.Type)
            {
                case MessageType.Http:
                    // 服务器接收到HTTP请求，要转发给客户端，消息要携带消息的来源IP
                    _messagesBeSend.Add(new MessageContent
                    {
                        Type = MessageType.Http,
                        SendToGID = message.GID,
                        Data = InitMessageForForward(message.Data, new MessageIPAndPort { IP = ipAndPort.Address.ToString(), Port = ipAndPort.Port })
                    });
                    break;
                case MessageType.Client:
                    // 服务器接收到来自客户端的响应，要转发给HTTP请求方
                    if (Gid != "")
                    {
                        _messagesBeSend.Add(new MessageContent
                        {
                            IPAndPort = message.IPAndPort,
                            Data = message.Data
                        });
                    }
                    break;
                case MessageType.Login:
                    // 服务器接收到登录请求，要给当前Session的GID赋值，并返回一条登录成功的信息
                    var messagetext = "";
                    if (_gid == "")
                    {
                        messagetext = "登录成功";
                        _gid = message.GID;
                    }
                    else
                    {
                        messagetext = "登录已注销";
                        _gid = "";
                    }

                    _messagesBeSend.Add(new MessageContent
                    {
                        SendToGID = message.GID,
                        Data = InitMessageForForward(Encoding.UTF8.GetBytes(messagetext), new MessageIPAndPort())
                    });
                    break;
                case MessageType.HeartBeat:
                    // 更新心跳时间
                    _heartBeatTime = DateTime.Now;

                    if (!string.IsNullOrEmpty(Gid))
                    {
                        Console.WriteLine($"收到来自{Gid}的心跳包");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 根据当前客户端种类构建消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageIPAndPort"></param>
        /// <returns></returns>
        public byte[] InitMessageForForward(byte[] data, MessageIPAndPort messageIPAndPort)
        {
            string message = "";
            byte[] dataResult = null;

            if (SelfType == SessionType.Client)
            {
                // 构建头
                message += StaticResources.ClientHeaderLine + "\r\n";
            }

            if (SelfType == SessionType.Server)
            {
                // 构建头
                message += StaticResources.ServerHeaderLine + "\r\n";
            }

            message += "IP:";
            message += String.IsNullOrEmpty(messageIPAndPort.IP) ? StaticResources.SuccessMessageIP : messageIPAndPort.IP + ":" + messageIPAndPort.Port;
            message += "\r\n";
            message += "Content-Length:" + data.Length;
            message += "\r\n";
            message += "\r\n";

            dataResult = Encoding.UTF8.GetBytes(message);
            var suffix = Encoding.UTF8.GetBytes("\r\n");

            dataResult = dataResult.Concat(data).Concat(suffix).ToArray();

            return dataResult;
        }

        /// <summary>
        /// 每个Session自己的缓冲区
        /// </summary>
        public readonly byte[] Buffer = new byte[1024];

    }
}
