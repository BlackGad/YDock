using System;
using System.Collections.Generic;
using System.Xml.Linq;
using YDock.Enum;

namespace YDock.Interface
{
    public interface ILayoutNode : IDisposable
    {
        #region Properties

        IEnumerable<ILayoutNode> Children { get; }
        ILayoutNode Parent { get; }
        LayoutNodeType Type { get; }

        #endregion

        #region Members

        void Load(XElement ele);

        #endregion
    }
}