using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner order
    [Serializable]
    public class AirlinerOrder : BaseModel
    {
        #region Constructors and Destructors

        public AirlinerOrder(AirlinerType type, List<AirlinerClass> classes, int amount, Boolean customConfiguration)
        {
            Type = type;
            Amount = amount;
            Classes = classes;
            CustomConfiguration = customConfiguration;
        }

        private AirlinerOrder(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("amount")]
        public int Amount { get; set; }

        [Versioning("classes")]
        public List<AirlinerClass> Classes { get; set; }

        [Versioning("custom")]
        public Boolean CustomConfiguration { get; set; }

        [Versioning("type")]
        public AirlinerType Type { get; set; }

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