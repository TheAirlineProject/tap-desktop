using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpInviteAlliance.xaml
    /// </summary>
    public partial class PopUpInviteAlliance : PopUpWindow
    {
        private Alliance Alliance;
        private List<Airline> InviteAirlines;
        public static object ShowPopUp(Alliance alliance)
        {
            PopUpWindow window = new PopUpInviteAlliance(alliance);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpInviteAlliance(Alliance alliance)
        {
            this.InviteAirlines = new List<Airline>();

            this.Alliance = alliance;
            InitializeComponent();

            this.Title = this.Alliance.Name;

            this.Width = 300;

            this.Height = 500;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            StackPanel panelMain = new StackPanel();

            ScrollViewer scroller = new ScrollViewer();
            scroller.MaxHeight = this.Height - 50;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            panelMain.Children.Add(scroller);

            StackPanel panelAirlines = new StackPanel();
            scroller.Content = panelAirlines;

            var airlines = from a in Airlines.GetAllAirlines() where !this.Alliance.Members.ToList().Exists(m => m.Airline == a) select a;

            foreach (Airline airline in airlines)
            {
                WrapPanel panelAirline = new WrapPanel();
                panelAirline.Margin = new Thickness(0, 0, 0, 5);

                ContentControl ccAirline = new ContentControl();
                ccAirline.ContentTemplate = this.Resources["AirlineItem"] as DataTemplate;
                ccAirline.Content = airline;

                panelAirline.Children.Add(ccAirline);

                CheckBox cbAirline = new CheckBox();
                cbAirline.IsChecked = false;
                cbAirline.Tag = airline;
                cbAirline.VerticalAlignment = VerticalAlignment.Bottom;
                cbAirline.Checked += new RoutedEventHandler(cbAirline_Checked);
                cbAirline.Unchecked += new RoutedEventHandler(cbAirline_Unchecked);

                panelAirline.Children.Add(cbAirline);

                panelAirlines.Children.Add(panelAirline);
            }

            panelMain.Children.Add(createButtonsPanel());

            this.Content = panelMain;

        }


        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(StyleProperty, "StandardButtonStyle");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
          //  btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(StyleProperty, "StandardButtonStyle");
            btnCancel.Height = Double.NaN;
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Width = Double.NaN;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
           // btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnCancel);

            return panelButtons;
        }
        private void cbAirline_Unchecked(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((CheckBox)sender).Tag;
            this.InviteAirlines.Remove(airline);
        }

        private void cbAirline_Checked(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((CheckBox)sender).Tag;
            this.InviteAirlines.Add(airline);
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = this.InviteAirlines;
            this.Close();

        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}