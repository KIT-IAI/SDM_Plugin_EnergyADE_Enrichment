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
  /// Interaktionslogik für BuildingUserDef.xaml
  /// </summary>
  public partial class BuildingUserDef : BaseUserControl
  {
    public BuildingUserDef()
    {
      InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnSettingsChanged();
    }

    private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      OnSettingsChanged();
    }

  }
}
