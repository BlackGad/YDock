//using Microsoft.VisualStudio.PlatformUI.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using YDock;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;
using YDock.View;

namespace YDockTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            _Init();
            /*
            var msg = ViewManager.Instance;
            msg.Preferences.IsPinnedTabPanelSeparate = true;
            msg.Preferences.DocumentDockPreference = Microsoft.VisualStudio.PlatformUI.Shell.Preferences.DockPreference.DockAtEnd;
            msg.Preferences.TabDockPreference = Microsoft.VisualStudio.PlatformUI.Shell.Preferences.DockPreference.DockAtEnd;
            var res = new ResourceDictionary()
            {
                Source = new Uri("/Microsoft.VisualStudio.Shell.ViewManager;component/themes/generic.xaml", UriKind.Relative)
            };
            msg.Theme = res;
            msg.Initialize(ViewManagerHost);
            msg.WindowProfile = WindowProfile.Create("MainWindow");
            for (int i = 0; i < 10; i++)
            {
                var view = View.Create(msg.WindowProfile, string.Format("Test_{0}", i));
                view.Title = view.Name;
                view.Content = new TextBlock() { Text = view.Name };
                view.ShowInFront();
            }
            */
        }

        static string SettingFileName { get { return string.Format(@"{0}\{1}", Environment.CurrentDirectory, "Layout.xml"); } }

        private Doc doc_0;
        private Doc doc_1;
        private Doc doc_2;
        private Doc doc_3;
        private Doc left;
        private Doc right;
        private Doc top;
        private Doc bottom;
        private Doc left_1;
        private Doc right_1;
        private Doc top_1;
        private Doc bottom_1;

        private void _Init()
        {
            doc_0 = new Doc("doc_0");
            doc_1 = new Doc("doc_1");
            doc_2 = new Doc("doc_2");
            doc_3 = new Doc("doc_3");
            left = new Doc("left");
            right = new Doc("right");
            top = new Doc("top");
            bottom = new Doc("bottom");
            left_1 = new Doc("left_1");
            right_1 = new Doc("right_1");
            top_1 = new Doc("top_1");
            bottom_1 = new Doc("bottom_1");

            DockManager1.RegisterDocument(doc_0);
            DockManager1.RegisterDocument(doc_1);
            DockManager1.RegisterDocument(doc_2);
            DockManager1.RegisterDocument(doc_3);

            DockManager1.RegisterDock(left);
            DockManager1.RegisterDock(right, DockSide.Right);
            DockManager1.RegisterDock(top, DockSide.Top);
            DockManager1.RegisterDock(bottom, DockSide.Bottom);

            DockManager1.RegisterDock(left_1);
            DockManager1.RegisterDock(right_1, DockSide.Right);
            DockManager1.RegisterDock(top_1, DockSide.Top);
            DockManager1.RegisterDock(bottom_1, DockSide.Bottom);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SettingFileName) && false)
            {
                var layout = XDocument.Parse(File.ReadAllText(SettingFileName));
                foreach (var item in layout.Root.Elements())
                {
                    var name = item.Attribute("Name").Value;
                    if (DockManager1.Layouts.ContainsKey(name))
                        DockManager1.Layouts[name].Load(item);
                    else DockManager1.Layouts[name] = new YDock.LayoutSetting.LayoutSetting(name, item);
                }

                DockManager1.ApplyLayout("MainWindow");
            }
            else
            {
                doc_0.DockControl.Show();
                doc_1.DockControl.Show();
                doc_2.DockControl.Show();
                doc_3.DockControl.Show();
                left.DockControl.Show();
                right.DockControl.Show();
                top.DockControl.Show();
                bottom.DockControl.Show();
                left_1.DockControl.Show();
                right_1.DockControl.Show();
                top_1.DockControl.Show();
                bottom_1.DockControl.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            DockManager1.SaveCurrentLayout("MainWindow");

            var doc = new XDocument();
            var rootNode = new XElement("Layouts");
            foreach (var layout in DockManager1.Layouts.Values)
                layout.Save(rootNode);
            doc.Add(rootNode);

            doc.Save(SettingFileName);

            DockManager1.Dispose();
        }
    }

    public class Doc : Button, IDockSource
    {
        public Doc(string header)
        {
            _header = header;
        }

        private IDockControl _dockControl;
        public IDockControl DockControl
        {
            get
            {
                return _dockControl;
            }

            set
            {
                _dockControl = value;
            }
        }

        private string _header;
        public string Header
        {
            get
            {
                return _header;
            }
        }

        public ImageSource Icon
        {
            get
            {
                return null;
            }
        }
    }
}