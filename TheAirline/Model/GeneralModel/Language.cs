using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a language
    public class Language
    {
        public enum UnitSystem { Metric, Imperial }
        public UnitSystem Unit { get; set; }
        public string Name { get; set; }
        private Dictionary<string, string> Words;
        public string CultureInfo { get; set; }
        public string ImageFile { get; set; }
        public Boolean IsEnabled { get; set; }
        public Language(string name,string cultureInfo, Boolean isEnabled)
        {
            this.Name = name;
            this.Unit = UnitSystem.Metric;
            this.CultureInfo = cultureInfo;
            this.IsEnabled = isEnabled;
            this.Words = new Dictionary<string, string>();
        }
        //adds a word to the language
        public void addWord(string wordOrginal, string wordLanguage)
        {
            this.Words.Add(wordOrginal, wordLanguage);
        }
        //converts a text to the language
        public string convert(string text)
        {
            return this.Words[text];
        }

    }
    //the collection of languages
    public class Languages
    {
        private static Dictionary<string, Language> languages = new Dictionary<string, Language>();
        //clears the list of languages
        public static void Clear()
        {
            languages = new Dictionary<string, Language>();
        }
        //adds a language to the collection
        public static void AddLanguage(Language language)
        {
            languages.Add(language.Name, language);
        }
        //returns a language 
        public static Language GetLanguage(string name)
        {
            if (languages.ContainsKey(name))
                return languages[name];
            else
            {
                string shortname = name.Substring(name.IndexOf("("));
                return languages.Values.ToList().Find(l => l.Name.Contains(shortname));
            }
        }
        //returns the list of languages
        public static List<Language> GetLanguages()
        {
            return languages.Values.ToList();
        }
    }
}
