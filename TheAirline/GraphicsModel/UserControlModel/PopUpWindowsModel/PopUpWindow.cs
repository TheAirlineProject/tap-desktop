using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Globalization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    public abstract class PopUpWindow : Window
    {
        public object Selected { get; set; }
        public PopUpWindow()
        {
           // this.Background = new SolidColorBrush(Color.FromRgb(33, 59, 84));

            this.Language = XmlLanguage.GetLanguage(new CultureInfo(GameObject.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 

            this.SetResourceReference(Window.BackgroundProperty, "HeaderBackgroundBrush2");

            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            this.WindowStyle = System.Windows.WindowStyle.ToolWindow;

            

        }
     }
}
