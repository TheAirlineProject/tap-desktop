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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineWages.xaml
    /// </summary>
    public partial class PageAirlineWages : Page
    {
        private Airline Airline;
        //private Dictionary<FeeType, double> WageValues;
        private Dictionary<FeeType, double> FeeValues;
        private ListBox lbWages, lbFees, lbFoodDrinks;
        public PageAirlineWages(Airline airline)
        {
            this.Airline = airline;

            //this.WageValues = new Dictionary<FeeType, double>();
            this.FeeValues = new Dictionary<FeeType, double>();

            //foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Wage))
              //  this.WageValues.Add(type, this.Airline.Fees.getValue(type));

            foreach (FeeType type in FeeTypes.GetTypes())
                this.FeeValues.Add(type, this.Airline.Fees.getValue(type));

            InitializeComponent();
            
            StackPanel panelWages = new StackPanel();
            panelWages.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeaderWages = new TextBlock();
            txtHeaderWages.Margin = new Thickness(0, 0, 0, 0);
            txtHeaderWages.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderWages.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderWages.FontWeight = FontWeights.Bold;
            txtHeaderWages.Text = "Wages";

            panelWages.Children.Add(txtHeaderWages);

            lbWages = new ListBox();
            lbWages.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbWages.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Wage))
                lbWages.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            panelWages.Children.Add(lbWages);

            TextBlock txtHeaderFoods = new TextBlock();
            txtHeaderFoods.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderFoods.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderFoods.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderFoods.FontWeight = FontWeights.Bold;
            txtHeaderFoods.Text = "Food and drinks";

            panelWages.Children.Add(txtHeaderFoods);

            lbFoodDrinks = new ListBox();
            lbFoodDrinks.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFoodDrinks.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks)) 
                lbFoodDrinks.Items.Add(new QuickInfoValue(type.Name,createWageSlider(type)));

            panelWages.Children.Add(lbFoodDrinks);

            TextBlock txtHeaderFees = new TextBlock();
            txtHeaderFees.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderFees.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderFees.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderFees.FontWeight = FontWeights.Bold;
            txtHeaderFees.Text = "Fees";

            panelWages.Children.Add(txtHeaderFees);

            lbFees = new ListBox();
            lbFees.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFees.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Fee))
                lbFees.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));

            panelWages.Children.Add(lbFees);

            panelWages.Children.Add(createButtonsPanel());

            this.Content = panelWages;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = "OK";
           btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            Button btnUndo = new Button();
            btnUndo.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnUndo.Height = Double.NaN;
            btnUndo.Margin = new Thickness(5, 0, 0, 0);
            btnUndo.Width = Double.NaN;
            btnUndo.Click += new RoutedEventHandler(btnUndo_Click);
            btnUndo.Content = "Undo";
            btnUndo.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnUndo);

            return buttonsPanel;
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            lbWages.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Wage))
            {
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbWages.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            }

            lbFees.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Fee))
            {
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbFees.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            }
            lbFoodDrinks.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks))
            {
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbFoodDrinks.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            }
              

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
           WPFMessageBoxResult result= WPFMessageBox.Show("Change wages", "Are you sure you want to change the wage rates?", WPFMessageBoxButtons.YesNo);
           if (result == WPFMessageBoxResult.Yes)
           {
               foreach (FeeType type in this.FeeValues.Keys)
                   this.Airline.Fees.setValue(type, this.FeeValues[type]);
           }

        }
        //creates the slider for a wage type
        private WrapPanel createWageSlider(FeeType type)
        {
            WrapPanel sliderPanel = new WrapPanel();

            TextBlock txtValue = UICreator.CreateTextBlock(string.Format("{0:C}", this.FeeValues[type]));
            txtValue.VerticalAlignment = VerticalAlignment.Bottom;
            txtValue.Margin = new Thickness(5, 0, 0, 0);
            txtValue.Tag = type;

            double frequency = (type.MaxValue - type.MinValue) / 200;

            Slider slider = new Slider();
            slider.Width = 200;
            slider.Value = this.FeeValues[type];
            slider.Tag = txtValue;
            slider.Maximum = type.MaxValue;
            slider.Minimum = type.MinValue;
            slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
            slider.TickFrequency = (type.MaxValue - type.MinValue) / slider.Width;
            slider.IsSnapToTickEnabled = true;
            slider.IsMoveToPointEnabled = true;
            sliderPanel.Children.Add(slider);



            sliderPanel.Children.Add(txtValue);

            return sliderPanel;
        }


        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            TextBlock txtBlock = (TextBlock)slider.Tag;
            txtBlock.Text = string.Format("{0:C}", slider.Value);

            FeeType type = (FeeType)txtBlock.Tag;

            this.FeeValues[type] = slider.Value;

        }

    }
}
