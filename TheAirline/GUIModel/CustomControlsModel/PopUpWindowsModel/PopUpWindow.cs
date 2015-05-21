using System;
using System.Windows;
using System.Windows.Media;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    public class PopUpWindow : Window
    {
       

        public object Selected { get; set; }
        public PopUpWindow()
        {
            //            this.Language = XmlLanguage.GetLanguage(new CultureInfo(GameObject.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 

            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004A7F")); 
      
            this.ResizeMode = ResizeMode.NoResize;

            this.WindowStyle = WindowStyle.ToolWindow;

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
