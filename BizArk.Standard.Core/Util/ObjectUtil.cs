﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BizArk.Standard.Core.Extensions.StringExt;
using System.Diagnostics;
using System.Reflection;

namespace BizArk.Standard.Core.Util
{

	/// <summary>
	/// Provides utility methods for working with objects.
	/// </summary>
	public static class ObjectUtil
	{

		/// <summary>
		/// Converts the object to a dictionary with name/value pairs.
		/// </summary>
		/// <param name="obj">Can be a POCO, DataRow, IDataReader.</param>
		/// <returns></returns>
		public static IDictionary<string, object> ToPropertyBag(object obj)
		{
			// Check to see if the object is already a property bag.
			var dict = obj as IDictionary<string, object>;
			if (dict != null) return dict;

			if (TryPropertyBagFromDictionary(obj as IDictionary, out dict))
				return dict;

			// Provides access to sub properties as well.
			return new ObjectDictionary(obj);
		}

		private static bool TryPropertyBagFromDictionary(IDictionary props, out IDictionary<string, object> dict)
		{
			dict = null;
			if (props == null) return false;

			if (props.Count == 0)
			{
				dict = new Dictionary<string, object>();
				return true;
			}

			dict = new Dictionary<string, object>();
			foreach (var key in props.Keys)
			{
				var name = key as string;
				if (name == null)
				{
					// All the keys must be of type string for this to work.
					dict = null;
					return false;
				}
				dict.Add(name, props[key]);
			}
			return true;
		}

		/// <summary>
		/// Gets the value from the object with the given name. Uses . notation to find values in object graphs. Supports array syntax as well, but does not support method calls. If any of the properties in the chain are null, dflt is returned. Uses ConvertEx to return the value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="dflt"></param>
		/// <returns></returns>
		public static T GetValue<T>(object obj, string key, T dflt = default(T))
		{
			var val = GetValue(obj, key);
			if (val == null)
				return dflt;
			else
				return ConvertEx.To<T>(val);
		}

		/// <summary>
		/// Gets the value from the object with the given name. Uses . notation to find values in object graphs. Supports array syntax as well, but does not support method calls. If any of the properties in the chain are null, null is returned.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key">Property name chain. Use . to separate properties. Use [#] for array lookup. Use [KEY] (quotes not required) for dictionary lookup.</param>
		/// <returns></returns>
		public static object GetValue(object obj, string key)
		{
			object value;
			if (TryGetValue(obj, key, out value))
				return value;
			else
				throw new ArgumentException($"Invalid property name in '{key}'.");
		}

		/// <summary>
		/// Tries to get the value. Returns false if the key is not valid. If we simply can't reach the object in question, returns true, but value will be null.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="dflt">Default value if unable to get the value.</param>
		/// <returns></returns>
		public static bool TryGetValue<T>(object obj, string key, out T value, T dflt = default(T))
		{
			value = dflt;

			object objVal;
			if (!TryGetValue(obj, key, out objVal))
				return false;

			if (objVal != null)
				value = ConvertEx.To<T>(objVal);

			return true;
		}

		/// <summary>
		/// Tries to get the value. Returns false if the key is not valid. If we simply can't reach the object in question, returns true, but value will be null.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryGetValue(object obj, string key, out object value)
		{
			value = null;

			if (key.IsEmpty())
				throw new ArgumentNullException("key");

			if (obj == null) return true;

			var propertyNames = key.Split('.');
			var currentObj = obj;
			foreach (var name in propertyNames)
			{
				// Need to put it in a variable because we will need to change it if it includes an index.
				var propName = name; 

				// Check to see if using an index of some type (string or numeric).
				var idx = propName.IndexOf('[');
				if (idx == 0)
					throw new InvalidOperationException($"{propName} is not a supported property name. Property names cannot start with '['");

				string indexer = null;
				if (idx > 0)
				{
					if (!propName.EndsWith("]"))
						throw new InvalidOperationException($"{propName} is not a supported property name. Property names that include an indexer must end with ']'");
					// Get the index (numeric or string)
					indexer = propName.Substring(idx + 1, propName.Length - idx - 2);
					// Get just the name of the property without the index.
					propName = propName.Substring(0, idx);
				}

				var prop = FindProperty(currentObj, propName);
				if (prop == null)
					return false;

				currentObj = prop.GetValue(currentObj);
				if (currentObj == null) return true;

				// Check to see if we need to get an indexed value.
				if (indexer != null)
				{
					currentObj = GetIndexedValue(currentObj, indexer);
					if (currentObj == null) return true;
				}

			}

			value = currentObj;
			return true;
		}

		private static object GetIndexedValue(object obj, string indexer)
		{
			var list = obj as IList;
			if (list != null)
			{
				int idx;
				if (!int.TryParse(indexer, out idx))
					throw new ArgumentException($"'[{indexer}]' is not a valid index argument for an array.");
				return list[idx];
			}

			var dict = obj as IDictionary;
			if (dict != null)
				return dict[indexer];

			throw new ArgumentException($"Type {obj.GetType().FullName} is not a supported type for an indexed property. Only IList and IDictionary objects are supported.");
		}

		private static PropertyInfo FindProperty(object obj, string name)
		{
			return obj.GetType().GetRuntimeProperty(name);
		}

	}
}
