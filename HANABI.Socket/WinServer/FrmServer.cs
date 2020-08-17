using Common;
using Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Common.Enums;
using static Common.Message.MessageContent;

namespace WinServer
{
    public partial class FrmServer : Form
    {
        // 创建一个私有变量，作为整个服务端程序的监听Sokect
        private static Socket _listener = null;
        private static List<Session> _clientSessions = new List<Session>();

        public FrmServer()
        {
            InitializeComponent();
            this.btnClose.Enabled = false;
            Console.SetOut(new ListTextWriter(this.lbLog, 3000));
        }

        private static void CloseAllSockets()
        {
            if (_clientSessions.Count > 0)
            {
                for (int count = _clientSessions.Count - 1; count >= 0; count--)
                {
                    CloseSession(_clientSessions[count]);
                }
            }

            _listener.Close();
        }

        /// <summary>
        /// 绑定端口并开始监听
        /// </summary>
        private static void BindAndStartLisen()
        {
            Console.WriteLine("正在准备监听端口");

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 设置Socket的IP和端口信息
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 100);

            // 将IP，端口信息和Socket进行绑定
            _listener.Bind(endPoint);
            // 开始监听指定端口，并指定连接队列最大长度
            _listener.Listen(100);

            Console.WriteLine("开始监听端口");
        }

        private static void StartAccept()
        {
            _listener.BeginAccept(AcceptCallBack, null);
        }

        /// <summary>
        /// 每次有新的连接到来，会进入这个函数，创建一个新的用来发送和接收的Socket
        /// </summary>
        /// <param name="AR">异步</param>
        private static void AcceptCallBack(IAsyncResult AR)
        {
            Socket socket = null;
            try
            {
                socket = _listener.EndAccept(AR);
            }
            catch (Exception)
            {

                return;
            }
            Session session = new Session
            {
                Socket = socket,
                SelfType = SessionType.Server,
                IPAndPort = new MessageIPAndPort
                {
                    IP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString(),
                    Port = ((IPEndPoint)socket.RemoteEndPoint).Port
                }
            };

            _clientSessions.Add(session);

            Console.WriteLine($"{session.AddressAndPort()}已连接");

            // 开始从连接的客户端中接收消息
            session.BeginReceive(ReceiveCallBack);

            // 开始接收新的连接
            _listener.BeginAccept(AcceptCallBack, null);
        }

        private static void ReceiveCallBack(IAsyncResult AR)
        {
            Session currentSession = AR.AsyncState as Session;
            Session responseSession = null;
            try
            {
                currentSession.PlusData(AR);
                if (currentSession.IsBeClosed)
                {
                    _clientSessions.Remove(currentSession);
                }
                else
                {
                    currentSession.HandleData();
                    // 如果要处理的消息数量大于0，进行处理
                    if (currentSession.MessagesBeSend.Count > 0)
                    {
                        foreach (var message in currentSession.MessagesBeSend)
                        {
                            // 转发给客户端
                            if (!string.IsNullOrEmpty(message.SendToGID))
                            {
                                responseSession = _clientSessions.Where(msg => msg.Gid == message.SendToGID).FirstOrDefault();
                                if (responseSession != null)
                                {
                                    responseSession.BeginSend(message.Data);
                                }
                                else
                                {
                                    CloseSession(currentSession);
                                    return;
                                }
                            }
                            else if (message.Type == MessageType.Http)
                            {
                                CloseSession(currentSession);
                                return;
                            }
                            else if (!String.IsNullOrEmpty(message.IPAndPort.IP))
                            {
                                responseSession = _clientSessions.Where(msg => msg.IPAndPort.IP == message.IPAndPort.IP &&
                                                                        msg.IPAndPort.Port == message.IPAndPort.Port)
                                                                 .FirstOrDefault();

                                if (responseSession != null)
                                {
                                    responseSession.BeginSend(message.Data);
                                    Console.WriteLine($"响应内容{message.Data.GetStringFromByteArray()}");
                                    Console.WriteLine($"响应{message.IPAndPort.IP}:{message.IPAndPort.Port}的请求成功");
                                    responseSession.Socket.Shutdown(SocketShutdown.Both);
                                    _clientSessions.Remove(responseSession);
                                }
                            }
                        }
                    }

                    currentSession.MessagesBeSend.Clear();
                    if (!currentSession.IsBeClosed)
                    {
                        currentSession.BeginReceive(ReceiveCallBack);
                    }
                }

            }
            catch (SocketException)
            {
                CloseSession(currentSession);
            }
        }

        /// <summary>
        /// 关闭并移除Session
        /// </summary>
        /// <param name="session">待处理的Session</param>
        private static void CloseSession(Session session)
        {
            session.IsBeClosed = true;
            session.Socket.Shutdown(SocketShutdown.Both);
            session.Socket.Close();
            _clientSessions.Remove(session);
        }

        #region Event

        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.btnOpen.Enabled = false;
            this.btnClose.Enabled = true;
            Task.Run(() =>
                        {
                            BindAndStartLisen();
                            StartAccept();
                        });
            Task.Run(() =>
            {
                while (true)
                {
                    var list = new List<string>();

                    var sessions = _clientSessions.Where(session => (DateTime.Now - session.HeartBeatTime).TotalSeconds > 120).ToArray();

                    if (sessions != null && sessions.Length > 0)
                    {
                        for (int count = sessions.Length - 1; count <= 0; count--)
                        {
                            CloseSession(sessions[count]);
                        }
                    }

                    if (_clientSessions != null)
                    {
                        list = _clientSessions.Where(session => !string.IsNullOrEmpty(session.Gid))
                                              .Select(session => $"{session.Gid}({session.IPAndPort.IP}:{session.IPAndPort.Port})")
                                              .ToList();
                    }

                    this.lbClients.Invoke(new Action(() =>
                    {

                        this.lbClients.Items.Clear();
                        if (list != null)
                        {
                            this.lbClients.Items.AddRange(list.ToArray());
                        }

                    }));


                    Thread.Sleep(500);
                }
            });
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.btnClose.Enabled = false;
            this.btnOpen.Enabled = true;

            CloseAllSockets();
        }

        #endregion
    }
}
