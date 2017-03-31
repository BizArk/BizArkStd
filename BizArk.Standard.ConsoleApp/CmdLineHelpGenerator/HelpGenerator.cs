﻿using BizArk.Standard.ConsoleApp.Parser;
using BizArk.Standard.Core;
using BizArk.Standard.Core.Extensions.ArrayExt;
using BizArk.Standard.Core.Extensions.FormatExt;
using BizArk.Standard.Core.Extensions.StringExt;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BizArk.Standard.ConsoleApp.CmdLineHelpGenerator
{

	/// <summary>
	/// Generates help text for a command-line object.
	/// </summary>
	public class HelpGenerator
	{

		#region Initialization and Destruction

		/// <summary>
		/// Creates an instance of HelpGenerator.
		/// </summary>
		/// <param name="results"></param>
		public HelpGenerator(CmdLineParseResults results)
		{
			ParseResults = results;
		}

		#endregion

		#region Fields and Properties

		/// <summary>
		/// Gets the command-line parser results that are used to generate the help.
		/// </summary>
		public CmdLineParseResults ParseResults { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the usage line.
		/// </summary>
		/// <returns></returns>
		public string GetUsage()
		{
			var usage = new StringBuilder();
			var options = ParseResults.Options;

			usage.Append(ParseResults.ApplicationFileName);

			// Display default properties first.
			var dfltProps = ParseResults.Properties.DefaultProperties
				.Where(p => p.ShowInUsage);
			foreach (var prop in dfltProps)
			{
				var name = GetUsageName(prop);
				var fmt = prop.Required ? " <{0}|{1}>" : " [<{0}|{1}>]";
				usage.AppendFormat(fmt, name, prop.PropertyType.Name);
			}

			var props = ParseResults.Properties
				.Where(p => p.ShowInUsage && !dfltProps.Contains(p));
			foreach (var prop in props)
			{
				var name = GetUsageName(prop);
				string fmt = prop.Required ? " {0}{1}{2}<{3}>" : " [{0}{1}{2}<{3}>]";
				var usageType = GetPropertyTypeDisplay(prop);
				usage.AppendFormat(fmt, options.ArgumentPrefix, name, options.AssignmentDelimiter ?? " ", usageType);
			}

			return usage.ToString();
		}

		/// <summary>
		/// Gets the display value for the usage type.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		private string GetPropertyTypeDisplay(CmdLineProperty prop)
		{			
			if (prop.PropertyType.GetTypeInfo().IsEnum)
			{
				var enumVals = string.Join("|", Enum.GetNames(prop.PropertyType));
				return enumVals;
			}
			else
				return prop.PropertyType.Name;
		}

		/// <summary>
		/// Gets the usage display name for the property.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		private string GetUsageName(CmdLineProperty prop)
		{
			if (prop.Aliases.Length == 0) return prop.Name;
			return prop.Aliases[0];
		}

		/// <summary>
		/// Gets the help text for a single property.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public string GetPropertyHelp(CmdLineProperty prop)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("{0}{1}", ParseResults.Options.ArgumentPrefix, prop.Name);
			if (prop.Aliases.Length > 0)
			{
				var aliases = string.Join(" | " + ParseResults.Options.ArgumentPrefix, prop.Aliases);
				sb.AppendFormat(" ({0}{1})", ParseResults.Options.ArgumentPrefix, aliases);
			}

			sb.AppendFormat(" <{0}>", GetPropertyTypeDisplay(prop));

			if (prop.Required)
				sb.Append(" REQUIRED");

			if (prop.Description.HasValue())
				sb.AppendFormat("\n\t{0}", prop.Description);

			var propTypeInfo = prop.PropertyType.GetTypeInfo();

			object dflt = prop.DefaultValue;
			if (!ConvertEx.IsEmpty(dflt) || propTypeInfo.IsEnum)
			{
				var arr = dflt as Array;
				if (arr != null)
				{
					var strs = arr.Convert<string>();
					if (dflt.GetType().GetElementType() == typeof(string))
						dflt = "[\"{0}\"]".Fmt(strs.Join("\", \""));
					else
						dflt = "[{0}]".Fmt(strs.Join(", "));
				}
				sb.AppendFormat("\n\tDefault: {0}", dflt);
			}

			foreach (var att in propTypeInfo.GetCustomAttributes())
			{
				var validator = att as ValidationAttribute;
				if (validator == null) continue;

				// The RequiredAttribute is handled differently.
				if (validator.GetType() == typeof(RequiredAttribute)) continue;

				string message = validator.FormatErrorMessage(prop.Name);
				sb.AppendFormat("\n\t{0}", message);
			}

			return sb.ToString();
		}

		#endregion

	}
}
