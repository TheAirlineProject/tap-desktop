using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageFinancesModel
{
    /// <summary>
    /// Interaction logic for PageFinances.xaml
    /// </summary>
    public partial class PageFinances : StandardPage
    {
        private Airline Airline;
        private Slider slMarketingBudget, slNewspaper, slInternet, slTelevision;
        private TextBox txtMarketingBudget, txtSliderValue;
        public PageFinances(Airline airline)
        {


            InitializeComponent();
            this.Uid = "2000";
            this.Title = Translator.GetInstance().GetString("PageFinances", this.Uid);
            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag);
            this.Airline = airline;

            StackPanel financesPanel = new StackPanel();
            financesPanel.Margin = new Thickness(10, 0, 10, 0);
            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(financesPanel, StandardContentPanel.ContentLocation.Left);

            StackPanel panelFinances = new StackPanel();
            panelFinances.Margin = new Thickness(0, 10, 50, 0);

            WrapPanel panelMarketingBudget = new WrapPanel();

            double minValue = 0;
            TextBlock txtSliderValue = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(minValue).ToString());
            txtSliderValue.Margin = new Thickness(5, 0, 5, 0);
            txtSliderValue.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            txtSliderValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelFinances.Children.Add(txtSliderValue);

            Slider slMarketingBudget = new Slider();
            slMarketingBudget.Value = 10000;
            slMarketingBudget.Minimum = 1;
            slMarketingBudget.Maximum = 10000000;
            slMarketingBudget.Width = 400;
            slMarketingBudget.Tag = txtSliderValue;
            slMarketingBudget.IsDirectionReversed = false;
            slMarketingBudget.IsSnapToTickEnabled = true;
            slMarketingBudget.IsMoveToPointEnabled = true;
            slMarketingBudget.TickFrequency = 25000;
            slMarketingBudget.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
            slMarketingBudget.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both;

            panelFinances.Children.Add(slMarketingBudget);

            double maxNPValue = slMarketingBudget.Value / 3;
            TextBlock txtNPSliderValue = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(minValue).ToString());
            txtNPSliderValue.Margin = new Thickness(5, 0, 5, 0);
            txtNPSliderValue.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            txtNPSliderValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelFinances.Children.Add(txtNPSliderValue);

            Slider slNewspaper = new Slider();
            slNewspaper.Value = slMarketingBudget.Value / 3;
            slNewspaper.Minimum = 1;
            slNewspaper.Maximum = slMarketingBudget.Value;
            slNewspaper.Width = 250;
            slNewspaper.Tag = txtNPSliderValue;
            slNewspaper.IsDirectionReversed = false;
            slNewspaper.IsSnapToTickEnabled = true;
            slNewspaper.IsMoveToPointEnabled = true;
            slNewspaper.TickFrequency = 25000;
            slNewspaper.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
            slNewspaper.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both;

            panelFinances.Children.Add(slNewspaper);

            TextBlock txtNPBudget = new TextBlock();
            txtNPBudget.Margin = new Thickness(5, 0, 5, 0);
            txtNPBudget.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtNPBudget.Text = Translator.GetInstance().GetString("PageFinances", "2003");
            txtNPBudget.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelFinances.Children.Add(txtNPBudget);

            Button btnApplyMB = new Button();
            btnApplyMB.Uid = "2001";
            btnApplyMB.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnApplyMB.Height = Double.NaN;
            btnApplyMB.Width = Double.NaN;
            btnApplyMB.Content = Translator.GetInstance().GetString("PageFinances", btnApplyMB.Uid);
            btnApplyMB.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnApplyMB.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnApplyMB.Margin = new Thickness(0, 10, 0, 10);
            btnApplyMB.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnApplyMB.Click += new RoutedEventHandler(btnApplyMB_Click);
            panelFinances.Children.Add(btnApplyMB);

            TextBlock txtMarketingBudget = new TextBlock();
            txtMarketingBudget.Margin = new Thickness(5, 0, 5, 0);
            txtMarketingBudget.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtMarketingBudget.Text = Translator.GetInstance().GetString("PageFinances", "2002");
            txtMarketingBudget.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;


            panelFinances.Children.Add(txtMarketingBudget);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            TextBlock txtBlock = (TextBlock)slider.Tag;
            txtBlock.Text = new ValueCurrencyConverter().Convert(slider.Value).ToString();

        }

        private void btnApplyMB_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Your marketing budget has been changed!", "Marketing Budget", MessageBoxButton.OK);
            /*News news;
            news.Body = "The marketing department has received and approved your new budget request. It will take place immediatelely. Please make sure you check your advertisement allocation as values have changed.";
            news.Subject = "Marketing Budget";
            news.Type = News.NewsType.Airline_News;*/
                       

        }
    }
    
}
