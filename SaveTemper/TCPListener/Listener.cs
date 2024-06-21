// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPListener
{
    public class Listener : IDisposable
    {
        public delegate void HandleResponse(string resp, int length, byte[] data);
        Socket newsock;

        int recv;
        byte[] data = new byte[1024];

        public void Initialize(HandleResponse handler)
        {
            handler.Invoke("Initializing...", 0, null); 
            Thread.Sleep(200);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 514);
            newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            handler.Invoke("Binding port...", 0, null);
            newsock.Bind(ipep);
            Thread.Sleep(200);
        }

        public void Listen(HandleResponse handler)
        {
            bool finished = false;
            handler.Invoke("Waiting for a client...", 0, null);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            recv = newsock.ReceiveFrom(data, ref Remote);

            Console.WriteLine("Message received from {0}:", Remote.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

            handler.Invoke(Encoding.ASCII.GetString(data, 0, recv), recv, data);

            string welcome = "FE100 KeyGrabber Server";
            data = Encoding.ASCII.GetBytes(welcome);
            newsock.SendTo(data, data.Length, SocketFlags.None, Remote);            

            while (!finished)
            {                
                data = new byte[1024];
                recv = newsock.ReceiveFrom(data, ref Remote);

                //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
                string response = Encoding.ASCII.GetString(data, 0, recv);
                handler.Invoke(response, recv, data);
                newsock.SendTo(data, recv, SocketFlags.None, Remote);

                if (response.Contains("Exiting"))
                {
                    finished = true;
                }
            }

            handler.Invoke("Disconnected", 0, null);
        }

        public void Dispose()
        {
            if (newsock != null)
            {
                if (newsock.Connected)
                {
                    newsock.Disconnect(false);
                }
            }
        }

    }
}
