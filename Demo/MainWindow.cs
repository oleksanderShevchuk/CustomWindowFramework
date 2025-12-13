namespace CustomWindowFramework.Demo
{
    public class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            Text = "Custom Window Framework (Bootstrap)";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 700);

            BackColor = Color.FromArgb(30, 30, 30);
        }
    }
}
