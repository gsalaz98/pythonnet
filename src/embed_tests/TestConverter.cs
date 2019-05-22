using System;
using System.Collections.Generic;
using NUnit.Framework;
using Python.Runtime;
using System;

namespace Python.EmbeddingTest
{
    public class TestConverter
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
        public void ConvertListRoundTrip()
        {
            var list = new List<Type> { typeof(decimal), typeof(int) };
            var py = list.ToPython();
            object result;
            var converted = Converter.ToManaged(py.Handle, typeof(List<Type>), out result, false);

            Assert.IsTrue(converted);
            Assert.AreEqual(result, list);
        }

        [Test]
        public void ConvertPyListToArray()
        {
            var array = new List<Type> { typeof(decimal), typeof(int) };
            var py = array.ToPython();
            object result;
            var converted = Converter.ToManaged(py.Handle, typeof(Type[]), out result, false);

            Assert.IsTrue(converted);
            Assert.AreEqual(result, array);
        }

        [Test]
        public void TestConvertSingleToManaged(
            [Values(float.PositiveInfinity, float.NegativeInfinity, float.MinValue, float.MaxValue, float.NaN,
                float.Epsilon)] float testValue)
        {
            var pyFloat = new PyFloat(testValue);

            object convertedValue;
            var converted = Converter.ToManaged(pyFloat.Handle, typeof(float), out convertedValue, false);

            Assert.IsTrue(converted);
            Assert.IsTrue(((float) convertedValue).Equals(testValue));
        }

        [Test]
        public void TestConvertDoubleToManaged(
            [Values(double.PositiveInfinity, double.NegativeInfinity, double.MinValue, double.MaxValue, double.NaN,
                double.Epsilon)] double testValue)
        {
            var pyFloat = new PyFloat(testValue);

            object convertedValue;
            var converted = Converter.ToManaged(pyFloat.Handle, typeof(double), out convertedValue, false);

            Assert.IsTrue(converted);
            Assert.IsTrue(((double) convertedValue).Equals(testValue));
        }
    }
}
