using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpSpecialContracts.xaml
    /// </summary>
    public partial class PopUpSpecialContracts : PopUpWindow
    {
        public ObservableCollection<SpecialContractMVVM> Contracts { get; set; }
        public static void ShowPopUp()
        {
            PopUpWindow window = new PopUpSpecialContracts(); 
            window.ShowDialog();
        }
        public PopUpSpecialContracts()
        {
            this.Contracts = new ObservableCollection<SpecialContractMVVM>();

            foreach (SpecialContractType sct in GameObject.GetInstance().Contracts)
            {
                DateTime date = sct.LastDate.AddMonths(1);
                this.Contracts.Add(new SpecialContractMVVM(sct,date));
            }

            InitializeComponent();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            SpecialContractMVVM sct = (SpecialContractMVVM)((Button)sender).Tag;

            DateTime startdate;

            if (sct.Type.IsFixedDate)
                startdate = sct.Type.Period.From;
            else
                startdate = GameObject.GetInstance().GameTime;

            SpecialContract sc = new SpecialContract(sct.Type, startdate,GameObject.GetInstance().HumanAirline);
            GameObject.GetInstance().HumanAirline.SpecialContracts.Add(sc);

            GameObject.GetInstance().Contracts.Remove(sct.Type);
            this.Contracts.Remove(sct);

              if (this.Contracts.Count == 0)
                this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SpecialContractMVVM sct = (SpecialContractMVVM)((Button)sender).Tag;

            GameObject.GetInstance().Contracts.Remove(sct.Type);
            this.Contracts.Remove(sct);

            if (this.Contracts.Count == 0)
                this.Close();
        }
    }
    public class SpecialContractMVVM
    {
        public DateTime Date { get; set; }
        public SpecialContractType Type { get; set; }
        public SpecialContractMVVM(SpecialContractType type, DateTime date)
        {
            this.Date = date;
            this.Type = type;
        }
    }
}
