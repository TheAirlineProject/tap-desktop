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
        public PageCalendar()
        {
            InitializeComponent();


            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageCalendar", this.Uid);

            StackPanel calendarPanel = new StackPanel();
            calendarPanel.Margin = new Thickness(10, 0, 10, 0);

       
            calendarPanel.Children.Add(new ucCalendar());

            base.setContent(calendarPanel);

            base.setHeaderContent(this.Title);
            


            showPage(this);

        }
    }
    
}
