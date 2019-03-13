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
                if (_layout != value)
                {
                    _layout = value;
                    var name_attr = _layout.Attribute("Name");
                    if (name_attr == null)
                    {
                        _layout.SetAttributeValue("Name", Name);
                    }
                    else
                    {
                        name_attr.Value = Name;
                    }
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