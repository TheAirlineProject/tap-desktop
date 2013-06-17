
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    /*! Airlines Fees Type.
     * This class is used for the fees values for an airline.
     * The class needs no parameters
     */
    [Serializable]
    public class AirlineFees
    {
        
        private Dictionary<FeeType, double> Fees;
        public AirlineFees()
        {
            this.Fees = new Dictionary<FeeType, double>();

            foreach (FeeType type in FeeTypes.GetTypes())
                this.Fees.Add(type, type.DefaultValue);

        }
        //returns the value of a specific fee type
        public double getValue(FeeType type)
        {
            lock (this.Fees)
            {
                if (!this.Fees.ContainsKey(type))
                {
                    this.Fees.Add(type, type.DefaultValue);
                }
            }

            return this.Fees[type];

        }
        //sets the value of a specific fee type
        public void setValue(FeeType type, double value)
        {
            this.Fees[type] = value;
        }
    }
}
