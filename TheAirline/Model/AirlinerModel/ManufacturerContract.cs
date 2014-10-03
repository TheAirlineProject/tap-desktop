using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlinerModel
{
    //the contract for an airline for a manufacturer
    [Serializable]
    public class ManufacturerContract : BaseModel
    {
        #region Constructors and Destructors

        public ManufacturerContract(Manufacturer manufacturer, DateTime date, int length, double discount)
        {
            Manufacturer = manufacturer;
            Airliners = length;
            SigningDate = date;
            Length = length;
            Discount = discount;
            ExpireDate = date.AddYears(Length);
            PurchasedAirliners = 0;
        }

        //returns the termination fee for the contract

        private ManufacturerContract(SerializationInfo info, StreamingContext ctxt) :base(info,ctxt)
        {
            
        }

        #endregion

        #region Public Properties

        [Versioning("airliners")]
        public int Airliners { get; set; }

        [Versioning("discount")]
        public double Discount { get; set; }

        [Versioning("expiredate")]
        public DateTime ExpireDate { get; set; }

        [Versioning("length")]
        public int Length { get; set; }

        [Versioning("manufacturer")]
        public Manufacturer Manufacturer { get; set; }

        [Versioning("purchasedairliners")]
        public int PurchasedAirliners { get; set; }

        [Versioning("signingdate")]
        public DateTime SigningDate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info,context);
        }

        public double GetDiscount()
        {
            if (Length <= 3)
            {
                Discount = (PurchasedAirliners/2) + 1;
            }
            else if (Length <= 5)
            {
                Discount = (PurchasedAirliners/2) + 2;
            }
            else if (Length <= 7)
            {
                Discount = (PurchasedAirliners/2) + 4;
            }
            else if (Length <= 15)
            {
                Discount = (PurchasedAirliners/2) + 7;
            }
            else
            {
                Discount = 1;
            }

            return Discount;
        }

        public double GetTerminationFee()
        {
            return GeneralHelpers.GetInflationPrice(Length*1000000);
        }

        #endregion
    }
}