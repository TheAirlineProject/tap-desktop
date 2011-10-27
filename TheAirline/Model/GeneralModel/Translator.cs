using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace TheAirline.Model.GeneralModel
{
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
    class Translator
    {
		#region Variables

		private static MemoryCache stCache;
		private Hashtable regions=new Hashtable();
		
		// Konstanten
		const string CONFIG_FILENAME = "XmlFile";
		const string CACHE_KEY = "Translator";
		const string XML_ROOTNODE = "Translator/Region";

		#endregion


		#region Property: DefaultLanguage

		/// <summary>
		/// Standardsprache (aus Config-Datei)
		/// </summary>
		public static string DefaultLanguage { get; set; }

		#endregion

		#region Property: SourceFile

		/// <summary>
		/// Physischer Pfad zur Xml-Datei (aus Config-Datei)
		/// </summary>
		public static string SourceFile
		{
			get { return Setup.getDataPath() + "\\Languages.xml"; }
		}

		#endregion


		#region Constructor

		/// <summary>
		/// Privater Konstruktor
		/// </summary>
		private Translator()
		{
		}

		#endregion


		#region Methods: GetInstance (Singleton-Pattern)

		/// <summary>
		/// Liefert eine Instanz von StringPool (Singleton)
		/// </summary>
		/// <returns></returns>
		public static Translator GetInstance() {

			// Erstelle Cache-Objekt wenn noch keins existiert
			if( null == stCache )
                stCache = new MemoryCache(CACHE_KEY);

			// Prüfe ob Cache leer ist
            if (stCache.GetCount() == 0)
            {
				// Erstelle Translator-Instanz und lade Strings
				Translator st = new Translator();
				st.LoadStrings();
				
				// Cache neu füllen
                stCache.Add(
					CACHE_KEY, st, new CacheItemPolicy());
			}

			// Gebe StringPool-Objekt aus Cache zurück
            return (Translator)stCache[CACHE_KEY];
		}

		#endregion

		#region Methods: LoadSourceFile

		/// <summary>
		/// Lädt die XML-Source-Datei in ein XmlDocument-Objekt
		/// </summary>
		/// <param name="file">Pfad zur Source-Datei</param>
		/// <returns>XmlDocument</returns>
		private XmlDocument LoadSourceFile(string file)
		{
			// Load the xml-file in a xml-document
			XmlDocument xDoc = new XmlDocument();
			
			try 
			{
				xDoc.Load(file);
			} 
			catch (System.IO.FileNotFoundException) 
			{ 
				throw new Exception("Xml-File "+file+" wurde nicht gefunden");
			} 
			catch (System.Exception ex) 
			{
				throw new Exception("Allgemeiner Fehler beim Laden von "+file+": " + ex.Message);
			}

			return xDoc;
		}

		#endregion

		#region Methods: LoadStrings

		/// <summary>
		/// Lädt die Strings aus dem XmlFile in das Datenmodell mit den verschachtelten Dictionaries
		/// </summary>
		public void LoadStrings() 
		{
			// Regions-Hashtable leeren
			regions.Clear();

			// XML-File laden
			XmlDocument xDoc = LoadSourceFile(SourceFile);
			
			// XML-Daten lesen
			try {
				// Durch Region-Nodes iterieren
				foreach(XmlNode regionNode in xDoc.SelectNodes(XML_ROOTNODE)) {

					Hashtable strs = new Hashtable();
				
					// Durch Text-Nodes in aktuellem Region-Node iterieren
					foreach(XmlNode textNode in regionNode.ChildNodes) {
					
						Dictionary<string, string> texts = new Dictionary<string, string>();

						// Durch Text-Elemente iterieren und diese in eine Hashtable speichern
						foreach(XmlNode txt in textNode.ChildNodes)
							texts.Add(txt.Name, txt.InnerText);
					
						// StringDictionary mit Text-Elementen in übergeordnetes HashTable-Item speichern
						strs.Add(textNode.Attributes["uid"].Value, texts);
					}

					this.regions.Add(regionNode.Attributes["name"].Value, strs);
				}
			} catch (System.Exception ex) {
				throw new Exception("Fehler beim einlesen des Xml-Files"+SourceFile+": " + ex.Message);
			}
		}

		#endregion

		#region Methods: GetString

		/// <summary>
		/// Liefert den gewünschten String abhängig vom Key in der gewünschhten Sprache
		/// </summary>
		/// <param name="region">Bestimmt den Namen der gesuchten Region</param>
		/// <param name="key">Key des String-Eintrags</param>
		/// <param name="language">Sprache</param>
		/// <returns>String</returns>
		public string GetString(string region, string uid) 
		{
			string text = null;

			// Lese Text mit den gegebenen Parametern region, key und language
			try
			{
				// Prüfen ob Regions vorhanden sind
				if(regions.Count > 0)
				{
					//  Prüfen ob angeforderte Region vorhanden ist
                    if (regions[region] != null)
                    {
                        if ( ((Hashtable)regions[region])[uid] != null )
                        {
                            // Prüfen ob gewählte Sprache vorhanden ist;
                            // ansonsten wird die Default-Sprache verwendet
                            //                        if (((Dictionary<string, string>)((Hashtable)regions[region])[key]).ContainsKey(language.ToString()))
                            if (((Dictionary<string, string>)((Hashtable)regions[region])[uid]).ContainsKey(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()))
                                text = ((Dictionary<string, string>)((Hashtable)regions[region])[uid])[System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()];
                            else
                                text = ((Dictionary<string, string>)((Hashtable)regions[region])[uid])[DefaultLanguage];

                            return text;
                        }
                        else
                        {
                            // we try it in the "General" region
                            if (((Dictionary<string, string>)((Hashtable)regions["General"])[uid]).ContainsKey(System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()))
                                text = ((Dictionary<string, string>)((Hashtable)regions["General"])[uid])[System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()];
                            else
                                text = ((Dictionary<string, string>)((Hashtable)regions["General"])[uid])[DefaultLanguage];

                            return text;
                        }
                    }
                    else
						// Region nicht vorhanden
						throw new Exception(string.Format(
							"Region {0} ist nicht vorhanden", region));
				} else
					return "NaN";
			}
			// Fehlerbehandlung
			catch (System.Exception ex)
			{
				throw new Exception("Folgende Parameter ergaben kein Ergebnis im aktuellen Objekt:" +
					"Region: " + region + " " +
					"Key: " + uid  + " " +
                    "Language: " + System.Threading.Thread.CurrentThread.CurrentUICulture.ToString() +
					"Fehlermeldung: " + ex.Message);
			}
			
		}
/*
		public string GetString(string region, string key)
		{
			return GetString(region, key, DefaultLanguage);
		}
*/
		#endregion
    }
}
