namespace CustomWindowFramework.Core
{
    public enum ButtonType { Close, Minimize, Maximize }

    public class WindowButton : IInputElement, IAnimatable
    {
        public Rectangle Bounds { get; set; }
        public ButtonType Type;

        private bool _hover;
        private bool _pressed;

        private Color _currentColor;
        private Color _targetColor;

        private float _hoverAnim;
        private float _pressAnim;

        public event Action<WindowButton>? Clicked;

        public WindowButton(ButtonType type, Rectangle bounds)
        {
            Type = type;
            Bounds = bounds;
            _currentColor = Color.Transparent;
            _targetColor = Color.Transparent;
        }

        public void OnMouseEnter() => _hover = true;
        public void OnMouseLeave() => _hover = false;

        public void OnMouseDown(Point p) => _pressed = true;

        public void OnMouseUp(Point p)
        {
            if (_pressed && Bounds.Contains(p))
                Clicked?.Invoke(this);

            _pressed = false;
        }

        public bool Tick(float dt)
        {
            bool dirty = false;

            dirty |= Animate(ref _hoverAnim, _hover ? 1f : 0f, dt * 10f);
            dirty |= Animate(ref _pressAnim, _pressed ? 1f : 0f, dt * 15f);

            return dirty;
        }

        private static bool Animate(ref float value, float target, float speed)
        {
            float prev = value;
            value += (target - value) * speed;
            value = Math.Clamp(value, 0f, 1f);
            return Math.Abs(prev - value) > 0.001f;
        }

        public void UpdateColor(WindowTheme theme)
        {
            _targetColor = _pressed ? theme.ButtonPressed :
                           _hover ? theme.ButtonHover :
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
