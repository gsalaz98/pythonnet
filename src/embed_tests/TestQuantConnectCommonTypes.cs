using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Python.Runtime;
using QuantConnect.Data.Market;

namespace Python.EmbeddingTest
{
    [TestFixture]
    public class TestQuantConnectCommonTypes
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            PythonEngine.Initialize();
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        [Test]
        public void ConvertsTradeBar()
        {
            var bar = new TradeBar
            {
                Open = 2,
                High = 4,
                Low = 1,
                Close = 3
            };

            dynamic converted = bar.ToPython();

            Assert.AreEqual(bar.High, (decimal)converted.High);
        }
    }
}
