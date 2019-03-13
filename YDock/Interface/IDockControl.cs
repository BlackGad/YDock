namespace YDock.Interface
{
    public interface IDockControl : IDockOrigin
    {
        #region Properties

        IDockElement Prototype { get; }

        #endregion

        #region Members

        void Close();
        void SetActive(bool isActive = true);
        void Show(bool activate = true);

        #endregion
    }
}