using System;
using System.IO;
using System.Reflection;
using BizArk.Standard.Core.Extensions.StringExt;
using BizArk.Standard.Core.Util;
using System.Text;

namespace BizArk.Standard.Core
{
    /// <summary>
    /// Primary class for application information and plugin support.
    /// </summary>
    public static class Application
    {
        static Application()
        {
            var asm = Assembly.GetEntryAssembly();

            if (asm == null) return;

            var titleAtt = asm.GetCustomAttribute<AssemblyTitleAttribute>();
            if (titleAtt != null)
                Title = titleAtt.Title;

            var descAtt = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (descAtt != null)
                Description = descAtt.Description;

            var companyAtt = asm.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (companyAtt != null)
                Company = companyAtt.Company;

            var productAtt = asm.GetCustomAttribute<AssemblyProductAttribute>();
            if (productAtt != null)
                Product = productAtt.Product;

            var copyrightAtt = asm.GetCustomAttribute<AssemblyCopyrightAttribute>();
            if (copyrightAtt != null)
                Copyright = copyrightAtt.Copyright;

            var trademarkAtt = asm.GetCustomAttribute<AssemblyTrademarkAttribute>();
            if (trademarkAtt != null)
                Trademark = trademarkAtt.Trademark;

            Version = asm.GetName().Version;

            Uri uri = new Uri(asm.CodeBase);
            if (uri.Scheme == "file")
                ExePath = uri.LocalPath + uri.Fragment;
            else
                ExePath = uri.ToString();

            ExeName = System.IO.Path.GetFileName(ExePath);
        }

        /// <summary>
        /// Gets the title of the executing assembly from AssemblyTitleAttribute.
        /// </summary>
        public static string Title { get; private set; }

        /// <summary>
        /// Gets the version of the executing assembly.
        /// </summary>
        public static Version Version { get; private set; }

        /// <summary>
        /// Gets the description of the executing assembly from AssemblyDescriptionAttribute.
        /// </summary>
        public static string Description { get; private set; }

        /// <summary>
        /// Gets the company name of the executing assembly from AssemblyCompanyAttribute.
        /// </summary>
        public static string Company { get; private set; }

        /// <summary>
        /// Gets the product name of the executing assembly from AssemblyProductAttribute.
        /// </summary>
        public static string Product { get; private set; }

        /// <summary>
        /// Gets the copyright of the executing assembly from AssemblyCopyrightAttribute.
        /// </summary>
        public static string Copyright { get; private set; }

        /// <summary>
        /// Gets the trademark of the executing assembly from AssemblyTrademarkAttribute.
        /// </summary>
        public static string Trademark { get; private set; }

        /// <summary>
        /// Gets the path the the executing assembly.
        /// </summary>
        public static string ExePath { get; private set; }

        /// <summary>
        /// Gets the just the name of the exe (without the extension).
        /// </summary>
        public static string ExeName { get; private set; }

        /// <summary>
        /// Returns an absolute path relative to the ExePath.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string GetPath(string relativePath)
        {
            var dirPath = System.IO.Path.GetDirectoryName(ExePath);
            return System.IO.Path.Combine(dirPath, relativePath);
        }

        /// <summary>
        /// Gets the path to the temporary directory for this application. This is a subdirectory off of the system temp directory.
        /// </summary>
        /// <returns></returns>
        public static string GetTempPath()
        {
            string tempPath = Path.GetTempPath();
            if (!ExeName.IsEmpty())
                tempPath = Path.Combine(tempPath, ExeName);

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            return tempPath;
        }

        /// <summary>
        /// Removes the temp directory for this application.
        /// </summary>
        public static void CleanTempDirectory()
        {
            var tempPath = GetTempPath();
            if (!Directory.Exists(tempPath)) return;
            FileEx.RemoveDirectory(tempPath);
        }

		/// <summary>
		/// Gets or sets the default encoding. Defaults to UTF7.
		/// </summary>
		public static Encoding DefaultEncoding { get; set; } = Encoding.UTF7;

    }
}
