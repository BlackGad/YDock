using System.Windows.Media;

namespace YDock.Interface
{
    public interface IDockSource
    {
        #region Properties

        /// <summary>
        ///     The layout manager, which contains some operations that control the layout of the Dock, such as Show, Hide, etc.
        /// </summary>
        IDockControl DockControl { get; set; }

        /// <summary>
        ///     Display title
        /// </summary>
        string Header { get; }

        /// <summary>
        ///     Display icon
        /// </summary>
        ImageSource Icon { get; }

        #endregion
    }
}