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
using TheAirline.GraphicsModel.SkinsModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PageSettings.xaml
    /// </summary>
    public partial class PageSettings : StandardPage
    {
        private ComboBox cbSkin;
        private Slider slGameSpeed;
        private TextBlock txtGameSpeed;
        private ComboBox cbLanguage;
        private ComboBox cbTurnMinutes;
        private CheckBox cbMailOnLandings;
        private RadioButton[] rbAirportCodes;
        public PageSettings()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageSettings", this.Uid);

            StackPanel settingsPanel = new StackPanel();
            settingsPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(settingsPanel, StandardContentPanel.ContentLocation.Left);

            ListBox lbSettings = new ListBox();
            lbSettings.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSettings.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            settingsPanel.Children.Add(lbSettings);

            WrapPanel panelSpeed = new WrapPanel();

            slGameSpeed = new Slider();
            slGameSpeed.Minimum = (int)GeneralHelpers.GameSpeedValue.Fastest;
            slGameSpeed.Maximum = (int)GeneralHelpers.GameSpeedValue.Slowest;
            slGameSpeed.Width = 100;
            slGameSpeed.IsDirectionReversed = true;
            slGameSpeed.TickFrequency = 150;
            slGameSpeed.IsSnapToTickEnabled = true;
            slGameSpeed.IsMoveToPointEnabled = true;
            slGameSpeed.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slGameSpeed_ValueChanged);

             panelSpeed.Children.Add(slGameSpeed);
            
            txtGameSpeed = UICreator.CreateTextBlock(GameTimer.GetInstance().GameSpeed.ToString());
            txtGameSpeed.Margin = new Thickness(5, 0, 0, 0);
            txtGameSpeed.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelSpeed.Children.Add(txtGameSpeed);

            slGameSpeed.Value = (int)GameTimer.GetInstance().GameSpeed;
            lbSettings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSettings","1001"),panelSpeed));

            cbTurnMinutes = new ComboBox();
            cbTurnMinutes.Width = 100;
            cbTurnMinutes.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbTurnMinutes.Items.Add(15);
            cbTurnMinutes.Items.Add(30);
            cbTurnMinutes.Items.Add(60);

            cbTurnMinutes.SelectedItem = 15;

            lbSettings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSettings","1006"),cbTurnMinutes));
            

            cbLanguage = new ComboBox();
            cbLanguage.Width = 200;
            cbLanguage.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            // chs, 2011-10-11 changed to display flag together with language
            cbLanguage.ItemTemplate = this.Resources["LanguageItem"] as DataTemplate;
          
            
            foreach (Language language in Languages.GetLanguages())
                cbLanguage.Items.Add(language);

            cbLanguage.SelectedItem = AppSettings.GetInstance().getLanguage();

            lbSettings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSettings", "1002"), cbLanguage));


            cbSkin = new ComboBox();
            cbSkin.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbSkin.Width = 200;
            cbSkin.SelectedValuePath = "Name";
            cbSkin.DisplayMemberPath = "Name";

            foreach (Skin skin in Skins.GetSkins())
                cbSkin.Items.Add(skin);

            cbSkin.SelectedItem = SkinObject.GetInstance().CurrentSkin;

            lbSettings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSettings", "1003"), cbSkin));

            cbMailOnLandings = new CheckBox();
            cbMailOnLandings.IsChecked = Settings.GetInstance().MailsOnLandings;

            lbSettings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSettings", "1004"), cbMailOnLandings));


            rbAirportCodes = new RadioButton[Enum.GetValues(typeof(Settings.AirportCode)).Length];

            WrapPanel panelAirpodeCode = new WrapPanel();
            int i = 0;
            foreach (Settings.AirportCode code in Enum.GetValues(typeof(Settings.AirportCode)))
            {
                rbAirportCodes[i] = new RadioButton();
                rbAirportCodes[i].Content = code.ToString();
                rbAirportCodes[i].GroupName = "AirportCode";
                rbAirportCodes[i].Tag = code;
                rbAirportCodes[i].Margin = new Thickness(0, 0, 5, 0);
                rbAirportCodes[i].IsChecked = code == Settings.GetInstance().AirportCodeDisplay;

                panelAirpodeCode.Children.Add(rbAirportCodes[i]);
                i++;
            }
            lbSettings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSettings", "1005"), panelAirpodeCode));


            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 10, 0, 0);
            settingsPanel.Children.Add(buttonsPanel);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            //btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            Button btnUndo = new Button();
            btnUndo.Uid = "103";
            btnUndo.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnUndo.Height = Double.NaN;
            btnUndo.Margin = new Thickness(5, 0, 0, 0);
            btnUndo.Width = Double.NaN;
            btnUndo.Click += new RoutedEventHandler(btnUndo_Click);
            btnUndo.Content = Translator.GetInstance().GetString("General", btnUndo.Uid);
            btnUndo.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnUndo);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            

            showPage(this);
        }

        private void slGameSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GeneralHelpers.GameSpeedValue speed = (GeneralHelpers.GameSpeedValue)Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), (int)slGameSpeed.Value);
            txtGameSpeed.Text = speed.ToString();
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            cbSkin.SelectedItem = SkinObject.GetInstance().CurrentSkin;
            slGameSpeed.Value = (int)GameTimer.GetInstance().GameSpeed;
            cbLanguage.SelectedItem = AppSettings.GetInstance().getLanguage();
            cbMailOnLandings.IsChecked = Settings.GetInstance().MailsOnLandings;
            cbTurnMinutes.SelectedItem = Settings.GetInstance().MinutesPerTurn;
            
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Skin selectedSkin = (Skin)cbSkin.SelectedItem;

            SkinObject.GetInstance().setCurrentSkin(selectedSkin);
      
            GeneralHelpers.GameSpeedValue speed = (GeneralHelpers.GameSpeedValue)Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), (int)slGameSpeed.Value);
            GameTimer.GetInstance().setGameSpeed(speed);

            Language language = (Language)cbLanguage.SelectedItem;
            AppSettings.GetInstance().setLanguage(language);
            Settings.GetInstance().MailsOnLandings = cbMailOnLandings.IsChecked.Value;
            Settings.GetInstance().MinutesPerTurn = (int)cbTurnMinutes.SelectedItem;

           

            foreach (RadioButton rbAirportCode in rbAirportCodes)
            {
                if (rbAirportCode.IsChecked.Value)
                    Settings.GetInstance().AirportCodeDisplay = (Settings.AirportCode)rbAirportCode.Tag;
            }

          PageNavigator.NavigateTo(new PageSettings()); 
         
        }
    }
}
