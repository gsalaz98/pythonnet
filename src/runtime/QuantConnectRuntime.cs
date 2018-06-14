using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Python.Runtime
{
    /// <summary>
    /// Provides integrated support for quantconnect types
    /// </summary>
    public static class QuantConnectRuntime
    {
        public static readonly IntPtr PyQcTradeBarType;
        public static readonly IntPtr TradeBarCtor;

        private static Func<object, IntPtr> tradeBarConverter;

        static QuantConnectRuntime()
        {
            var code = File.ReadAllText(@"C:\src\QuantConnect\pythonnet\src\embed_tests\quantconnect.common.py");
            IntPtr qcMod = PythonEngine.ModuleFromString("quantconnect.common", code).Handle;
            TradeBarCtor = Runtime.PyObject_GetAttrString(qcMod, "TradeBar");
            if (TradeBarCtor == null) throw new PythonException();

            var tradeBarArgs = Runtime.PyTuple_New(9);
            Runtime.PyTuple_SetItem(tradeBarArgs, 0, 0.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 1, false.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 2, new DateTime(2000, 1, 1, 1, 2, 3, 4).ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 3, new DateTime(2000, 1, 2, 1, 2, 3, 4).ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 4, Runtime.PyNone);
            Runtime.PyTuple_SetItem(tradeBarArgs, 5, 1m.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 6, 2m.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 7, 3m.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 8, 4m.ToPython().Handle);

            IntPtr op = Runtime.PyObject_CallObject(TradeBarCtor, tradeBarArgs);
            PyQcTradeBarType = Runtime.PyObject_Type(op);
            Runtime.XDecref(op);
            Runtime.XDecref(TradeBarCtor);
            Runtime.XDecref(qcMod);
        }

        public static IntPtr TradeBarToPython(object tradeBar)
        {
            return GetTradeBarConverter(tradeBar)(tradeBar);
        }

        private static Func<object, IntPtr> GetTradeBarConverter(object tradeBar)
        {
            if (tradeBarConverter != null)
            {
                return tradeBarConverter;
            }

            var tradeBarType = tradeBar.GetType();
            var dataTypeProperty = tradeBarType.GetProperty("DataType");
            var isFillForwardProperty = tradeBarType.GetProperty("IsFillForward");
            var timeProperty = tradeBarType.GetProperty("Time");
            var endTimeProperty = tradeBarType.GetProperty("EndTime");
            var symbolProperty = tradeBarType.GetProperty("Symbol");
            var valueProperty = tradeBarType.GetProperty("Value");
            var openProperty = tradeBarType.GetProperty("Open");
            var highProperty = tradeBarType.GetProperty("High");
            var lowProperty = tradeBarType.GetProperty("Low");

            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var pyTubleNew = typeof(Runtime).GetMethod("PyTuple_New", flags);
            var pyObjectCallObject = typeof(Runtime).GetMethod("PyObject_CallObject", flags);

            var parameter = Expression.Parameter(typeof(object), "input");
            var tradeBarExpr = Expression.Convert(parameter, tradeBarType);
            var resultVariableExpr = Expression.Variable(typeof(IntPtr));

            var tradeBarArgsExpr = Expression.Call(pyTubleNew, Expression.Constant(9));

            var dataTypeSetItem = PyTupleSetItem(tradeBarArgsExpr, 0, Expression.Property(tradeBarExpr, dataTypeProperty));
            var isFillForwardSetItem = PyTupleSetItem(tradeBarArgsExpr, 1, Expression.Property(tradeBarExpr, isFillForwardProperty));
            var timeSetItem = PyTupleSetItem(tradeBarArgsExpr, 2, Expression.Property(tradeBarExpr, timeProperty));
            var endTimeSetItem = PyTupleSetItem(tradeBarArgsExpr, 3, Expression.Property(tradeBarExpr, endTimeProperty));
            var symbolSetItem = PyTupleSetItem(tradeBarArgsExpr, 4, Expression.Property(tradeBarExpr, symbolProperty));
            var valueSetItem = PyTupleSetItem(tradeBarArgsExpr, 5, Expression.Property(tradeBarExpr, valueProperty));
            var openSetItem = PyTupleSetItem(tradeBarArgsExpr, 6, Expression.Property(tradeBarExpr, openProperty));
            var highSetItem = PyTupleSetItem(tradeBarArgsExpr, 7, Expression.Property(tradeBarExpr, highProperty));
            var lowSetItem = PyTupleSetItem(tradeBarArgsExpr, 8, Expression.Property(tradeBarExpr, lowProperty));

            var callTradeBarCtor = Expression.Call(pyObjectCallObject, Expression.Constant(TradeBarCtor), tradeBarArgsExpr);

            var label = Expression.Label(typeof(IntPtr));

            var bodyExpression = new Expression[] {
                dataTypeSetItem,
                isFillForwardSetItem,
                timeSetItem,
                endTimeSetItem,
                symbolSetItem,
                valueSetItem,
                openSetItem,
                highSetItem,
                lowSetItem,
                Expression.Assign(resultVariableExpr, callTradeBarCtor),
                resultVariableExpr
            };
            var block = Expression.Block(new[]{resultVariableExpr}, bodyExpression);

            var lambda = Expression.Lambda<Func<object, IntPtr>>(block, parameter);

            var func = lambda.Compile();

            return tradeBarConverter = func;
        }

        private static MethodCallExpression PyTupleSetItem(Expression args, int index, Expression value)
        {
            var pyTupleSetItem = typeof(Runtime).GetMethod("PyTuple_SetItem", BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
            var pythonHandle = ToPythonHandle(value);
            return Expression.Call(pyTupleSetItem, args, Expression.Constant(index), pythonHandle);
        }

        private static MethodCallExpression ToPythonHandle(Expression value)
        {
            // must be of type object
            if (value.Type != typeof(object))
            {
                value = Expression.Convert(value, typeof(object));
            }

            // ConverterExtension.ToPythonHandle(x)
            var toPython = typeof(ConverterExtension).GetMethod("ToPythonHandle");
            return Expression.Call(toPython, value);
        }


        private static IntPtr GetTradeBarArgs(object tradeBar)
        {
            // TODO : Reflection -> Expression code-gen for performance

            var tradeBarType = tradeBar.GetType();
            var dataTypeProperty = tradeBarType.GetProperty("DataType");
            var isFillForwardProperty = tradeBarType.GetProperty("IsFillForward");
            var timeProperty = tradeBarType.GetProperty("Time");
            var endTimeProperty = tradeBarType.GetProperty("EndTime");
            var symbolProperty = tradeBarType.GetProperty("Symbol");
            var valueProperty = tradeBarType.GetProperty("Value");
            var openProperty = tradeBarType.GetProperty("Open");
            var highProperty = tradeBarType.GetProperty("High");
            var lowProperty = tradeBarType.GetProperty("Low");

            var dataType = dataTypeProperty.GetValue(tradeBar);
            var isFillForward = isFillForwardProperty.GetValue(tradeBar);
            var time = timeProperty.GetValue(tradeBar);
            var endTime = endTimeProperty.GetValue(tradeBar);
            var symbol = symbolProperty.GetValue(tradeBar);
            var value = valueProperty.GetValue(tradeBar);
            var open = openProperty.GetValue(tradeBar);
            var high = highProperty.GetValue(tradeBar);
            var low = lowProperty.GetValue(tradeBar);

            var tradeBarArgs = Runtime.PyTuple_New(9);

            Runtime.PyTuple_SetItem(tradeBarArgs, 0, dataType.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 1, isFillForward.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 2, time.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 3, endTime.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 4, symbol.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 5, value.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 6, open.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 7, high.ToPython().Handle);
            Runtime.PyTuple_SetItem(tradeBarArgs, 8, low.ToPython().Handle);

            return tradeBarArgs;
        }
    }
}
