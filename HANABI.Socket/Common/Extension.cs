using System;
using System.Net;
using System.Text;

namespace Common
{
    public static class Extension
    {
        #region 针对Seesion做一些扩展

        public static string AddressAndPort(this Session session)
        {
            var endPoint = (IPEndPoint)session.Socket.RemoteEndPoint;

            return $"{endPoint.Address + ":" + endPoint.Port}";
        }

        #endregion

        public static string GetStringFromByteArray(this byte[] data)
        {
            var text = Encoding.UTF8.GetString(data);

            if (text.Length > 100)
            {
                text = text.Substring(0, 100);
            }

            return text;
        }

    }
}
