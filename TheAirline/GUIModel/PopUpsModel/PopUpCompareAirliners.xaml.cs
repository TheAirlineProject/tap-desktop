using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for PopUpCompareAirliners.xaml
    /// </summary>
    public partial class PopUpCompareAirliners : PopUpWindow
    {
        public Airliner Airliner1 { get; set; }
        public Airliner Airliner2 { get; set; }
        public static void ShowPopUp(Airliner airliner1, Airliner airliner2)
        {
            PopUpWindow window = new PopUpCompareAirliners(airliner1,airliner2);
            window.ShowDialog();

         
        }
        public PopUpCompareAirliners(Airliner airliner1, Airliner airliner2)
        {
            this.Airliner1 = airliner1;
            this.Airliner2 = airliner2;

            InitializeComponent();
        }
    }
}
