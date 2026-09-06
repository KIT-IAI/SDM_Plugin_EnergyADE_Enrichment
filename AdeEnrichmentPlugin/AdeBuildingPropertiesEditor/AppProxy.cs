using KIT.BuW.AdvEnrichment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KIT.BuW.AdvEnrichment
{
  public class BPEditorProxyImpl : MarshalByRefObject
  {
    public void Start()
    {
      //App.ResourceAssembly = Assembly.GetExecutingAssembly();
      //App.ShowMainWindow();
    }

    public void Show(IAdeDataFactory theFactory, IntPtr hParentWnd)
    {
      App.ResourceAssembly = Assembly.GetExecutingAssembly();

      if (m_HostingApp == null)
      {
        m_HostingApp = new App
        {
          ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
      }

      if (m_TheEditor != null && m_TheEditor.IsWindowClosed)
      {
        m_TheEditor = null;
      }

      if (m_TheEditor == null)
      {
        m_TheEditor = new BuildingPropertiesEditor(theFactory, hParentWnd);
      }

      m_TheEditor.Show();
    }

    public override object InitializeLifetimeService()
    {
      return null;
    }

    public bool IsEditorVisible
    {
      get { return (this == null || m_TheEditor == null) ? false : !m_TheEditor.IsWindowClosed; }
    }

    private App m_HostingApp;
    private BuildingPropertiesEditor m_TheEditor;
  }
}
