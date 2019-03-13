using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace YDock.View.Render
{
    public class BaseRenderPanel : FrameworkElement,
                                   IDisposable
    {
        #region Constructors

        public BaseRenderPanel()
        {
            Children = new List<BaseVisual>();
            DataContextChanged += OnDataContextChanged;
            //RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        #endregion

        #region Properties

        public IList<BaseVisual> Children { get; private set; }

        protected override int VisualChildrenCount
        {
            get
            {
                if (Children == null) return 0;
                return Children.Count;
            }
        }

        #endregion

        #region Override members

        protected override Visual GetVisualChild(int index)
        {
            return Children[index];
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            foreach (var child in Children)
            {
                child.Update(sizeInfo.NewSize);
            }
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            DataContext = null;
            Children.Clear();
            Children = null;
        }

        #endregion

        #region Event handlers

        protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Members

        public void AddChild(BaseVisual child)
        {
            Children.Add(child);
            AddLogicalChild(child);
            AddVisualChild(child);
        }

        public void RemoveChild(BaseVisual child)
        {
            Children.Remove(child);
            RemoveLogicalChild(child);
            RemoveVisualChild(child);
        }

        public virtual void UpdateChildren()
        {
            foreach (var child in Children)
            {
                child.Update(new Size(ActualWidth, ActualHeight));
            }
        }

        #endregion
    }
}