using System.Windows.Media;

namespace YDock.Interface
{
    public interface IDockSource
    {
        #region Properties

        /// <summary>
        ///     布局管理器，包含控制Dock布局的一些操作，例如Show，Hide之类
        /// </summary>
        IDockControl DockControl { get; set; }

        /// <summary>
        ///     显示用的标题
        /// </summary>
        string Header { get; }

        /// <summary>
        ///     显示用的图标
        /// </summary>
        ImageSource Icon { get; }

        #endregion
    }
}