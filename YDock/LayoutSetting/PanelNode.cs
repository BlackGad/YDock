using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;

namespace YDock.LayoutSetting
{
    public class PanelNode : ILayoutNode
    {
        private readonly LinkedList<ILayoutNode> _children;
        private PanelNode _parent;

        #region Constructors

        public PanelNode(PanelNode parent)
        {
            _parent = parent;
            _children = new LinkedList<ILayoutNode>();
        }

        #endregion

        #region Properties

        public Direction Direction { get; private set; }

        public bool IsDocument { get; private set; }

        public DockSide Side { get; private set; }

        #endregion

        #region ILayoutNode Members

        public LayoutNodeType Type
        {
            get { return LayoutNodeType.Panel; }
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
            bool.TryParse(element.Attribute("IsDocument")?.Value, out var isDocument);
            IsDocument = isDocument;

            System.Enum.TryParse(element.Attribute("Side")?.Value, out DockSide side);
            Side = side;

            System.Enum.TryParse(element.Attribute("Direction")?.Value, out Direction direction);
            Direction = direction;

            foreach (var item in element.Elements())
            {
                ILayoutNode node;
                if (item.Name == "Panel")
                {
                    node = new PanelNode(this);
                }
                else
                {
                    node = new GroupNode(this);
                }

                node.Load(item);
                _children.AddLast(node);
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

        public void ApplyLayout(DockManager dockManager, bool isFloat = false)
        {
            if (Side != DockSide.None)
            {
                TryCompleteLayout(dockManager, null, isFloat);
                return;
            }

            if (IsDocument)
            {
                var node = _children.First;
                var relativeDockControl = default(IDockControl);

                while (node != null)
                {
                    relativeDockControl = ((GroupNode)node.Value).TryApplyLayoutAsDocument(dockManager, isFloat);
                    node = node.Next;
                    if (relativeDockControl != null) break;
                }

                while (node != null)
                {
                    var relative = ((GroupNode)node.Value).TryApplyLayoutAsDocument(dockManager, relativeDockControl);
                    node = node.Next;
                    if (relative != null) relativeDockControl = relative;
                }

                return;
            }

            var panelNode = _children.OfType<PanelNode>().FirstOrDefault(n => n.Type == LayoutNodeType.Panel && n.Side == DockSide.None);
            if (panelNode == null)
            {
                TryCompleteLayout(dockManager, null, isFloat);
                return;
            }

            {
                panelNode.ApplyLayout(dockManager);
                var node = _children.Find(panelNode);
                if (node?.Previous != null)
                {
                    var cur = node.Previous;
                    while (cur != null)
                    {
                        if (cur.Value.Type == LayoutNodeType.Panel)
                        {
                            ((PanelNode)cur.Value).ApplyLayout(dockManager, isFloat);
                        }
                        else
                        {
                            ((GroupNode)cur.Value).ApplyLayout(dockManager, isFloat);
                        }

                        cur = cur.Previous;
                    }
                }

                if (node?.Next != null)
                {
                    var cur = node.Next;
                    while (cur != null)
                    {
                        if (cur.Value.Type == LayoutNodeType.Panel)
                        {
                            ((PanelNode)cur.Value).ApplyLayout(dockManager, isFloat);
                        }
                        else
                        {
                            ((GroupNode)cur.Value).ApplyLayout(dockManager, isFloat);
                        }

                        cur = cur.Next;
                    }
                }
            }
        }

        public IDockControl TryGetFirstLevelElement(DockManager dockManager, IDockControl target = null, Direction dir = Direction.None, bool isFloat = false)
        {
            if (_children.First.Value.Type == LayoutNodeType.Panel)
            {
                return ((PanelNode)_children.First.Value).TryGetFirstLevelElement(dockManager, target, dir, isFloat);
            }

            if (target != null)
            {
                return ((GroupNode)_children.First.Value).ApplyLayout(dockManager, target, dir);
            }

            return ((GroupNode)_children.First.Value).ApplyLayout(dockManager, isFloat);
        }

        private void TryCompleteLayout(DockManager dockManager, IDockControl relative, bool isFloat = false)
        {
            var dic = new Dictionary<IDockControl, PanelNode>();
            var children = relative == null ? _children : _children.Skip(1);

            if (relative != null && _children.First.Value.Type == LayoutNodeType.Panel)
            {
                dic.Add(relative, _children.First.Value as PanelNode);
            }

            foreach (var item in children)
            {
                if (relative == null)
                {
                    if (item.Type == LayoutNodeType.Group)
                    {
                        relative = ((GroupNode)item).ApplyLayout(dockManager, isFloat);
                    }
                    else
                    {
                        relative = ((PanelNode)item).TryGetFirstLevelElement(dockManager, null, Direction.None, isFloat);
                        dic.Add(relative, item as PanelNode);
                    }
                }
                else
                {
                    if (item.Type == LayoutNodeType.Group)
                    {
                        relative = ((GroupNode)item).ApplyLayout(dockManager, relative, Direction);
                    }
                    else
                    {
                        relative = ((PanelNode)item).TryGetFirstLevelElement(dockManager, relative, Direction);
                        dic.Add(relative, item as PanelNode);
                    }
                }
            }

            foreach (var pair in dic)
            {
                pair.Value.TryCompleteLayout(dockManager, pair.Key, isFloat);
            }
        }

        #endregion
    }
}