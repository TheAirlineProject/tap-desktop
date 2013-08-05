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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAlliancesModel.PanelAlliancesModel
{
    /// <summary>
    /// Interaction logic for PanelAlliance.xaml
    /// </summary>
    public partial class PanelAlliance : Page
    {
        private Alliance Alliance;
        private StandardPage ParentPage;
        public PanelAlliance(StandardPage parent, Alliance alliance)
        {
            this.ParentPage = parent;
            this.Alliance = alliance;

            InitializeComponent();


            createPanel();
        }
        //creates the panel
        private void createPanel()
        {
            ScrollViewer scroller = new ScrollViewer();
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;
            scroller.Margin = new Thickness(0, 0, 50, 0);

            StackPanel panelAlliance = new StackPanel();
            
            WrapPanel panelHeader = new WrapPanel();
            panelAlliance.Children.Add(panelHeader);

            if (this.Alliance.Logo != null)
            {
                Image imgLogo = new Image();
                imgLogo.Source = new BitmapImage(new Uri(this.Alliance.Logo, UriKind.RelativeOrAbsolute));
                imgLogo.Width = 32;
                imgLogo.Margin = new Thickness(0, 0, 10, 0);
                imgLogo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);
                panelHeader.Children.Add(imgLogo);
            }

            TextBlock txtAirlineName = UICreator.CreateTextBlock(this.Alliance.Name);
            txtAirlineName.FontSize = 20;
            txtAirlineName.FontWeight = FontWeights.Bold;
            txtAirlineName.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelHeader.Children.Add(txtAirlineName);

            
            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["AirlinesHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.Margin = new Thickness(0, 5, 0, 0);
            panelAlliance.Children.Add(txtHeader);


            ListBox lbMembers = new ListBox();
            lbMembers.ItemTemplate = this.Resources["AirlineItem"] as DataTemplate;
            lbMembers.MaxHeight = GraphicsHelpers.GetContentHeight()/2;
            lbMembers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            List<Airline> airlines = this.Alliance.Members.Select(m=>m.Airline).ToList();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            foreach (Airline airline in airlines)
                lbMembers.Items.Add(airline);

            panelAlliance.Children.Add(lbMembers);

            ContentControl txtPendingHeader = new ContentControl();
            txtPendingHeader.ContentTemplate = this.Resources["PendingsHeader"] as DataTemplate;
            txtPendingHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtPendingHeader.Margin = new Thickness(0, 5, 0, 0);

            panelAlliance.Children.Add(txtPendingHeader);

            ListBox lbPendings = new ListBox();
            lbPendings.ItemTemplate = this.Resources["PendingMemberItem"] as DataTemplate;
            lbPendings.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;
            lbPendings.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            this.Alliance.PendingMembers.ToList().ForEach(p => lbPendings.Items.Add(p));

            panelAlliance.Children.Add(lbPendings);

            panelAlliance.Children.Add(createInfoPanel());

            Button btnMap = new Button();
            btnMap.Uid = "204";
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Height = Double.NaN;
            btnMap.Width = Double.NaN;
            btnMap.Content = Translator.GetInstance().GetString("PanelAlliance", btnMap.Uid);
            btnMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMap.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnMap.Click += new RoutedEventHandler(btnMap_Click);
            btnMap.Margin = new Thickness(0, 5, 0, 0);

            panelAlliance.Children.Add(btnMap);

            panelAlliance.Children.Add(createButtonsPanel());

            scroller.Content = panelAlliance;

            this.Content = scroller;
        }
        //creates the info panel
        private StackPanel createInfoPanel()
        {
            StackPanel panelInfo = new StackPanel();
            panelInfo.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelAlliance", "205");

            panelInfo.Children.Add(txtHeader);

            ListBox lbInfo = new ListBox();
            lbInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            panelInfo.Children.Add(lbInfo);

            ContentControl ccHeadquarter = new ContentControl();
            ccHeadquarter.SetResourceReference(ContentControl.ContentTemplateProperty, "AirportCountryLink");
            ccHeadquarter.Content = this.Alliance.Headquarter;

            int routes = this.Alliance.Members.Sum(a => a.Airline.Routes.Count);

            int destinations = this.Alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Count();
            int countries = this.Alliance.Members.SelectMany(m => m.Airline.Airports).Select(a => a.Profile.Country).Distinct().Count();
            double annualPassengers = this.Alliance.Members.Sum(m => m.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, StatisticsTypes.GetStatisticsType("Passengers")));
            int hubs = this.Alliance.Members.Sum(m => m.Airline.getHubs().Count);
            int weeklyDepartures = this.Alliance.Members.Sum(m => m.Airline.Routes.SelectMany(r => r.TimeTable.Entries).Count());

            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "211"), ccHeadquarter));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "206"), UICreator.CreateTextBlock(this.Alliance.FormationDate.Year.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "209"), UICreator.CreateTextBlock(this.Alliance.Type.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "207"), UICreator.CreateTextBlock(routes.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "208"), UICreator.CreateTextBlock(destinations.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "214"), UICreator.CreateTextBlock(countries.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "213"), UICreator.CreateTextBlock(weeklyDepartures.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAlliance", "212"), UICreator.CreateTextBlock(hubs.ToString())));
            lbInfo.Items.Add(new QuickInfoValue(string.Format(Translator.GetInstance().GetString("PanelAlliance", "210"), GameObject.GetInstance().GameTime.Year - 1), UICreator.CreateTextBlock(annualPassengers.ToString())));

            return panelInfo;
        }
        //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnJoin = new Button();
            btnJoin.Uid = "200";
            btnJoin.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnJoin.Height = Double.NaN;
            btnJoin.Width = Double.NaN;
            btnJoin.Content = Translator.GetInstance().GetString("PanelAlliance", btnJoin.Uid);
            btnJoin.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnJoin.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnJoin.Click += new RoutedEventHandler(btnJoin_Click);
            btnJoin.Visibility = this.Alliance.IsHumanAlliance ? Visibility.Collapsed : System.Windows.Visibility.Visible;

            buttonsPanel.Children.Add(btnJoin);

            Button btnInvite = new Button();
            btnInvite.Uid = "201";
            btnInvite.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnInvite.Height = Double.NaN;
            btnInvite.Width = Double.NaN;
            btnInvite.Content = Translator.GetInstance().GetString("PanelAlliance", btnInvite.Uid);
            btnInvite.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnInvite.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnInvite.Click += new RoutedEventHandler(btnInvite_Click);
            btnInvite.Visibility = this.Alliance.IsHumanAlliance ? Visibility.Visible : Visibility.Collapsed;

            buttonsPanel.Children.Add(btnInvite);

            Button btnDelete = new Button();
            btnDelete.Uid = "202";
            btnDelete.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDelete.Height = Double.NaN;
            btnDelete.Width = Double.NaN;
            btnDelete.Content = Translator.GetInstance().GetString("PanelAlliance", btnDelete.Uid);
            btnDelete.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDelete.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnDelete.Margin = new Thickness(5, 0, 0, 0);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            btnDelete.Visibility = this.Alliance.IsHumanAlliance && this.Alliance.Members.Count == 1 ? Visibility.Visible : Visibility.Collapsed;

            buttonsPanel.Children.Add(btnDelete);

            Button btnExit = new Button();
            btnExit.Uid = "203";
            btnExit.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnExit.Height = Double.NaN;
            btnExit.Width = Double.NaN;
            btnExit.Content = Translator.GetInstance().GetString("PanelAlliance", btnExit.Uid);
            btnExit.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnExit.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnExit.Margin = new Thickness(5, 0, 0, 0);
            btnExit.Visibility = this.Alliance.IsHumanAlliance && this.Alliance.Members.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            btnExit.Click += new RoutedEventHandler(btnExit_Click);

            buttonsPanel.Children.Add(btnExit);



            return buttonsPanel;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2602"), string.Format(Translator.GetInstance().GetString("MessageBox", "2602", "message"), this.Alliance.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Alliance.removeMember(GameObject.GetInstance().HumanAirline);
            }


            this.ParentPage.updatePage();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2603"), string.Format(Translator.GetInstance().GetString("MessageBox", "2603", "message"), this.Alliance.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Alliance.removeMember(GameObject.GetInstance().HumanAirline);
                Alliances.RemoveAlliance(this.Alliance);
            }

            this.ParentPage.updatePage();

        }

        private void btnInvite_Click(object sender, RoutedEventArgs e)
        {

            object o = PopUpInviteAlliance.ShowPopUp(this.Alliance);

            if (o != null)
            {
                List<Airline> airlines = (List<Airline>)o;

                foreach (Airline airline in airlines)
                {
                    if (AIHelpers.DoAcceptAllianceInvitation(airline, this.Alliance))
                    {
                        WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2605"), string.Format(Translator.GetInstance().GetString("MessageBox", "2605", "message"), airline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);
                        this.Alliance.addMember(new AllianceMember(airline,GameObject.GetInstance().GameTime));
                    }
                    else
                    {
                        WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2606"), string.Format(Translator.GetInstance().GetString("MessageBox", "2606", "message"), airline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);

                    }

                   
                }

                createPanel();

            }

        }

        private void btnJoin_Click(object sender, RoutedEventArgs e)
        {

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2601"), string.Format(Translator.GetInstance().GetString("MessageBox", "2601", "message"), this.Alliance.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (AIHelpers.CanJoinAlliance(GameObject.GetInstance().HumanAirline, this.Alliance))
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2606"), string.Format(Translator.GetInstance().GetString("MessageBox", "2606", "message"), GameObject.GetInstance().HumanAirline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);
               
                    this.Alliance.addMember(new AllianceMember(GameObject.GetInstance().HumanAirline,GameObject.GetInstance().GameTime));
                }
                else
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2607"), string.Format(Translator.GetInstance().GetString("MessageBox", "2607", "message"), GameObject.GetInstance().HumanAirline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);
               
                }
            }

            this.ParentPage.updatePage();
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {

            PopUpMap.ShowPopUp((this.Alliance.Members.SelectMany(a => a.Airline.Routes)).ToList());
        }

        private void lnkAirline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));



        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            PendingAllianceMember member = (PendingAllianceMember)((Button)sender).Tag;

            this.Alliance.addMember(new AllianceMember(member.Airline,GameObject.GetInstance().GameTime));
            this.Alliance.removePendingMember(member);

            createPanel();
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            PendingAllianceMember member = (PendingAllianceMember)((Button)sender).Tag;

            this.Alliance.removePendingMember(member);

            createPanel();
        }

    }
}
