using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.View.Render;

namespace YDock.View.Window
{
    public class DropWindow : Popup,
                              IDisposable
    {
        #region Constructors

        public DropWindow(IDragTarget host)
        {
            AllowsTransparency = true;
            Host = host;
            if (host.Mode == DragMode.RootPanel)
            {
                DropPanel = new RootDropPanel(host, host.DockManager.DragManager.DragItem);
            }
            else
            {
                DropPanel = new DropPanel(host, host.DockManager.DragManager.DragItem);
            }

            DropPanel.SizeChanged += OnSizeChanged;

            Child = DropPanel;

            if (host.Mode != DragMode.RootPanel)
            {
                if (host.Mode == DragMode.Document
                    && host.DockManager.DragManager.DragItem.DragMode == DragMode.Anchor)
                {
                    MinWidth = Constants.DropUnitLength * 5;
                    MinHeight = Constants.DropUnitLength * 5;
                }
                else
                {
                    MinWidth = Constants.DropUnitLength * 3;
                    MinHeight = Constants.DropUnitLength * 3;
                }
            }
            else
            {
                MinWidth = 0;
                MinHeight = 0;
            }
        }

        #endregion

        #region Properties

        public BaseDropPanel DropPanel { get; private set; }

        public IDragTarget Host { get; private set; }

        #endregion

        #region Override members

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            DropPanel.Dispose();
            DropPanel = null;
            Host = null;
        }

        #endregion

        #region IDropWindow Members

        public void Hide()
        {
            Child.Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            Child.Visibility = Visibility.Visible;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Update(Point position)
        {
            DropPanel.Update(position);
        }

        #endregion

        #region Event handlers

        //Popup在全屏时显示不全，这里将PopupRoot的高度强制为ScreenHeight
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DropPanel.SizeChanged -= OnSizeChanged;
            DependencyObject parent = Child;
            do
            {
                parent = VisualTreeHelper.GetParent(parent);

                if (parent != null && parent.ToString() == "System.Windows.Controls.Primitives.PopupRoot")
                {
                    (parent as FrameworkElement).Height = Math.Max(DropPanel.OuterRect.Height, MinHeight);
                    break;
                }
            } while (parent != null);
        }

        #endregion
    }
}