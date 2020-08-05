using System.Configuration;
using System.Linq;

namespace QuestionGenerator.Library
{
    public class SettingsManager
    {
        /// <summary>
        /// Gets the value from application setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value from appsettings.</returns>
        public static string GetValueFromAppSettings(string key) => ConfigurationManager.AppSettings.AllKeys.Contains(key) ? ConfigurationManager.AppSettings[key].ToString() : null;

        /// <summary>
        /// Sets the value to application settings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetValueToAppSettings(string key, string value)
        {
            var existingValue = GetValueFromAppSettings(key);
            if (string.Equals(existingValue, value))
            {
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (existingValue == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }

            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
