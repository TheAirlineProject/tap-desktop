using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Globalization;
using TheAirline.Model.GeneralModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    public class PopUpWindow : Window
    {
       

        public object Selected { get; set; }
        public PopUpWindow()
        {
            //            this.Language = XmlLanguage.GetLanguage(new CultureInfo(GameObject.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 

            this.SetResourceReference(Window.BackgroundProperty, "HeaderBackgroundBrush2");

            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            this.WindowStyle = System.Windows.WindowStyle.ToolWindow;

            this.BorderThickness = new Thickness(2, 2, 2, 2);
            this.BorderBrush = new SolidColorBrush(Colors.Black);

            this.Activated += PopUpWindow_Activated;
      }

        private void PopUpWindow_Activated(object sender, EventArgs e)
        {
            this.Height = this.Height + SystemParameters.CaptionHeight;
        }
       
      
    }

}
