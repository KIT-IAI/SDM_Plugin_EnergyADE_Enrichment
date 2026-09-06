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
  /// Interaktionslogik für ZoneUserDef.xaml
  /// </summary>
  public partial class ZoneUserDef : BaseUserControl
  {
    public ZoneUserDef()
    {
      InitializeComponent();
    }

    private void CheckBox_Click(object sender, RoutedEventArgs e)
    {
      OnSettingsChanged();
    }

  }
}
