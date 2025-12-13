namespace CustomWindowFramework.Core
{
    public enum ButtonType { Close, Minimize, Maximize }

    public class WindowButton
    {
        public Rectangle Bounds;
        public ButtonType Type;

        public bool IsHover;
        public bool IsPressed;

        private Color _currentColor;
        private Color _targetColor;

        public event Action<WindowButton>? Clicked;

        public WindowButton(ButtonType type, Rectangle bounds)
        {
            Type = type;
            Bounds = bounds;
            _currentColor = Color.Transparent;
            _targetColor = Color.Transparent;
        }

        public void UpdateColor(WindowTheme theme)
        {
            _targetColor = IsPressed ? theme.ButtonPressed :
                           IsHover ? theme.ButtonHover :
                           theme.ButtonNormal;

            _currentColor = LerpColor(_currentColor, _targetColor, 0.2f);
        }

        public void Draw(Graphics g, WindowTheme theme)
        {
            UpdateColor(theme);

            using (Brush brush = new SolidBrush(_currentColor))
                g.FillRectangle(brush, Bounds);

            using (Pen pen = new Pen(theme.TextColor, 2))
            {
                int cx = Bounds.X + Bounds.Width / 2;
                int cy = Bounds.Y + Bounds.Height / 2;

                switch (Type)
                {
                    case ButtonType.Close:
                        g.DrawLine(pen, cx - 4, cy - 4, cx + 4, cy + 4);
                        g.DrawLine(pen, cx + 4, cy - 4, cx - 4, cy + 4);
                        break;
                    case ButtonType.Minimize:
                        g.DrawLine(pen, Bounds.X + 4, cy, Bounds.Right - 4, cy);
                        break;
                    case ButtonType.Maximize:
                        g.DrawRectangle(pen, Bounds.X + 4, Bounds.Y + 4, Bounds.Width - 8, Bounds.Height - 8);
                        break;
                }
            }
        }

        public void OnClick() => Clicked?.Invoke(this);

        private static Color LerpColor(Color from, Color to, float t)
        {
            int r = (int)(from.R + (to.R - from.R) * t);
            int g = (int)(from.G + (to.G - from.G) * t);
            int b = (int)(from.B + (to.B - from.B) * t);
            int a = (int)(from.A + (to.A - from.A) * t);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
