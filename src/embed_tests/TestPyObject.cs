using System;
using System.Diagnostics;
using NUnit.Framework;
using Python.Runtime;

namespace Python.EmbeddingTest
{
    public interface IPyObjectTester
    {
        void AccessProperty();
        void AccessUndefinedProperty();
        void AccessUndefinedMethod();
        void AccessMethod();
    }
    public class PyObjectTesterBase : IPyObjectTester
    {
        public bool Property { get; set; }
        public void Method() { }
        public virtual void AccessProperty() { throw new NotImplementedException(); }
        public virtual void AccessUndefinedProperty() { throw new NotImplementedException(); }
        public virtual void AccessUndefinedMethod() { throw new NotImplementedException(); }
        public virtual void AccessMethod() { throw new NotImplementedException(); }
    }

    public class PyObjectTesterWrapper : IPyObjectTester
    {
        private readonly dynamic pyObject;

        public PyObjectTesterWrapper(dynamic pyObject)
        {
            this.pyObject = pyObject;
        }
        public void AccessProperty()
        {
            using (Py.GIL())
            {
                pyObject.AccessProperty();
            }
        }

        public void AccessUndefinedProperty()
        {
            using (Py.GIL())
            {
                pyObject.AccessUndefinedProperty();
            }
        }

        public void AccessUndefinedMethod()
        {
            using (Py.GIL())
            {
                pyObject.AccessUndefinedMethod();
            }
        }

        public void AccessMethod()
        {
            using (Py.GIL())
            {
                pyObject.AccessMethod();
            }
        }
    }

    [TestFixture]
    public class TestPyObject
    {
        private IPyObjectTester wrapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            PythonEngine.Initialize();

            var pModule = PythonEngine.ModuleFromString("PyObjectTester", @"
import clr
clr.AddReference(""Python.EmbeddingTest"")
from Python.EmbeddingTest import PyObjectTesterBase

class PyObjectTester(PyObjectTesterBase):
    def AccessProperty(self):
        return self.Property
    def AccessUndefinedProperty(self):
        return self.UndefinedProperty
    def AccessUndefinedMethod(self):
        return self.UndefinedMethod()
    def AccessMethod(self):
        self.Method()
    def Method(self):
        pass
").Handle;

            IntPtr type = Runtime.Runtime.PyObject_GetAttrString(pModule, "PyObjectTester");
            IntPtr propertyAccessorPtr = Runtime.Runtime.PyObject_CallObject(type, IntPtr.Zero);
            dynamic pyObjectTester = new PyObject(propertyAccessorPtr);
            wrapper = new PyObjectTesterWrapper(pyObjectTester);

            // jit
            wrapper.AccessProperty();
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        [Test]
        public void AccessingUndefinedProperty()
        {
            Assert.That(() => wrapper.AccessUndefinedProperty(),
                Throws.Exception.Message.Contain("'PyObjectTester' object has no attribute 'UndefinedProperty'"));
        }

        [Test]
        public void AccessingUndefinedMethod()
        {
            Assert.That(() => wrapper.AccessUndefinedMethod(),
                Throws.Exception.Message.Contain("'PyObjectTester' object has no attribute 'UndefinedMethod'"));
        }

        [Test]
        public void BenchmarkGetPropertyAccess()
        {
            Benchmark(100000, wrapper.AccessProperty);
        }

        [Test]
        public void BenchmarkMethodInvocation()
        {
            Benchmark(100000, wrapper.AccessProperty);
        }

        private static void Benchmark(int count, Action action)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                action.Invoke();
            }
            sw.Stop();

            Console.WriteLine($"{count} total in {sw.Elapsed.TotalSeconds}s, {count / sw.Elapsed.TotalMilliseconds:0.0}/ms  {1000 * sw.Elapsed.TotalMilliseconds / count:0.00000}ns/invoke");
        }
    }
}
