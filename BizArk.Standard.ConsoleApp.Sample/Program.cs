using System;
using BizArk.Standard.ConsoleApp;

namespace BizArk.Standard.ConsoleApp.Sample
{
    class Program
    {
        static int Main(string[] args)
        {
			/* Recommended approach for building console applications. */
			// command-line: /Name "Billy Bob" /Age 23 /Occupation "Chicken Catcher" /HasHair /Children Billy Bob Sue /Status ItsComplicated /W
			return BaCon.Start<SampleConsoleApp>();

			/* A simpler example for building console applications.*/
			// command-line: /Name "Billy Bob" /BirthDate 7/19/1999 /W
			//return BaCon.Start<BasicConsoleApp>();


			/* Just want a good command-line parser to populate your POCO? Here you go.
			// command-line: /Name "Billy Bob" /Age 23 /Occupation "Chicken Catcher" /HasHair /Children Billy Bob Sue /Status ItsComplicated /W
			// ParseArgs actually returns validation information and other information as well, if you want it.
			var results = BaCon.ParseArgs<BasicArgs>();
			var myargs = results.Args;
			// BaCon.WriteHelp(results); // Displays help if you want to.
			Console.WriteLine(myargs.Name);
			Console.ReadKey(true);
			return 0;
			*/
		}
	}
}