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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpBuildRunway.xaml
    /// </summary>
    public partial class PopUpBuildRunway : PopUpWindow
    {
        private Airport Airport;
        private Button btnOk;
        private ComboBox cbSurface, cbLenght;
        private TextBox txtName;
        public static object ShowPopUp(Airport airport)
        {
            PopUpWindow window = new PopUpBuildRunway(airport);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpBuildRunway(Airport airport)
        {
            this.Airport = airport;
            InitializeComponent();

            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpBuildRunway", this.Uid);

            this.Width = 400;

            this.Height = 210;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            mainPanel.Children.Add(lbContent);

            txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.TextChanged += new TextChangedEventHandler(txtName_TextChanged);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpBuildRunway", "1001"), txtName));

            cbSurface = new ComboBox();
            cbSurface.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbSurface.Width = 100;
            cbSurface.SelectedIndex = 0;

            foreach (Runway.SurfaceType surface in this.Airport.Runways.Select(r => r.Surface).Distinct())
                cbSurface.Items.Add(surface);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpBuildRunway", "1002"), cbSurface));

            cbLenght = new ComboBox();
            cbLenght.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbLenght.Width = 100;
            //cbLenght.ItemStringFormat = new NumberMeterToUnitConverter().Convert("{0}").ToString();
            cbLenght.SelectedIndex = 0;

            for (int i = 1500; i < 4500; i += 250)
                cbLenght.Items.Add(i);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpBuildRunway", "1003"), cbLenght));

            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
        }

       
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.IsEnabled = false;
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);

            return panelButtons;
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            Runway.SurfaceType type = (Runway.SurfaceType)cbSurface.SelectedItem;
            int lenght = Convert.ToInt32(cbLenght.SelectedItem);

            Runway runway = new Runway(name,lenght,type,GameObject.GetInstance().GameTime.AddDays(90),false);
            this.Selected = runway;
            this.Close();

        }
        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOk.IsEnabled = txtName.Text.Trim().Length > 2;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}
