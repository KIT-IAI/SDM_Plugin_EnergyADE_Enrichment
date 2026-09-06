using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  public class FuncEntry
  {
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsEnabled { get; set; }
    public bool UseThisEntry { get; set; }
    public int SelectedFunctionIndex { get; set; }
    public string HelpText { get; set; }
    public IEnumerable<KeyValuePair<string, UsageProfileGroup>> AvailableUsageProfiles { get; set; }
  }

  public enum BuildingFunctionCalculationBasis
  {
    DIN18599,
    SIA2024,
    Individual
  };

  public class FuncViewModel
  {
    private BuildingFunctionCalculationBasis _calculationBasis = BuildingFunctionCalculationBasis.DIN18599;

    public bool IsExpanded { get; set; } = false;
    public bool UseBuildingFunction { get; set; } = true;
    public int BuildingFunctionLevel { get; set; } = 0;
    public BuildingFunctionCalculationBasis CalculationBasis
    {
      get => _calculationBasis;
      set 
      {
        if (_calculationBasis == value) return;
        _calculationBasis = value;
      }
    }
    public int BuildingFunctionLevelCount { get; set; } = 0;
    public ObservableCollection<FuncEntry> FuncEntries { get; set; } = new ObservableCollection<FuncEntry>();
    public ObservableCollection<KeyValuePair<string, UsageProfile>> Functions{ get; set; }

    //public bool IsFuncSelected(int periodStart, int periodEnd)
    //{
    //  var entry = YocEntries.First(i => i.PeriodStart == periodStart && i.PeriodEnd == periodEnd);
    //  return (entry != null && (!entry.UseThisEntry || entry.SelectedConstructionIndex != -1));
    //}
  }

  /// <summary>
  /// Interaktionslogik für FunctionAutoEnrich.xaml
  /// </summary>
  public partial class FunctionAutoEnrich : BaseUserControl
  {
    public FunctionAutoEnrich()
    {
      InitializeComponent();
    }

    public void SetDefaultStates()
    {
      CheckBox_Use_BuildingFunction(null, null);
    }

    private void CheckBox_Use_BuildingFunction(object sender, RoutedEventArgs e)
    {
      if (gridFilterBuildingFunction != null)
      {
        UpdateBuildingFunctionFilter();
        OnChildCheckBoxChanged( new RoutedEventArgs());
        if (DataContext is BuildingPropertiesEditor)
        {
          EnableItems((DataContext as BuildingPropertiesEditor).MyFuncViewModel.UseBuildingFunction, gridFilterBuildingFunction.Children);
        }
        if (itemsFuncFilterRows.ItemsSource != null)
        {
          CollectionViewSource.GetDefaultView(itemsFuncFilterRows.ItemsSource).Refresh();
        }
      }
    }

    public event EventHandler<RoutedEventArgs> ChildProfilesChanged;
    protected virtual void OnChildProfilesChanged(RoutedEventArgs e)
    {
      ChildProfilesChanged?.Invoke(this, e);
    }

    private void RadioButton_CalculationBasis(object sender, RoutedEventArgs e)
    {
      OnChildProfilesChanged(e);
      UpdateBuildingFunctionFilter();
      //BuildUsageProfileList();
      //SelectUsageProfiles();
    }

    /// <summary>
    /// Enable or disable sibling UI elements in a grid row
    /// </summary>
    /// <param name="el">An element from the grid row to use as reference</param>
    /// <param name="bEnable">Enable or disable the row</param>
    private void EnableFromUIElement(FrameworkElement el, bool bEnable)
    {
      Grid grid = el.Parent as Grid;
      int row = Grid.GetRow(el);

      foreach (var ui in grid.Children)
      {
        if (ui is FrameworkElement)
        {
          var child = ui as FrameworkElement;
          if (Grid.GetRow(child) == row)
          {
            if (child is CheckBox)
            {
              var checkbox = child as CheckBox;
              checkbox.IsChecked = false;
            }
            //if (child is ComboBox)
            //{
            //  var combobox = child as ComboBox;
            //  combobox.SelectedIndex = -1;
            //}
            child.IsEnabled = bEnable;
          }
        }
      }
    }


    public event EventHandler<SelectionChangedEventArgs> ChildComboSelChanged;
    protected virtual void OnChildComboSelChanged(object sender, SelectionChangedEventArgs e)
    {
      ChildComboSelChanged?.Invoke(sender, e);
    }
    private void Combo_SelectionChanged_UpdateCheckbox(object sender, SelectionChangedEventArgs e)
    {
      OnChildComboSelChanged(sender, e);
    }


    public event EventHandler<RoutedEventArgs> ChildCheckboxChanged;
    protected virtual void OnChildCheckBoxChanged(RoutedEventArgs e)
    {
      ChildCheckboxChanged?.Invoke(this, e);
    }
    private void CheckBox_BuildingFunction(object sender, RoutedEventArgs e)
    {
      OnChildCheckBoxChanged(e);
    }


    private void UpdateBuildingFunctionFilter()
    {
      if (itemsFuncFilterRows.DataContext != null)
      {
        var m = (itemsFuncFilterRows.DataContext as BuildingPropertiesEditor).MyFuncViewModel;
        foreach (var row in m.FuncEntries)
        {
          row.IsEnabled = row.Count > 0;
        }
      }
    }

    public event EventHandler<SelectionChangedEventArgs> ProfileLevelSelChanged;
    protected virtual void OnProfileLevelSelChanged(object sender, SelectionChangedEventArgs e)
    {
      ProfileLevelSelChanged?.Invoke(sender, e);
    }
    private void Combo_BuildingFunction_Level_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnProfileLevelSelChanged(sender, e);
    }
  }
}
