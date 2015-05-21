using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Airlines
{
    /*! Airlines Fees Type.
     * This class is used for the fees values for an airline.
     * The class needs no parameters
     */

    [Serializable]
    public class AirlineFees : BaseModel
    {
        #region Fields

        [Versioning("fees")] private readonly Dictionary<FeeType, double> _fees;

        #endregion

        #region Constructors and Destructors

        public AirlineFees()
        {
            _fees = new Dictionary<FeeType, double>();

            foreach (FeeType type in FeeTypes.GetTypes())
            {
                _fees.Add(type, type.DefaultValue);
            }
        }

        //returns the value of a specific fee type

        private AirlineFees(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info,context);
        }

        public double GetValue(FeeType type)
        {
            lock (_fees)
            {
                if (!_fees.ContainsKey(type))
                {
                    _fees.Add(type, type.DefaultValue);
                }
            }

            return _fees[type];
        }

        //sets the value of a specific fee type
        public void SetValue(FeeType type, double value)
        {
            _fees[type] = value;
        }

        #endregion
    }
}