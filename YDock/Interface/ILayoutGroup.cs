﻿using System.Collections.Generic;
using System.ComponentModel;
using YDock.Enum;

namespace YDock.Interface
{
    public interface ILayoutGroup : IDockModel,
                                    INotifyPropertyChanged
    {
        #region Properties

        IEnumerable<IDockElement> Children { get; }
        DockMode Mode { get; }

        #endregion

        #region Members

        void Attach(IDockElement element, int index = -1);
        void CloseAll();
        void CloseAllExcept(IDockElement element);
        void Detach(IDockElement element);
        int IndexOf(IDockElement child);
        void MoveTo(int src, int des);
        void RaisePropertyChanged(string propertyName);
        void ShowWithActive(IDockElement element, bool toActice = true);
        void ShowWithActive(int index, bool toActice = true);
        void ToFloat();

        #endregion
    }
}