﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BizArk.Standard.Core.Extensions.FormatExt;
using System.ComponentModel;
using System.Reflection;

namespace BizArk.Standard.Core.Data
{

	/// <summary>
	/// Provides a property bag implementation as well as keeps track of what fields are modified for efficient updates.
	/// </summary>
	public class BaObject : IDictionary<string, object>, INotifyPropertyChanged
	{

		#region Initialization and Destruction

		/// <summary>
		/// Creates a new instance of BaObject.
		/// </summary>
		/// <param name="strict">If true, only fields added can be set or retrieved. If false, getting a field that doesn't exist returns null and setting a field that doesn't exist automatically adds it.</param>
		/// <param name="schema">An object that contains properties that will be used to initialize the fields of the object. Can be a DataRow, IDataReader, or POCO.</param>
		public BaObject(bool strict, object schema = null)
			: this(new BaObjectOptions(strict, schema))
		{
		}

		/// <summary>
		/// Creates a new instance of BaObject.
		/// </summary>
		/// <param name="options">The options used to setup the BaObject.</param>
		public BaObject(BaObjectOptions options = null)
		{
			Options = options ?? new BaObjectOptions();

			if (Options.Schema != null
				&& !InitSchemaFromObject(Options.Schema, true))
			{ } // Just a simple way to call the init schema methods without a bunch of if/else statements. 

		}

		/// <summary>
		/// Initializes the schema. Should be called from the constructor.
		/// </summary>
		/// <param name="schema">The object used to discover the schema.</param>
		/// <param name="setDflt">Determines if we will get the default value from the schema or not.</param>
		protected bool InitSchemaFromObject(object schema, bool setDflt)
		{
			foreach (var prop in schema.GetType().GetTypeInfo().GetProperties())
			{
				object dflt = null;
				if (setDflt)
					dflt = prop.GetValue(schema);
				Add(prop.Name, prop.PropertyType, dflt);
			}
			return true;
		}

		#endregion

		#region Fields and Properties

		/// <summary>
		/// Gets the options object used to create the BaObject.
		/// </summary>
		public BaObjectOptions Options { get; private set; }

		/// <summary>
		/// Gets the fields in this object.
		/// </summary>
		public BaFieldList Fields { get; } = new BaFieldList();

		/// <summary>
		/// Gets a value that determines if the object has changed.
		/// </summary>
		public bool HasChanged
		{
			get
			{
				foreach (var fld in Fields)
				{
					if (fld.IsChanged)
						return true;
				}
				return false;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the field to the object.
		/// </summary>
		/// <typeparam name="T">The datatype for the field.</typeparam>
		/// <param name="fldName">Name of the field.</param>
		/// <param name="dflt">Default value for the field. Used to determine if the field has changed.</param>
		/// <returns></returns>
		public BaField Add<T>(string fldName, T dflt)
		{
			if (Fields.ContainsField(fldName))
				throw new ArgumentException("A field already exists with this field name.");
			var fld = new BaField(this, fldName, typeof(T), dflt);
			Fields.Add(fld);
			return fld;
		}

		/// <summary>
		/// Adds the field to the object.
		/// </summary>
		/// <param name="fldName">Name of the field.</param>
		/// <param name="fldType">The data type for the field.</param>
		/// <param name="dflt">Default value for the field. Used to determine if the field has changed. If null, it is converted to the default value for fieldType.</param>
		/// <returns></returns>
		public BaField Add(string fldName, Type fldType, object dflt = null)
		{
			if (Fields.ContainsField(fldName))
				throw new ArgumentException("A field already exists with this field name.");
			var fld = new BaField(this, fldName, fldType, dflt);
			Fields.Add(fld);
			return fld;
		}

		private ICollection<KeyValuePair<string, object>> GetKeyValuePairs()
		{
			return null;
		}

		/// <summary>
		/// Tries to get the value. Returns true if the value is in the object or if strict is false.
		/// </summary>
		/// <param name="fldName">Name of the field to get.</param>
		/// <param name="result">The value contained in the field. Value will be null if the field is not found and strict is false.</param>
		/// <returns>True if the field is found.</returns>
		public bool TryGet(string fldName, out object result)
		{
			result = null;

			var fld = Fields[fldName];
			if (fld != null)
			{
				result = fld.Value;
				return true;
			}

			if (Options.StrictGet)
				return false;

			result = null;
			return true;
		}

		/// <summary>
		/// Tries to set the value. If strict is off and the field is not found, it will be added.
		/// </summary>
		/// <param name="fldName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TrySet(string fldName, object value)
		{
			var fld = Fields[fldName];
			if (fld != null)
			{
				Fields[fldName].Value = value;
				return true;
			}

			if (Options.StrictSet)
				return false;

			var fldType = typeof(object);
			if (value != null) fldType = value.GetType();
			Add(fldName, fldType, value);
			return true;
		}

		/// <summary>
		/// Returns a dictionary of changed values.
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, object> GetChanges()
		{
			var changes = new Dictionary<string, object>();
			foreach (var fld in Fields)
			{
				if (fld.IsChanged)
					changes.Add(fld.Name, fld.Value);
			}
			return changes;
		}

		/// <summary>
		/// Updates the default value to be the same as value so that the fields show up as not changed.
		/// </summary>
		public void UpdateDefaults()
		{
			foreach (var fld in Fields)
			{
				if (fld.IsSet) // If it's not set, Value returns the DefaultValue.
					fld.DefaultValue = fld.Value;
			}
		}

		#endregion

		#region IDictionary

		/// <summary>
		/// Gets or sets the value for the given field.
		/// </summary>
		/// <param name="fldName"></param>
		/// <returns></returns>
		public object this[string fldName]
		{
			get
			{
				object value;
				if (TryGet(fldName, out value))
					return value;

				// TryGet only returns false if strict is off.
				throw new ArgumentOutOfRangeException("key", $"Field [{fldName}] not found.");
			}
			set
			{
				// TrySet only returns false if strict is on.
				if (!TrySet(fldName, value))
					throw new ArgumentOutOfRangeException("key", $"Field [{fldName}] not found.");
			}
		}

		int ICollection<KeyValuePair<string, object>>.Count
		{
			get
			{
				return Fields.Count;
			}
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		ICollection<string> IDictionary<string, object>.Keys
		{
			get
			{
				return Fields.Select(fld => fld.Name).ToArray();
			}
		}

		ICollection<object> IDictionary<string, object>.Values
		{
			get
			{
				return Fields.Select(fld => fld.Value).ToArray();
			}
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			((IDictionary<string, object>)this).Add(item.Key, item.Value);
		}

		void IDictionary<string, object>.Add(string key, object value)
		{
			var fldType = typeof(object);
			if (value != null)
				fldType = value.GetType();
			var fld = Add(key, fldType);
			fld.Value = value;
		}

		void ICollection<KeyValuePair<string, object>>.Clear()
		{
			throw new NotSupportedException("Cannot clear list of fields.");
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			var fld = Fields[item.Key];
			if (fld == null) return false;
			return fld.Value == item.Value;
		}

		bool IDictionary<string, object>.ContainsKey(string key)
		{
			return Fields.ContainsField(key);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			var pairs = GetKeyValuePairs();
			return pairs.GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			var pairs = GetKeyValuePairs();
			return pairs.GetEnumerator();
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			var fld = Fields[item.Key];
			if (fld == null) return false;
			if (fld.Value != item.Value) return false;
			return Fields.Remove(fld);
		}

		bool IDictionary<string, object>.Remove(string key)
		{
			var fld = Fields[key];
			if (fld == null) return false;
			return Fields.Remove(fld);
		}

		bool IDictionary<string, object>.TryGetValue(string key, out object value)
		{
			return TryGet(key, out value);
		}

		#endregion

		#region INotifyPropertyChanged

		/// <summary>
		/// Event raised when a field changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="fldName"></param>
		protected internal void OnPropertyChanged(string fldName)
		{
			var e = new PropertyChangedEventArgs(fldName);
			OnPropertyChanged(e);
		}

		#endregion

	}

}
