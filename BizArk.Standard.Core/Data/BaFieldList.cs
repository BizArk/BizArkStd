﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizArk.Standard.Core.Data
{
	/// <summary>
	/// List of BizArk fields.
	/// </summary>
	public class BaFieldList : IReadOnlyList<BaField>
	{

		#region Initialization and Destruction

		/// <summary>
		/// Creates an instance of BaFieldList.
		/// </summary>
		public BaFieldList()
		{

		}

		#endregion

		#region Fields and Properties

		private List<BaField> mFields = new List<BaField>();

		/// <summary>
		/// Gets the field with the given name.
		/// </summary>
		/// <param name="fldName"></param>
		/// <returns></returns>
		public BaField this[string fldName]
		{
			get
			{
				foreach (var fld in mFields)
				{
					if (fld.Name == fldName)
						return fld;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets the field at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public BaField this[int index]
		{
			get
			{
				return mFields[index];
			}
		}

		/// <summary>
		/// Gets the number of fields in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return mFields.Count;
			}
		}

		#endregion

		#region Methods

		internal void Add(BaField fld)
		{
			mFields.Add(fld);
		}

		/// <summary>
		/// Determines if the field exists in the list.
		/// </summary>
		/// <param name="fldName"></param>
		/// <returns></returns>
		public bool ContainsField(string fldName)
		{
			foreach (var fld in mFields)
			{
				if (fld.Name == fldName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the enumerator for the fields.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<BaField> GetEnumerator()
		{
			return mFields.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal bool Remove(BaField fld)
		{
			return mFields.Remove(fld);
		}

		#endregion

	}
}
