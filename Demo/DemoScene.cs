using CustomWindowFramework.Core;

namespace CustomWindowFramework.Demo
{
    public class DemoScene : UIElement
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawString(
                "Custom UI Scene\n(no WinForms designer)",
                Font,
                Brushes.White,
                new PointF(20, 20));
        }
    }
}
