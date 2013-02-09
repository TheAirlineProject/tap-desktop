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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageAlliancesModel.PanelAlliancesModel
{
    /// <summary>
    /// Interaction logic for PanelNewAlliance.xaml
    /// </summary>
    public partial class PanelNewAlliance : Page
    {
        private TextBox txtAllianceName;
        private ComboBox cbHeadquarter;
        private ComboBox cbType;
        private Button btnCreate;
        public PanelNewAlliance()
        {
            InitializeComponent();

            StackPanel panelNewAlliance = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelNewAlliance", "200");
            panelNewAlliance.Children.Add(txtHeader);

            ListBox lbNewAlliance = new ListBox();
            lbNewAlliance.SetResourceReference(ListBox.ItemTemplateProperty,"QuickInfoItem");
            lbNewAlliance.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            panelNewAlliance.Children.Add(lbNewAlliance);

            txtAllianceName = new TextBox();
            txtAllianceName.Background = Brushes.Transparent;
            txtAllianceName.BorderBrush = Brushes.Black;
            txtAllianceName.Width = 200;
            txtAllianceName.TextChanged += new TextChangedEventHandler(txtAllianceName_TextChanged);

            lbNewAlliance.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewAlliance", "201"), txtAllianceName));

            cbType = new ComboBox();
            cbType.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbType.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbType.ItemsSource = Enum.GetValues(typeof(Alliance.AllianceType));
            cbType.Width = 100;
            cbType.SelectedIndex = 0;

            lbNewAlliance.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewAlliance", "204"), cbType));

            cbHeadquarter = new ComboBox();
            cbHeadquarter.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbHeadquarter.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbHeadquarter.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbHeadquarter.ItemsSource = GameObject.GetInstance().HumanAirline.Airports;
            cbHeadquarter.SelectedIndex = 0;

            lbNewAlliance.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewAlliance", "202"), cbHeadquarter));

            btnCreate = new Button();
            btnCreate.Uid = "203";
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Content = Translator.GetInstance().GetString("PanelNewAlliance", btnCreate.Uid);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCreate.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCreate.Margin = new Thickness(0, 5, 0, 0);
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.IsEnabled = false;

            panelNewAlliance.Children.Add(btnCreate);


            this.Content = panelNewAlliance;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2604"), Translator.GetInstance().GetString("MessageBox", "2604", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                Alliance alliance = new Alliance(GameObject.GetInstance().GameTime,(Alliance.AllianceType)cbType.SelectedItem, txtAllianceName.Text.Trim(), (Airport)cbHeadquarter.SelectedItem);
                alliance.addMember(new AllianceMember(GameObject.GetInstance().HumanAirline,GameObject.GetInstance().GameTime));

                Alliances.AddAlliance(alliance);

                PageNavigator.NavigateTo(new PageAlliances());
            }
        }

        private void txtAllianceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            btnCreate.IsEnabled = txtAllianceName.Text.Trim().Length > 0 && !Alliances.GetAlliances().Exists(a=>a.Name.ToUpper()==txtAllianceName.Text.Trim().ToUpper());
        }
    }
}
