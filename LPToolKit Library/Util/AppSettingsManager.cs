using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Reflection;

namespace LPToolKit.Util
{
    /// <summary>
    /// Allows subclasses to load and save settings within the 
    /// app.config file via properties with using the attribute
    /// AppSettings.
    /// </summary>
    public abstract class AppSettingsManager<T> where T : AppSettingsManager<T>
    {
        public AppSettingsManager()
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var attr = prop.GetCustomAttribute<AppSettingsAttribute>();
                if (attr != null)
                {
                    AppSettingsKeys.Add(attr.Key, prop);
                    AppSettingsOptions.Add(attr.Key, attr);
                }
            }
            Load();
        }

        public Dictionary<string, PropertyInfo> AppSettingsKeys = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, AppSettingsAttribute> AppSettingsOptions = new Dictionary<string, AppSettingsAttribute>();

        public void Load()
        {
            foreach (var key in AppSettingsKeys.Keys)
            {
                var prop = AppSettingsKeys[key];
                var options = AppSettingsOptions[key];
                var value = ConfigurationManager.AppSettings[key] ?? options.Default;
                if (prop.PropertyType == typeof(int))
                {
                    int ivalue;
                    if (int.TryParse(value, out ivalue) == false)
                    {
                        ivalue = 0;
                    }
                    prop.SetValue(this, ivalue);
                }
                else if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(this, value);
                }
                else
                {
                    throw new Exception("Unsupported PropertyType = " + prop.PropertyType.ToString());
                }
            }
        }

        public void Save()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (var key in AppSettingsKeys.Keys)
            {
                var prop = AppSettingsKeys[key];
                object value = prop.GetValue(this);
                config.AppSettings.Settings.Remove(key);
                if (value != null)
                {
                    config.AppSettings.Settings.Add(key, value.ToString());
                }
            }
            config.Save(ConfigurationSaveMode.Modified);
            System.IO.File.Copy(config.FilePath, config.FilePath.Replace("vshost.", ""), true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }

    /// <summary>
    /// Attribute that declares a property to be loaded and saved to
    /// the app.config file in a subclass of AppSettingsManager.
    /// </summary>
    public class AppSettingsAttribute : Attribute
    {
        public AppSettingsAttribute(string key)
        {
            Key = key;
        }

        public string Key;

        /// <summary>
        /// The default if this appSettings key is blank
        /// </summary>
        public string Default;
    }
}
