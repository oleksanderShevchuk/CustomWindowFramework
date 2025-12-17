namespace CustomWindowFramework.Core
{
    internal sealed class SnapPreviewForm : Form
    {
        public SnapPreviewForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Enabled = false;
            BackColor = Color.FromArgb(0, 120, 215);
            Opacity = 0.25;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }
    }
}
