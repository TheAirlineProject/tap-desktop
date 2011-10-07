using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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
        private static List<Skin> skins = new List<Skin>();
        //clears the list
        public static void Clear()
        {
            skins = new List<Skin>();
        }
        //adds a new skin to the list
        public static void AddSkin(Skin skin)
        {
            skins.Add(skin);
        }
        //returns the list of skins
        public static List<Skin> GetSkins()
        {
            return skins;
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
