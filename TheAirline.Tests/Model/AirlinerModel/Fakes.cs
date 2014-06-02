namespace TheAirline.Tests.Model.AirlinerModel
{
    using System;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;

    internal static class Fakes
    {
        #region Public Methods and Operators

        public static Airline CreateNewDummyAirline()
        {
            return new Airline(
                new AirlineProfile("test Airline", "TS", "Red", "TestName testSurname", true, 2000, 2199),
                Airline.AirlineMentality.Moderate,
                Airline.AirlineFocus.Global,
                Airline.AirlineLicense.Long_Haul,
                Route.RouteType.Passenger);
        }

        public static AirlinerType GetNewDummyCargoAirliner()
        {
            return new AirlinerCargoType(
                null,
                "Test Airliner",
                "Test",
                2,
                40,
                412,
                3040,
                0,
                0,
                0.062,
                90000000,
                2000,
                20000,
                AirlinerType.BodyType.Wide_Body,
                AirlinerType.TypeRange.Long_Range,
                AirlinerType.EngineType.Turboprop,
                new Period<DateTime>(new DateTime(1999, 1, 1), new DateTime(2015, 1, 1)),
                3,
                false,
                true);
        }

        public static AirlinerType GetNewDummyPassengerAirliner()
        {
            return new AirlinerPassengerType(
                null,
                "Test Airliner",
                "Test",
                0,
                2,
                2,
                535,
                3040,
                0,
                0,
                0.051,
                90000000,
                3,
                2000,
                20000,
                AirlinerType.BodyType.Narrow_Body,
                AirlinerType.TypeRange.Long_Range,
                AirlinerType.EngineType.Jet,
                new Period<DateTime>(new DateTime(1990, 1, 1), new DateTime(2010, 1, 1)),
                5,
                true,
                true);
        }

        #endregion

        public static AirlinerType GetNewDummyCombiAirliner()
        {
            return new AirlinerCombiType(
                null,
                "Test Airliner",
                "Test",
                200,
                2,
                3,
                412,
                3040,
                0,
                0,
                0.062,
                90000000,
                3,
                2000,
                20000,
                AirlinerType.BodyType.Wide_Body,
                AirlinerType.TypeRange.Long_Range,
                AirlinerType.EngineType.Turboprop,
                new Period<DateTime>(new DateTime(1999, 1, 1), new DateTime(2015, 1, 1)),
                3,
                40,
                true,
                true);
        }
    }
}