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
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PagePerformance.xaml
    /// </summary>
    public partial class PagePerformance : StandardPage
    {
        public PagePerformance()
        {
            InitializeComponent();

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["PerformanceHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            panelContent.Children.Add(txtHeader);

            ListBox lbCounters = new ListBox();
            lbCounters.ItemTemplate = this.Resources["PerformanceItem"] as DataTemplate;
            lbCounters.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;
            lbCounters.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            foreach (string pageCounter in PerformanceCounters.GetPages())
                lbCounters.Items.Add(pageCounter);



            panelContent.Children.Add(lbCounters);

          
            base.setContent(panelContent);

            base.setHeaderContent("Performance Counters");

            showPage(this);

        
        }
    }
    public class PerformanceCountersConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<PagePerformanceCounter> counters = PerformanceCounters.GetPerformanceCounters(value.ToString()); 
            string type = parameter.ToString();

            if (type == "A")
            {
                return counters.Average(c => c.Counter);
            }
            if (type == "H")
            {
                return counters.Max(c => c.Counter);
            }
            if (type == "L")
            {
                return counters.Min(c => c.Counter);
            }
            if (type == "C")
            {
                return counters.Count;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
