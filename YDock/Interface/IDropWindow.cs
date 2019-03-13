using System.Windows;

namespace YDock.Interface
{
    public interface IDropWindow
    {
        #region Members

        void Close();
        void Hide();
        void Show();
        void Update(Point mouseP);

        #endregion
    }
}