using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General
{
    [Serializable]
    //the class for a period
    public class Period<T> : BaseModel
    {
        #region Constructors and Destructors

        public Period(T from, T to)
        {
            To = to;
            From = from;
        }

        private Period(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("from")]
        public T From { get; set; }

        [Versioning("to")]
        public T To { get; set; }

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