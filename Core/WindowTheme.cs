namespace CustomWindowFramework.Core
{
    public class WindowTheme
    {
        public Color TitleBarColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ButtonNormal { get; set; }
        public Color ButtonHover { get; set; }
        public Color ButtonPressed { get; set; }
        public Color TextColor { get; set; }

        public static WindowTheme Light => new WindowTheme
        {
            TitleBarColor = Color.FromArgb(240, 240, 240),
            BackgroundColor = Color.White,
            ButtonNormal = Color.FromArgb(200, 200, 200),
            ButtonHover = Color.FromArgb(180, 180, 180),
            ButtonPressed = Color.FromArgb(150, 150, 150),
            TextColor = Color.Black
        };

        public static WindowTheme Dark => new WindowTheme
        {
            TitleBarColor = Color.FromArgb(40, 40, 40),
            BackgroundColor = Color.FromArgb(32, 32, 32),
            ButtonNormal = Color.FromArgb(40, 40, 40),
            ButtonHover = Color.FromArgb(50, 50, 50),
            ButtonPressed = Color.FromArgb(70, 70, 70),
            TextColor = Color.White
        };
    }
}
