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
            this.Title = Translator.GetInstance().GetString("PageSettings", this.Uid);

            StackPanel settingsPanel = new StackPanel();
            settingsPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(settingsPanel, StandardContentPanel.ContentLocation.Left);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);



            showPage(this);

        }
    }
}
