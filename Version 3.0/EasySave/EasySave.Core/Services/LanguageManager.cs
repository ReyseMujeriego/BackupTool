using System.Globalization;
using System.Resources;

namespace EasySave.Core.Services
{
    public static class LanguageManager
    {
        // Initializes a ResourceManager to fetch localized strings from resource files
        private static ResourceManager resourceManager = new ResourceManager("EasySave.Core.Properties.Resources", typeof(LanguageManager).Assembly);

        public static event Action? LanguageChanged;

        /// <summary>
        /// Sets the application's UI culture to the specified language.
        /// </summary>
        /// <param name="cultureCode">The culture code (e.g., "en", "fr").</param>
        public static void SetLanguage(string cultureCode)
        {
            // Updates the current thread's UI culture to match the selected language
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            LanguageChanged?.Invoke();
        }

        /// <summary>
        /// Retrieves a localized string based on the given key and optional format arguments.
        /// </summary>
        /// <param name="key">The key of the resource string.</param>
        /// <param name="args">Optional arguments for string formatting.</param>
        /// <returns>The translated string, formatted with provided arguments if any.</returns>
        public static string GetString(string key, params object[] args)
        {
            // Fetches the localized string based on the current UI culture
            string value = resourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);

            // If additional arguments are provided, format the string using those arguments
            if (string.IsNullOrEmpty(value))
            {
                return $"[MISSING TRANSLATION: {key}]"; // 🔥 Affiche un message clair dans les logs
            }
            return args.Length > 0 ? string.Format(value, args) : value;

        }
    }
}