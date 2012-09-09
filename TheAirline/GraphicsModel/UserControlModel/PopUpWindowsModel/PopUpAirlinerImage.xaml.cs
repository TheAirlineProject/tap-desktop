using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerImage.xaml
    /// </summary>
    public partial class PopUpAirlinerImage : PopUpWindow
    {
        private AirlinerType Type;
        //shows the pop up for an airliner
        public static void ShowPopUp(AirlinerType type)
        {
            PopUpAirlinerImage window = new PopUpAirlinerImage(type);
            window.ShowDialog();
        }
        public PopUpAirlinerImage(AirlinerType type)
        {
            this.Type = type;

            InitializeComponent();

            this.Width = 499;

            this.Height = 170;
            this.Title = type.Name;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            Image imgMap = new Image();
            imgMap.Width = 499;
            imgMap.Height = 150;
            imgMap.Source = new BitmapImage(new Uri(this.Type.Image, UriKind.RelativeOrAbsolute));
            RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

            this.Content = imgMap;
        }
    }
}
