﻿using BizArk.Standard.Core;
using System;

namespace BizArk.Standard.ConsoleApp
{

	/// <summary>
	/// Command-line object argument attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CmdLineArgAttribute : Attribute
	{

		/// <summary>
		/// Creates an instance of CmdLineArgAttribute.
		/// </summary>
		public CmdLineArgAttribute()
		{
			ShowInHelp = true;
		}

		/// <summary>
		/// Creates an instance of CmdLineArgAttribute.
		/// </summary>
		/// <param name="alias"></param>
		public CmdLineArgAttribute(string alias)
			: this()
		{
			Aliases = new string[] { alias };
		}

		/// <summary>
		/// Gets or sets a list of aliases to use for the command-line property.
		/// </summary>
		public string[] Aliases { get; set; }

		/// <summary>
		/// Gets or sets a flag that determines if the property will be displayed in the help.
		/// </summary>
		public bool ShowInHelp { get; set; }

		// Attributes do not support null properties, so we need to make the field visible internally.
		internal bool? mShowDefaultValue = null;
		/// <summary>
		/// Gets or sets a value that determines if the default value is displayed in the help.
		/// </summary>
		public bool ShowDefaultValue
		{
			get { return mShowDefaultValue ?? false; }
			set { mShowDefaultValue = value; }
		}

		// Attributes do not support null properties, so we need to make the field visible internally.
		internal bool? mShowInUsage = null;
		/// <summary>
		/// Gets or sets a value that determines if the property is displayed in the usage.
		/// </summary>
		public bool ShowInUsage
		{
			get { return mShowInUsage ?? false; }
			set { mShowInUsage = value; }
		}
	}

}
