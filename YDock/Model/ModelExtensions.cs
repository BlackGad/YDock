using YDock.Enum;

namespace YDock.Model
{
    public static class ModelExtensions
    {
        #region Static members

        public static bool Assert(this DockSide side)
        {
            if (side == DockSide.None ||
                side == DockSide.Left ||
                side == DockSide.Right ||
                side == DockSide.Top ||
                side == DockSide.Bottom)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}