namespace CustomWindowFramework.Core
{
    public interface IInputElement
    {
        Rectangle Bounds { get; }

        void OnMouseEnter();
        void OnMouseLeave();

        void OnMouseDown(Point p);
        void OnMouseUp(Point p);
    }
}
