using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Core.Models;

namespace EasySave.Core.Services
{
    public class ClientManager : IDisposable
    {
        private bool isDisposed;
        private readonly Socket socket;

        public event EventHandler? Disposed;
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public ClientManager(Socket socket)
        {
            this.socket = socket;
        }

        public void Start()
        {
            new Thread(_ => Receive()).Start();
            string jobsJson = JsonSerializer.Serialize(JobManager.Instance.ListJobs(false));
            byte[] buffer = Encoding.UTF8.GetBytes(jobsJson);
            socket.Send(buffer);
        }

        private void Receive()
        {
            byte[] buffer = new byte[1024];

            while (!isDisposed)
            {
                try
                {
                    int count = socket.Receive(buffer);
                    if (count == 0)
                        Dispose();
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, count);
                        MessageReceived?.Invoke(this, new MessageEventArgs(message));
      
                    }
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
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void Send(string message)
        { 
            if (string.IsNullOrEmpty(message))
                return;

            var buffer = Encoding.UTF8.GetBytes(message);
            socket.Send(buffer);
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}

