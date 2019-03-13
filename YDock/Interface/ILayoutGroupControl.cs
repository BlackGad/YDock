using System.ComponentModel;

namespace YDock.Interface
{
    public interface ILayoutGroupControl : ILayoutViewWithSize,
                                           INotifyDisposable,
                                           INotifyPropertyChanged
    {
        #region Members

        void AttachToParent(ILayoutPanel parent, int index);
        bool TryDetachFromParent(bool isDispose = true);

        #endregion
    }
}