using System;

namespace EasySave.Command.Views;

public class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        ConsoleView view = new ConsoleView();
        view.Start();
    }
}