using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    //the class for a runway at an airport
    [Serializable]
    public class Runway : BaseModel
    {
        #region Constructors and Destructors

        public Runway(string name, long length, RunwayType type, SurfaceType surface, DateTime builtDate, Boolean standard)
        {
            Name = name;
            Length = length;
            Surface = surface;
            BuiltDate = builtDate;
            Standard = standard;
            Type = type;
        }

        private Runway(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
                Type = RunwayType.Regular;
        }

        #endregion

        #region Enums

        public enum RunwayType
        {
            Regular,

            Helipad
        }

        public enum SurfaceType
        {
            Asphalt,

            Concrete,

            Grass,

            Bitumen,

            Dirt,

            Gravel,

            Ice,

            Salt,

            Paved,

            Steel,

            Unpaved,

            Sand,

            PSP,
            Clay
        }

        #endregion

        #region Public Properties

        [Versioning("builtdate")]
        public DateTime BuiltDate { get; set; }

        [Versioning("length")]
        public long Length { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("standard")]
        public Boolean Standard { get; set; }

        [Versioning("surface")]
        public SurfaceType Surface { get; set; }

        [Versioning("type", Version = 2)]
        public RunwayType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}