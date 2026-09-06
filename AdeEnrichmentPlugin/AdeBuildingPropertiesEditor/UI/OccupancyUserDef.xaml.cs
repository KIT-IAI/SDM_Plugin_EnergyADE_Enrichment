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
  /// Interaktionslogik für OccupancyUserDef.xaml
  /// </summary>
  public partial class OccupancyUserDef : BaseUserControl
  {
    public OccupancyUserDef()
    {
      InitializeComponent();

      // disable edit buttons for usage profiles
      var grid = cbUsageProfileSet.Parent as Grid;
      SetEditButtonState(grid, 2, false);
    }

    private void ComboBox_UsageProfileSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnUsageProfileSetsSelectionChanged(sender, e);
    }

    private void ComboBox_UsageProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnUsageProfileSelectionChanged(sender, e);
    }

    private void Button_Click_Edit_Profile(object sender, RoutedEventArgs e)
    {
      AdeBuildingPropertyEditor.DailyProfileEditor profileEditor = new AdeBuildingPropertyEditor.DailyProfileEditor();
      profileEditor.ShowDialog();
    }





    public event EventHandler<SelectionChangedEventArgs> UsageProfileSetsSelectionChanged;

    protected virtual void OnUsageProfileSetsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      UsageProfileSetsSelectionChanged?.Invoke(sender, e);
    }

    public event EventHandler<SelectionChangedEventArgs> UsageProfileSelectionChanged;

    protected virtual void OnUsageProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      UsageProfileSelectionChanged?.Invoke(sender, e);
    }

  }
}
