using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel
{
    [ProtoContract]
    public class AirlineBudget
    {
        [ProtoMember(1)]
        public long TotalBudget { set; get; }
        [ProtoMember(2)]
        public DateTime BudgetActive { set; get; }
        [ProtoMember(3)]
        public DateTime BudgetExpires { set; get; }
        [ProtoMember(4)]
        public long MarketingBudget { set; get; }
        [ProtoMember(5)]
        public long MaintenanceBudget { set; get; }
        [ProtoMember(6)]
        public long SecurityBudget { set; get; }
        [ProtoMember(7)]
        public long CSBudget { set; get; }
        [ProtoMember(8)]
        public long PrintBudget { set; get; }
        [ProtoMember(9)]
        public long TelevisionBudget { set; get; }
        [ProtoMember(10)]
        public long RadioBudget { set; get; }
        [ProtoMember(11)]
        public long InternetBudget { set; get; }
        [ProtoMember(12)]
        public long OverhaulBudget { set; get; }
        [ProtoMember(13)]
        public long PartsBudget { set; get; }
        [ProtoMember(14)]
        public long EnginesBudget { set; get; }
        [ProtoMember(15)]
        public long RemoteBudget { set; get; }
        [ProtoMember(16)]
        public long InFlightBudget { set; get; }
        [ProtoMember(17)]
        public long AirportBudget { set; get; }
        [ProtoMember(18)]
        public long EquipmentBudget { set; get; }
        [ProtoMember(19)]
        public long ITBudget { set; get; }
        [ProtoMember(20)]
        public long CompBudget { set; get; }
        [ProtoMember(21)]
        public long PromoBudget { set; get; }
        [ProtoMember(22)]
        public long ServCenterBudget { set; get; }
        [ProtoMember(23)]
        public long PRBudget { set; get; }
        [ProtoMember(25)]
        public long EndYearCash { set; get; }
        [ProtoMember(24)]
        public int FleetSize { set; get; }
        [ProtoMember(26)]
        public long FleetValue { set; get; }
        [ProtoMember(27)]
        public int Subsidiaries { set; get; }
        [ProtoMember(28)]
        public long TotalSubValue { set; get; }
        [ProtoMember(29)]
        public int TotalEmployees { set; get; }
        [ProtoMember(30)]
        public int TotalPayroll { set; get; }
        [ProtoMember(31)]
        public long RemainingBudget { set; get; }
        [ProtoMember(32)]
        public long Cash { set; get; }
    }
}
