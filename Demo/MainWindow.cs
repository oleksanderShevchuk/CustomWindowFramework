using CustomWindowFramework.Core;

namespace CustomWindowFramework.Demo
{
    public class MainWindow : WindowBase
    {
        public MainWindow()
        {
            Text = "Custom Window Framework Demo";
            Size = new Size(900, 600);

            AddContent(new DemoScene
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            });
        }
    }
}
