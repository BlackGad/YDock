using System.Xml.Linq;
using YDock.Enum;

namespace YDock.Interface
{
    public interface IDockElement : IDockOrigin
    {
        #region Members

        void Load(XElement element);
        XElement Save();
        void ToDockSide(DockSide side, bool isActive = false);

        #endregion
    }
}