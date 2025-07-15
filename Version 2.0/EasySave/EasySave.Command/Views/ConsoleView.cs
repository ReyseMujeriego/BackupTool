using EasySave.Core.Services;

public class ConsoleView
{
    private readonly JobViewModel jobViewModel;
    private readonly Dictionary<string, Action> menuActions;

    public ConsoleView()
    {
        jobViewModel = new JobViewModel();

        // Gère l'affichage en temps réel des messages
        jobViewModel.Messages.CollectionChanged += (sender, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (var newMessage in e.NewItems)
                {
                    Console.WriteLine(newMessage);
                }
            }
        };
        ///<summary>
        /// Initialize the menu for the affiliated actions
        /// </summary>
        menuActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            { "1", CreateAndAddJobMenu },
            { "2", RemoveJobMenu },
            { "3", UpdateJobMenu },
            { "4", () => jobViewModel.ListJobs() },
            { "5", ExecuteSingleJobMenu },
            { "6", ExecuteMultipleJobsMenu },
            { "7", ConfigureEncryption },
            { "8", DecryptFilesMenu },
            { "9", SelectLanguage },
            { "10", ConfigureLogFormat },
            { "0", ExitProgram }
        };
    }

    /// <summary>
    /// Starting method
    /// </summary>
    public void Start()
    {
        SelectLanguage();
        ConfigureEncryption();
        ConfigureLogFormat();
        while (true)
        {
            DisplayMenu();
            Console.Write(jobViewModel.GetTranslation("YourChoice"));
            string? choice = Console.ReadLine()?.Trim();

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

    /// <summary>
    /// Displays the menu options to the user.
    /// </summary>
    private void DisplayMenu()
    {
        Console.WriteLine("\n📌 " + jobViewModel.GetTranslation("MenuTitle"));
        Console.WriteLine("1️ -- " + jobViewModel.GetTranslation("CreateJob"));
        Console.WriteLine("2️ -- " + jobViewModel.GetTranslation("RemoveJob"));
        Console.WriteLine("3️ -- " + jobViewModel.GetTranslation("UpdateJob"));
        Console.WriteLine("4️ -- " + jobViewModel.GetTranslation("ListJobs"));
        Console.WriteLine("5️ -- " + jobViewModel.GetTranslation("ExecuteJob"));
        Console.WriteLine("6️ -- " + jobViewModel.GetTranslation("ExecuteMultipleJobs"));
        Console.WriteLine("7️ -- " + jobViewModel.GetTranslation("ConfigureEncryption"));
        Console.WriteLine("8  -- " + jobViewModel.GetTranslation("DecryptFiles"));
        Console.WriteLine("9  -- " + jobViewModel.GetTranslation("ChangeLanguage"));
        Console.WriteLine("10  -- " + jobViewModel.GetTranslation("ChangeLogsFormat"));
        Console.WriteLine("0️ -- " + jobViewModel.GetTranslation("Exit"));
    }

    /// <summary>
    /// Displays the menu to create a job
    /// </summary>
    private void CreateAndAddJobMenu()
    {
        string name = ReadValidatedString("EnterJobName");
        string source = ReadValidatedString("EnterSource");
        string destination = ReadValidatedString("EnterDestination");
        int type = ReadValidatedInt("EnterJobType", new List<int> { 1, 2 });

        jobViewModel.AddJob(name, source, destination, type);
    }

    /// <summary>
    /// Display the menu to remove a job.
    /// </summary>
    private void RemoveJobMenu()
    {
        string jobToDelete = ReadValidatedString("EnterJobNameToDelete");
        jobViewModel.RemoveJob(jobToDelete);
    }

    /// <summary>
    /// Displays the menu to update a job
    /// </summary>
    private void UpdateJobMenu()
    {
        string oldName = ReadValidatedString("EnterJobNameToUpdate");
        string? newName = ReadOptionalInput("NewNameOptional");
        string? newSource = ReadOptionalInput("NewSourceOptional");
        string? newDestination = ReadOptionalInput("NewDestinationOptional");

        jobViewModel.UpdateJob(oldName, newName, newSource, newDestination);
    }

    /// <summary>
    /// Displays the menu to ask what job to execute
    /// </summary>
    private void ExecuteSingleJobMenu()
    {
        int index = ReadValidatedInt("EnterJobIndexToExecute", minValue: 1) - 1;
        jobViewModel.ExecuteJob(index);
    }

    /// <summary>
    /// Displays the menu to ask what indexes the user wants to execute 
    /// </summary>
    private void ExecuteMultipleJobsMenu()
    {
        string input = ReadValidatedString("EnterJobIndexesToExecute");
        List<int> indices = ParseJobIndices(input);

        if (indices.Count > 0)
        {
            jobViewModel.ExecuteMultipleJobs(indices.ToArray());
        }
        else
        {
            Console.WriteLine(jobViewModel.GetTranslation("InvalidIndexes"));
        }
    }

    /// <summary>
    /// Displays the menu to ask what language to use (french or english)
    /// </summary>
    private void SelectLanguage()
    {
        string lang;
        do
        {
            Console.WriteLine("🌍 " + jobViewModel.GetTranslation("SelectLanguage") + " (fr/en) : ");
            lang = Console.ReadLine()?.Trim().ToLower() ?? "";
        }
        while (lang != "fr" && lang != "en");

        jobViewModel.ChangeLanguage(lang);
    }

    /// <summary>
    /// Exits the program
    /// </summary>
    private void ExitProgram()
    {
        Console.WriteLine(jobViewModel.GetTranslation("Goodbye"));
        Environment.Exit(0);
    }

    /// <summary>
    /// Reads and validates string input from the user
    /// </summary>
    private string ReadValidatedString(string translationKey, Func<string, bool>? validationFunc = null)
    {
        string? input;
        do
        {
            Console.Write(jobViewModel.GetTranslation(translationKey));
            input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || (validationFunc != null && !validationFunc(input)))
            {
                Console.WriteLine(jobViewModel.GetTranslation("InvalidInput"));
            }
        }
        while (string.IsNullOrEmpty(input) || (validationFunc != null && !validationFunc(input)));

        return input;
    }

    /// <summary>
    /// Reads and validates int input from the user
    /// </summary>
    private int ReadValidatedInt(string translationKey, List<int>? validValues = null, int? minValue = null, int? maxValue = null)
    {
        int value;
        string? input;
        do
        {
            Console.Write(jobViewModel.GetTranslation(translationKey));
            input = Console.ReadLine()?.Trim();

            if (!int.TryParse(input, out value) ||
                (validValues != null && !validValues.Contains(value)) ||
                (minValue.HasValue && value < minValue.Value) ||
                (maxValue.HasValue && value > maxValue.Value))
            {
                Console.WriteLine(jobViewModel.GetTranslation("InvalidInput"));
            }
        }
        while (!int.TryParse(input, out value) ||
               (validValues != null && !validValues.Contains(value)) ||
               (minValue.HasValue && value < minValue.Value) ||
               (maxValue.HasValue && value > maxValue.Value));

        return value;
    }

    private string? ReadOptionalInput(string translationKey)
    {
        Console.Write(jobViewModel.GetTranslation(translationKey));
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? null : input;
    }

    /// <summary>
    /// Parses job input, wich may include ranges (e.g, 1-3)
    /// </summary>
    /// <param name="input"> input of the indexes needed (e.g, 1-2)</param>
    /// <returns></returns>
    private List<int> ParseJobIndices(string input)
    {
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

        return indices.Distinct().Select(n => n - 1).ToList();
    }

    /// <summary>
    /// Configures encryption settings
    /// </summary>
    private void ConfigureEncryption()
    {
        Console.WriteLine(jobViewModel.GetTranslation("EnableEncryption"));
        string? response = Console.ReadLine()?.Trim().ToLower();
        bool enableEncryption = response == "y";

        if (!enableEncryption)
        {
            jobViewModel.ConfigureEncryption(false, "", new List<string>());
            Console.WriteLine(jobViewModel.GetTranslation("EncryptionDisabled"));
            return;
        }

        string key = ReadValidatedString("EnterEncryptionKey");
        Console.WriteLine(jobViewModel.GetTranslation("EnterExtensionsToEncrypt") + " (ex: .txt,.pdf,.docx)");
        string? input = Console.ReadLine()?.Trim();
        List<string> extensions = input?.Split(',').Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        jobViewModel.ConfigureEncryption(true, key, extensions);
        Console.WriteLine(jobViewModel.GetTranslation("EncryptionConfigured"));
    }

    /// <summary>
    /// Configures the log format (either JSON or XML)
    /// </summary>
    private void ConfigureLogFormat()
    {
        Console.WriteLine(jobViewModel.GetTranslation("EnterLogsFormat"));
        string? format;

        do
        {
            format = Console.ReadLine()?.Trim().ToLower();
            if (format != "json" && format != "xml")
            {
                Console.WriteLine(jobViewModel.GetTranslation("InvalidLogsFormat"));
            }
        } while (format != "json" && format != "xml");

        jobViewModel.SetLogFormat(format);
        Console.WriteLine(LanguageManager.GetString("LogFormatSet", format.ToUpper()));
    }

    /// <summary>
    /// Decrypts files based on configuration
    /// </summary>
    private void DecryptFilesMenu()
    {
        var options = new Dictionary<string, Action>
    {
        { "1", () => jobViewModel.DecryptFiles(ReadValidatedString("EnterJobNameToDecrypt")) },
        { "2", () => jobViewModel.DecryptFiles(customPath: ReadValidatedString("EnterPathToDecrypt")) }
    };

        Console.WriteLine("\n🔓 " + jobViewModel.GetTranslation("DecryptFilesMenu"));
        Console.WriteLine("1 -- " + jobViewModel.GetTranslation("DecryptExistingJob"));
        Console.WriteLine("2 -- " + jobViewModel.GetTranslation("DecryptCustomPath"));
        Console.WriteLine("0 -- " + jobViewModel.GetTranslation("BackToMenu"));

        string choice = ReadValidatedString("YourChoice", input => options.ContainsKey(input) || input == "0");
        if (options.ContainsKey(choice)) options[choice].Invoke();
    }
}