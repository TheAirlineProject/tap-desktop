using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    class AppSettings
    {
        private static AppSettings AppSettingsInstance;

        private Language Language;

        /*! private static variable basePath.
         * stores the actual defined working directory.
         */
        private static string basePath = Environment.CurrentDirectory;

        /*! private static variable dataPath.
         * stores the path to the Data directory.
         */
        private static string dataPath = basePath + "\\data";

        /*! private static variable basePath.
         * stores the path to the Plugin directory.
         * Plugins are always relative to the location of the exe file, so they do not use
         * the working directory as base path, but the location of the exe file as base
         */
        private static string pluginsPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";

        private AppSettings()
        {
            Translator.Init();
            this.setLanguage(Languages.GetLanguages()[0]);
            Translator.DefaultLanguage = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
        }

        //returns the game instance
        public static AppSettings GetInstance()
        {
            if (AppSettingsInstance == null)
                AppSettingsInstance = new AppSettings();
            return AppSettingsInstance;
        }

        // simple method for initializing the Translator
        public static void Init()
        {
            AppSettings.GetInstance();
        }

        /*! public static method getBasePath.
             * returns the path to the working directory.
             * \return working directory path as string.
             */
        public static string getBasePath()
        {
            return basePath;
        }

        /*! public static method getDataPath.
         * returns the path to the Data directory.
         * \return Data directory path as string.
         */
        public static string getDataPath()
        {
            return dataPath;
        }

        /*! public static method getPluginPath.
         * returns the path to the Plugin directory.
         * \return Plugin directory path as string.
         */
        public static string getPluginPath()
        {
            return pluginsPath;
        }

        //sets the language
        public void setLanguage(Language language)
        {
            this.Language = language;

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(language.CultureInfo, true);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(language.CultureInfo, true);
        }

        //returns the current language
        public Language getLanguage()
        {
            return this.Language;
        }
    }
}
