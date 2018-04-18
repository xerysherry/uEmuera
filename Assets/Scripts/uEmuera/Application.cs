using System;

namespace uEmuera
{
    public static class Application
    {
        internal static void EnableVisualStyles()
        {
            uEmuera.Logger.Info("Application.EnableVisualStyles");
        }

        internal static void SetCompatibleTextRenderingDefault(bool v)
        {
            uEmuera.Logger.Info("Application.SetCompatibleTextRenderingDefault");
        }

        internal static void Run(uEmuera.Window.MainWindow win)
        {
            uEmuera.Logger.Info("Application.Run");
            win.Init();
        }
    }
}
