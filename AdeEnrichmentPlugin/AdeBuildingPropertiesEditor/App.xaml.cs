using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using KIT.BuW.AdvEnrichment;

namespace KIT.BuW.AdvEnrichment
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public void ShowMainWindow()
        {
            if (m_mainWnd == null)
            {
                m_mainWnd = new Dummy.MainWindow();
            }
            m_mainWnd.Show();
        }

        internal Dummy.DummyDataFactory theFactory = new Dummy.DummyDataFactory();
        private Dummy.MainWindow m_mainWnd;
    }
}
