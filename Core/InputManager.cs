namespace CustomWindowFramework.Core
{
    public sealed class InputManager
    {
        private IInputElement? _hovered;
        private IInputElement? _captured;
        private Point _lastPos;

        public void ProcessMouseMove(Point p, IEnumerable<IInputElement> elements)
        {
            if (_captured != null)
            {
                if (_captured is IDragElement drag)
                    drag.OnDrag(p, new Point(p.X - _lastPos.X, p.Y - _lastPos.Y));

                _lastPos = p;
                return;
            }

            foreach (var el in elements)
            {
                if (el.Bounds.Contains(p))
                {
                    if (_hovered != el)
                    {
                        _hovered?.OnMouseLeave();
                        _hovered = el;
                        _hovered.OnMouseEnter();
                    }
                    return;
                }
            }

            _hovered?.OnMouseLeave();
            _hovered = null;
        }

        public void ProcessMouseDown(Point p, IEnumerable<IInputElement> elements)
        {
            foreach (var el in elements)
            {
                if (el.Bounds.Contains(p))
                {
                    _captured = el;
                    _lastPos = p;
                    el.OnMouseDown(p);
                    return;
                }
            }
        }

        public void ProcessMouseUp(Point p)
        {
            if (_captured != null)
            {
                _captured.OnMouseUp(p);
                _captured = null;
            }
        }
    }
}
