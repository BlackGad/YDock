using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using YDock.Commands;
using YDock.Enum;
using YDock.Global;
using YDock.Interface;

namespace YDock.View
{
    public class DocumentMenu : ContextMenu,
                                IDisposable
    {
        #region Constructors

        public DocumentMenu(IDockElement targetObj)
        {
            TargetObj = targetObj;
            _InitMenuItem();
            ResourceExtension.LanguageChanged += OnLanguageChanged;
        }

        #endregion

        #region Properties

        public IDockElement TargetObj { get; private set; }

        #endregion

        #region Override members

        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(GlobalCommands.CloseCommand, OnCommandExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.CloseAllExceptCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.CloseAllCommand, OnCommandExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.ToFloatCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.ToFloatAllCommand, OnCommandExecute, OnCommandCanExecute));
            base.OnInitialized(e);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            TargetObj = null;
            ResourceExtension.LanguageChanged -= OnLanguageChanged;
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

            if (e.Command == GlobalCommands.CloseAllExceptCommand)
            {
                e.CanExecute = TargetObj.Container.Children.Count() > 1;
            }

            if (e.Command == GlobalCommands.ToFloatCommand)
            {
                e.CanExecute = TargetObj.CanFloat;
            }

            if (e.Command == GlobalCommands.ToFloatAllCommand)
            {
                e.CanExecute = TargetObj.Container.Mode != DockMode.Float;
            }
        }

        private void OnCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (TargetObj == null || TargetObj.IsDisposed)
            {
                return;
            }

            if (e.Command == GlobalCommands.CloseCommand)
            {
                TargetObj.Hide();
            }

            if (e.Command == GlobalCommands.CloseAllExceptCommand)
            {
                TargetObj.Container.CloseAllExcept(TargetObj);
            }

            if (e.Command == GlobalCommands.CloseAllCommand)
            {
                TargetObj.Container.CloseAll();
            }

            if (e.Command == GlobalCommands.ToFloatCommand)
            {
                TargetObj.ToFloat();
            }

            if (e.Command == GlobalCommands.ToFloatAllCommand)
            {
                TargetObj.Container.ToFloat();
            }
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            foreach (var item in Items)
            {
                if (item is MenuItem)
                {
                    var _item = item as MenuItem;
                    switch ((int)_item.Tag)
                    {
                        case 0:
                            _item.Header = Properties.Resources._Close;
                            break;
                        case 1:
                            _item.Header = Properties.Resources.Close_All_Except;
                            break;
                        case 2:
                            _item.Header = Properties.Resources.Close_All;
                            break;
                        case 3:
                            _item.Header = Properties.Resources.Float;
                            break;
                        case 4:
                            _item.Header = Properties.Resources.Float_All;
                            break;
                    }
                }
            }
        }

        #endregion

        #region Members

        private void _InitMenuItem()
        {
            for (var i = 0; i < 5; i++)
            {
                var item = new MenuItem
                {
                    Tag = i
                };

                switch (i)
                {
                    case 0:
                        item.Header = Properties.Resources._Close;
                        item.Command = GlobalCommands.CloseCommand;
                        break;
                    case 1:
                        item.Header = Properties.Resources.Close_All_Except;
                        item.Command = GlobalCommands.CloseAllExceptCommand;
                        break;
                    case 2:
                        item.Header = Properties.Resources.Close_All;
                        item.Command = GlobalCommands.CloseAllCommand;
                        break;
                    case 3:
                        Items.Add(new Separator());
                        item.Header = Properties.Resources.Float;
                        item.Command = GlobalCommands.ToFloatCommand;
                        break;
                    case 4:
                        item.Header = Properties.Resources.Float_All;
                        item.Command = GlobalCommands.ToFloatAllCommand;
                        break;
                }

                Items.Add(item);
            }
        }

        #endregion
    }
}