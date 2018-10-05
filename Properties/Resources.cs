// Decompiled with JetBrains decompiler
// Type: TLO.local.Properties.Resources
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TLO.local.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (TLO.local.Properties.Resources.resourceMan == null)
          TLO.local.Properties.Resources.resourceMan = new ResourceManager("TLO.local.Properties.Resources", typeof (TLO.local.Properties.Resources).Assembly);
        return TLO.local.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return TLO.local.Properties.Resources.resourceCulture;
      }
      set
      {
        TLO.local.Properties.Resources.resourceCulture = value;
      }
    }
  }
}
