namespace CustomWindowFramework.Core
{
    public abstract class WindowBase : Form
    {
        protected WindowBase()
        {
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            SetStyle(ControlStyles.ResizeRedraw, true);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(400, 300);
            BackColor = Color.FromArgb(32, 32, 32);
        }
    }
}
