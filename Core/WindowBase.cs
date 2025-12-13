namespace CustomWindowFramework.Core
{
    public abstract class WindowBase : Form
    {
        private const int BORDER_WIDTH = 6;

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

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                Point cursor = PointToClient(Cursor.Position);
                int w = ClientSize.Width;
                int h = ClientSize.Height;

                // top title-bar
                int captionHeight = 30;

                // resize at the edges
                if (cursor.X < BORDER_WIDTH && cursor.Y < BORDER_WIDTH)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (cursor.X > w - BORDER_WIDTH && cursor.Y < BORDER_WIDTH)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (cursor.X < BORDER_WIDTH && cursor.Y > h - BORDER_WIDTH)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (cursor.X > w - BORDER_WIDTH && cursor.Y > h - BORDER_WIDTH)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (cursor.X < BORDER_WIDTH)
                    m.Result = (IntPtr)HTLEFT;
                else if (cursor.X > w - BORDER_WIDTH)
                    m.Result = (IntPtr)HTRIGHT;
                else if (cursor.Y < BORDER_WIDTH)
                    m.Result = (IntPtr)HTTOP;
                else if (cursor.Y > h - BORDER_WIDTH)
                    m.Result = (IntPtr)HTBOTTOM;
                else if (cursor.Y < captionHeight)
                    m.Result = (IntPtr)HTCAPTION;
                else
                    m.Result = (IntPtr)HTCLIENT;

                return;
            }

            base.WndProc(ref m);
        }

        // Win32 hit-test constants
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
    }
}
