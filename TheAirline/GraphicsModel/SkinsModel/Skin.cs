using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.SkinsModel
{
    //the class for a skin
    public class Skin
    {
        public string Name { get; set; }
        public List<KeyValuePair<string, object>> Properties;
     
        public Skin(string name)
        {
            this.Name = name;
            this.Properties = new List<KeyValuePair<string,object>>();
        }
        //adds a property to the skin
        public void addProperty(KeyValuePair<string,object> property)
        {
            this.Properties.Add(property);
        }
    }

    //the collection of skins
    public class Skins
    {
        // The Skins class knows where skins are stored
        private static DirectoryInfo skinsDirectory = new DirectoryInfo(AppSettings.getDataPath() + "\\skins");
        // list of found skins
        private static List<Skin> skins = new List<Skin>();
        //clears the list
        public static void Clear()
        {
            skins = new List<Skin>();
        }
        //returns a skin based on the name
        public static Skin GetSkin(string name)
        {
            return skins.Find(delegate(Skin skin) { return skin.Name == name; });
        }
        //adds a new skin to the list
        private static void AddSkin(Skin skin)
        {
            skins.Add(skin);
        }
        //returns the list of skins
        public static List<Skin> GetSkins()
        {
            return skins;
        }
        // loads all available Skins and makes the first the default
        public static void Init()
        {
            LoadAvailableSkins();
            SkinObject.GetInstance().setCurrentSkin(Skins.GetSkins()[Skins.GetSkins().Count-1]);
        }
        // loads all skins out of the skin directory
        public static void LoadAvailableSkins()
        {
            foreach (FileInfo file in skinsDirectory.GetFiles("*.xml"))
            {
                LoadSkin(file.FullName);
            }
        }
        // load a given skin file
        private static void LoadSkin(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            string name = root.Attributes["name"].Value;

            XmlNodeList propertiesList = root.SelectNodes("//property");

            Skin skin = new Skin(name);

            foreach (XmlElement propertyElement in propertiesList)
            {
                string propertyName = propertyElement.Attributes["name"].Value;

                StringReader reader = new StringReader(propertyElement.InnerXml);
                XmlReader xmlReader = XmlReader.Create(reader);
                object o = XamlReader.Load(xmlReader);

                skin.addProperty(new KeyValuePair<string, object>(propertyName, o));
            }
            Skins.AddSkin(skin);
        }
    }

    //the skin helper class
    public class SkinObject
    {
        public Skin CurrentSkin { get; set; }
        private static SkinObject SkinInstance;
        //returns the game instance
        public static SkinObject GetInstance()
        {
            if (SkinInstance == null)
                SkinInstance = new SkinObject();
            return SkinInstance;
        }
        //sets a skin as the current skin
        public void setCurrentSkin(Skin skin)
        {
            SkinObject.GetInstance().CurrentSkin = skin;
            /*
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(skin.Background);
            img.EndInit();
             * */

            foreach (KeyValuePair<string, object> property in this.CurrentSkin.Properties)
            {
                App.Current.Resources[property.Key] = property.Value;
            }
          /*  
            App.Current.Resources["BackgroundImage"] = img;
          
            App.Current.Resources["<LinearGradientBrush StartPoint="0,0" EndPoint="0,1" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"><LinearGradientBrush.GradientStops><GradientStop Color="#FFFFD700" Offset="0.2" /><GradientStop Color="#FFFFFF00" Offset="0.85" /><GradientStop Color="#FFFFD700" Offset="1" /></LinearGradientBrush.GradientStops></LinearGradientBrush>"] = this.CurrentSkin.HeaderBackgroundColor2;
            App.Current.Resources["HeaderBackgroundBrush"] = this.CurrentSkin.HeaderBackgroundColor1;
            App.Current.Resources["ButtonBrush"] = this.CurrentSkin.ButtonBrush;
           * */
        }
    }
}
