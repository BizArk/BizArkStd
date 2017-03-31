using BizArk.Standard.Core.Data;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace BizArk.Standard.Core.Tests
{

	[TestClass]
	public class BaObjectTest
	{

		[TestMethod]
		public void CreateStrictBaObject()
		{
			var obj = new BaObject(true);
			var fld = obj.Add("Test", 123);
			Assert.AreEqual("Test", fld.Name);
			Assert.AreEqual(123, fld.Value);
			Assert.AreEqual(123, fld.DefaultValue);
			Assert.AreEqual(typeof(int), fld.FieldType);
			Assert.IsFalse(fld.IsChanged);
			Assert.IsFalse(fld.IsSet);
			fld.Value = fld.Value;
			Assert.IsTrue(fld.IsSet); // Even though we are setting it to itself, it should still be set now (just not changed).
			Assert.IsFalse(fld.IsChanged);
			fld.Value = 321;
			Assert.IsTrue(fld.IsChanged);

			AssertEx.Throws<ArgumentOutOfRangeException>(() => { var x = obj["INVALID"]; });
		}

		[TestMethod]
		public void CreateBaObjectWithSchema()
		{
			var obj = new BaObject(true, new { Test = 123 });
			var fld = obj.Fields["Test"];
			Assert.IsNotNull(fld);
			obj["Test"] = 123;
			Assert.AreEqual("Test", fld.Name);
			Assert.AreEqual(123, fld.Value);
			Assert.AreEqual(123, fld.DefaultValue);
			Assert.AreEqual(typeof(int), fld.FieldType);
			Assert.IsTrue(fld.IsSet);
			Assert.IsFalse(fld.IsChanged);

			AssertEx.Throws<ArgumentOutOfRangeException>(() => { var x = obj["INVALID"]; });
		}

		[TestMethod]
		public void CreateRelaxedBaObject()
		{
			var obj = new BaObject(false);
			Assert.IsNull(obj["INVALID"]);

			obj["VALID"] = 123;
			Assert.AreEqual(123, obj["VALID"]);
		}

		[TestMethod]
		public void CreateMixedBaObject()
		{
			var obj = new BaObject(new BaObjectOptions() { StrictSet = false, StrictGet = true });
			object test;
			AssertEx.Throws<ArgumentOutOfRangeException>(() => { test = obj["Test"]; });
			obj["Test"] = 123;
			Assert.AreEqual(123, obj["Test"]);
		}

		[TestMethod]
		public void BaObjectChanges()
		{
			var obj = new BaObject(true, new { Name = "", Greeting = "" });

			var changes = obj.GetChanges();
			Assert.AreEqual(0, changes.Count);

			obj["Name"] = "John";
			changes = obj.GetChanges();
			Assert.AreEqual(1, changes.Count);
			Assert.AreEqual("John", changes["Name"]);

			obj["Greeting"] = "Hello";
			changes = obj.GetChanges();
			Assert.AreEqual(2, changes.Count);
			Assert.AreEqual("John", changes["Name"]);
			Assert.AreEqual("Hello", changes["Greeting"]);

			obj["Greeting"] = "";
			changes = obj.GetChanges();
			Assert.AreEqual(1, changes.Count);
			Assert.IsFalse(changes.ContainsKey("Greeting"));

			obj.UpdateDefaults();
			changes = obj.GetChanges();
			Assert.AreEqual(0, changes.Count);

		}

		[TestMethod]
		public void SetValueToDifferentType()
		{
			var obj = new BaObject(true, new { Str = "", Base = new MyBaseObject(), Derived = new MyDerivedObject() });

			AssertEx.Throws<InvalidOperationException>(() => { obj["Str"] = 123; });
			obj["Base"] = new MyDerivedObject();
			AssertEx.Throws<InvalidOperationException>(() => { obj["Derived"] = new MyBaseObject(); });
		}

		[TestMethod]
		public void CustomBaObject()
		{
			var obj = new MyObject();
			obj.Name = "Bart";
			var changes = obj.GetChanges();
			Assert.AreEqual(1, changes.Count);
			Assert.IsTrue(changes.ContainsKey(nameof(obj.Name)));
			obj.Greeting = "Greetings";
			Assert.AreEqual("Bart", obj.Name);
			Assert.AreEqual("Greetings", obj.Greeting);

			var fld = obj.Fields[nameof(obj.Name)];
			Assert.IsNotNull(fld);
			Assert.AreEqual(typeof(string), fld.FieldType);
		}

		[TestMethod]
		public void PropertyChangedEvent()
		{
			var obj = new BaObject(true, new { Name = (string)null, Greeting = (string)null });
			var lastChanged = (string)null;
			obj.PropertyChanged += (sender, e) =>
			{
				lastChanged = e.PropertyName;
				Debug.WriteLine($"Field '{e.PropertyName}' changed.");
			};

			obj["Name"] = "John";
			Assert.AreEqual("Name", lastChanged);
		}

		#region MyObject

		private class MyObject : BaObject
		{
			public MyObject() : base(true)
			{
				// Initialize the schema from this object, but don't get 
				// default values (that would cause the class to call the 
				// properties which would fail).
				InitSchemaFromObject(this, false);
			}

			public string Name
			{
				get { return (string)this[nameof(Name)]; }
				set { this[nameof(Name)] = value; }
			}

			public string Greeting
			{
				get { return (string)this[nameof(Greeting)]; }
				set { this[nameof(Greeting)] = value; }
			}

		}

		#endregion

		#region Inherited Classes

		private class MyBaseObject
		{

		}

		private class MyDerivedObject: MyBaseObject
		{

		}

		#endregion

	}
}
