namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using TheAirline.Model.Services;
    using System.Collections.Generic;

    public class AppSettings
    {
        /*! private static variable basePath.
         * stores the actual defined working directory.
         */

        #region Static Fields

        private static readonly string basePath = Environment.CurrentDirectory;

        /*! private static variable dataPath.
         * stores the path to the Data directory.
         */

        private static readonly string dataPath = basePath + "\\data\\data";

        /*! private static variable basePath.
         * stores the path to the Plugin directory.
         * Plugins are always relative to the location of the exe file, so they do not use
         * the working directory as base path, but the location of the exe file as base
         */

        private static readonly string pluginsPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";

        private static AppSettings AppSettingsInstance;

        #endregion

        #region Fields

        private Boolean IsLanguageSet;

        private Language Language;

        #endregion

        #region Constructors and Destructors

        private AppSettings()
        {
            Translator.Init();
            this.setLanguage(Languages.GetLanguages()[0]);
            Translator.DefaultLanguage = Thread.CurrentThread.CurrentUICulture.ToString();

            this.IsLanguageSet = false;
        }

        #endregion

        //returns the game instance

        #region Public Methods and Operators

        public static AppSettings GetInstance()
        {
            return AppSettingsInstance == null ? new AppSettings() : AppSettingsInstance;
        }

        // simple method for initializing the Translator
        public static void Init()
        {
            GetInstance();
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

        /*! public static method getCommonApplicationData
         *  return the path to the path for saving data
         */

        public static string getCommonApplicationDataPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\theairlineproject\\";
            createPaths(path);
            return path;
        }

        public static string getDataPath()
        {
            return dataPath;
        }

        //creates all relevant paths

        /*! public static method getPluginPath.
         * returns the path to the Plugin directory.
         * \return Plugin directory path as string.
         */

        public static string getPluginPath()
        {
            return pluginsPath;
        }

        public Language getLanguage()
        {
            return this.Language;
        }

        //sets the language

        //returns if language has been set
        public Boolean hasLanguage()
        {
            return this.IsLanguageSet;
        }

        //sets the currency format
        public void setCurrencyFormat(string format)
        {
            Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol = format;
            Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencySymbol = format;
        }

        public void setLanguage(Language language)
        {
            this.Language = language;

            var ci = new CultureInfo(language.CultureInfo, true);
            //ci.NumberFormat.CurrencySymbol = "TT";
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            this.IsLanguageSet = true;
        }

        #endregion

        #region Methods

        private static void createPaths(string path)
        {
            Filesystem filesystem = new Filesystem();
            List<string> paths = new List<string> { path, path + "\\saves" };
            filesystem.CreateIfNotExists(paths);
            //Directory.CreateDirectory(path);
            //Directory.CreateDirectory(path + "\\saves");
            //LoadSaveHelpers.CreateBaseXml(path + "\\saves");
        }

        #endregion

        //returns the current language
    }
}