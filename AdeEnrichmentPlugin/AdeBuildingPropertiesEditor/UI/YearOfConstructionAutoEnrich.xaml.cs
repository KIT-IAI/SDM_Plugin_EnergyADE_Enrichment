using KIT.BuW.AdvEnrichment.Dummy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
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
  public class YocEntry
  {
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsEnabled { get; set; }
    public bool UseThisEntry { get; set; }
    public int SelectedConstructionIndex { get; set; }
    public string HelpText { get; set; }
    public int PeriodStart { get; set; }
    public int PeriodEnd { get; set; }
    public IEnumerable<KeyValuePair<string, CityObjectGroup>> AvailableConstructions { get; set; }
  }

  public class YocViewModel
  {
    public bool IsExpanded { get; set; } = false;
    public bool UseYearOfConstruction { get; set; } = false;
    public ConstructionDefinitionType ConstructionBasis { get; set; } = ConstructionDefinitionType.TABULA;
    public ObservableCollection<YocEntry> YocEntries { get; set; }
    public ObservableCollection<KeyValuePair<string, CityObjectGroup>> Constructions { get; set; }

    public bool IsYocSelected( int periodStart, int periodEnd)
    {
      var entry = YocEntries.First(i => i.PeriodStart == periodStart && i.PeriodEnd == periodEnd);
      return (entry != null && (!entry.UseThisEntry || entry.SelectedConstructionIndex != -1));
    }
    public YocViewModel()
    {
      YocEntries = new ObservableCollection<YocEntry> {};
    }
  }

  /// <summary>
  /// Interaktionslogik für YearOfConstructionAutoEnrich.xaml
  /// </summary>
  public partial class YearOfConstructionAutoEnrich : BaseUserControl
  {
    public YearOfConstructionAutoEnrich()
    {
      InitializeComponent();
      DefaultButtonContent = (Button_Enrich as Button).Content as String;
    }

    public void SetDefaultStates()
    {
      CheckBox_Use_YearOfConstruction(null, null);
    }

    private void CheckBox_Use_YearOfConstruction(object sender, RoutedEventArgs e)
    {
      UpdateYearOfConstructionFilter();
      OnSelectionChanged(new RoutedEventArgs());
      if (YocFilterRows.ItemsSource != null)
      {
        CollectionViewSource.GetDefaultView(YocFilterRows.ItemsSource).Refresh();
      }
    }

    /// <summary>
    /// Update all children of the YearOfConstruction filter grid
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="e"></param>
    private void UpdateYearOfConstructionFilter()
    {
      if (YocFilterRows.DataContext != null)
      {
        var m = (YocFilterRows.DataContext as BuildingPropertiesEditor).MyYocViewModel;
        foreach ( var row in m.YocEntries)
        {
          row.IsEnabled = row.Count > 0;
        }
      }
    }


    private void RadioButton_ConstructionBasis(object sender, RoutedEventArgs e)
    {
      if (DataContext == null)
      {
        return;
      }

      ConstructionDefinitionType type = (DataContext as BuildingPropertiesEditor).MyYocViewModel.ConstructionBasis;

      OnConstructionDefinitionTypeChanged(type);
      UpdateYearOfConstructionFilter();
      OnSelectionChanged(new RoutedEventArgs());
      if (YocFilterRows.ItemsSource != null)
      {
        CollectionViewSource.GetDefaultView(YocFilterRows.ItemsSource).Refresh();
      }
    }

    /// <summary>
    /// Enable the grid row depending on the content of a text box
    /// </summary>
    /// <param name="sender">The text box lement to be checked</param>
    /// <param name="e">The event arguments</param>
    private void DisableRowOnNullValue_YearOfConstruction(object sender, TextChangedEventArgs e)
    {
      bool bDefault = (YocFilterRows.DataContext as BuildingPropertiesEditor).MyYocViewModel.UseYearOfConstruction;
      int count = 0;
      if (!int.TryParse((sender as TextBox).Text, out count))
      {
        count = 0;
      }
      EnableFromUIElement(sender as FrameworkElement, count > 0 && bDefault);
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


    public event EventHandler<ConstructionDefinitionType> ConstructionDefinitionTypeChanged;
    protected virtual void OnConstructionDefinitionTypeChanged(ConstructionDefinitionType type)
    {
      ConstructionDefinitionTypeChanged?.Invoke(this, type);
    }


    public event EventHandler<SelectionChangedEventArgs> ChildComboSelChanged;
    protected virtual void OnChildComboSelChanged(object sender, SelectionChangedEventArgs e)
    {
      ChildComboSelChanged?.Invoke(sender, e);
    }
    /// <summary>
    /// Set or clear a checkbox depending on the selection state of a neighboring combo box
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="e">The event args</param>
    private void Combo_SelectionChanged_UpdateCheckbox(object sender, SelectionChangedEventArgs e)
    {
      OnChildComboSelChanged(sender, e);
    }


    public event EventHandler<RoutedEventArgs> SelectionChanged;
    protected virtual void OnSelectionChanged(RoutedEventArgs e)
    {
      SelectionChanged?.Invoke(this, e);
    }
    private void CheckBox_YearOfConstruction(object sender, RoutedEventArgs e)
    {
      OnSelectionChanged(e);
    }

    // Forward the button click to the application...
    public event EventHandler<RoutedEventArgs> GenerateYearOfConstruction;
    protected virtual void OnGenerateYearOfConstruction(RoutedEventArgs e)
    {
      GenerateYearOfConstruction?.Invoke(this, e);
    }

    string DefaultButtonContent;
    public void SetStatus(string statusText)
    {
      if (Button_Enrich is Button)
      { 
        var buttonText = Button_Enrich as Button;
        if (statusText != null)
        {
          buttonText.Content = statusText;
        }
        else
        {
          buttonText.Content = DefaultButtonContent;
          RadioButton_ConstructionBasis(this, new RoutedEventArgs());
        }
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      OnGenerateYearOfConstruction(e);
      OnSettingsChanged();
    }

    public bool UseYearOfConstructionXxxx
    {
      get
      {
        return (YocFilterRows.DataContext as BuildingPropertiesEditor).MyYocViewModel.UseYearOfConstruction;
      }
    }

    public bool AreAllConstructionsSelected
    {
      get
      {
        if (YocFilterRows.DataContext != null)
        {
          var m = (YocFilterRows.DataContext as BuildingPropertiesEditor).MyYocViewModel;
          foreach (var row in m.YocEntries)
          {
            if (row.UseThisEntry && row.SelectedConstructionIndex == -1)
              return false;
          }
        }

        return true;
      }
    }
  }
}
