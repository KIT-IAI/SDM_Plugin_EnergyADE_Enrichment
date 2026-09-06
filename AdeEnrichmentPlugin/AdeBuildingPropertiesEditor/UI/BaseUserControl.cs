using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KIT.BuW.AdvEnrichment.UI
{
  public class StringToIntValidationRule : ValidationRule
  {
    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
      int i;
      if (int.TryParse(value.ToString(), out i))
        return new ValidationResult(true, null);

      return new ValidationResult(false, "Please enter a valid integer value.");
    }
  }

  public class NumericValidationRule : ValidationRule
  {
    public double Min { get; set; } = 0;
    public double Max { get; set; } = 100;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
      var s = value.ToString();
      double dVal;
      if (!double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out dVal))
      {
        return new ValidationResult(false, "Not a floating point number.");
      }

      if (dVal < Min || dVal > Max)
      {
        return new ValidationResult(false, "Not in valid range.");
      }

      return ValidationResult.ValidResult;
    }
  }


  public partial class BaseUserControl : UserControl
  {

    #region Custom Events

    public event EventHandler<EventArgs> SettingsChanged;

    protected virtual void OnSettingsChanged()
    {
      SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<ToolTipEventArgs> ChildToolTipOpening;
    protected virtual void OnChildToolTipOpening(ToolTipEventArgs e)
    {
      ChildToolTipOpening?.Invoke(this, e);
    }

    internal void Image_ToolTipOpening(object sender, ToolTipEventArgs e)
    {
      OnChildToolTipOpening(e);
    }

    #endregion


    protected void EnableItems(bool bEnable, UIElementCollection items)
    {
      foreach (UIElement item in items)
      {
        //Console.WriteLine("GroupItem: {0}", item);
        if (!(item is RadioButton))
        {
          item.IsEnabled = bEnable;
        }
      }
    }

    protected void SetEditButtonState(Grid grid, int column, bool state)
    {
      foreach (UIElement ui in grid.Children)
      {
        if (Grid.GetColumn(ui) == column) // Button
        {
          if (ui is Button)
          {
            Button button = ui as Button;
            button.IsEnabled = state;
          }
        }
      }
    }


  }
}
