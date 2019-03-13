using System;
using System.ComponentModel;
using System.Windows.Media;
using YDock.Enum;

namespace YDock.Interface
{
    public interface IDockOrigin : ILayout,
                                   ILayoutSize,
                                   IDockItem,
                                   INotifyPropertyChanged,
                                   IDisposable
    {
        #region Properties

        bool CanSelect { get; }
        ILayoutGroup Container { get; }
        object Content { get; }
        int ID { get; }
        ImageSource ImageSource { get; }
        bool IsActive { get; }
        bool IsAutoHide { get; }
        bool IsDocked { get; }
        bool IsDocument { get; }
        bool IsFloat { get; }
        bool IsVisible { get; }
        DockMode Mode { get; }
        string Title { get; set; }

        #endregion
    }
}