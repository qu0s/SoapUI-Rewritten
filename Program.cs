using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SoapUI.UI;

namespace SoapUI
{
    /// <summary>
    /// Application entry point. Keeps Main() minimal - all startup
    /// behaviour lives in <see cref="MainUI"/>.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if !DEBUG
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainUI());
        }

#if !DEBUG
        private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
        {
            var resourceName = "SoapUI." + new AssemblyName(args.Name).Name + ".dll";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return Assembly.Load(memoryStream.ToArray());
                }
            }
        }
#endif
    }
}
