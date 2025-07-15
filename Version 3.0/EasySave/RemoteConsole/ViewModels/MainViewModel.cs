using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.RemoteConsole.Utils;

namespace EasySave.RemoteConsole.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string consoleLog = "";
        private string message;
        private readonly RemoteClient client;
        public ObservableCollection<Job> Jobs { get; } = new();
    
        public string Message
        {
            get => message;
            set
            {
                message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string ConsoleLog
        {
            get => consoleLog;
            set { consoleLog = value; OnPropertyChanged(); }
        }

        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            client = new RemoteClient("127.0.0.1", 54285);

            client.Start();
            client.MessageReceived += OnMessageReceived;

            PauseCommand = new RelayCommand(_ => SendCommand("pause"));
            ResumeCommand = new RelayCommand(_ => SendCommand("resume"));
            StopCommand = new RelayCommand(_ => SendCommand("stop"));

        
        }

        private void SendCommand(string command)
        {
            client.Send(command);
        }
        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("log:"))
            {
                string logContent = message.Substring(4);
                AppendConsoleLog($"{logContent}\n");
            }
            else
            {
                if (message.StartsWith("close"))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                }
                else
                {
                    try
                    {
                        var jobList = JsonSerializer.Deserialize<List<Job>>(message);
                        if (jobList != null)
                        {
                            UpdateJobsList(message);
                        }
                    }
                    catch (JsonException)
                    {
                        AppendConsoleLog(LanguageManager.GetString("UnknownMessage") + $"{message}");
                    }
                }
            }
        }

        private void UpdateJobsList(string message)
        {
            var jobList = JsonSerializer.Deserialize<List<Job>>(message);
            if (jobList != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Jobs.Clear();
                    foreach (var job in jobList)
                    {
                        Jobs.Add(job);
                    }
                });
            }
        }
        

        private void AppendConsoleLog(string message)
        {
            ConsoleLog += message + "\n";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}