namespace YDock.Interface
{
    public interface IDockItem
    {
        #region Properties

        bool CanDock { get; }
        bool CanDockAsDocument { get; }
        bool CanFloat { get; }
        bool CanHide { get; }
        bool CanSwitchAutoHideStatus { get; }
        bool IsDisposed { get; }

        #endregion

        #region Members

        void Hide();
        void SwitchAutoHideStatus();
        void ToDock(bool isActive = true);
        void ToDockAsDocument(bool isActive = true);
        void ToDockAsDocument(int index, bool isActive = true);
        void ToFloat(bool isActive = true);

        #endregion
    }
}