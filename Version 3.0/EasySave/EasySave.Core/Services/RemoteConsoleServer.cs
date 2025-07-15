using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using EasySave.Core.Models;
using Logger.Models;

namespace EasySave.Core.Services
{
    public sealed class RemoteConsoleServer : IDisposable
    {
        private readonly Listener listener;
        private readonly List<ClientManager> clients;
        public event EventHandler<string>? LogMessage; //Message from listener to viewmodel
        public event EventHandler<string>? MessageReceived; 

        public int Port { get; }
        public static RemoteConsoleServer Instance { get; } = new RemoteConsoleServer();
        private RemoteConsoleServer() { }
        public RemoteConsoleServer(int port)
        {
            Port = port;

            clients = new List<ClientManager>();

            listener = new Listener(Port);
            listener.LogMessage += OnLogListened;
            listener.ClientConnected += Listener_ClientConnected;
            JobManager.Instance.JobAdded += (s, e) => SendJobsListToAllClients();
            JobManager.Instance.JobRemoved += (s, e) => SendJobsListToAllClients();
            JobManager.Instance.JobUpdated += (s, e) => SendJobsListToAllClients();
            JobManager.Instance.OnMessageLogged += OnLogReceived;

        }

        private void OnLogReceived(string message)
        {
            if (clients.Count == 0) return;
            foreach (var client in clients)
            {
                client.Send("log:" + message);
            }
        }
        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }
        private void OnLogListened(object? sender, string message)
        {
            LogMessage?.Invoke(this, message);
        } 
        private void Listener_ClientConnected(object? sender, SocketEventArgs e)
        {
            OnMessageReceived(LanguageManager.GetString("ClientConnected"));
            
            ClientManager client = new(e.Socket);
            client.Disposed += Client_Disposed;
            client.MessageReceived += Client_MessageReceived;

            clients.Add(client);
            client.Start();
        }

        
        private void Client_MessageReceived(object? sender, MessageEventArgs e)
        {
            string receivedMessage = e.Message;
            switch (receivedMessage.ToLower())
            {
                case "pause":
                    JobManager.Instance.PauseJob();
                    break;

                case "resume":
                    JobManager.Instance.ResumeJob();
                    break;

                case "stop":
                    JobManager.Instance.StopJob();
                    break;

                default:
                   
                    break;
            }
            
        }

        private void Client_Disposed(object? sender, EventArgs e)
        {
            if (sender is not ClientManager client)
                return;

            OnMessageReceived(LanguageManager.GetString("ClientDisconnected"));
            client.MessageReceived -= Client_MessageReceived;
            client.Disposed -= Client_Disposed;
            clients.Remove(client);
        }

        public void Start()
        {
            listener.Start();
        }

        public void SendJobsListToAllClients()
        {
            
            // Retrieve the job list from JobManager
            List<Job> jobs = JobManager.Instance.ListJobs(false);
            
                var messageContent = jobs.Select(job => new
                {
                    Name = job.Name,
                    SourcePath = job.SourcePath,
                    DestinationPath = job.DestinationPath,
                    SaveType = job.SaveType
                }).ToList();

         
            
            string jobListMessage = JsonSerializer.Serialize(messageContent, new JsonSerializerOptions { WriteIndented = true });

            foreach (var client in clients)
                
                   client.Send(jobListMessage);

   

        }

        
        public void Dispose()
        {
            listener.ClientConnected -= Listener_ClientConnected;
            listener.Dispose();

            foreach (var client in clients)
            {
                client.Send("close");
                client.Disposed -= Client_Disposed;
                client.Dispose();
            }
            clients.Clear();
        }
    }
}
