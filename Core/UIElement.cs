namespace CustomWindowFramework.Core
{
    public abstract class UIElement : Control
    {
        protected UIElement()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true
            );
        }
    }
}
