using BizArk.Standard.ConsoleApp.CmdLineHelpGenerator;
using BizArk.Standard.ConsoleApp.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using cm = System.ComponentModel;

namespace BizArk.Standard.ConsoleApp.Tests
{
	[TestClass]
	public class CmdLineHelpTest
	{

		[TestMethod]
		public void GenerateUsageTest()
		{
			var parser = new CmdLineParser<TestCmdLineObj>();
			var results = parser.Parse(new string[] { });
			results.ApplicationFileName = "test.exe";
			var generator = new HelpGenerator(results);

			var usage = generator.GetUsage();
			Assert.AreEqual("test.exe <Name|String> [/Type <Father|Mother|Child>]", usage);
		}

		[TestMethod]
		public void GeneratePropHelpTest()
		{
			var parser = new CmdLineParser<TestCmdLineObj>();
			var results = parser.Parse(new string[] { });
			var generator = new HelpGenerator(results);

			var help = generator.GetPropertyHelp(results.Properties["Name"]);
			AssertEx.Contains("/Name <String> REQUIRED", help);
			AssertEx.Contains("\tTEST DESC", help);

			// Type doesn't have a description, but the flag is set to show it.
			help = generator.GetPropertyHelp(results.Properties["Type"]);
			AssertEx.Contains("/PersonType (/Type) <Father|Mother|Child>", help);
			AssertEx.Contains("\tDefault: Father", help);

			help = generator.GetPropertyHelp(results.Properties["Children"]);
			AssertEx.Contains("/Children", help);
			AssertEx.Contains("\tDefault: [\"One\", \"Two\", \"Three\"]", help);
		}

		#region TestCmdLineObj

		[CmdLineOptions(DefaultArgNames = new string[] { "Name", "Job" })]
		private class TestCmdLineObj
		{

			public TestCmdLineObj()
			{
				PersonType = PersonType.Father;
				Children = new string[] { "One", "Two", "Three" };
			}

			[Required(ErrorMessage = "TEST ERR")]
			[cm.Description("TEST DESC")]
			public string Name { get; set; }

			public int Age { get; set; }

			[CmdLineArg("Job")]
			[StringLength(10, ErrorMessage = "TEST ERR")]
			public string Occupation { get; set; }

			public bool HasHair { get; set; }

			public string[] Children { get; set; }

			[CmdLineArg("Type", ShowInUsage = true)]
			public PersonType PersonType { get; set; }
		}

		private enum PersonType
		{
			Father,
			Mother,
			Child
		}

		#endregion

	}
}
