using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageAirliners.xaml
    /// </summary>
    public partial class PageAirliners : Page
    {
        public List<AirlineFleetSizeMVVM> MostUsedAircrafts { get; set; }
        public List<AirlinerType> NewestAircrafts { get; set; }
        public Hashtable AirlinersFilters { get; set; }
   
        public PageAirliners()
        {
            this.NewestAircrafts = AirlinerTypes.GetTypes(a => a.Produced.From <= GameObject.GetInstance().GameTime).OrderByDescending(a => a.Produced.From).Take(5).ToList();
            this.MostUsedAircrafts = new List<AirlineFleetSizeMVVM>();

            var query = GameObject.GetInstance().HumanAirline.Fleet.GroupBy(a=>a.Airliner.Type)
                  .Select(group =>
                        new
                        {
                            Type = group.Key,
                            Fleet = group
                        })
                  .OrderByDescending(g=>g.Fleet.Count());

            var aircrafts = query.Take(Math.Min(query.Count(),5));

            foreach (var group in aircrafts)
            {
                this.MostUsedAircrafts.Add(new AirlineFleetSizeMVVM(group.Type,group.Fleet.Count()));
            }

            this.Loaded += PageAirliners_Loaded;
            
            InitializeComponent();
        }

        private void PageAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageUsedAirliners() { Tag = this });
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Used" && frmContent != null)
                frmContent.Navigate(new PageUsedAirliners() { Tag = this });

            if (selection == "Order" && frmContent != null)
                frmContent.Navigate(new PageManufacturers() { Tag = this });

            if (selection == "New" && frmContent != null)
                frmContent.Navigate(new PageNewAirliners() { Tag = this });


        }
    }
}
