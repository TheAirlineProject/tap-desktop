using System;
using System.Collections.Generic;
using System.Linq;

namespace TheAirline.Model.GeneralModel
{
    //the class for a language
    public class Language
    {
        #region Fields

        private readonly Dictionary<string, string> _words;

        #endregion

        #region Constructors and Destructors

        public Language(string name, string cultureInfo, Boolean isEnabled)
        {
            Name = name;
            Unit = UnitSystem.Metric;
            CultureInfo = cultureInfo;
            IsEnabled = isEnabled;
            _words = new Dictionary<string, string>();
        }

        #endregion

        #region Enums

        public enum UnitSystem
        {
            Metric,

            Imperial
        }

        #endregion

        #region Public Properties

        public string CultureInfo { get; set; }

        public string ImageFile { get; set; }

        public Boolean IsEnabled { get; set; }

        public string Name { get; set; }

        public UnitSystem Unit { get; set; }

        #endregion

        #region Public Methods and Operators

        public void AddWord(string wordOrginal, string wordLanguage)
        {
            _words.Add(wordOrginal, wordLanguage);
        }

        //converts a text to the language
        public string Convert(string text)
        {
            return _words[text];
        }

        #endregion

        //adds a word to the language
    }

    //the collection of languages
    public class Languages
    {
        #region Static Fields

        private static Dictionary<string, Language> _languages = new Dictionary<string, Language>();

        #endregion

        #region Public Methods and Operators

        public static void AddLanguage(Language language)
        {
            if (!_languages.ContainsKey(language.Name))
            {
                _languages.Add(language.Name, language);
            }
        }

        public static void Clear()
        {
            _languages = new Dictionary<string, Language>();
        }

        //returns a language 
        public static Language GetLanguage(string name)
        {
            if (_languages.ContainsKey(name))
            {
                return _languages[name];
            }
            string shortname = name.Substring(name.IndexOf("(", StringComparison.Ordinal));
            return _languages.Values.ToList().Find(l => l.Name.Contains(shortname));
        }

        //returns the list of languages
        public static List<Language> GetLanguages()
        {
            return _languages.Values.ToList();
        }

        #endregion

        //clears the list of languages

        //adds a language to the collection
    }
}