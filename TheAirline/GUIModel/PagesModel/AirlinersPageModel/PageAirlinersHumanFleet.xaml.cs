namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageAirlinersHumanFleet.xaml
    /// </summary>
    public partial class PageAirlinersHumanFleet : Page
    {
        #region Constructors and Destructors

        public PageAirlinersHumanFleet()
        {
            this.Fleet =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate < GameObject.GetInstance().GameTime)
                    .ToList();
            this.OrderedFleet =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate >= GameObject.GetInstance().GameTime)
                    .ToList();

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<FleetAirliner> Fleet { get; set; }

        public List<FleetAirliner> OrderedFleet { get; set; }

        #endregion
    }
}