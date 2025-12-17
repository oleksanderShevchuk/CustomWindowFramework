using Microsoft.Win32;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace CustomWindowFramework.Core
{
    public abstract class WindowBase : Form
    {
        protected Panel ContentHost;
        private readonly InputManager _input = new();
        private readonly List<WindowButton> _buttons = new();
        private WindowTheme _theme;
        private Timer _animationTimer;
        private bool _draggingWindow;
        private Point _windowDragOffset;
        private SnapState _snapState = SnapState.None;
        private Rectangle _restoreBounds;
        private bool _restoreArmed;
        private SnapPreviewForm? _snapPreview;

        private float _dpiScale => DeviceDpi / 96f;
        private int TitleHeight => Math.Max((int)(30 * _dpiScale), Font.Height + (int)(10 * _dpiScale));
        private int BorderWidth => (int)(6 * _dpiScale);

        protected WindowBase()
        {
            _theme = IsSystemInDarkMode() ? WindowTheme.Dark : WindowTheme.Light;

            InitializeWindow();
            InitializeButtons();
            InitializeAnimation();
            SubscribeSystemThemeChange();
        }

        private void InitializeWindow()
        {
            ContentHost = new Panel
            {
                BackColor = _theme.BackgroundColor,
                Enabled = false
            };

            base.Controls.Add(ContentHost);

            FormBorderStyle = FormBorderStyle.None;

            SetStyle(
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint,
                true);

            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size((int)(400 * _dpiScale), (int)(300 * _dpiScale));
            BackColor = _theme.BackgroundColor;

            _snapPreview = new SnapPreviewForm();

            MouseMove += OnMouseMoveInternal;
            MouseDown += OnMouseDownInternal;
        }

        protected void AddContent(Control control)
        {
            ContentHost.Controls.Add(control);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            ContentHost.Bounds = new Rectangle(
                0,
                TitleHeight,
                ClientSize.Width,
                ClientSize.Height - TitleHeight);
        }

        #region Theme

        private void SubscribeSystemThemeChange()
        {
            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                if (e.Category == UserPreferenceCategory.General)
                    SetTheme(IsSystemInDarkMode() ? WindowTheme.Dark : WindowTheme.Light);
            };
        }

        public void SetTheme(WindowTheme theme)
        {
            _theme = theme;
            BackColor = _theme.BackgroundColor;
            Invalidate();
        }

        private static bool IsSystemInDarkMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
            }
            catch
            {
                return true;
            }
        }

        #endregion

        #region Buttons / Input

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

        private void OnMouseMoveInternal(object? sender, MouseEventArgs e)
        {
            if (_draggingWindow)
            {
                Location = new Point(
                    Location.X + e.X - _windowDragOffset.X,
                    Location.Y + e.Y - _windowDragOffset.Y
                );

                DetectSnap();
                return;
            }

            _input.ProcessMouseMove(e.Location, _buttons);
        }

        private void OnMouseDownInternal(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (e.Y < TitleHeight && !_buttons.Any(b => b.Bounds.Contains(e.Location)))
            {
                _draggingWindow = true;
                _windowDragOffset = e.Location;
                Capture = true;
                return;
            }

            _input.ProcessMouseDown(e.Location, _buttons);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_draggingWindow)
            {
                _draggingWindow = false;
                Capture = false;

                HideSnapPreview();

                if (_snapState != SnapState.None)
                    ApplySnapFinal();

                return;
            }

            _input.ProcessMouseUp(e.Location);
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

        private void DetectSnap()
        {
            Screen screen = Screen.FromHandle(Handle);
            Rectangle work = screen.WorkingArea;

            if (Top <= work.Top + SNAP_THRESHOLD)
            {
                ApplySnap(SnapState.Maximized, work);
                return;
            }

            if (Left <= work.Left + SNAP_THRESHOLD)
            {
                ApplySnap(SnapState.Left, work);
                return;
            }

            if (Right >= work.Right - SNAP_THRESHOLD)
            {
                ApplySnap(SnapState.Right, work);
                return;
            }

            if (_snapState != SnapState.None)
            {
                int dx = Math.Abs(Cursor.Position.X - (_restoreBounds.Left + _restoreBounds.Width / 2));
                int dy = Math.Abs(Cursor.Position.Y - (_restoreBounds.Top + 10));

                if (dx > SNAP_THRESHOLD * 2 || dy > SNAP_THRESHOLD * 2)
                    RestoreFromSnap();
            }
        }

        private void ApplySnap(SnapState state, Rectangle work)
        {
            if (_snapState == state)
                return;

            if (_snapState == SnapState.None)
                _restoreBounds = Bounds;

            _snapState = state;
            _restoreArmed = true;

            Rectangle previewBounds;

            switch (state)
            {
                case SnapState.Maximized:
                    previewBounds = work;
                    break;

                case SnapState.Left:
                    previewBounds = new Rectangle(
                        work.Left,
                        work.Top,
                        work.Width / 2,
                        work.Height);
                    break;

                case SnapState.Right:
                    previewBounds = new Rectangle(
                        work.Left + work.Width / 2,
                        work.Top,
                        work.Width / 2,
                        work.Height);
                    break;

                default:
                    return;
            }

            ShowSnapPreview(previewBounds);
        }

        private void ShowSnapPreview(Rectangle bounds)
        {
            if (_snapPreview == null)
                return;

            _snapPreview.Bounds = bounds;

            if (!_snapPreview.Visible)
                _snapPreview.Show();
        }

        private void RestoreFromSnap()
        {
            if (_snapState == SnapState.None || !_restoreArmed)
                return;

            _snapState = SnapState.None;
            Bounds = _restoreBounds;
            _restoreArmed = false;

            HideSnapPreview();
        }

        private void HideSnapPreview()
        {
            if (_snapPreview?.Visible == true)
                _snapPreview.Hide();
        }

        private void ApplySnapFinal()
        {
            Screen screen = Screen.FromHandle(Handle);
            Rectangle work = screen.WorkingArea;

            switch (_snapState)
            {
                case SnapState.Maximized:
                    Bounds = work;
                    break;

                case SnapState.Left:
                    Bounds = new Rectangle(
                        work.Left,
                        work.Top,
                        work.Width / 2,
                        work.Height);
                    break;

                case SnapState.Right:
                    Bounds = new Rectangle(
                        work.Left + work.Width / 2,
                        work.Top,
                        work.Width / 2,
                        work.Height);
                    break;
            }
        }
        #endregion

        #region Animation

        private void InitializeAnimation()
        {
            _animationTimer = new Timer { Interval = 16 };
            _animationTimer.Tick += (_, _) => Invalidate();
            _animationTimer.Start();
        }
        #endregion

        #region Paint
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
            //base.OnPaint(e);
            var g = e.Graphics;

            using var titleBrush = new SolidBrush(_theme.TitleBarColor);
            g.FillRectangle(titleBrush, 0, 0, ClientSize.Width, TitleHeight);

            using var textBrush = new SolidBrush(_theme.TextColor);
            g.DrawString(Text, Font, textBrush, 10, 6);

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
            if (Environment.OSVersion.Version.Major >= 6)
            {
                var m = new MARGINS { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
                DwmExtendFrameIntoClientArea(Handle, ref m);
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
                if (WindowState == FormWindowState.Maximized)
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                Point cursor = PointToClient(Cursor.Position);
                int w = ClientSize.Width;
                int h = ClientSize.Height;

                if (cursor.X < BorderWidth && cursor.Y < BorderWidth)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (cursor.X > w - BorderWidth && cursor.Y < BorderWidth)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (cursor.X < BorderWidth && cursor.Y > h - BorderWidth)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (cursor.X > w - BorderWidth && cursor.Y > h - BorderWidth)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (cursor.X < BorderWidth)
                    m.Result = (IntPtr)HTLEFT;
                else if (cursor.X > w - BorderWidth)
                    m.Result = (IntPtr)HTRIGHT;
                else if (cursor.Y < BorderWidth)
                    m.Result = (IntPtr)HTTOP;
                else if (cursor.Y > h - BorderWidth)
                    m.Result = (IntPtr)HTBOTTOM;
                else if (cursor.Y < TitleHeight)
                {
                    foreach (var btn in _buttons)
                        if (btn.Bounds.Contains(cursor))
                        {
                            m.Result = (IntPtr)HTCLIENT;
                            return;
                        }

                    m.Result = (IntPtr)HTCLIENT;
                }
                else
                    m.Result = (IntPtr)HTCLIENT;

                return;
            }

            base.WndProc(ref m);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

        #endregion
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
        private const int SNAP_THRESHOLD = 20;
    }
}