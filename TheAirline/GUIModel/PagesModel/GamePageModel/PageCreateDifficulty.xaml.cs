using System;
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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageCreateDifficulty.xaml
    /// </summary>
    public partial class PageCreateDifficulty : Page
    {
        public List<DifficultyMVVM> Difficulties { get; set; }
        public PageCreateDifficulty()
        {
            this.Difficulties = new List<DifficultyMVVM>();

            DifficultyLevel easyLevel = DifficultyLevels.GetDifficultyLevel("Easy");
            DifficultyLevel normalLevel = DifficultyLevels.GetDifficultyLevel("Normal");
            DifficultyLevel hardLevel = DifficultyLevels.GetDifficultyLevel("Hard");

            this.Difficulties.Add(new DifficultyMVVM("money",Translator.GetInstance().GetString("PageCreateDifficulty", "1000"), easyLevel.MoneyLevel, normalLevel.MoneyLevel, hardLevel.MoneyLevel));
            this.Difficulties.Add(new DifficultyMVVM("price",Translator.GetInstance().GetString("PageCreateDifficulty", "1001"), easyLevel.PriceLevel, normalLevel.PriceLevel, hardLevel.PriceLevel));
            this.Difficulties.Add(new DifficultyMVVM("loan",Translator.GetInstance().GetString("PageCreateDifficulty", "1002"), easyLevel.LoanLevel, normalLevel.LoanLevel, hardLevel.LoanLevel));
            this.Difficulties.Add(new DifficultyMVVM("passengers",Translator.GetInstance().GetString("PageCreateDifficulty", "1003"), easyLevel.PassengersLevel, normalLevel.PassengersLevel, hardLevel.PassengersLevel));
            this.Difficulties.Add(new DifficultyMVVM("AI",Translator.GetInstance().GetString("PageCreateDifficulty", "1004"), easyLevel.AILevel, normalLevel.AILevel, hardLevel.AILevel));
            this.Difficulties.Add(new DifficultyMVVM("startdata",Translator.GetInstance().GetString("PageCreateDifficulty", "1005"), easyLevel.StartDataLevel, normalLevel.StartDataLevel, hardLevel.StartDataLevel));

            InitializeComponent();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            Slider slMoney = UIHelpers.FindChild<Slider>(this, "money");
            Slider slLoan = UIHelpers.FindChild<Slider>(this, "loan");
            Slider slPrice = UIHelpers.FindChild<Slider>(this, "price");
            Slider slPassengers = UIHelpers.FindChild<Slider>(this, "passengers");
            Slider slAI = UIHelpers.FindChild<Slider>(this, "AI");
            Slider slStartData = UIHelpers.FindChild<Slider>(this, "startdata");

            double money = slMoney.Value;
            double loan= slLoan.Value;
            double passengers = slPassengers.Value;
            double price = slPrice.Value;
            double AI = slAI.Value;
            double startData = slStartData.Value;
                    
            DifficultyLevel level = new DifficultyLevel("Custom", money, loan, passengers, price, AI,startData);

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2406"), Translator.GetInstance().GetString("MessageBox", "2406", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                DifficultyLevels.AddDifficultyLevel(level);

                PageNavigator.NavigateTo(new PageNewGame());
            }
        }
    }
}
