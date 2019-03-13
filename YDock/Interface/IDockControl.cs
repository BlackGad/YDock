namespace YDock.Interface
{
    public interface IDockControl : IDockOrigin
    {
        #region Properties

        IDockElement ProtoType { get; }

        #endregion

        #region Members

        void Close();
        void SetActive(bool _isActive = true);
        void Show(bool toActice = true);

        #endregion
    }
}