using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace KIT.BuW.AdvEnrichment.UI
{
  /// <summary>
  /// Interaktionslogik für BuildingPartStats.xaml
  /// </summary>
  public partial class BuildingPartStats : UserControl
  {
    public BuildingPartStats()
    {
      InitializeComponent();
    }

    public void UpdateBuildingPartsStatsUI(bool bEnabled)
    {
      foreach (var item in gridBuildingPartStats.Children)
      {
        if (item is Control)
        {
          var ctrl = item as Control;
          ctrl.IsEnabled = bEnabled;
        }
        if (item is DockPanel)
        {
          var pnl = item as DockPanel;
          foreach (var dockItem in pnl.Children)
          {
            var ctrl = dockItem as Control;
            ctrl.IsEnabled = bEnabled;
          }
        }
      }
    }

  }
}
