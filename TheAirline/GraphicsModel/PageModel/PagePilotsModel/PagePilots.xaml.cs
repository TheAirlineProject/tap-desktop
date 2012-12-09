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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel
{
    /// <summary>
    /// Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : StandardPage
    {
        public PagePilots()
        {
            InitializeComponent();

            this.Uid = "1003";
            this.Title = string.Format(Translator.GetInstance().GetString("PageRoutes", this.Uid), GameObject.GetInstance().HumanAirline.Profile.Name);

            StackPanel routesPanel = new StackPanel();
            routesPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(routesPanel, StandardContentPanel.ContentLocation.Left);


            StackPanel panelSideMenu = new StackPanel();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            
            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            showPage(this);

            showPilots();

        }
        //shows the list of pilots
        private void showPilots()
        {

        }
    }
}
