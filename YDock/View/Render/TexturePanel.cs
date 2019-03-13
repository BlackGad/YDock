using System.Windows;
using YDock.Interface;

namespace YDock.View.Render
{
    public class TexturePanel : BaseRenderPanel
    {
        #region Constructors

        public TexturePanel()
        {
            AddChild(new TextureHeaderVisual());
        }

        #endregion

        #region Override members

        protected override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            base.OnDataContextChanged(sender, e);
            if (e.OldValue != null && e.OldValue is IDockElement)
            {
                (e.OldValue as IDockElement).PropertyChanged -= OnModelPropertyChanged;
            }

            if (e.NewValue != null && e.NewValue is IDockElement)
            {
                (e.NewValue as IDockElement).PropertyChanged += OnModelPropertyChanged;
                UpdateChildren();
            }
        }

        #endregion

        #region Event handlers

        private void OnModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                UpdateChildren();
            }
        }

        #endregion
    }
}