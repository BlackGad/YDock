using YDock.Enum;

namespace YDock.Interface
{
    public interface IAttcah
    {
        #region Members

        void AttachWith(IDockView source, AttachMode mode = AttachMode.Center);

        #endregion
    }
}