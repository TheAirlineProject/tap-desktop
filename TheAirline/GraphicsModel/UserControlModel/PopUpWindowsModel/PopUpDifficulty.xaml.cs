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
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpDifficulty.xaml
    /// </summary>
    public partial class PopUpDifficulty : PopUpWindow
    {
    
        private Slider slMoney, slLoan, slPassengers, slPrice, slAI, slStartData;
        private DifficultyLevel Level;
        public static object ShowPopUp(DifficultyLevel level)
        {
            PopUpWindow window = new  PopUpDifficulty(level);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpDifficulty(DifficultyLevel level)
        {
            this.Level = level;

            InitializeComponent();

            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpDifficulty", this.Uid);

            this.Width = 400;

            this.Height = 210;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            mainPanel.Children.Add(lbContent);

            DifficultyLevel easyLevel = DifficultyLevels.GetDifficultyLevel("Easy");
            DifficultyLevel normalLevel = DifficultyLevels.GetDifficultyLevel("Normal");
            DifficultyLevel hardLevel = DifficultyLevels.GetDifficultyLevel("Hard");

            slMoney = createDifficultySlider(easyLevel.MoneyLevel, normalLevel.MoneyLevel, hardLevel.MoneyLevel,level.MoneyLevel);
            slLoan = createDifficultySlider(easyLevel.LoanLevel, normalLevel.LoanLevel, hardLevel.LoanLevel,level.LoanLevel);
            slAI = createDifficultySlider(easyLevel.AILevel, normalLevel.AILevel, hardLevel.AILevel,level.AILevel);
            slPassengers = createDifficultySlider(easyLevel.PassengersLevel, normalLevel.PassengersLevel, hardLevel.PassengersLevel,level.PassengersLevel);
            slPrice = createDifficultySlider(easyLevel.PriceLevel, normalLevel.PriceLevel, hardLevel.PriceLevel,level.PriceLevel);
            slStartData = createDifficultySlider(easyLevel.StartDataLevel, normalLevel.StartDataLevel, hardLevel.StartDataLevel, level.StartDataLevel);

            lbContent.Items.Add(new QuickInfoValue("", createIndicator()));
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpDifficulty","200"), slMoney));
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpDifficulty", "201"), slPrice));
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpDifficulty", "202"), slLoan));
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpDifficulty", "203"), slPassengers));
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpDifficulty", "204"), slAI));
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpDifficulty","205"),slStartData));

            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
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
        //create the slider indicator
        private Grid createIndicator()
        {
            Grid grdIndicator = UICreator.CreateGrid(3);
            grdIndicator.Width = 200;

            TextBlock txtEasy = UICreator.CreateTextBlock(DifficultyLevels.GetDifficultyLevel("Easy").Name);
            TextBlock txtNormal = UICreator.CreateTextBlock(DifficultyLevels.GetDifficultyLevel("Normal").Name);
            TextBlock txtHard = UICreator.CreateTextBlock(DifficultyLevels.GetDifficultyLevel("Hard").Name);

            Grid.SetColumn(txtEasy, 0);
            grdIndicator.Children.Add(txtEasy);

            txtNormal.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetColumn(txtNormal, 1);
            grdIndicator.Children.Add(txtNormal);

            Grid.SetColumn(txtHard, 2);
            txtHard.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            grdIndicator.Children.Add(txtHard);

            return grdIndicator;
        }
        //returns a slider
        private Slider createDifficultySlider(double easyLevel, double normalLevel, double hardLevel, double value)
        {
            Slider slDifficulty = new Slider();
            slDifficulty.Minimum = Math.Min(hardLevel, easyLevel);
            slDifficulty.Maximum = Math.Max(hardLevel, easyLevel) ;
            slDifficulty.Value = value;
            //slDifficulty.TickFrequency =(Math.Max(hardLevel,easyLevel) - Math.Min(hardLevel,easyLevel))/7;
            slDifficulty.Width = 200;
            slDifficulty.IsSnapToTickEnabled = true;
            slDifficulty.IsMoveToPointEnabled = true;
            slDifficulty.IsDirectionReversed = easyLevel > hardLevel;

            double stepValue = (normalLevel - Math.Min(hardLevel,easyLevel)) / 3;

            for (double tick = Math.Min(hardLevel, easyLevel); tick < normalLevel;tick+=stepValue )
                slDifficulty.Ticks.Add(tick);

            stepValue = (Math.Max(hardLevel, easyLevel) - normalLevel) / 3;

            for (double tick = normalLevel; tick <= Math.Max(hardLevel, easyLevel); tick += stepValue)
                slDifficulty.Ticks.Add(tick);

            if (!slDifficulty.Ticks.Contains(value))
                slDifficulty.Ticks.Add(value);
     
            return slDifficulty;

        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DifficultyLevel level = new DifficultyLevel("Custom", slMoney.Value, slLoan.Value, slPassengers.Value, slPrice.Value, slAI.Value,slStartData.Value);
            this.Selected = level;
            this.Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}
