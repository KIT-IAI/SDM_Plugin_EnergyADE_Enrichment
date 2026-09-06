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
  public class SettingsViewModel
  {
    public bool ThermalZonesByBuilding { get; set; } = true;
    public bool ThermalZonesByBuildingPart { get; set; } = false;
    public bool ThermalZonesByStorey { get; set; } = false;
    public bool SplitGeometryIntoStoreys { get; set; } = false;
  }

  /// <summary>
  /// Interaktionslogik für SettingsControl.xaml
  /// </summary>
  public partial class SettingsControl : UserControl
  {

    public SettingsControl()
    {
      InitializeComponent();
    }
  }
}
