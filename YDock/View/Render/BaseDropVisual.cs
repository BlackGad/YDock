﻿using System.Windows;

namespace YDock.View
{
    public abstract class BaseDropVisual : BaseVisual
    {
        #region Constructors

        internal BaseDropVisual(int flag)
        {
            Flag = flag;
        }

        #endregion

        #region Properties

        public BaseDropPanel DropPanel
        {
            get { return VisualParent as BaseDropPanel; }
        }

        internal virtual int Flag { get; set; }

        #endregion

        #region Members

        public void Update()
        {
            Update(new Size(DropPanel.ActualWidth, DropPanel.ActualHeight));
        }

        #endregion
    }
}