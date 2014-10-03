using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    //the class for a policy for an airline
    public class AirlinePolicy : BaseModel
    {
        #region Constructors and Destructors

        public AirlinePolicy(string name, object policyValue)
        {
            Name = name;
            PolicyValue = policyValue;
        }

        private AirlinePolicy(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("value")]
        public object PolicyValue { get; set; }

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