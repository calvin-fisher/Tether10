// Decompiled with JetBrains decompiler
// Type: TetherWindows.Resources
// Assembly: TetherWindows, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EC2D7451-4D97-4986-80E3-DF30C217B4F5
// Assembly location: C:\Program Files (x86)\ClockworkMod\Tether\TetherWindows.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TetherWindows
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class Resources
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals((object)Resources.resourceMan, (object)null))
                    Resources.resourceMan = new ResourceManager("TetherWindows.Resources", typeof(Resources).Assembly);
                return Resources.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return Resources.resourceCulture;
            }
            set
            {
                Resources.resourceCulture = value;
            }
        }

        internal static Bitmap usb_broken
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("usb_broken", Resources.resourceCulture);
            }
        }

        internal static Bitmap usb_off
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("usb_off", Resources.resourceCulture);
            }
        }

        internal static Bitmap usb_on
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("usb_on", Resources.resourceCulture);
            }
        }

        internal static Bitmap usb_pending
        {
            get
            {
                return (Bitmap)Resources.ResourceManager.GetObject("usb_pending", Resources.resourceCulture);
            }
        }

        internal Resources()
        {
        }
    }
}
