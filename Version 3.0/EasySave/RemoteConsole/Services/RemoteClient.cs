using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    public sealed class RemoteClient : IDisposable
    {
        private Socket? socket;
        private bool isDisposed;
        public event Action<string>? MessageReceived;
        public string IP { get; }
        public int Port { get; }

        public RemoteClient(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        public void Start()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(IP), Port));
            SendLogMessage(LanguageManager.GetString("ServerConnected"));
            new Thread(_ => Receive()).Start();
        }

        private void SendLogMessage(string message)
        {
            MessageReceived?.Invoke(message);
        }
        private void Receive()
        {
            byte[] buffer = new byte[1024];

            while (!isDisposed)
            {
                try
                {
                    int count = socket.Receive(buffer);
                    if (count == 0) // Le client a fermé sa socket
                        Dispose();
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, count);
                        SendLogMessage($"{message}");
                    }
                }
                catch { Dispose(); }
            }
        }

        public void Send(string? message)
        {
            if (socket == null)
                throw new InvalidOperationException("Call Start() before Send");

            if (string.IsNullOrEmpty(message))
                return;

            var buffer = Encoding.UTF8.GetBytes(message);
            socket.Send(buffer);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            socket?.Dispose();
            socket = null;
        }

    }
}
