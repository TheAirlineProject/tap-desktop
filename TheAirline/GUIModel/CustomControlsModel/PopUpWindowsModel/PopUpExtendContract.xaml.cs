using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpExtendContract.xaml
    /// </summary>
    public partial class PopUpExtendContract : PopUpWindow, INotifyPropertyChanged
    {
        private AirportContract Contract;
        private Boolean _hasfreegates;
        public Boolean HasFreeGates
        {
            get { return _hasfreegates; }
            set { _hasfreegates = value; NotifyPropertyChanged("HasFreeGates"); }
        }
        private int _numberofgates;
        public int NumberOfGates
        {
            get { return _numberofgates; }
            set { _numberofgates = value; NotifyPropertyChanged("NumberOfGates"); }
        }
        private DateTime _expiredate;
        public DateTime ExpireDate
        {
            get { return _expiredate; }
            set { _expiredate = value; NotifyPropertyChanged("ExpireDate"); }
        }
        public static object ShowPopUp(AirportContract contract)
        {
            PopUpWindow window = new PopUpExtendContract(contract);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpExtendContract(AirportContract contract)
        {
            this.Contract = contract;
            this.DataContext = contract;

            this.ExpireDate = this.Contract.ExpireDate;
            this.NumberOfGates = this.Contract.NumberOfGates;

            this.HasFreeGates = this.Contract.Airport.Terminals.getFreeGates(this.Contract.TerminalType) > 0;

            InitializeComponent();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int gates = Convert.ToInt16(txtGates.Text);

            this.Contract.NumberOfGates = gates;
            this.Contract.AutoRenew = cbAutoRenew.IsChecked.Value;

            this.Selected = this.Contract;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
        private void btnAddGate_Click(object sender, RoutedEventArgs e)
        {
           
            this.NumberOfGates++;

            int diffGates = this.NumberOfGates - this.Contract.NumberOfGates;

          
            this.HasFreeGates = this.Contract.Airport.Terminals.getFreeGates(this.Contract.TerminalType) - diffGates > 0;

          
        }
        private void btnExtend_Click(object sender, RoutedEventArgs e)
        {
            this.ExpireDate = this.ExpireDate.AddYears(1);
            this.Contract.ExpireDate = this.ExpireDate.AddYears(1);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
