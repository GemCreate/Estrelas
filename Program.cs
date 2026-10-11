namespace Estrelas
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetColorMode(SystemColorMode.Dark);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
          //  Application.Run(new Form2());
              Application.Run(new Form1(args.Length > 0 ? args[0] : ""));
        }
    }
}