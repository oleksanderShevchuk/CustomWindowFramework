namespace CustomWindowFramework.Core
{
    public enum ButtonType { Close, Minimize, Maximize }

    public class WindowButton
    {
        public Rectangle Bounds;
        public ButtonType Type;
        public bool IsHover;
        public bool IsPressed;

        public event Action<WindowButton>? Clicked;

        public WindowButton(ButtonType type, Rectangle bounds)
        {
            Type = type;
            Bounds = bounds;
        }

        public void Draw(Graphics g)
        {
            Color bg = IsHover ? Color.FromArgb(50, 50, 50) : Color.FromArgb(40, 40, 40);
            if (IsPressed) bg = Color.FromArgb(70, 70, 70);

            using (Brush brush = new SolidBrush(bg))
                g.FillRectangle(brush, Bounds);

            using (Pen pen = new Pen(Color.White, 2))
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
    }
}
