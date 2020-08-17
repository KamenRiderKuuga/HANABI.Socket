using HZH_Controls.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using Controls;
using System.Security.Cryptography;
using System.Configuration;
using static Common.Enums;
using Common.Message;
using static Common.Message.MessageContent;
using System.Threading;

namespace WinClient
{
    public partial class FrmClient : FrmWithTitle
    {
        private static Session _clientSession = null;
        private static byte[] _buffer = new byte[1024];
        private string _gid = ConfigurationManager.AppSettings["Gid"];
        private static string _natIP = ConfigurationManager.AppSettings["NATIP"];

        private static List<Session> _httpSockets = new List<Session>();

        public FrmClient()
        {
            InitializeComponent();
            Console.SetOut(new ListTextWriter(this.lbLog, 3000));
            this.TextIP.InputText = ConfigurationManager.AppSettings["ServerIP"];
        }

        /// <summary>
        /// 连接到服务器，并开始接收消息
        /// </summary>
        private void ConnectToServer()
        {
            this.btnConnect.Invoke(new Action(() =>
            {
                this.btnConnect.BtnText = "正在尝试登录...";
            }));

            IPAddress.TryParse(this.TextIP.InputText, out var iPAddress);

            if (iPAddress == null)
            {
                Console.WriteLine("请输入有效的IP地址！");
                return;
            }

            if (!int.TryParse(this.TextPort.InputText, out var port) || port > 65535)
            {
                Console.WriteLine("请输入有效的端口号！");
            }

            string strIPAddress = this.TextIP.InputText;

            int attempts = 0;

            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSession = new Session { Socket = clientSocket, SelfType = SessionType.Client };
            while (!clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine($"第{attempts}次尝试连接 ");

                    // 连接界面上指定的服务端
                    clientSocket.Connect(iPAddress, int.Parse(this.TextPort.InputText));
                }
                catch (SocketException)
                {
                    if (attempts == 10)
                    {
                        Console.WriteLine("连接失败，请重试！");
                        this.btnConnect.Invoke(new Action(() => { this.btnConnect.BtnText = "连接至服务器"; }));
                        return;
                    }
                }
            }

            if (true)
            {
                this.btnConnect.Invoke(new Action(() =>
                {
                    this.btnConnect.BtnText = "断开连接";
                }));
            }

            Console.WriteLine("连接成功");

            // 构建登录报文
            string login = StaticResources.LoginHeaderLine + "\r\n";
            login += "Gid:" + _gid + "\r\n";
            login += "\r\n";

            var buff = Encoding.UTF8.GetBytes(login);

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        var buffHeartBeat = Encoding.UTF8.GetBytes(StaticResources.HeartBeat + "\r\n");
                        _clientSession.Socket.Send(buffHeartBeat, SocketFlags.None);
                        Console.WriteLine("发送心跳包成功！");
                        Thread.Sleep(60000);
                    }
                }
                catch (Exception)
                {
                    this.btnConnect.Invoke(new Action(() =>
                    {
                        btnConnect.BtnText = "连接至服务器";
                    }));

                    return;
                }
            });

            // 开始发送信息
            _clientSession.BeginSend(buff);
            _clientSession.BeginReceive(ReceiveCallBack);
        }

        /// <summary>
        /// 接收消息回调
        /// </summary>
        /// <param name="AR"></param>
        private static void ReceiveCallBack(IAsyncResult AR)
        {
            Session currentSession = AR.AsyncState as Session;
            try
            {
                // 获取接收到的字节长度，并保存到一个byte数组
                currentSession.PlusData(AR);

                if (!currentSession.IsBeClosed)
                {
                    HandleData(currentSession);
                }
                else
                {
                    if (_httpSockets.Contains(currentSession))
                    {
                        _httpSockets.Remove(currentSession);
                    }
                    return;
                }

                currentSession.BeginReceive(ReceiveCallBack);
            }
            catch (SocketException)
            {
                Console.WriteLine("连接已断开");
            }
        }

        private static void HandleData(Session currentSession)
        {
            foreach (var message in currentSession.MessagesList)
            {
                var ipAndPort = currentSession.Socket.RemoteEndPoint as IPEndPoint;

                switch (message.Type)
                {
                    case MessageType.Http:
                        // 客户端收到来自HTTP的响应，要返回给Server
                        _clientSession.BeginSend(_clientSession.InitMessageForForward(message.Data, new MessageIPAndPort
                        {
                            IP = currentSession.IPAndPort.IP,
                            Port = currentSession.IPAndPort.Port
                        }));

                        Console.WriteLine($"成功转发来自{((IPEndPoint)_clientSession.Socket.RemoteEndPoint).Address}的响应！");

                        //currentSession.BeginReceive(ReceiveCallBack);

                        break;
                    case MessageType.Server:

                        var text = message.Data.GetStringFromByteArray();

                        // 客户端收到来自服务器的响应，输出到界面，并判断是否发送请求给内网API
                        Console.WriteLine(text);

                        if (message.IPAndPort.IP + ":" + message.IPAndPort.Port.ToString() != StaticResources.SuccessMessageIP)
                        {
                            var httpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            var ipSplit = _natIP.Split(':');
                            var httpSession = new Session
                            {
                                Socket = httpSocket,
                                IPAndPort = new MessageIPAndPort
                                {
                                    IP = message.IPAndPort.IP,
                                    Port = message.IPAndPort.Port
                                }
                            };
                            _httpSockets.Add(httpSession);
                            try
                            {
                                httpSocket.Connect(ipSplit[0], int.Parse(ipSplit[1]));
                                httpSession.BeginSend(message.Data);
                                httpSession.BeginReceive(new AsyncCallback(ReceiveCallBack));
                            }
                            catch (Exception)
                            {

                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            currentSession.MessagesList.Clear();
        }

        #region Event

        private void btnConnect_BtnClick(object sender, EventArgs e)
        {
            switch (this.btnConnect.BtnText)
            {
                case "正在连接...":
                    break;

                case "连接至服务器":
                    Task.Run(() =>
                    {
                        ConnectToServer();
                    });
                    break;

                case "断开连接":
                    // 构建登录报文
                    string login = StaticResources.LoginHeaderLine + "\r\n";
                    login += "Gid:" + _gid + "\r\n";
                    login += "\r\n";

                    var buff = Encoding.UTF8.GetBytes(login);

                    // 开始发送信息
                    _clientSession.BeginSend(buff);

                    _clientSession.Socket.Shutdown(SocketShutdown.Both);
                    this.btnConnect.BtnText = "连接至服务器";
                    break;
            }
        }

        #endregion

    }
}
