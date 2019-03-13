using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace YDock.Global
{
    public class ResourceExtension : MarkupExtension,
                                     INotifyPropertyChanged
    {
        #region Static members

        public static void RaiseLanguageChanged()
        {
            LanguageChanged(null, new EventArgs());
        }

        #endregion

        #region Constructors

        public ResourceExtension()
        {
            LanguageChanged += OnLanguageChanged;
        }

        public ResourceExtension(string key) : this()
        {
            Key = key;
        }

        #endregion

        #region Properties

        [ConstructorArgument("Key")]
        public string Key { get; set; }

        public string Value
        {
            get { return Properties.Resources.ResourceManager.GetString(Key, Properties.Resources.Culture); }
        }

        #endregion

        #region Events

        public static event EventHandler LanguageChanged = delegate { };

        #endregion

        #region Override members

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var setter = target.TargetObject as Setter;
            if (setter == null)
            {
                return new Binding("Value") { Source = this, Mode = BindingMode.OneWay };
            }

            var binding = new Binding("Value") { Source = this, Mode = BindingMode.OneWay };
            return binding.ProvideValue(serviceProvider);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        #region Event handlers

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }

        #endregion
    }
}