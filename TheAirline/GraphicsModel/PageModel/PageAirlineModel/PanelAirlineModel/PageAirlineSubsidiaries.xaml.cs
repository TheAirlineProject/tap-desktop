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
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineSubsidiaries.xaml
    /// </summary>
    public partial class PageAirlineSubsidiaries : Page
    {
        private Airline Airline;
        private ListBox lbSubsidiaryAirline;
        private StandardPage PageParent;
        public PageAirlineSubsidiaries(Airline airline, StandardPage parent)
        {
            this.PageParent = parent;
            this.Airline = airline;

            InitializeComponent();

            StackPanel panelSubsidiaries = new StackPanel();
            panelSubsidiaries.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtSubsidiariesHeader = new TextBlock();
            txtSubsidiariesHeader.Uid = "1001";
            txtSubsidiariesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtSubsidiariesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtSubsidiariesHeader.FontWeight = FontWeights.Bold;
            txtSubsidiariesHeader.Text = Translator.GetInstance().GetString("PageAirlineSubsidiaries", txtSubsidiariesHeader.Uid);

            panelSubsidiaries.Children.Add(txtSubsidiariesHeader);

            lbSubsidiaryAirline = new ListBox();
            lbSubsidiaryAirline.ItemTemplate = this.Resources["SubsidiaryItem"] as DataTemplate;
            lbSubsidiaryAirline.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSubsidiaryAirline.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;

            showSubsidiaries();

            panelSubsidiaries.Children.Add(lbSubsidiaryAirline);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Visibility = this.Airline.IsHuman && !(this.Airline is SubsidiaryAirline) && this.Airline.Money>100000 ? Visibility.Visible : Visibility.Collapsed;
            panelButtons.Margin = new Thickness(0,5,0,0);

            panelSubsidiaries.Children.Add(panelButtons);

            Button btnCreate = new Button();
            btnCreate.Uid = "200"; 
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Content = Translator.GetInstance().GetString("PageAirlineSubsidiaries",btnCreate.Uid);
            btnCreate.Click+=new RoutedEventHandler(btnCreate_Click);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCreate);

            this.Content = panelSubsidiaries;

            

        }
        //shows the subsidiary airlines
        private void showSubsidiaries()
        {
            lbSubsidiaryAirline.Items.Clear();

            this.Airline.Subsidiaries.ForEach(s => lbSubsidiaryAirline.Items.Add(s));

        }
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)PopUpNewSubsidiary.ShowPopUp();

            if (airline != null)
            {
               AirlineHelpers.AddSubsidiaryAirline(GameObject.GetInstance().MainAirline, airline, airline.Money,airline.Airports[0]);
               airline.Airports.RemoveAt(0);
               airline.Profile.Logo = airline.Profile.Logo;
               airline.Profile.Color = airline.Profile.Color;

               this.PageParent.updatePage();

               showSubsidiaries();

               
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2111"), string.Format(Translator.GetInstance().GetString("MessageBox", "2111", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    GameObject.GetInstance().MainAirline.removeSubsidiaryAirline(airline);

                    showSubsidiaries();

                    this.PageParent.updatePage();

                }
            }
        }
    }
}
