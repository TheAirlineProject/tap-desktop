using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.ViewModels.Airline;

namespace TheAirline.Tests.GUIModel.PagesModel.AirlinePageModel
{
    [TestClass]
    public class AirlinerQuantityMVVMTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void PassCabinConfigurationToConstructor()
        {
            const string Expected = "12F | 24C | 180Y";

            var target = new AirlinerQuantityMVVM(this.GetNewDummyPassengerAirliner(), Expected, 1, 0);

            Assert.AreEqual(Expected, target.CabinConfiguration);
        }

        #endregion

        #region Methods

        private AirlinerType GetNewDummyPassengerAirliner()
        {
            //var country = new Moq.Mock<Country>();
            //var manufacturer = new Moq.Mock<Manufacturer>();
            //var airlinerPassengerType = new Moq.Mock<AirlinerPassengerType>();

            return new AirlinerPassengerType(
                null,
                "Test Airliner",
                "Test",
                200,
                2,
                2,
                535,
                3040,
                0,
                0,
                0,
                0.051,
                90000000,
                3,
                2000,
                20000,
                AirlinerType.BodyType.NarrowBody,
                AirlinerType.TypeRange.LongRange,
                AirlinerType.TypeOfEngine.Jet,
                new Period<DateTime>(new DateTime(1990, 1, 1), new DateTime(2010, 1, 1)),
                5,
                true,
                true);
        }

        #endregion
    }
}