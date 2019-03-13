using YDock.Enum;

namespace YDock.Interface
{
    public interface ILayoutViewParent
    {
        #region Members

        void AttachChild(IDockView child, AttachMode mode, int index);
        void DetachChild(IDockView child, bool force = true);
        int IndexOf(IDockView child);

        #endregion
    }
}