namespace YDock.Interface
{
    public interface ILayoutPanel : ILayoutViewWithSize,
                                    ILayout,
                                    ILayoutViewParent,
                                    INotifyDisposable
    {
        #region Properties

        int Count { get; }
        bool IsAnchorPanel { get; }
        bool IsDocumentPanel { get; }

        #endregion
    }
}