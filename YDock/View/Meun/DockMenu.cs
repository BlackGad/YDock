using System;
using System.Windows.Controls;
using System.Windows.Input;
using YDock.Commands;
using YDock.Global;
using YDock.Interface;

namespace YDock.View
{
    public class DockMenu : ContextMenu,
                            IDisposable
    {
        #region Constructors

        public DockMenu(IDockItem targetObj)
        {
            TargetObj = targetObj;
            _InitMenuItem();
            ResourceExtension.LanguageChanged += OnLanguageChanged;
        }

        #endregion

        #region Properties

        public IDockItem TargetObj { get; private set; }

        #endregion

        #region Override members

        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(GlobalCommands.ToFloatCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.ToDockCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.ToDockAsDocumentCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.SwitchAutoHideStatusCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.HideStatusCommand, OnCommandExecute, OnCommandCanExecute));
            base.OnInitialized(e);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            ResourceExtension.LanguageChanged -= OnLanguageChanged;
            TargetObj = null;
        }

        #endregion

        #region Event handlers

        private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (TargetObj == null || TargetObj.IsDisposed)
            {
                e.CanExecute = false;
                return;
            }

            if (e.Command == GlobalCommands.ToFloatCommand)
            {
                e.CanExecute = TargetObj.CanFloat;
            }

            if (e.Command == GlobalCommands.ToDockCommand)
            {
                e.CanExecute = TargetObj.CanDock;
            }

            if (e.Command == GlobalCommands.ToDockAsDocumentCommand)
            {
                e.CanExecute = TargetObj.CanDockAsDocument;
            }

            if (e.Command == GlobalCommands.SwitchAutoHideStatusCommand)
            {
                e.CanExecute = TargetObj.CanSwitchAutoHideStatus;
            }

            if (e.Command == GlobalCommands.HideStatusCommand)
            {
                e.CanExecute = TargetObj.CanHide;
            }
        }

        private void OnCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (TargetObj == null || TargetObj.IsDisposed)
            {
                return;
            }

            if (e.Command == GlobalCommands.ToFloatCommand)
            {
                TargetObj.ToFloat();
            }

            if (e.Command == GlobalCommands.ToDockCommand)
            {
                TargetObj.ToDock();
            }

            if (e.Command == GlobalCommands.ToDockAsDocumentCommand)
            {
                TargetObj.ToDockAsDocument();
            }

            if (e.Command == GlobalCommands.SwitchAutoHideStatusCommand)
            {
                TargetObj.SwitchAutoHideStatus();
            }

            if (e.Command == GlobalCommands.HideStatusCommand)
            {
                TargetObj.Hide();
            }
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            var index = 0;
            foreach (MenuItem item in Items)
            {
                switch (index)
                {
                    case 0:
                        item.Header = Properties.Resources.Float;
                        break;
                    case 1:
                        item.Header = Properties.Resources.Dock;
                        break;
                    case 2:
                        item.Header = Properties.Resources.Dock_Document;
                        break;
                    case 3:
                        item.Header = Properties.Resources.AutoHide;
                        break;
                    case 4:
                        item.Header = Properties.Resources.Hide;
                        break;
                }

                index++;
            }
        }

        #endregion

        #region Members

        private void _InitMenuItem()
        {
            for (var i = 0; i < 5; i++)
            {
                var item = new MenuItem();
                switch (i)
                {
                    case 0:
                        item.Header = Properties.Resources.Float;
                        item.Command = GlobalCommands.ToFloatCommand;
                        break;
                    case 1:
                        item.Header = Properties.Resources.Dock;
                        item.Command = GlobalCommands.ToDockCommand;
                        break;
                    case 2:
                        item.Header = Properties.Resources.Dock_Document;
                        item.Command = GlobalCommands.ToDockAsDocumentCommand;
                        break;
                    case 3:
                        item.Header = Properties.Resources.AutoHide;
                        item.Command = GlobalCommands.SwitchAutoHideStatusCommand;
                        break;
                    case 4:
                        item.Header = Properties.Resources.Hide;
                        item.Command = GlobalCommands.HideStatusCommand;
                        break;
                }

                Items.Add(item);
            }
        }

        #endregion
    }
}