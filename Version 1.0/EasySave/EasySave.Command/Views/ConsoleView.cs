using System;
using System.Collections.Generic;
using System.Linq;

public class ConsoleView
{
    private readonly JobViewModel jobViewModel;
    private readonly Dictionary<string, Action> menuActions;

    public ConsoleView()
    {
        jobViewModel = new JobViewModel();

        // Handles real-time message updates in the UI
        jobViewModel.Messages.CollectionChanged += (sender, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var newMessage in e.NewItems)
                {
                    Console.WriteLine(newMessage); // Prints new messages as they arrive
                }
            }
        };

        // Initializes the menu options and their corresponding methods
        menuActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            { "1", CreateAndAddJobMenu },
            { "2", RemoveJobMenu },
            { "3", UpdateJobMenu },
            { "4", () => jobViewModel.ListJobs() },
            { "5", ExecuteSingleJobMenu },
            { "6", ExecuteMultipleJobsMenu },
            { "7", SelectLanguage },
            { "0", ExitProgram }
        };
    }

    public void Start()
    {
        SelectLanguage(); // Set language before displaying menu

        while (true)
        {
            DisplayMenu();
            Console.Write(jobViewModel.GetTranslation("YourChoice"));
            string? choice = Console.ReadLine()?.Trim();

            // Executes the selected menu action if it's valid
            if (!string.IsNullOrEmpty(choice) && menuActions.TryGetValue(choice, out var action))
            {
                action.Invoke();
            }
            else
            {
                Console.WriteLine(jobViewModel.GetTranslation("InvalidChoice"));
            }
        }
    }

    // Displays the main menu options
    private void DisplayMenu()
    {
        Console.WriteLine("\n📌 " + jobViewModel.GetTranslation("MenuTitle"));
        Console.WriteLine("1️ -- " + jobViewModel.GetTranslation("CreateJob"));
        Console.WriteLine("2️ -- " + jobViewModel.GetTranslation("RemoveJob"));
        Console.WriteLine("3️ -- " + jobViewModel.GetTranslation("UpdateJob"));
        Console.WriteLine("4️ -- " + jobViewModel.GetTranslation("ListJobs"));
        Console.WriteLine("5️ -- " + jobViewModel.GetTranslation("ExecuteJob"));
        Console.WriteLine("6️ -- " + jobViewModel.GetTranslation("ExecuteMultipleJobs"));
        Console.WriteLine("7️ -- " + jobViewModel.GetTranslation("ChangeLanguage"));
        Console.WriteLine("0️ -- " + jobViewModel.GetTranslation("Exit"));
    }

    // Prompts the user for job details and adds a new job
    private void CreateAndAddJobMenu()
    {
        string name = ReadNonEmptyInput("EnterJobName");
        string source = ReadNonEmptyInput("EnterSource");
        string destination = ReadNonEmptyInput("EnterDestination");
        int type = ReadIntInput("EnterJobType");

        if (type > 2 || type == 0)
        {
            Console.WriteLine("Error : the number must be 1 or 2 !");
            return;
        }

        jobViewModel.AddJob(name, source, destination, type);
    }

    // Prompts the user for a job name and removes it
    private void RemoveJobMenu()
    {
        string jobToDelete = ReadNonEmptyInput("EnterJobNameToDelete");
        jobViewModel.RemoveJob(jobToDelete);
    }

    // Prompts the user for job update details and applies modifications
    private void UpdateJobMenu()
    {
        string oldName = ReadNonEmptyInput("EnterJobNameToUpdate");

        // Reads optional values; if input is empty, it remains null
        string? newName = ReadOptionalInput("NewNameOptional");
        string? newSource = ReadOptionalInput("NewSourceOptional");
        string? newDestination = ReadOptionalInput("NewDestinationOptional");

        jobViewModel.UpdateJob(oldName, newName, newSource, newDestination);
    }

    // Prompts the user for a job index and executes the selected job
    private void ExecuteSingleJobMenu()
    {
        int index = ReadIntInput("EnterJobIndexToExecute") - 1;
        jobViewModel.ExecuteJob(index);
    }

    // Prompts the user for multiple job indices and executes them sequentially
    private void ExecuteMultipleJobsMenu()
    {
        string input = ReadNonEmptyInput("EnterJobIndexesToExecute");

        List<int> indices = new List<int>();

        foreach (var part in input.Split(','))
        {
            if (part.Contains('-'))
            {
                var rangeParts = part.Split('-');
                if (rangeParts.Length == 2 && int.TryParse(rangeParts[0].Trim(), out int start) && int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    if (start <= end && start >= 1 && end <= 5)
                    {
                        indices.AddRange(Enumerable.Range(start, end - start + 1));
                    }
                }
            }
            else if (int.TryParse(part.Trim(), out int num) && num >= 1 && num <= 5)
            {
                indices.Add(num);
            }
        }

        indices = indices.Distinct().Select(n => n - 1).ToList();

        if (indices.Count > 0)
        {
            jobViewModel.ExecuteMultipleJobs(indices.ToArray());
        }
        else
        {
            Console.WriteLine(jobViewModel.GetTranslation("InvalidIndexes"));
        }
    }

    // Allows the user to change the language setting
    private void SelectLanguage()
    {
        string? lang;
        do
        {
            Console.WriteLine("🌍 " + jobViewModel.GetTranslation("SelectLanguage") + " (fr/en) : ");
            lang = Console.ReadLine()?.Trim().ToLower();

            if (lang != "fr" && lang != "en")
            {
                Console.WriteLine(jobViewModel.GetTranslation("UnrecognizedLanguage"));
            }
        }
        while (lang != "fr" && lang != "en"); // Keeps asking until a valid language is entered

        jobViewModel.ChangeLanguage(lang);
    }

    // Exits the application
    private void ExitProgram()
    {
        Console.WriteLine(jobViewModel.GetTranslation("Goodbye"));
        Environment.Exit(0); // Terminates the program immediately
    }

    // Reads input and ensures it is not empty or whitespace
    private string ReadNonEmptyInput(string translationKey)
    {
        string? input;
        do
        {
            Console.Write(jobViewModel.GetTranslation(translationKey));
            input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine(jobViewModel.GetTranslation("InvalidInput") + " (field cannot be empty)");
            }
        }
        while (string.IsNullOrEmpty(input));  // Keeps asking until a valid input is entered

        return input;
    }

    // Reads and validates an integer input
    private int ReadIntInput(string translationKey)
    {
        int value;
        string? input;
        do
        {
            Console.Write(jobViewModel.GetTranslation(translationKey));
            input = Console.ReadLine()?.Trim();

            if (!int.TryParse(input, out value))
            {
                Console.WriteLine(jobViewModel.GetTranslation("InvalidInput") + " (must be a number)");
            }
        }
        while (!int.TryParse(input, out value)); // Keeps asking until a valid number is entered

        return value;
    }

    // Reads optional input; returns null if the input is empty
    private string? ReadOptionalInput(string translationKey)
    {
        Console.Write(jobViewModel.GetTranslation(translationKey));
        string? input = Console.ReadLine()?.Trim();

        return string.IsNullOrEmpty(input) ? null : input;
    }
}
