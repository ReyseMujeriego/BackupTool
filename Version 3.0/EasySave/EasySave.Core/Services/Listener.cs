using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    public sealed class Listener : IDisposable
    {
        private bool isDisposed;
        private readonly Socket socket;

        public event EventHandler<SocketEventArgs>? ClientConnected;
        public event EventHandler<string>? LogMessage;

        public int Port { get; }

        public Listener(int port)
        {
            Port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            socket.Listen();
            LogMessage?.Invoke(this, $"En écoute sur le port {Port}");
           

            new Thread(_ => Listen()).Start();
        }

        private void Listen()
        {
            while (!isDisposed)
            {
                try
                {
                    var client = socket.Accept();
                   ClientConnected?.Invoke(this, new SocketEventArgs(client));
                }
                catch { Dispose(); }
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            socket.Dispose();
        }
    }

    public class SocketEventArgs : EventArgs
    {
        public Socket Socket { get; }

        public SocketEventArgs(Socket socket)
        {
            Socket = socket;
        }
    }
}

