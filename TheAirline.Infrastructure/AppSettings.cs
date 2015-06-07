using System;
using System.Globalization;
using System.IO;
using System.Threading;
using TheAirline.Models.General;

namespace TheAirline.Infrastructure
{
    public class AppSettings
    {
        #region Constructors and Destructors

        private AppSettings()
        {
            Translator.Init();
            SetLanguage(Languages.GetLanguages()[0]);
            Translator.DefaultLanguage = Thread.CurrentThread.CurrentUICulture.ToString();

            _isLanguageSet = false;
        }

        #endregion

        #region Methods

        private static void CreatePaths(string path)
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\saves");
            //LoadSaveHelpers.CreateBaseXml(path + "\\saves");
        }

        #endregion

        #region Static Fields

        private static readonly string BasePath = Environment.CurrentDirectory;

        /*! private static variable dataPath.
         * stores the path to the Data directory.
         */

        private static readonly string DataPath = BasePath + "\\data";

        /*! private static variable basePath.
         * stores the path to the Plugin directory.
         * Plugins are always relative to the location of the exe file, so they do not use
         * the working directory as base path, but the location of the exe file as base
         */

        private static readonly string PluginsPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";

        private static AppSettings _appSettingsInstance;

        #endregion

        #region Fields

        private bool _isLanguageSet;

        private Language _language;

        #endregion

        #region Public Methods and Operators

        public static AppSettings GetInstance()
        {
            return _appSettingsInstance ?? (_appSettingsInstance = new AppSettings());
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

        public static string GetBasePath()
        {
            return BasePath;
        }

        /*! public static method getDataPath.
         * returns the path to the Data directory.
         * \return Data directory path as string.
         */

        /*! public static method getCommonApplicationData
         *  return the path to the path for saving data
         */

        public static string GetCommonApplicationDataPath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\theairlineproject\\";

            if (!Directory.Exists(path))
            {
                CreatePaths(path);
            }

            return path;
        }

        public static string GetDataPath()
        {
            return DataPath;
        }

        //creates all relevant paths

        /*! public static method getPluginPath.
         * returns the path to the Plugin directory.
         * \return Plugin directory path as string.
         */

        public static string GetPluginPath()
        {
            return PluginsPath;
        }

        public Language GetLanguage()
        {
            return _language;
        }

        //sets the language

        //returns if language has been set
        public bool HasLanguage()
        {
            return _isLanguageSet;
        }

        //sets the currency format
        public void SetCurrencyFormat(string format)
        {
            Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol = format;
            Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencySymbol = format;
        }

        public void SetLanguage(Language language)
        {
            _language = language;

            var ci = new CultureInfo(language.CultureInfo, true);
            //ci.NumberFormat.CurrencySymbol = "TT";
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            _isLanguageSet = true;
        }

        #endregion
    }
}