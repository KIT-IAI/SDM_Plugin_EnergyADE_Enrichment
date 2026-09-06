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
  /// Interaktionslogik für BuildingPhysicsUserDef.xaml
  /// </summary>
  public partial class BuildingPhysicsUserDef : BaseUserControl
  {
    public BuildingPhysicsUserDef()
    {
      InitializeComponent();
    }

    private bool useConstructions;

    public bool UseConstructions {  get {  return useConstructions; } }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnSettingsChanged();
    }

    private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      OnSettingsChanged();
    }

    private void RadioButton_UValue_Checked(object sender, RoutedEventArgs e)
    {
      EnableItems(true, gridPropertiesUValue.Children);
      EnableItems(false, gridPropertiesConstruction.Children);

      this.useConstructions = false;

      OnRadioButtonChecked();
    }

    private void RadioButton_Construction_Checked(object sender, RoutedEventArgs e)
    {
      EnableItems(false, gridPropertiesUValue.Children);
      EnableItems(true, gridPropertiesConstruction.Children);

      // disbale edit buttons
      var radioButton = sender as RadioButton;
      var grid = radioButton.Parent as Grid;
      SetEditButtonState(grid, 2, false);

      this.useConstructions = true;

      OnRadioButtonChecked();
    }

    private void ComboBox_ConstructionSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnConstructionSetsSelectionChanged(sender, e);
    }

    private void ComboBox_Construction_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      OnConstructionSelectionChanged(sender, e);
    }

    private void Button_Click_Edit_Construction(object sender, RoutedEventArgs e)
    {
      // selected Construction
      var button = sender as Button;
      var grid = button.Parent as Grid;

      //grid.Children.OfType<ComboBox>().Where(i => Grid.GetRow(i) == 1);

      var row = Grid.GetRow(button);

      Construction selectedConstruction = null;

      foreach (UIElement ui in grid.Children)
      {
        if (Grid.GetRow(ui) == row && Grid.GetColumn(ui) == 1/*ui.GetType() == ComboBox*/)
        {
          if (ui is ComboBox comboBox && comboBox.SelectedItem != null)
          {
            var keyValuePair = (KeyValuePair<string, Construction>)comboBox.SelectedItem;
            //selectedConstruction = comboBox.SelectedItem as Construction;
            selectedConstruction = keyValuePair.Value;
          }
        }
      }

      if (selectedConstruction != null)
      {
        AdeBuildingPropertyEditor.ConstructionEditor constructionEditor = new AdeBuildingPropertyEditor.ConstructionEditor(selectedConstruction);
        constructionEditor.ShowDialog();
      }
    }


    #region Custom Events

    public event EventHandler<EventArgs> RadioButtonChecked;

    protected virtual void OnRadioButtonChecked()
    {
      RadioButtonChecked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<SelectionChangedEventArgs> ConstructionSetsSelectionChanged;

    protected virtual void OnConstructionSetsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ConstructionSetsSelectionChanged?.Invoke(sender, e);
    }

    public event EventHandler<SelectionChangedEventArgs> ConstructionSelectionChanged;

    protected virtual void OnConstructionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ConstructionSelectionChanged?.Invoke(sender, e);
    }

    #endregion
  }
}
