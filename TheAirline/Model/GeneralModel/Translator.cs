namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Xml;

    // this is just a comment to test all my settings and make sure they are correct
    // ps, pjank42 if you see this, don't forget to commit this file!
    /*! Translator class.
     * This class is a singleton and is used for translating static texts.
     * The translations are loaded from a xml file and stored in a MemoryCache
     * with a Hashtable of regions. The uid per region are also stored in hashtables.
     * This will speedup the usage.
     * 
     * The translation is done by 2-dimensional keys of [region] and [uid].
     * A [region] is a page or a popup of the game. The [region] "General" is used for
     * gamewide identical translations, like "Ok", "Cancel", "Add", "Remove", aso. So
     * "Cancel" and the others are needed to be translated only once an can be used elsewhere
     * in the game.
     * [uid] is the identifier for translation. Per [region] the [uid] must be unique, for
     * different text values to translate. We use [uid] for translation, because if we would switch
     * to C# Ressource files they can be used without new creation og them.
     * 
     * TODO: Extend the translation file and make the game fully translatable. Each button and static
     *       text field needs an [uid] to be translatable. Add unique [uid] for new buttons and text
     *       fields. Add the [region] and [uid] to the language.xml file.
     * TODO2: On Game start some time usage while loading the translation file. Check to
     *       implement a precompiled binary reading, if bin-file exists. Could speed up.
     * TODO3: create LanguageEditor, for comfortable editing of the translation file.
     */

    internal class Translator
    {
        #region Constants

        private const string CACHE_KEY = "Translator";

        private const string CONFIG_FILENAME = "XmlFile";

        private const string XML_ROOTNODE = "Translator/Region";

        #endregion

        #region Static Fields

        private static MemoryCache stCache;

        #endregion

        #region Fields

        private Hashtable regions = new Hashtable();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Privater Konstruktor
        /// </summary>
        private Translator()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Standardsprache (aus Config-Datei)
        /// </summary>
        public static string DefaultLanguage { get; set; }

        /// <summary>
        ///     Physischer Pfad zur Xml-Datei (aus Config-Datei)
        /// </summary>
        public static string SourceFile
        {
            get
            {
                return AppSettings.getDataPath() + "\\Languages.xml";
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Liefert eine Instanz von Translator (Singleton)
        /// </summary>
        /// <returns></returns>
        public static Translator GetInstance()
        {
            // Erstelle Cache-Objekt wenn noch keins existiert
            if (null == stCache)
            {
                stCache = new MemoryCache(CACHE_KEY);
            }

            // Prüfe ob Cache leer ist
            if (stCache.GetCount() == 0)
            {
                // Erstelle Translator-Instanz und lade Strings
                var st = new Translator();
                st.LoadStrings();

                // Cache neu füllen
                stCache.Add(CACHE_KEY, st, new CacheItemPolicy());
            }

            // Gebe Translator-Objekt aus Cache zurück
            return (Translator)stCache[CACHE_KEY];
        }

        public static void Init()
        {
            /*            if (null != stCache)
            {
                stCache.Remove(CACHE_KEY);
                stCache = null;
            }
*/
            GetInstance();
        }

        /// <summary>
        ///     Liefert den gewünschten String abhängig vom Key in der gewünschhten Sprache
        /// </summary>
        /// <param name="region">Bestimmt den Namen der gesuchten Region</param>
        /// <param name="uid">uid des String-Eintrags</param>
        /// <param name="attribute">which attribute should be translated, by default allways attribute "name"</param>
        /// <returns>String</returns>
        public string GetString(string region, string uid, string attribute = "name")
        {
            string translationString = string.Format("{0}_{1}_{2}", region, uid, attribute);

            var rm = new ResourceManager("TheAirline.Data.languagefiles.language", Assembly.GetExecutingAssembly());

            string text = rm.GetString(translationString);

            if (text != null)
            {
                return text;
            }
            else
            {
                string culture = AppSettings.GetInstance().getLanguage().CultureInfo;

                // Lese Text mit den gegebenen Parametern region, key und language
                try
                {
                    // Prüfen ob Regions vorhanden sind
                    if (this.regions.Count > 0)
                    {
                        //  Prüfen ob angeforderte Region vorhanden ist
                        if (this.regions[region] != null)
                        {
                            if (((Hashtable)this.regions[region])[uid] != null)
                            {
                                // Prüfen ob gewählte Sprache vorhanden ist;
                                // ansonsten wird die Default-Sprache verwendet
                                //                        if (((Dictionary<string, string>)((Hashtable)regions[region])[key]).ContainsKey(language.ToString()))
                                if (((Hashtable)((Hashtable)this.regions[region])[uid]).ContainsKey(culture))
                                {
                                    text =
                                        ((Dictionary<string, string>)
                                            (((Hashtable)((Hashtable)this.regions[region])[uid])[culture]))[attribute];
                                }
                                else
                                {
                                    text =
                                        ((Dictionary<string, string>)
                                            (((Hashtable)((Hashtable)this.regions[region])[uid])[DefaultLanguage]))[
                                                attribute];
                                }

                                return text;
                            }
                            else
                            {
                                // we try it in the "General" region
                                if (((Hashtable)((Hashtable)this.regions["General"])[uid]).ContainsKey(culture))
                                {
                                    text =
                                        ((Dictionary<string, string>)
                                            (((Hashtable)((Hashtable)this.regions["General"])[uid])[culture]))[attribute
                                            ];
                                }
                                else
                                {
                                    text =
                                        ((Dictionary<string, string>)
                                            (((Hashtable)((Hashtable)this.regions["General"])[uid])[DefaultLanguage]))[
                                                attribute];
                                }

                                return text;
                            }
                        }
                        else
                        {
                            return uid;
                        }
                        // Region nicht vorhanden
                        //						throw new Exception(string.Format(
                        //							"Region {0} ist nicht vorhanden", region));
                    }
                    else
                    {
                        return uid;
                    }
                }
                    // Fehlerbehandlung
                catch (Exception ex)
                {
                    throw new Exception(
                        "Folgende Parameter ergaben kein Ergebnis im aktuellen Objekt:" + "Region: " + region + " "
                        + "Key: " + uid + " " + "Language: " + Thread.CurrentThread.CurrentUICulture.ToString()
                        + "Fehlermeldung: " + ex.Message);
                }
            }
        }

        /// <summary>
        ///     Lädt die Strings aus dem XmlFile in das Datenmodell mit den verschachtelten Dictionaries
        /// </summary>
        public void LoadStrings()
        {
            // Regions-Hashtable leeren
            this.regions.Clear();

            // XML-File laden
            XmlDocument xDoc = this.LoadSourceFile(SourceFile);

            // XML-Daten lesen
            try
            {
                //  Languages.Clear();
                // read available languages
                foreach (XmlNode LanguageNode in xDoc.SelectNodes("Translator/Languages"))
                {
                    foreach (XmlNode language in LanguageNode.ChildNodes)
                    {
                        var read = new Language(
                            language.Attributes["name"].Value,
                            language.Attributes["culture"].Value,
                            Convert.ToBoolean(language.Attributes["isEnabled"].Value));
                        // chs, 2011-10-11 changed to display flag together with language
                        read.ImageFile = AppSettings.getDataPath() + @"\graphics\flags\"
                                         + language.Attributes["flag"].Value;
                        if (language.Attributes["UnitSystem"].Value == Language.UnitSystem.Metric.ToString())
                        {
                            read.Unit = Language.UnitSystem.Metric;
                        }
                        else
                        {
                            read.Unit = Language.UnitSystem.Imperial;
                        }

                        if (language.HasChildNodes)
                        {
                            foreach (XmlNode conversion in language.ChildNodes)
                            {
                                read.addWord(
                                    conversion.Attributes["original"].Value,
                                    conversion.Attributes["translated"].Value);
                            }
                        }
                        Languages.AddLanguage(read);
                    }
                }
                /*
                // Durch Region-Nodes iterieren
				foreach(XmlNode regionNode in xDoc.SelectNodes(XML_ROOTNODE)) {

                    // we ignore comments
                    if (regionNode.NodeType == XmlNodeType.Comment)
                        continue;

                    Hashtable strs = new Hashtable();
				
					// Durch Text-Nodes in aktuellem Region-Node iterieren
					foreach(XmlNode textNode in regionNode.ChildNodes) {

                        // we ignore comments
                        if (textNode.NodeType == XmlNodeType.Comment)
                            continue;

                        // dictionary with pairs of culture->translated text for the uid element
                        Hashtable translations = new Hashtable();
                        //Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();

						// Durch Text-Elemente iterieren und diese in eine Hashtable speichern
                        foreach (XmlNode language in textNode.ChildNodes)
                        {
                            // dictionary with pairs of attribute->translated attribute text for the uid element
                            Dictionary<string, string> attributes = new Dictionary<string, string>();

                            foreach (XmlAttribute attr in language.Attributes)
                                attributes.Add(attr.Name, attr.Value);

                            translations.Add(language.Name, attributes);
                        }
					
						// StringDictionary mit Text-Elementen in übergeordnetes HashTable-Item speichern
                        strs.Add(textNode.Attributes["uid"].Value, translations);
					}

					this.regions.Add(regionNode.Attributes["name"].Value, strs);
				}*/
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim einlesen des Xml-Files" + SourceFile + ": " + ex.Message);
            }
        }

        /*
		public string GetString(string region, string key)
		{
			return GetString(region, key, DefaultLanguage);
		}
*/

        public void addTranslation(String region, string uid, XmlNode node)
        {
            Hashtable strs = null;
            // try to get the existing Hashtable for the region. If no exist, create one 
            if (this.regions[region] != null)
            {
                strs = ((Hashtable)this.regions[region]);
            }
            else
            {
                // crerate a new Hashtable for the region and add it to the cache
                strs = new Hashtable();
                this.regions.Add(region, strs);
            }

            // if an entry for this uid already exist, return directely
            if (((Hashtable)this.regions[region])[uid] != null)
            {
                return;
            }

            // dictionary with pairs of culture->translated text for the uid element
            var translations = new Hashtable();
            //Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();

            if (node.HasChildNodes)
            {
                // Durch Text-Nodes in aktuellem Region-Node iterieren
                foreach (XmlNode language in node.ChildNodes)
                {
                    // dictionary with pairs of attribute->translated attribute text for the uid element
                    var attributes = new Dictionary<string, string>();

                    foreach (XmlAttribute attr in language.Attributes)
                    {
                        attributes.Add(attr.Name, attr.Value);
                    }

                    translations.Add(language.Name, attributes);
                }
            }

            // StringDictionary mit Text-Elementen in übergeordnetes HashTable-Item speichern
            strs.Add(uid, translations);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Lädt die XML-Source-Datei in ein XmlDocument-Objekt
        /// </summary>
        /// <param name="file">Pfad zur Source-Datei</param>
        /// <returns>XmlDocument</returns>
        private XmlDocument LoadSourceFile(string file)
        {
            // Load the xml-file in a xml-document
            var xDoc = new XmlDocument();

            try
            {
                xDoc.Load(file);
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Xml-File " + file + " wurde nicht gefunden");
            }
            /*
			catch (System.Exception ex) 
			{
				throw new Exception("Allgemeiner Fehler beim Laden von "+file+": " + ex.Message);
			}*/

            return xDoc;
        }

        #endregion
    }
}