using CustomWindowFramework.Demo;

namespace CustomWindowFramework.App
{
    public sealed class AppContext : ApplicationContext
    {
        private Form _mainWindow;

        public AppContext()
        {
            CreateMainWindow();
        }

        private void CreateMainWindow()
        {
            _mainWindow = new MainWindow();
            _mainWindow.FormClosed += OnMainWindowClosed;
            _mainWindow.Show();
        }

        private void OnMainWindowClosed(object? sender, FormClosedEventArgs e)
        {
            ExitThread();
        }
    }
}
