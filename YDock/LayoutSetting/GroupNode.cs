using System.Collections.Generic;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;

namespace YDock.LayoutSetting
{
    public class GroupNode : ILayoutNode
    {
        private readonly LinkedList<ItemNode> _children;
        private PanelNode _parent;

        #region Constructors

        public GroupNode(PanelNode parent)
        {
            _parent = parent;
            _children = new LinkedList<ItemNode>();
        }

        #endregion

        #region Properties

        public bool IsDocument { get; private set; }

        public DockSide Side { get; private set; }

        #endregion

        #region ILayoutNode Members

        public LayoutNodeType Type
        {
            get { return LayoutNodeType.Group; }
        }

        public ILayoutNode Parent
        {
            get { return _parent; }
        }

        public IEnumerable<ILayoutNode> Children
        {
            get { return _children; }
        }

        public void Load(XElement ele)
        {
            IsDocument = bool.Parse(ele.Attribute("IsDocument").Value);
            Side = (DockSide)System.Enum.Parse(typeof(DockSide), ele.Attribute("Side").Value);
            foreach (var item in ele.Elements())
            {
                var itemNode = new ItemNode(this);
                itemNode.Load(item);
                _children.AddLast(itemNode);
            }
        }

        public void Dispose()
        {
            foreach (var child in _children)
            {
                child.Dispose();
            }

            _children.Clear();

            _parent = null;
        }

        #endregion

        #region Members

        public IDockControl ApplyLayout(DockManager dockManager, bool isFloat = false)
        {
            var relative = default(IDockControl);
            foreach (var child in _children)
            {
                var ele = dockManager.GetDockControl(child.ID);
                if (relative == null)
                {
                    relative = ele;
                    if (!isFloat)
                    {
                        ele.ProtoType.ToDockSide(Side);
                        ele.ToDock(false);
                    }
                    else
                    {
                        ele.ToFloat(false);
                    }
                }
                else
                {
                    dockManager.AttachTo(ele, relative, AttachMode.Center);
                }
            }

            return relative;
        }

        public IDockControl ApplyLayout(DockManager dockManager, IDockControl target, Direction dir)
        {
            var relative = default(IDockControl);
            foreach (var child in _children)
            {
                var ele = dockManager.GetDockControl(child.ID);
                dockManager.AttachTo(ele, target, dir == Direction.Horizontal ? AttachMode.Right : AttachMode.Bottom);
                if (relative == null)
                {
                    relative = ele;
                }
            }

            return relative;
        }

        public IDockControl TryApplyLayoutAsDocument(DockManager dockManager, bool isFloat = false)
        {
            var relative = default(IDockControl);
            foreach (var child in _children)
            {
                var ele = dockManager.GetDockControl(child.ID);
                if (ele != null)
                {
                    if (!isFloat)
                    {
                        ele.ToDockAsDocument(false);
                    }
                    else
                    {
                        ele.ToFloat(false);
                    }

                    if (relative == null)
                    {
                        relative = ele;
                    }
                }
            }

            return relative;
        }

        public IDockControl TryApplyLayoutAsDocument(DockManager dockManager, IDockControl target)
        {
            var relative = default(IDockControl);
            foreach (var child in _children)
            {
                var ele = dockManager.GetDockControl(child.ID);
                if (ele != null)
                {
                    if (relative == null)
                    {
                        dockManager.AttachTo(ele, target, _parent.Direction == Direction.Horizontal ? AttachMode.Right_WithSplit : AttachMode.Bottom_WithSplit);
                        relative = ele;
                    }
                    else
                    {
                        dockManager.AttachTo(ele, relative, AttachMode.Center);
                    }
                }
            }

            return relative;
        }

        #endregion
    }
}