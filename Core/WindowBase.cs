using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace CustomWindowFramework.Core
{
    public abstract class WindowBase : Form
    {
        private const int BORDER_WIDTH = 6;
        private List<WindowButton> _buttons = new List<WindowButton>();

        // DPI-aware
        private float _dpiScale => DeviceDpi / 96f;
        protected int TitleHeight => (int)(30 * _dpiScale);

        private WindowTheme _theme = WindowTheme.Dark;
        private Timer _animationTimer;

        protected WindowBase()
        {
            InitializeWindow();
            InitializeButtons();
            InitializeAnimation();
        }

        private void InitializeWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size((int)(400 * _dpiScale), (int)(300 * _dpiScale));
            BackColor = _theme.BackgroundColor;

            MouseMove += WindowBase_MouseMove;
            MouseDown += WindowBase_MouseDown;
        }

        public void SetTheme(WindowTheme theme)
        {
            _theme = theme;
            BackColor = _theme.BackgroundColor;
            Invalidate();
        }

        private void InitializeButtons()
        {
            int btnSize = TitleHeight;
            int w = ClientSize.Width;

            _buttons.Clear();
            _buttons.Add(new WindowButton(ButtonType.Close, new Rectangle(w - btnSize, 0, btnSize, btnSize)));
            _buttons.Add(new WindowButton(ButtonType.Maximize, new Rectangle(w - 2 * btnSize, 0, btnSize, btnSize)));
            _buttons.Add(new WindowButton(ButtonType.Minimize, new Rectangle(w - 3 * btnSize, 0, btnSize, btnSize)));

            foreach (var btn in _buttons)
                btn.Clicked += OnButtonClicked;
        }

        private void InitializeAnimation()
        {
            _animationTimer = new Timer { Interval = 16 }; // ~60 FPS
            _animationTimer.Tick += (s, e) => Invalidate();
            _animationTimer.Start();
        }

        private void OnButtonClicked(WindowButton btn)
        {
            switch (btn.Type)
            {
                case ButtonType.Close:
                    Close();
                    break;
                case ButtonType.Minimize:
                    WindowState = FormWindowState.Minimized;
                    break;
                case ButtonType.Maximize:
                    WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    break;
            }
        }

        private void WindowBase_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var btn in _buttons)
                if (btn.Bounds.Contains(e.Location))
                    btn.OnClick();
        }

        private void WindowBase_MouseMove(object sender, MouseEventArgs e)
        {
            bool needInvalidate = false;
            foreach (var btn in _buttons)
            {
                bool hover = btn.Bounds.Contains(e.Location);
                if (hover != btn.IsHover)
                {
                    btn.IsHover = hover;
                    needInvalidate = true;
                }
            }

            if (needInvalidate)
                Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_buttons?.Count >= 3)
            {
                int btnSize = TitleHeight;
                int w = ClientSize.Width;

                for (int i = 0; i < 3; i++)
                    _buttons[i].Bounds = new Rectangle(w - (i + 1) * btnSize, 0, btnSize, btnSize);

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // title bar
            using (Brush brush = new SolidBrush(_theme.TitleBarColor))
                g.FillRectangle(brush, 0, 0, ClientSize.Width, TitleHeight);

            // title text
            using (Brush brush = new SolidBrush(_theme.TextColor))
                g.DrawString(Text, Font, brush, 10, 5);

            // buttons
            foreach (var btn in _buttons)
                btn.Draw(g, _theme);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnableShadow();
        }

        private void EnableShadow()
        {
            if (Environment.OSVersion.Version.Major >= 6) // Vista+
            {
                MARGINS margins = new MARGINS() { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
                DwmExtendFrameIntoClientArea(Handle, ref margins);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int WM_GETMINMAXINFO = 0x0024;

            if (m.Msg == WM_GETMINMAXINFO)
            {
                base.WndProc(ref m);
                return;
            }

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);

                Point cursor = PointToClient(Cursor.Position);
                int w = ClientSize.Width;
                int h = ClientSize.Height;

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
                else if (cursor.Y < TitleHeight)
                {
                    foreach (var btn in _buttons)
                        if (btn.Bounds.Contains(cursor))
                        {
                            m.Result = (IntPtr)HTCLIENT;
                            return;
                        }

                    m.Result = (IntPtr)HTCAPTION;
                }
                else
                    m.Result = (IntPtr)HTCLIENT;

                return;
            }

            base.WndProc(ref m);
        }

        // Win32 DWM Shadow
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        // Hit-test constants
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