using System.Collections.Generic;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;

namespace YDock.LayoutSetting
{
    public class ItemNode : ILayoutNode
    {
        private GroupNode _parent;

        #region Constructors

        public ItemNode(GroupNode parent)
        {
            _parent = parent;
        }

        #endregion

        #region Properties

        public int ID { get; private set; }

        #endregion

        #region ILayoutNode Members

        public LayoutNodeType Type
        {
            get { return LayoutNodeType.Item; }
        }

        public ILayoutNode Parent
        {
            get { return _parent; }
        }

        public IEnumerable<ILayoutNode> Children
        {
            get { yield break; }
        }

        public void Load(XElement element)
        {
            ID = int.Parse(element.Value);
        }

        public void Dispose()
        {
            _parent = null;
        }

        #endregion
    }
}