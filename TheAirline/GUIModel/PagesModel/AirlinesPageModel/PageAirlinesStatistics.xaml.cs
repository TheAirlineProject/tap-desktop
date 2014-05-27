namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Controls;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlineModel.SubsidiaryModel;

    /// <summary>
    ///     Interaction logic for PageAirlinesStatistics.xaml
    /// </summary>
    public partial class PageAirlinesStatistics : Page
    {
        #region Constructors and Destructors

        public PageAirlinesStatistics()
        {
            this.AllAirlines = new ObservableCollection<AirlinesMVVM>();

            foreach (
                Airline airline in
                    Airlines.GetAllAirlines().FindAll(a => !a.IsSubsidiary).OrderByDescending(a => a.IsHuman))
            {
                this.AllAirlines.Add(new AirlinesMVVM(airline));

                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                {
                    this.AllAirlines.Add(new AirlinesMVVM(sAirline));
                }
            }

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinesMVVM> AllAirlines { get; set; }

        #endregion
    }
}