using EasySave.Core.Models;
using EasySave.Core.Services;
using Logger.Services;
using System.Collections.ObjectModel;

public class JobViewModel
{
    // Reference to the JobManager singleton, which handles job operations
    private readonly JobManager jobManager;

    // Observable collection of messages, useful for real-time UI updates 
    public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

    /// <summary>
    /// Constructor for JobViewModel.
    /// Initializes JobManager instance and subscribes to its logging event to update messages.
    /// </summary>
    public JobViewModel()
    {
        jobManager = JobManager.Instance;
        jobManager.OnMessageLogged += (message) => Messages.Add(message);
        CryptoService.OnMessageLogged += (message) => Messages.Add(message);
    }

    /// <summary>
    /// Creates and adds a new job using the specified parameters.
    /// Uses a factory pattern to determine whether the job is Complete or Differential.
    /// </summary>
    /// <param name="name">The name of the job.</param>
    /// <param name="source">The source directory.</param>
    /// <param name="destination">The destination directory.</param>
    /// <param name="type">The type of job: 1 for Complete, 2 for Differential.</param>
    public void AddJob(string name, string source, string destination, int type)
    {
        // Factory pattern to determine the job type
        JobFactory? factory = type switch
        {
            1 => new CompleteJobFactory(),  // Full backup
            2 => new DifferentialJobFactory(), // Differential backup
            _ => null  // Invalid job type
        };

        // If the job type is invalid, display an error message and return
        if (factory == null)
        {
            Messages.Add(GetTranslation("InvalidJobType"));
            return;
        }

        // Create a new job instance using the selected factory
        Job job = factory.CreateJob(name, source, destination);

        // Add the job to JobManager for execution
        jobManager.AddJob(job);
    }

    /// <summary>
    /// Removes a job from the job list.
    /// </summary>
    /// <param name="jobName">The name of the job to remove.</param>
    public void RemoveJob(string jobName) => jobManager.RemoveJob(jobName);

    /// <summary>
    /// Updates an existing job with new parameters.
    /// </summary>
    /// <param name="jobName">The name of the job to update.</param>
    /// <param name="newName">New name for the job (optional).</param>
    /// <param name="newSource">New source path (optional).</param>
    /// <param name="newDestination">New destination path (optional).</param>
    public void UpdateJob(string jobName, string? newName = null, string? newSource = null, string? newDestination = null)
    {
        jobManager.UpdateJob(jobName, newName, newSource, newDestination);
    }

    /// <summary>
    /// Executes a single job based on its index.
    /// </summary>
    /// <param name="index">The index of the job to execute.</param>
    public void ExecuteJob(int index)
    {
        if (JobManager.IsBusinessSoftwareRunning())
        {
            Messages.Add(GetTranslation("BusinessSoftwareRunning"));
            return;
        }
        jobManager.ExecuteJob(index);
    }

    /// <summary>
    /// Executes multiple jobs sequentially.
    /// </summary>
    /// <param name="indices">An array of job indices to execute.</param>
    public void ExecuteMultipleJobs(int[] indices)
    {
        if (JobManager.IsBusinessSoftwareRunning())
        {
            Messages.Add(GetTranslation("BusinessSoftwareRunning"));
            return;
        }
        jobManager.ExecuteJobsSequentially(indices);
    }


    /// <summary>
    /// Displays a list of all jobs currently available.
    /// </summary>
    public void ListJobs() => jobManager.ListJobs();

    /// <summary>
    /// Changes the application's language and notifies the user.
    /// </summary>
    /// <param name="langCode">The new language code (e.g., "en" for English, "fr" for French).</param>
    public void ChangeLanguage(string langCode)
    {
        LanguageManager.SetLanguage(langCode);
        Messages.Add(GetTranslation("LanguageChanged"));
    }

    /// <summary>
    /// Retrieves a translated string based on the provided key.
    /// This method allows the view layer to access localized messages.
    /// </summary>
    /// <param name="key">The key of the translation string.</param>
    /// <returns>The translated string.</returns>
    public string GetTranslation(string key, params object[] args) => LanguageManager.GetString(key, args);

    public void ConfigureEncryption(bool enable, string encryptionKey, List<string> extensions)
    {
        jobManager.EncryptionSettings.EnableEncryption = enable;
        jobManager.EncryptionSettings.EncryptionKey = encryptionKey;
        jobManager.EncryptionSettings.EncryptedExtensions = extensions;
    }
    /// <summary>
    /// Déchiffre les fichiers d'un Job ou d'un chemin personnalisé.
    /// </summary>
    public void DecryptFiles(string? jobName = null, string? customPath = null)
    {
        string encryptionKey = JobManager.Instance.EncryptionSettings.EncryptionKey;

        if (!string.IsNullOrWhiteSpace(jobName))
        {
            Job? job = jobManager.SearchJob(jobName);
            if (job == null)
            {
                Messages.Add(GetTranslation("JobNotFound", jobName));
                return;
            }
            CryptoService.DecryptFilesInDirectory(job.DestinationPath, encryptionKey);
            Console.WriteLine(encryptionKey);
        }
        else if (!string.IsNullOrWhiteSpace(customPath))
        {
            CryptoService.DecryptFilesInDirectory(customPath, encryptionKey);
        }
        else
        {
            Messages.Add(GetTranslation("InvalidPath", customPath));
        }

    }
    public void SetLogFormat(string format)
    {
        ConfigManager configManager = new ConfigManager();
        configManager.SetLogFormat(format);
    }



}