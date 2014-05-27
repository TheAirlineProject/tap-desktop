namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    //the class for a language
    public class Language
    {
        #region Fields

        private readonly Dictionary<string, string> Words;

        #endregion

        #region Constructors and Destructors

        public Language(string name, string cultureInfo, Boolean isEnabled)
        {
            this.Name = name;
            this.Unit = UnitSystem.Metric;
            this.CultureInfo = cultureInfo;
            this.IsEnabled = isEnabled;
            this.Words = new Dictionary<string, string>();
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

        //adds a word to the language

        #region Public Methods and Operators

        public void addWord(string wordOrginal, string wordLanguage)
        {
            this.Words.Add(wordOrginal, wordLanguage);
        }

        //converts a text to the language
        public string convert(string text)
        {
            return this.Words[text];
        }

        #endregion
    }

    //the collection of languages
    public class Languages
    {
        #region Static Fields

        private static Dictionary<string, Language> languages = new Dictionary<string, Language>();

        #endregion

        //clears the list of languages

        //adds a language to the collection

        #region Public Methods and Operators

        public static void AddLanguage(Language language)
        {
            if (!languages.ContainsKey(language.Name))
            {
                languages.Add(language.Name, language);
            }
        }

        public static void Clear()
        {
            languages = new Dictionary<string, Language>();
        }

        //returns a language 
        public static Language GetLanguage(string name)
        {
            if (languages.ContainsKey(name))
            {
                return languages[name];
            }
            string shortname = name.Substring(name.IndexOf("("));
            return languages.Values.ToList().Find(l => l.Name.Contains(shortname));
        }

        //returns the list of languages
        public static List<Language> GetLanguages()
        {
            return languages.Values.ToList();
        }

        #endregion
    }
}