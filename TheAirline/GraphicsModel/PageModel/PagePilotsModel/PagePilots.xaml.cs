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
using TheAirline.Model.PilotModel;
using TheAirline.GraphicsModel.PageModel.PagePilotsModel.PanelPilotsModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel
{
    /// <summary>
    /// Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : StandardPage
    {
        private ListBox lbPilots;
        private Frame panelSideMenu;
        public PagePilots()
        {
            InitializeComponent();

            this.Uid = "1003";
            this.Title = string.Format(Translator.GetInstance().GetString("PageRoutes", this.Uid), GameObject.GetInstance().HumanAirline.Profile.Name);

            StackPanel pilotsPanel = new StackPanel();
            pilotsPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(pilotsPanel, StandardContentPanel.ContentLocation.Left);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["PilotsHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            pilotsPanel.Children.Add(txtHeader);

            lbPilots = new ListBox();
            lbPilots.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPilots.ItemTemplate = this.Resources["PilotItem"] as DataTemplate;
            lbPilots.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;

            pilotsPanel.Children.Add(lbPilots);

            panelSideMenu = new Frame();
            
            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);
            
            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            showPage(this);

            showPilots();

        }
        //shows the list of pilots
        private void showPilots()
        {
            foreach (Pilot pilot in Pilots.GetUnassignedPilots())
            {
                lbPilots.Items.Add(pilot);
            }
        }

        private void lnkPilot_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Hyperlink)sender).Tag;
            panelSideMenu.Content = new PanelPilot(pilot);
          
        }
    }
}
