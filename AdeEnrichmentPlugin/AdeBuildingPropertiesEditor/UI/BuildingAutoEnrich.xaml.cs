using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KIT.BuW.AdvEnrichment.UI
{
  /// <summary>
  /// Interaktionslogik für BuildingAutoEnrich.xaml
  /// </summary>
  public partial class BuildingAutoEnrich : BaseUserControl //, IDataErrorInfo
  {
    public class AutoEnrichViewModel
    {
      private double openingPercentageWalls;
      private double openingPercentageRoofs;
      private double floorAreaRatio;
      private double storeyHeight;
      private double airInfiltrationRate;


      public string OpeningPercentageWallsText_ { get; set; }
      public string OpeningPercentageRoofsText_ { get; set; }
      public string FloorAreaRatioText_ { get; set; }
      public string StoreyHeightText_ { get; set; }
      public string AirInfiltrationRateText_ { get; set; }

      public double OpeningPercentageWalls
      {
        get => openingPercentageWalls;
        set { openingPercentageWalls = value; OpeningPercentageWallsText_ = value.ToString(CultureInfo.CurrentCulture); }
      }
      public double OpeningPercentageRoofs
      {
        get => openingPercentageRoofs;
        set { openingPercentageRoofs = value; OpeningPercentageRoofsText_ = value.ToString(CultureInfo.CurrentCulture); }
      }
      public double FloorAreaRatio
      {
        get => floorAreaRatio;
        set { floorAreaRatio = value; FloorAreaRatioText_ = value.ToString(CultureInfo.CurrentCulture); }
      }
      public double StoreyHeight
      {
        get => storeyHeight;
        set { storeyHeight = value; StoreyHeightText_ = value.ToString(CultureInfo.CurrentCulture); }
      }
      public string StoreyHeightUom { get; set; }
      public double AirInfiltrationRate
      { 
        get => airInfiltrationRate;
        set { airInfiltrationRate = value; AirInfiltrationRateText_ = value.ToString(CultureInfo.CurrentCulture); }
      }
      public string AirInfiltrationRateUom { get; set; }

      public void CopyFrom(AdeBuildingProperties rhs)
      {
        OpeningPercentageWalls = rhs.OpeningPercentageWalls.Value;
        OpeningPercentageRoofs = rhs.OpeningPercentageRoofs.Value;
        FloorAreaRatio = rhs.FloorAreaRatio.Value;
        StoreyHeight = rhs.StoreyHeight.Value;
        StoreyHeightUom = rhs.StoreyHeight.Uom;
        AirInfiltrationRate = rhs.AirInfiltrationRate.Value;
        AirInfiltrationRateUom = rhs.AirInfiltrationRate.Uom;
      }

      public bool UseOpeningPercentageWalls { get; set; } = true;
      public bool UseOpeningPercentageRoofs { get; set; } = false;
      public bool UseStoreyHeight { get; set; } = false;
      public bool UseFloorAreaRatio { get; set; } = true;
      public bool UseAirInfiltrationRate { get; set; } = false;
    }

    public BuildingAutoEnrich()
    {
      InitializeComponent();
      DataContext = this;
    }

    public AutoEnrichViewModel autoEnrichViewModel { get; set; } = new AutoEnrichViewModel();

    //public string this[string columnName]
    //{
    //  get { return "dummy"; }
    //}

    //public string Error => null;

    public event EventHandler<TextChangedEventArgs> ChildTextChanged;
    protected virtual void OnChildTextChanged(TextChangedEventArgs e)
    {
      ChildTextChanged?.Invoke(this, e);
    }

    private void AutoEnrich_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      Debug.Write(DataContext);
      OnChildTextChanged(e);
    }

    private void AutoEnrich_TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is System.Windows.Controls.TextBox tb) || autoEnrichViewModel == null)
      {
        return;
      }

      // Verwende den tatsächlichen Text der TextBox und parse kultur-sensitiv
      if (!double.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var value))
      {
        return;
      }

      switch (tb.Name)
      {
        case "txtOPWalls":
          autoEnrichViewModel.OpeningPercentageWalls = value;
          break;

        case "txtOPRoofs":
          autoEnrichViewModel.OpeningPercentageRoofs = value;
          break;

        case "txtFloorAreaRatio":
          autoEnrichViewModel.FloorAreaRatio = value;
          break;

        case "txtStoreyHeight":
          autoEnrichViewModel.StoreyHeight = value;
          break;

        case "txtAirInfiltrationRate":
          autoEnrichViewModel.AirInfiltrationRate = value;
          break;

        default:
          // unbekannte TextBox — keine Aktion
          break;
      }
    }

    //private void AutoEnrich_TextBox_LostFocus(object sender, RoutedEventArgs e)
    //{
    //  double d = 0;
    //  if (sender == txtOPWalls)
    //  {
    //    if (double.TryParse(autoEnrichViewModel.OpeningPercentageWallsText_, out d))
    //    {
    //      autoEnrichViewModel.OpeningPercentageWalls = d;
    //    }
    //  }
    //  else if (sender == txtOPRoofs)
    //  {
    //    if (double.TryParse(autoEnrichViewModel?.OpeningPercentageRoofsText_, out d))
    //    {
    //      autoEnrichViewModel.OpeningPercentageRoofs = d;
    //    }
    //  }
    //  else if (sender == txtFloorAreaRatio)
    //  {
    //    if (double.TryParse(autoEnrichViewModel?.FloorAreaRatioText_, out d))
    //    {
    //      autoEnrichViewModel.FloorAreaRatio = d;
    //    }
    //  }
    //  else if (sender == txtStoreyHeight)
    //  {
    //    if (double.TryParse(autoEnrichViewModel?.StoreyHeightText_, out d))
    //    {
    //      autoEnrichViewModel.StoreyHeight = d;
    //    }
    //  }
    //  else if (sender == txtAirInfiltrationRate)
    //  {
    //    if (double.TryParse(autoEnrichViewModel?.AirInfiltrationRateText_, out d))
    //    {
    //      autoEnrichViewModel.AirInfiltrationRate = d;
    //    }
    //  }
    //}
  }
}
