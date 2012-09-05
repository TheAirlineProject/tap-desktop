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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.GraphicsModel.UserControlModel.CalendarModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PageCalendar.xaml
    /// </summary>
    public partial class PageCalendar : StandardPage
    {
        private CheckBox cbShowAll;
        private ucCalendar ucCalendar;
        public PageCalendar()
        {
            InitializeComponent();


            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageCalendar", this.Uid);

            StackPanel calendarPanel = new StackPanel();
            calendarPanel.Margin = new Thickness(10, 0, 10, 0);

            cbShowAll = new CheckBox();
            cbShowAll.Content = Translator.GetInstance().GetString("PageCalendar","200");
            cbShowAll.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            cbShowAll.Checked += new RoutedEventHandler(cbShowAll_Checked);
            cbShowAll.Unchecked += new RoutedEventHandler(cbShowAll_Unchecked);
            cbShowAll.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbShowAll.IsChecked = false;

            calendarPanel.Children.Add(cbShowAll);

            ScrollViewer viewer = new ScrollViewer();
            viewer.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;

            ucCalendar = new ucCalendar();

            viewer.Content = ucCalendar;

            calendarPanel.Children.Add(viewer);

            base.setContent(calendarPanel);

            base.setHeaderContent(this.Title);
            


            showPage(this);

        }

        private void cbShowAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ucCalendar.ShowAll = false;
            ucCalendar.showCurrentMonth();
 
        }

        void cbShowAll_Checked(object sender, RoutedEventArgs e)
        {
            ucCalendar.ShowAll = true;
            ucCalendar.showCurrentMonth();
        }
    }
    
}
