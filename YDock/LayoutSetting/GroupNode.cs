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

        public void Load(XElement element)
        {
            IsDocument = bool.Parse(element.Attribute("IsDocument")?.Value);
            Side = (DockSide)System.Enum.Parse(typeof(DockSide), element.Attribute("Side").Value);
            foreach (var item in element.Elements())
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
                var element = dockManager.GetDockControl(child.ID);
                if (relative == null)
                {
                    relative = element;
                    if (!isFloat)
                    {
                        element.ProtoType.ToDockSide(Side);
                        element.ToDock(false);
                    }
                    else
                    {
                        element.ToFloat(false);
                    }
                }
                else
                {
                    dockManager.AttachTo(element, relative, AttachMode.Center);
                }
            }

            return relative;
        }

        public IDockControl ApplyLayout(DockManager dockManager, IDockControl target, Direction dir)
        {
            var relative = default(IDockControl);
            foreach (var child in _children)
            {
                var element = dockManager.GetDockControl(child.ID);
                dockManager.AttachTo(element, target, dir == Direction.Horizontal ? AttachMode.Right : AttachMode.Bottom);
                if (relative == null)
                {
                    relative = element;
                }
            }

            return relative;
        }

        public IDockControl TryApplyLayoutAsDocument(DockManager dockManager, bool isFloat = false)
        {
            var relative = default(IDockControl);
            foreach (var child in _children)
            {
                var element = dockManager.GetDockControl(child.ID);
                if (element != null)
                {
                    if (!isFloat)
                    {
                        element.ToDockAsDocument(false);
                    }
                    else
                    {
                        element.ToFloat(false);
                    }

                    if (relative == null)
                    {
                        relative = element;
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
                var element = dockManager.GetDockControl(child.ID);
                if (element != null)
                {
                    if (relative == null)
                    {
                        dockManager.AttachTo(element, target, _parent.Direction == Direction.Horizontal ? AttachMode.Right_WithSplit : AttachMode.Bottom_WithSplit);
                        relative = element;
                    }
                    else
                    {
                        dockManager.AttachTo(element, relative, AttachMode.Center);
                    }
                }
            }

            return relative;
        }

        #endregion
    }
}