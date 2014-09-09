using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    public class AirlineBudget : BaseModel
    {
        #region Constructors and Destructors

        public AirlineBudget()
        {
        }

        private AirlineBudget(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airportbudget")]
        public long AirportBudget { set; get; }

        [Versioning("budgetactive")]
        public DateTime BudgetActive { set; get; }

        [Versioning("budgetexpires")]
        public DateTime BudgetExpires { set; get; }

        [Versioning("csbudget")]
        public long CSBudget { set; get; }

        [Versioning("cash")]
        public long Cash { set; get; }

        [Versioning("compbudget")]
        public long CompBudget { set; get; }

        [Versioning("endyearcash")]
        public long EndYearCash { set; get; }

        [Versioning("enginesbudget")]
        public long EnginesBudget { set; get; }

        [Versioning("equipmentbudget")]
        public long EquipmentBudget { set; get; }

        [Versioning("fleetsize")]
        public int FleetSize { set; get; }

        [Versioning("fleetvalue")]
        public long FleetValue { set; get; }

        [Versioning("ITbudget")]
        public long ITBudget { set; get; }

        [Versioning("inflightbudget")]
        public long InFlightBudget { set; get; }

        [Versioning("internetbudget")]
        public long InternetBudget { set; get; }

        [Versioning("maintenancebudget")]
        public long MaintenanceBudget { set; get; }

        [Versioning("marktingbudget")]
        public long MarketingBudget { set; get; }

        [Versioning("overhaulbudget")]
        public long OverhaulBudget { set; get; }

        [Versioning("prbudget")]
        public long PRBudget { set; get; }

        [Versioning("patsbudget")]
        public long PartsBudget { set; get; }

        [Versioning("printbudget")]
        public long PrintBudget { set; get; }

        [Versioning("promobudget")]
        public long PromoBudget { set; get; }

        [Versioning("radiobudget")]
        public long RadioBudget { set; get; }

        [Versioning("remainingbudget")]
        public long RemainingBudget { set; get; }

        [Versioning("remotebudget")]
        public long RemoteBudget { set; get; }

        [Versioning("securitybudget")]
        public long SecurityBudget { set; get; }

        [Versioning("scbudget")]
        public long ServCenterBudget { set; get; }

        [Versioning("subsidiaries")]
        public int Subsidiaries { set; get; }

        [Versioning("tvbudget")]
        public long TelevisionBudget { set; get; }

        [Versioning("totalbudget")]
        public long TotalBudget { set; get; }

        [Versioning("totalemployees")]
        public int TotalEmployees { set; get; }

        [Versioning("totalpayroll")]
        public int TotalPayroll { set; get; }

        [Versioning("totalsubvalue")]
        public long TotalSubValue { set; get; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}