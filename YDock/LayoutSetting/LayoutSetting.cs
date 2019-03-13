using System.Xml.Linq;

namespace YDock.LayoutSetting
{
    public class LayoutSetting
    {
        private XElement _layout;

        #region Constructors

        public LayoutSetting(string name, XElement layout)
        {
            Name = name;
            Layout = layout;
        }

        #endregion

        #region Properties

        public string Name { get; }

        internal XElement Layout
        {
            get { return _layout; }
            set
            {
                if (_layout == value) return;

                _layout = value;

                var attribute = _layout.Attribute("Name");
                if (attribute == null)
                {
                    _layout.SetAttributeValue("Name", Name);
                }
                else
                {
                    attribute.Value = Name;
                }
            }
        }

        #endregion

        #region Members

        public void Load(XElement layout)
        {
            Layout = layout;
        }

        public void Save(XElement parent)
        {
            parent.Add(_layout);
        }

        #endregion
    }
}