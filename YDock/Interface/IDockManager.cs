using System;
using System.Collections.Generic;
using System.Windows.Media;
using YDock.Enum;

namespace YDock.Interface
{
    public interface IDockManager : IDockView
    {
        #region Properties

        IDockControl ActiveControl { get; }
        IEnumerable<IDockControl> DockControls { get; }
        ImageSource DockImageSource { get; set; }
        string DockTitle { get; set; }
        int DocumentTabCount { get; }
        IDockControl SelectedDocument { get; }

        #endregion

        #region Events

        event EventHandler ActiveDockChanged;

        #endregion

        #region Members

        bool ApplyLayout(string name);
        void AttachTo(IDockControl source, IDockControl target, AttachMode mode, double ratio = 1);
        int GetDocumentTabIndex(IDockControl dockControl);

        void HideAll();

        void RegisterDock(IDockSource content,
                          DockSide side = DockSide.Left,
                          bool canSelect = false,
                          double desiredWidth = Constants.DockDefaultWidthLength,
                          double desiredHeight = Constants.DockDefaultHeightLength,
                          double floatLeft = 0.0,
                          double floatTop = 0.0);

        void RegisterDocument(IDockSource content,
                              bool canSelect = false,
                              double desiredWidth = Constants.DockDefaultWidthLength,
                              double desiredHeight = Constants.DockDefaultHeightLength,
                              double floatLeft = 0.0,
                              double floatTop = 0.0);

        void RegisterFloat(IDockSource content,
                           DockSide side = DockSide.Left,
                           double desiredWidth = Constants.DockDefaultWidthLength,
                           double desiredHeight = Constants.DockDefaultHeightLength,
                           double floatLeft = 0.0,
                           double floatTop = 0.0);

        void SaveCurrentLayout(string name);
        void UpdateTitleAll();

        #endregion
    }
}