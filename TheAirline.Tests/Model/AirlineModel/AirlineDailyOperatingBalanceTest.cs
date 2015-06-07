using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheAirline.Models.Airlines;
using TheAirline.Tests.Model.AirlinerModel;

namespace TheAirline.Tests.Model.AirlineModel
{
    [TestClass]
    public class AirlineDailyOperatingBalanceTests
    {
        [TestMethod]
        public void DailyOperatingBalanceIsZeroByDefault()
        {
            Assert.AreEqual(0, Fakes.CreateNewDummyAirline().DailyOperatingBalanceHistory);
        }


        [TestMethod]
        public void DailyOperatingBalanceReturnsNewestValue()
        {
            var target = Fakes.CreateNewDummyAirline();
            const int Expected = 40;

            //target.DailyOperatingBalanceHistory.Add(new KeyValuePair<DateTime, double>(new DateTime(1995, 1, 1), 10));
            //target.DailyOperatingBalanceHistory.Add(new KeyValuePair<DateTime, double>(new DateTime(1995, 1, 2), 20));
            //target.DailyOperatingBalanceHistory.Add(new KeyValuePair<DateTime, double>(new DateTime(1995, 1, 3), 30));
            //target.DailyOperatingBalanceHistory.Add(new KeyValuePair<DateTime, double>(new DateTime(1995, 1, 4), Expected));

            Assert.AreEqual(Expected, target.DailyOperatingBalanceHistory);

        }

        [TestMethod]
        public void AirlinerRaisesNotifyChangeForDailyOperatingBalance()
        {
            //bool flag = false;
            //Airline target = Fakes.CreateNewDummyAirline();

            //target.PropertyChanged +=
            //    delegate(object sender, PropertyChangedEventArgs args)
            //    {
            //        flag = args.PropertyName == "DailyOperatingBalance";
            //    };

            //target.DailyOperatingBalanceHistory.Add(new KeyValuePair<DateTime, double>(new DateTime(1995, 1, 1), 12345));

            //Assert.IsTrue(flag);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Setup.ClearLists();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Setup.LoadAirlinerFacilities();
            Setup.CreateAdvertisementTypes();
            Setup.LoadInflationYears();
        }

    }
}
