namespace CustomWindowFramework.Core
{
    public interface IAnimatable
    {
        /// <summary>
        /// Update animation state.
        /// Return true if redraw is required.
        /// </summary>
        bool Tick(float deltaTime);
    }
}
