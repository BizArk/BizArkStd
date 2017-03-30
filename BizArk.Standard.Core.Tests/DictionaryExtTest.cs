using BizArk.Standard.Core.Extensions.DictionaryExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace BizArk.Standard.Core.Tests
{
	[TestClass]
    public class DictionaryExtTest
    {
        [TestMethod]
        public void ConvertValueFromPropertyBag()
        {
            var dict = new Dictionary<string, object>();
            var val = dict.TryGetValue<int>("Test");
            Assert.AreEqual(0, val);

            dict["Test"] = "123";
            val = dict.TryGetValue<int>("Test");
            Assert.AreEqual(123, val);

            val = dict.TryGetValue<int>("INVALID", -1);
            Assert.AreEqual(-1, val);
        }

        [TestMethod]
        public void ConvertValueFromIntStringDict()
        {
            var dict = new Dictionary<int, string>();
            var val = dict.TryGetValue<int, string, decimal>(123);
            Assert.AreEqual(0m, val);

            dict[123] = "1.23";
            val = dict.TryGetValue<int, string, decimal>(123);
            Assert.AreEqual(1.23m, val);

            val = dict.TryGetValue<int, string, decimal>(-1, decimal.MinValue);
            Assert.AreEqual(decimal.MinValue, val);
        }

    }
}
