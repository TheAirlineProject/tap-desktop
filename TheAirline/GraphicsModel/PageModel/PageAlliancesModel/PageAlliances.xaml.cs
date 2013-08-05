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
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAlliancesModel.PanelAlliancesModel;

namespace TheAirline.GraphicsModel.PageModel.PageAlliancesModel
{
    /// <summary>
    /// Interaction logic for PageAlliances.xaml
    /// </summary>
    public partial class PageAlliances : StandardPage
    {
        private Frame panelSideMenu;
        private ListBox lbAlliances, lbRequests;
        public PageAlliances()
        {
            InitializeComponent();

            this.Uid = "1000";
       
            //ask for alliance + _CreateDelegate alliance + statistics?

            StackPanel alliancesPanel = new StackPanel();
            alliancesPanel.Margin = new Thickness(10, 0, 10, 0);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["AlliancesHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            alliancesPanel.Children.Add(txtHeader);

            lbAlliances = new ListBox();
            lbAlliances.ItemTemplate = this.Resources["AllianceItem"] as DataTemplate;
            lbAlliances.MaxHeight = GraphicsHelpers.GetContentHeight()/2;
            lbAlliances.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            lbAlliances.ItemsSource = Alliances.GetAlliances();

            alliancesPanel.Children.Add(lbAlliances);

            alliancesPanel.Children.Add(createButtonsPanel());

            ContentControl txtRequests = new ContentControl();
            txtRequests.ContentTemplate = this.Resources["RequestsHeader"] as DataTemplate;
            txtRequests.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtRequests.Margin = new Thickness(0, 5, 0, 0);

            alliancesPanel.Children.Add(txtRequests);

            lbRequests = new ListBox();
            lbRequests.ItemTemplate = this.Resources["RequestItem"] as DataTemplate;
            lbRequests.MaxHeight = GraphicsHelpers.GetContentHeight() / 4;
            lbRequests.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            foreach (PendingAllianceMember member in from a in Alliances.GetAlliances() where a.PendingMembers.ToList().Find(p => p.Airline == GameObject.GetInstance().HumanAirline) != null select a.PendingMembers.ToList().Find(p => p.Airline == GameObject.GetInstance().HumanAirline))
                lbRequests.Items.Add(member);

            alliancesPanel.Children.Add(lbRequests);


            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(alliancesPanel, StandardContentPanel.ContentLocation.Left);
                     
            panelSideMenu = new Frame();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent("Airline Alliances");

            showPage(this);
        }
        //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnCreate = new Button();
            btnCreate.Uid = "200";
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Content = Translator.GetInstance().GetString("PageAlliances", btnCreate.Uid);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCreate.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);

            buttonsPanel.Children.Add(btnCreate);

            return buttonsPanel;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            panelSideMenu.Content = new PanelNewAlliance();
        }
      

        private void lnkAlliance_Click(object sender, RoutedEventArgs e)
        {
            Alliance alliance = (Alliance)((Hyperlink)sender).Tag;
            panelSideMenu.Content = new PanelAlliance(this,alliance);
        }
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            PendingAllianceMember pending = (PendingAllianceMember)((Button)sender).Tag;

            pending.Alliance.addMember(new AllianceMember(pending.Airline,GameObject.GetInstance().GameTime));
            pending.Alliance.removePendingMember(pending);
      
            updatePage();
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            PendingAllianceMember pending = (PendingAllianceMember)((Button)sender).Tag;

            pending.Alliance.removePendingMember(pending);
      
            updatePage();
        }

        public override void updatePage()
        {
            panelSideMenu.Content = null;

            lbAlliances.Items.Refresh();
            
            lbRequests.Items.Clear();
            foreach (PendingAllianceMember member in from a in Alliances.GetAlliances() where a.PendingMembers.ToList().Find(p => p.Airline == GameObject.GetInstance().HumanAirline) != null select a.PendingMembers.ToList().Find(p => p.Airline == GameObject.GetInstance().HumanAirline))
                lbRequests.Items.Add(member);

        }
        
    }
    //the converter for an alliances contains the human player to visibility
    public class HumanAllianceToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Alliance alliance = (Alliance)value;

            return alliance.IsHumanAlliance ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
