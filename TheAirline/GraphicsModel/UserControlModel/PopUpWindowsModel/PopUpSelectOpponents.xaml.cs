using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpSelectOpponents.xaml
    /// </summary>
    public partial class PopUpSelectOpponents : PopUpWindow
    {
        private Airline Human;
        private int Opponents, StartYear;
        private ListBox lbSelectedAirlines, lbOpponentAirlines;
        public static object ShowPopUp(Airline human, int opponents, int startyear, Region region, Continent continent = null)
        {
            PopUpWindow window = new PopUpSelectOpponents(human, opponents, startyear,region,continent);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpSelectOpponents(Airline human, int opponents, int startyear, Region region, Continent continent)
        {
            this.Human = human;
            this.Opponents = opponents;
            this.StartYear = startyear;

            InitializeComponent();
            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpSelectOpponents", this.Uid);

            this.Width = 500;

            this.Height = 500;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel panelMain = new StackPanel();

            Grid grdMain = UICreator.CreateGrid(2);
            panelMain.Children.Add(grdMain);

            StackPanel panelSelectAirlines = new StackPanel();
            panelSelectAirlines.Margin = new Thickness(5, 0, 5, 0);

            TextBlock txtSelectedHeader = new TextBlock();
            txtSelectedHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtSelectedHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtSelectedHeader.FontWeight = FontWeights.Bold;
            txtSelectedHeader.Uid = "1001";
            txtSelectedHeader.Text = string.Format(Translator.GetInstance().GetString("PopUpSelectOpponents", txtSelectedHeader.Uid),this.Opponents);

            panelSelectAirlines.Children.Add(txtSelectedHeader);

            lbSelectedAirlines = new ListBox();
            lbSelectedAirlines.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSelectedAirlines.SetResourceReference(ListBox.ItemTemplateProperty, "AirlineLogoItem");
            lbSelectedAirlines.MaxHeight = 400;
             lbSelectedAirlines.SelectionChanged += lbSelectedAirlines_SelectionChanged;
     
           
            panelSelectAirlines.Children.Add(lbSelectedAirlines);

            Grid.SetColumn(panelSelectAirlines, 0);
            grdMain.Children.Add(panelSelectAirlines);

            StackPanel panelOpponents = new StackPanel();
            panelOpponents.Margin = new Thickness(5, 0, 5, 0);

            TextBlock txtOpponentsHeader = new TextBlock();
            txtOpponentsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtOpponentsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtOpponentsHeader.FontWeight = FontWeights.Bold;
            txtOpponentsHeader.Uid = "1002";
            txtOpponentsHeader.Text = Translator.GetInstance().GetString("PopUpSelectOpponents", txtOpponentsHeader.Uid);

            panelOpponents.Children.Add(txtOpponentsHeader);

            lbOpponentAirlines = new ListBox();
            lbOpponentAirlines.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbOpponentAirlines.SetResourceReference(ListBox.ItemTemplateProperty, "AirlineLogoItem");
            lbOpponentAirlines.MaxHeight = 400;
            lbOpponentAirlines.SelectionChanged += lbOpponentAirlines_SelectionChanged;
       
            panelOpponents.Children.Add(lbOpponentAirlines);
            
            foreach (Airline airline in Airlines.GetAirlines(a => a.Profile.Founded <= startyear && a.Profile.Folded > startyear && a != this.Human && (a.Profile.Country.Region == region || (continent != null && (continent.Uid == "100"  || continent.hasRegion(a.Profile.Country.Region))))))
                lbOpponentAirlines.Items.Add(airline);

            Grid.SetColumn(panelOpponents, 1);
            grdMain.Children.Add(panelOpponents);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Margin = new Thickness(5, 5, 0, 0);
            btnOk.Click += btnOk_Click;
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
          
            panelMain.Children.Add(btnOk);

            this.Content = panelMain;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            List<Airline> opponents = new List<Airline>();

            foreach (object airline in lbSelectedAirlines.Items)
                opponents.Add((Airline)airline);

            while (opponents.Count < this.Opponents)
            {
                Airline airline = (Airline)lbOpponentAirlines.Items[rnd.Next(lbOpponentAirlines.Items.Count)];

                lbOpponentAirlines.Items.Remove(airline);

                opponents.Add(airline);
            }
       
            this.Selected = opponents;
            this.Close();
        }

        private void lbSelectedAirlines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbSelectedAirlines.SelectedIndex > -1)
            {
                object newItem = lbSelectedAirlines.SelectedItem;
                lbOpponentAirlines.Items.Add(newItem);
                lbSelectedAirlines.Items.Remove(newItem);

                lbOpponentAirlines.Items.SortDescriptions.Add(new SortDescription("Profile.Name", ListSortDirection.Ascending));
   
                lbSelectedAirlines.SelectedIndex = -1;
                lbOpponentAirlines.SelectedIndex = -1;
            }
        }

        private void lbOpponentAirlines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbOpponentAirlines.SelectedIndex > -1 && this.Opponents > lbSelectedAirlines.Items.Count)
            {
                object newItem = lbOpponentAirlines.SelectedItem;
                lbSelectedAirlines.Items.Add(newItem);
                lbOpponentAirlines.Items.Remove(newItem);

                lbSelectedAirlines.Items.SortDescriptions.Add(new SortDescription("Profile.Name", ListSortDirection.Ascending));
   
                lbOpponentAirlines.SelectedIndex = -1;
                lbSelectedAirlines.SelectedIndex = -1;
            }
        }
    }
}
