using CustomWindowFramework.Core;

namespace CustomWindowFramework.Demo
{
    public class MainWindow : WindowBase
    {
        private Button _toggleThemeButton;

        public MainWindow()
        {
            Text = "Custom Window Demo";
            Size = new Size(800, 500);

            _toggleThemeButton = new Button
            {
                Text = "Toggle Theme",
                Location = new Point(20, TitleHeight + 20),
                Size = new Size(120, 30)
            };
            _toggleThemeButton.Click += (s, e) =>
            {
                SetTheme(ThemeIsDark ? WindowTheme.Light : WindowTheme.Dark);
                ThemeIsDark = !ThemeIsDark;
            };

            Controls.Add(_toggleThemeButton);

            ThemeIsDark = IsSystemInDarkMode();
        }

        private bool ThemeIsDark = true;
    }
}
