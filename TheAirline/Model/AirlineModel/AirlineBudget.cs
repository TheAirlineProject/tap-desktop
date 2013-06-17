
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    public class AirlineBudget
    {
        
        public long TotalBudget { set; get; }
        
        public DateTime BudgetActive { set; get; }
        
        public DateTime BudgetExpires { set; get; }
        
        public long MarketingBudget { set; get; }
        
        public long MaintenanceBudget { set; get; }
        
        public long SecurityBudget { set; get; }
        
        public long CSBudget { set; get; }
        
        public long PrintBudget { set; get; }
        
        public long TelevisionBudget { set; get; }
        
        public long RadioBudget { set; get; }
        
        public long InternetBudget { set; get; }
        public long OverhaulBudget { set; get; }
        public long PartsBudget { set; get; }
        public long EnginesBudget { set; get; }
        public long RemoteBudget { set; get; }
        public long InFlightBudget { set; get; }
        public long AirportBudget { set; get; }
        public long EquipmentBudget { set; get; }
        public long ITBudget { set; get; }
        public long CompBudget { set; get; }
        public long PromoBudget { set; get; }
        public long ServCenterBudget { set; get; }
        public long PRBudget { set; get; }
        public long EndYearCash { set; get; }
        public int FleetSize { set; get; }
        public long FleetValue { set; get; }
        public int Subsidiaries { set; get; }
        public long TotalSubValue { set; get; }
        public int TotalEmployees { set; get; }
        public int TotalPayroll { set; get; }
        public long RemainingBudget { set; get; }
        public long Cash { set; get; }
    }
}
