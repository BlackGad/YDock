﻿using System.Windows;
using System.Windows.Media;

namespace YDock.View.Render
{
    public abstract class BaseVisual : DrawingVisual
    {
        #region Members

        public abstract void Update(Size size);

        #endregion
    }
}