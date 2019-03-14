namespace YDock.Properties {
    using System;
    
   
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("YDock.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        public static string _AutoHide {
            get {
                return ResourceManager.GetString("_AutoHide", resourceCulture);
            }
        }

        public static string _Close {
            get {
                return ResourceManager.GetString("_Close", resourceCulture);
            }
        }
        
        public static string AutoHide {
            get {
                return ResourceManager.GetString("AutoHide", resourceCulture);
            }
        }

        public static string Close {
            get {
                return ResourceManager.GetString("Close", resourceCulture);
            }
        }

        public static string Close_All {
            get {
                return ResourceManager.GetString("Close_All", resourceCulture);
            }
        }

        public static string Close_All_Except {
            get {
                return ResourceManager.GetString("Close_All_Except", resourceCulture);
            }
        }

        public static string Dock {
            get {
                return ResourceManager.GetString("Dock", resourceCulture);
            }
        }

        public static string Dock_Document {
            get {
                return ResourceManager.GetString("Dock_Document", resourceCulture);
            }
        }

        public static string Float {
            get {
                return ResourceManager.GetString("Float", resourceCulture);
            }
        }

        public static string Float_All {
            get {
                return ResourceManager.GetString("Float_All", resourceCulture);
            }
        }

        public static string Hide {
            get {
                return ResourceManager.GetString("Hide", resourceCulture);
            }
        }

        public static string Maximize {
            get {
                return ResourceManager.GetString("Maximize", resourceCulture);
            }
        }

        public static string Minimize {
            get {
                return ResourceManager.GetString("Minimize", resourceCulture);
            }
        }

        public static string Restore {
            get {
                return ResourceManager.GetString("Restore", resourceCulture);
            }
        }

        public static string Window_Position {
            get {
                return ResourceManager.GetString("Window_Position", resourceCulture);
            }
        }
    }
}
