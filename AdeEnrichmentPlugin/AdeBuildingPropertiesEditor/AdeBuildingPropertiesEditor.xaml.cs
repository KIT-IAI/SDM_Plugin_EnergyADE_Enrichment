using KIT.BuW.AdvEnrichment.UI;
using MapControl;
using MapControl.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace KIT.BuW.AdvEnrichment
{
  [ValueConversion(typeof(int), typeof(bool))]
  public class IndexToBoolConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if ((int)value != -1)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  [ValueConversion(typeof(Object), typeof(bool))]
  public class ObjectToBoolConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class BooleanToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is Boolean && (bool)value)
      {
        return Visibility.Visible;
      }
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is Visibility && (Visibility)value == Visibility.Visible)
      {
        return true;
      }
      return false;
    }
  }

  [ValueConversion(typeof(bool), typeof(BuildingFunctionCalculationBasis))]
  public class BuildingFunctionCalculationBasisToBooleanConverter : IValueConverter
  {
    // value: enum value (z.B. BuildingFunctionCalculationBasis.DIN18599)
    // parameter: enum name string (z.B. "DIN18599")
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null || value == null)
        return false;

      var parameterString = parameter.ToString();
      if (Enum.IsDefined(value.GetType(), value))
      {
        var paramValue = Enum.Parse(value.GetType(), parameterString);
        return paramValue.Equals(value);
      }

      return false;
    }

    // value: bool (IsChecked), targetType: enum type
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null || !(value is bool))
        return Binding.DoNothing;

      bool use = (bool)value;
      if (!use)
        return Binding.DoNothing;

      return Enum.Parse(targetType, parameter.ToString());
    }
  }

  [ValueConversion(typeof(bool), typeof(BuildingFunctionCalculationBasis))]
  public class BuildingFunctionCalculationBasisToBooleanNotConverter : IValueConverter
  {
    // value: enum value (z.B. BuildingFunctionCalculationBasis.DIN18599)
    // parameter: enum name string (z.B. "DIN18599")
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null || value == null)
        return false;

      var parameterString = parameter.ToString();
      if (Enum.IsDefined(value.GetType(), value))
      {
        var paramValue = Enum.Parse(value.GetType(), parameterString);
        return !paramValue.Equals(value);
      }

      return false;
    }

    // value: bool (IsChecked), targetType: enum type
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null || !(value is bool))
        return Binding.DoNothing;

      bool use = (bool)value;
      if (!use)
        return Binding.DoNothing;

      return Enum.Parse(targetType, parameter.ToString());
    }
  }


  [ValueConversion(typeof(bool), typeof(ConstructionDefinitionType))]
  public class ConstructionDefinitionTypeToBooleanConverter : IValueConverter
  {
    // value: enum value (z.B. ConstructionDefinitionType.TABULA)
    // parameter: enum name string (z.B. "TABULA")
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null || value == null)
        return false;

      var parameterString = parameter.ToString();
      if (Enum.IsDefined(value.GetType(), value))
      {
        var paramValue = Enum.Parse(value.GetType(), parameterString);
        return paramValue.Equals(value);
      }

      return false;
    }

    // value: bool (IsChecked), targetType: enum type
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null || !(value is bool))
        return Binding.DoNothing;

      bool use = (bool)value;
      if (!use)
        return Binding.DoNothing;

      return Enum.Parse(targetType, parameter.ToString());
    }
  }


  public class BooleanAndConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      // Sicherstellen, dass alle Werte bool sind und true zurückgeben
      return values.OfType<bool>().All(b => b);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //// Erwartet mehrere Werte (bools oder convertible to bool). Liefert Visible nur wenn ALLE true sind, sonst Collapsed.
  //public class BoolAndToVisibilityConverter : IMultiValueConverter
  //{
  //  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
  //  {
  //    if (values == null || values.Length == 0)
  //      return Visibility.Collapsed;

  //    foreach (var v in values)
  //    {
  //      bool b;
  //      if (v == null)
  //        b = false;
  //      else if (v is bool vb)
  //        b = vb;
  //      else
  //      {
  //        // versuche Konvertierung
  //        try
  //        {
  //          b = System.Convert.ToBoolean(v);
  //        }
  //        catch
  //        {
  //          b = false;
  //        }
  //      }

  //      if (!b)
  //        return Visibility.Collapsed;
  //    }

  //    return Visibility.Visible;
  //  }

  //  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
  //  {
  //    throw new NotSupportedException();
  //  }
  //}


  public static class MyExtensions
  {
    // Function to check for null or zero values
    public static bool IsNullOrDefault<T>(T value)
    {
      return object.Equals(value, default(T));
    }
  }

  public abstract class BaseConverter : MarkupExtension
  {
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }

  [ValueConversion(typeof(object), typeof(string))]
  public class StringFormatConverter : BaseConverter, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
    {
      string format = parameter as string;
      if (!string.IsNullOrEmpty(format))
      {
        return string.Format(culture, format, value);
      }
      else
      {
        return value.ToString();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
    {
      return null;
    }
  }


  // Working data entry, holds original and modified data for a building
  internal class InternalBuildingProperties
  {
    public string id;
    public AdeBuildingProperties orgProps;
    public AdeBuildingProperties actProps;
  }


  internal class InternalYocResults
  {
    public int bldgCount = 0;
    public int bldgEnrichedCount = 0;
    public int bldgNoDataCount = 0;
    public int bldgIgnoredCount = 0;
  }


  /// <summary>
  /// Interaktionslogik für BuildingPropertiesEditor.xaml
  /// </summary>
  public partial class BuildingPropertiesEditor : Window
  {
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    private IntPtr hParentWnd_ { get; set; }

    public static System.Windows.Forms.Keys ModifierKeys { get; }

    protected IAdeDataFactory dataFactory_ { get; set; }
    internal Dictionary<string, InternalBuildingProperties> buildingPropsCache = new Dictionary<string, InternalBuildingProperties>();
    protected bool isModified_user = false;
    protected bool isModified_auto = false;
    protected bool isAutoEnhancement = true;
    internal Dictionary<string, string> infoTextDictionary;
    internal string applicationDataPath = null;

    internal ConstructionList constructionList = new ConstructionList();

    public ConstructionList myConstructionList { get { return constructionList; } }
    public SortedDictionary<string, Construction> myWallConstructions { set; get; }
    public SortedDictionary<string, Construction> myRoofConstructions { set; get; }
    public SortedDictionary<string, Construction> myGroundPlateConstructions { set; get; }
    public SortedDictionary<string, Construction> myWindowConstructions { set; get; }

    internal UsageProfileList usageProfileList = new UsageProfileList();

    public UsageProfileList myUsageProfileList { get { return usageProfileList; } }
    public SortedDictionary<string, UsageProfile> myUsageProfileHeating { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileCooling { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileHeatGainOccupants { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileHeatGainElectricalDevices { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileLighting { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileVentilation { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileShading { set; get; }
    public SortedDictionary<string, UsageProfile> myUsageProfileServiceHours { set; get; }

    public BuildingPropertiesEditor(IAdeDataFactory dataFactory, IntPtr hParentWnd)
    {
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      if (dataFactory.SelectedBuildings.Count == 0)
      {
        MessageBox.Show("No building selected or available!", "Error", MessageBoxButton.OK);
        Loaded += Window_CloseOnStart;
        return;
      }

      dataFactory_ = dataFactory;
      dataFactory_.Logger.Write("Start processing global properties");
      hParentWnd_ = hParentWnd;

      InitializeComponent();
      DataContext = this;
      var hlp = new WindowInteropHelper(this);
      hlp.Owner = hParentWnd;

      userPhysics.rbSelectByUValues.IsChecked = true;
      dataFactory_.Logger.Write("Finished processing global properties");

      // Determine the default data path
      if (!string.IsNullOrEmpty(dataFactory.ApplicationSettings.DataDirectory))
      {
        applicationDataPath = dataFactory.ApplicationSettings.DataDirectory;
      }
      else
      {
        applicationDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data");
      }

      var title = this.Title;
      var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      var txtVersionComplete = version.ToString();
      var dateVersion = (new DateTime(2016, 1, 1)).AddDays(version.Revision).ToString("d");

      this.Title = string.Format("{0} - V{1} ({2})", title, txtVersionComplete, dateVersion);

      // Display the source properties
      sourceProps = dataFactory_.DataSourceProperties;

      ///Default parameters value input
      AutoEnhanceTemplateManager.LoadTemplates();
      DefaultParameters = AutoEnhanceTemplateManager.AutoEnhanceTemplates["Alpha building"];
      bldgAutoEnrich.autoEnrichViewModel.CopyFrom(DefaultParameters);

      // Prepare the selection combo box
      modelItems = new SortedDictionary<string, string>();
      foreach (var b in dataFactory_.SelectedBuildings)
      {
        var s = GetCacheProps(b).actProps.DisplayString;
        var sKey = string.IsNullOrWhiteSpace(s) ? b : s;
        modelItems[sKey] = b;
      }
      checkedItems = new ObservableCollection<KeyValuePair<string, string>>();

      //--------Constructions list----------------------
      MyYocViewModel = new YocViewModel();
      BuildConstructionList();

      // Calculate statisics for the model
      CalculateBuildingStatistics();
      CalculateBuildingFunctionStatistics(0, MyFuncViewModel);
      CalculateYearOfConstructionStatistics();

      UpdateBuildingStatsUI();

      // update default states
      yocAutoEnrich.SetDefaultStates();

      var zoomLevel = 16;
      BoundingBox bbox;
      //if (sourceProps.MapLocation == null)
      {
        sourceProps.MapLocation = CalculateSceneProperties(out bbox);
        if (sourceProps.MapLocation != null && sourceProps.MapLocation.Lat == 0 && sourceProps.MapLocation.Lon == 0)
        {
          sourceProps.MapLocation.Lat = uiMap.wbMap.Center.Latitude;
          sourceProps.MapLocation.Lon = uiMap.wbMap.Center.Longitude;
          zoomLevel = (int)uiMap.wbMap.ZoomLevel;
        }
      }

      if (sourceProps.MapLocation != null )
      {
        uiMap.wbMap.Center = new Location(sourceProps.MapLocation.Lat, sourceProps.MapLocation.Lon);
        uiMap.wbMap.ZoomLevel = zoomLevel;
        uiMap.wbMap.RenderSize = new Size(200, 200);
        uiMap.wbMap.ZoomToBounds(bbox);
      }
      else
      {
        dataFactory_.Logger.WriteWarning(string.Format("Failed to calculate scene bounding box."));
      }

      SetMapProperties(uiMap.MapViewModel);

      // Determine the path for additionally loaded files
      var infoDictFile = Path.Combine(applicationDataPath, "InfoDictionary.xml");
      infoTextDictionary = InfoDictionaryManager.LoadInfoDictionary(infoDictFile);
      if (infoTextDictionary.Count == 0)
      {
        dataFactory_.Logger.WriteWarning(string.Format("The file {0} is either not found or contains no items.", infoDictFile));
      }

      //--------Usage profile list----------------------
      MyFuncViewModel = new FuncViewModel();
      functionAutoEnrich.SetDefaultStates();
      BuildUsageProfileList();

      SelectUsageProfiles();

      if (BuildingStats.BuildingsWithFunction > 0)
      {
        MyFuncViewModel.UseBuildingFunction = true;
        MyFuncViewModel.IsExpanded = true;
        BuildingFunctionStats.UpdateStates(MyFuncViewModel);
      }

      // fill ComboBox_BuildingFunctions
      foreach (string key in myUsageProfileList.UsageProfileSets.Keys)
      {
        if (key.Contains("DIN_18599_31001_"))
        {
          userBuilding.ComboBox_BuildingFunctions.Items.Add(key.Substring(10));
        }
      }


      SelectConstructions();

      if (BuildingStats.BuildingsWithYear > 0)
      {
        MyYocViewModel.IsExpanded = true;
        MyYocViewModel.UseYearOfConstruction = true;
        YearOfConstructionStats.UpdateStates(MyYocViewModel);
      }

      UpdateConsideredBuildings();

      ImageLoader.HttpClient.DefaultRequestHeaders.Remove("User-Agent");
      ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", $"BuildingPropertiesEditor/{typeof(BuildingPropertiesEditor).Assembly.GetName().Version}");
      TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
      //MessageBox.Show($"Cache Folder: {TileImageLoader.DefaultCacheFolder}    User-Agent: {ImageLoader.HttpClient.DefaultRequestHeaders.ToString()}");
      if (TileImageLoader.Cache is ImageFileCache cache)
      {
        Loaded += async (s, e) =>
        {
          await Task.Delay(2000);
          cache.DeleteExpiredItems();
        };
      }
    }

    ~BuildingPropertiesEditor()
    {
    }

    private void UpdateComboBoxAndButtonState(Grid grid, ComboBox comboBox)
    {
      int row = -1;

      if (comboBox.Items.Count > 0)
        comboBox.IsEnabled = true;
      else
        comboBox.IsEnabled = false;

      foreach (UIElement ui in grid.Children)
      {
        if (ui == comboBox)
        {
          row = Grid.GetRow(ui);
        }

        if (ui is Button && Grid.GetRow(ui) == row)
        {
          Button button = ui as Button;

          if (comboBox.IsEnabled == true && comboBox.SelectedIndex >= 0)
            button.IsEnabled = true;
          else
            button.IsEnabled = false;
        }
      }
    }

    private void UpdateBuildingStatsUI()
    {
      bool bEnabled = BuildingStats.BuildingsWithParts != 0;
      bldgParts.UpdateBuildingPartsStatsUI(bEnabled);
    }

    internal InternalBuildingProperties GetCacheProps(string id)
    {
      // Get the properties from the data source, if they are not yet contained in the cache
      if (!buildingPropsCache.ContainsKey(id))
      {
        var props = dataFactory_.GetBuildingProperties(id);
        var cacheItem = new InternalBuildingProperties();
        cacheItem.id = id;
        cacheItem.orgProps = props;
        cacheItem.actProps = new AdeBuildingProperties(props);
        buildingPropsCache.Add(id, cacheItem);
      }

      // return the cache entry for the specified id
      var cacheEntry = buildingPropsCache[id];
      return cacheEntry;
    }

    public AdeDataSourceProperties sourceProps { get; set; }
    public AdeBuildingProperties modelProps { get; set; }
    public AdeBuildingProperties usedProps { get; set; }
    public SortedDictionary<string, string> modelItems { get; set; }
    public ObservableCollection<KeyValuePair<string, string>> checkedItems { get; set; }
    public AdeBuildingProperties DefaultParameters { get; set; }
    public BuildingStatistics BuildingStats { get; set; }
    public BuildingStatistics BuildingPartStats { get; set; }
    public BuildingFunctionStatistics BuildingFunctionStats { get; set; }
    public YearOfConstructionStatistics YearOfConstructionStats { get; set; }
    public YocViewModel MyYocViewModel { get; set; }
    public FuncViewModel MyFuncViewModel { get; set; }
    public SettingsViewModel GlobalSettingsViewModel { get; set; } = new SettingsViewModel();


    // Notification handler usage profiles
    private void Button_Click_Cancel(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      isModified_user = true;
    }

    private void Button_Click_Edit_Profile(object sender, RoutedEventArgs e)
    {
      AdeBuildingPropertyEditor.DailyProfileEditor profileEditor = new AdeBuildingPropertyEditor.DailyProfileEditor();
      profileEditor.ShowDialog();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SetForegroundWindow(hParentWnd_);

      if (isModified_user && MessageBox.Show("Data has been modified in the user controlled enhancement mode.\nAre you sure you want to to cancel the operation?", "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
      {
        e.Cancel = true;
      }

      if (isModified_auto && MessageBox.Show("Data has been modified in the automatic enhancement mode.\nAre you sure you want to to cancel the operation?", "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
      {
        e.Cancel = true;
      }

      if (!e.Cancel)
      {
        m_isWindowClosed = true;
      }
    }

    private void Window_CloseOnStart(object sender, EventArgs e)
    {
      this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      isModified_user = false;
      isModified_auto = false;
      m_isWindowClosed = false;
    }

    private void Button_Click_Apply(object sender, RoutedEventArgs e)
    {
      if (Check_ActivateFilter.IsChecked.Value == true)
      {
        if (MyFuncViewModel.UseBuildingFunction && BuildingFunctionStats.ConsideredBuildings == 0 || 
            MyYocViewModel.UseYearOfConstruction && YearOfConstructionStats.ConsideredBuildings == 0)
        {
          MessageBox.Show("There are no matching buildings for the defined filter!", "Warning", MessageBoxButton.OK);
          return;
        }
        else
        {
          // TODO: Check if this code must be converted to use MyFuncViewModel

          //if (BuildingFunctionStats.UseBuildingFunction_other && functionAutoEnrich.Combo_BuildingFunction_Other.SelectedItem == null)
          //{
          //  MessageBox.Show("Please select a usage profile for building function type 'other'!", "Warning", MessageBoxButton.OK);
          //  return;
          //}
          //else if (BuildingFunctionStats.UseBuildingFunction_undefined && functionAutoEnrich.Combo_BuildingFunction_Undefined.SelectedItem == null)
          //{
          //  MessageBox.Show("Please select a usage profile for building function type 'undefined'!", "Warning", MessageBoxButton.OK);
          //  return;
          //}

          if (MyYocViewModel.UseYearOfConstruction && !MyYocViewModel.IsYocSelected( 0, 0))
          {
            MessageBox.Show("Please select a construction set for year of construction type 'undefined'!", "Warning", MessageBoxButton.OK);
            return;
          }

          if (MyYocViewModel.UseYearOfConstruction && !yocAutoEnrich.AreAllConstructionsSelected)
          {
            MessageBox.Show("Select a construction set for all selected time ranges", "Warning", MessageBoxButton.OK);
            return;
          }

          if ((BuildingStats.BuildingsWithLod1 > 0 || BuildingStats.BuildingsWithLod2 > 0) &&
              (!bldgAutoEnrich.autoEnrichViewModel.UseOpeningPercentageWalls) || ((bldgAutoEnrich.autoEnrichViewModel.UseOpeningPercentageWalls) && (DefaultParameters.OpeningPercentageWalls == 0.0)))
          {
            if (MessageBox.Show("Opening percentage not defined.\nDo you want to proceed without opening creation?", "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
              return;
            }
          }

          if ((BuildingStats.BuildingsWithLod1 > 0 || BuildingStats.BuildingsWithLod2 > 0) &&
              ((bldgAutoEnrich.autoEnrichViewModel.UseStoreyHeight) && (DefaultParameters.StoreyHeight.Value == 0.0)))
          {
            MessageBox.Show("Please specify a valid storey height!", "Warning", MessageBoxButton.OK);
            return;
          }
        }
      }
      else
      {
        if ((BuildingStats.BuildingsWithLod1 > 0 || BuildingStats.BuildingsWithLod2 > 0) &&
            (userBuilding.Check_Walls_Open.IsChecked == false) || ((userBuilding.Check_Walls_Open.IsChecked == true) && (DefaultParameters.OpeningPercentageWalls == 0.0)))
        {
          if (MessageBox.Show("Opening percentage not defined.\nDo you want to proceed without opening creation?", "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
          {
            return;
          }
        }
      }

      if (!MyYocViewModel.UseYearOfConstruction &&
          ((DefaultParameters.useConstruction == true && (myWindowConstructions.Count == 0 || userPhysics.WindowConstructionList.SelectedIndex == -1)) ||
           (DefaultParameters.useConstruction == false && DefaultParameters.UValueWindows == null)))
      {
        if (MessageBox.Show(string.Format("No window construction chosen.\nDo you want to proceed (Windows will be ignored)?", BuildingFunctionStats.ConsideredBuildings), "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
          return;
        }
      }

      var numBuildings = dataFactory_.SelectedBuildings.Count;
      if (MyFuncViewModel.UseBuildingFunction && (BuildingFunctionStats.ConsideredBuildings < numBuildings)) numBuildings = BuildingFunctionStats.ConsideredBuildings;
      if (MyYocViewModel.UseYearOfConstruction && (YearOfConstructionStats.ConsideredBuildings < numBuildings)) numBuildings = YearOfConstructionStats.ConsideredBuildings;
      //var numBuildings = Math.Min(BuildingFunctionStats.ConsideredBuildings, YearOfConstructionStats.ConsideredBuildings);
      if (MessageBox.Show(string.Format("{0} building(s) are considered for the enrichment process.\nDo you want to continue with enrichment?", numBuildings), "Enrichment Information", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
      {
        return;
      }

      if (isAutoEnhancement)
      {
        //---------------------------------------------------------------------------------------------
        //Automatic enhancement mode

        dataFactory_.Logger.Write("The Apply Button was clicked, performing automatic data updates");

        // Adding all buildings to cache
        foreach (var item in dataFactory_.SelectedBuildings)
        {
          EnrichmentSingleBuilding(item);

          //atttempt at automatic processing of all class properties
          //PropertyInfo[] properties = typeof(AdeBuildingProperties).GetProperties();
          //foreach (PropertyInfo property in properties)
          //{
          //  (isModified_auto && IsNullOrValue(DefaultParameters.property)
          //  {
          //    DefaultParameters.YearOfConstruction = usedProps.YearOfConstruction;
          //    property.SetValue(DefaultParameters, usedProps);
          //  }
          //}

          //attempt at writing only when default values are changed.
          //DefaultParameters.YearOfConstruction = (!MyExtensions.IsNullOrDefault(usedProps.YearOfConstruction)) ?? usedProps.YearOfConstruction;          
        }

        // Build a list of modified properties

        var resultList = new List<AdeBuildingProperties>();

        foreach (var item in buildingPropsCache)
        {
          resultList.Add(item.Value.actProps);
        }

        // Send the modified data to the application
        var operationResult = dataFactory_.UpdateCallbacks.UpdateBuildingProperties(resultList);
        // Handle the operation result from the application
        if (operationResult is ApplicationException)
        {
          dataFactory_.Logger.WriteWarning(string.Format("The callback 'UpdateBuildingProperties' failed. The result is '{0}'", operationResult.Message));
        }
        else
        {
          var button = sender as Button;
          button.IsEnabled = false;

          isModified_auto = false;
        }

        dataFactory_.Logger.Write("Automatic enhancement of building properties is completed");
      }

      else if (isModified_user)
      {
        //---------------------------------------------------------------------------------------------
        //Manual enhancement mode

        dataFactory_.Logger.Write("The Apply Button was clicked, performing manual data updates");

        // Build a list of modified properties
        var resultList = new List<AdeBuildingProperties>();
        foreach (var item in buildingPropsCache)
        {
          resultList.Add(item.Value.actProps);
        }

        // Send the modified data to the application
        var operationResult = dataFactory_.UpdateCallbacks.UpdateBuildingProperties(resultList);

        // Handle the operation result from the application
        if (operationResult is ApplicationException)
        {
          dataFactory_.Logger.Write(string.Format("The callback 'UpdateBuildingProperties' failed. The result is '{0}'", operationResult.Message));
        }
        else
        {
          var button = sender as Button;
          button.IsEnabled = false;

          isModified_user = false;
        }

        dataFactory_.Logger.Write("User controlled enhancement of building properties is completed");
      }

      Close();
    }

    private void EnrichmentSingleBuilding(string item)
    {
      var props = GetCacheProps(item);
      modelProps = props.orgProps;
      usedProps = props.actProps;

      if (modelProps.singleThermalZone && modelProps.isBuildingPart)
      {
        return;
      }

      if (!EnrichUsageProfileSingleBuilding())
      {
        return;
      }

      if (!EnrichConstructionSingleBuilding())
      {
        return;
      }

      usedProps.enrichBuildingData = true;

      // only assign values when the checkbox is ticked and the model property is Null or zero / missing or "" for strings
      // Building Parameters

      if ((userBuilding.Check_Year_of_constr.IsChecked ?? true) && ((MyExtensions.IsNullOrDefault(usedProps.YearOfConstruction)) || (OverwriteFileValues.IsChecked ?? true)))
      {
        usedProps.YearOfConstruction = DefaultParameters.YearOfConstruction;
      }

      if (((userBuilding.Check_Walls_Open.IsChecked ?? true) || (bldgAutoEnrich.autoEnrichViewModel.UseOpeningPercentageWalls)) && MyExtensions.IsNullOrDefault(usedProps.OpeningPercentageWalls))
      {
        DefaultParameters.OpeningPercentageWalls = bldgAutoEnrich.autoEnrichViewModel.OpeningPercentageWalls;
        usedProps.OpeningPercentageWalls = DefaultParameters.OpeningPercentageWalls;
      }
      if (((userBuilding.Check_Roof_Open.IsChecked ?? true) || (bldgAutoEnrich.autoEnrichViewModel.UseOpeningPercentageRoofs)) && MyExtensions.IsNullOrDefault(usedProps.OpeningPercentageRoofs))
      {
        DefaultParameters.OpeningPercentageRoofs = bldgAutoEnrich.autoEnrichViewModel.OpeningPercentageRoofs;
        usedProps.OpeningPercentageRoofs = DefaultParameters.OpeningPercentageRoofs;
      }
      if (((userBuilding.Check_Floor_Area_Ratio.IsChecked ?? true) || (bldgAutoEnrich.autoEnrichViewModel.UseFloorAreaRatio)) && MyExtensions.IsNullOrDefault(usedProps.FloorAreaRatio))
      {
        DefaultParameters.FloorAreaRatio.Value = bldgAutoEnrich.autoEnrichViewModel.FloorAreaRatio;
        usedProps.FloorAreaRatio = DefaultParameters.FloorAreaRatio;
      }
      if (((userBuilding.Check_Storey_Height.IsChecked ?? true) || (bldgAutoEnrich.autoEnrichViewModel.UseStoreyHeight)) && MyExtensions.IsNullOrDefault(usedProps.StoreyHeight))
      {
        DefaultParameters.StoreyHeight.Value = bldgAutoEnrich.autoEnrichViewModel.StoreyHeight;
        DefaultParameters.StoreyHeight.Uom = bldgAutoEnrich.autoEnrichViewModel.StoreyHeightUom;
        usedProps.StoreyHeight = DefaultParameters.StoreyHeight;
      }
      if (((userBuilding.Check_Air_Infiltr_Rate.IsChecked ?? true) || (bldgAutoEnrich.autoEnrichViewModel.UseAirInfiltrationRate)) && MyExtensions.IsNullOrDefault(usedProps.AirInfiltrationRate))
      {
        DefaultParameters.AirInfiltrationRate.Value = bldgAutoEnrich.autoEnrichViewModel.AirInfiltrationRate;
        DefaultParameters.AirInfiltrationRate.Uom = bldgAutoEnrich.autoEnrichViewModel.AirInfiltrationRateUom;
        usedProps.AirInfiltrationRate = DefaultParameters.AirInfiltrationRate;
      }

      if ((userBuilding.Check_Build_Size.IsChecked ?? true) && (MyExtensions.IsNullOrDefault(usedProps.BuildingSizeType) || (usedProps.BuildingSizeType == "")))
      {
        usedProps.BuildingSizeType = DefaultParameters.BuildingSizeType;
      }

      // Zone Parameters
      usedProps.singleThermalZone = DefaultParameters.singleThermalZone;
      usedProps.isHeated = DefaultParameters.isHeated;
      usedProps.isCooled = DefaultParameters.isCooled;
      usedProps.isMechanicallyVentilated = DefaultParameters.isMechanicallyVentilated;
    }

    private bool EnrichUsageProfileSingleBuilding()
    {
      bool enrichUsageProfiles = false;

      int buildingFunctionProfileLevel = 0;

      if (MyFuncViewModel.BuildingFunctionLevel == 0)
      {
        buildingFunctionProfileLevel = 1000;
      }
      else if (MyFuncViewModel.BuildingFunctionLevel == 1)
      {
        buildingFunctionProfileLevel = 100;
      }
      else if (MyFuncViewModel.BuildingFunctionLevel == 2)
      {
        buildingFunctionProfileLevel = 10;
      }
      else if (MyFuncViewModel.BuildingFunctionLevel == 3)
      {
        buildingFunctionProfileLevel = 1;
      }

      UsageProfileGroup usageProfileGroup = null;

      int functionCode = 0;
      if (MyFuncViewModel.CalculationBasis != BuildingFunctionCalculationBasis.Individual && (usedProps.BuildingFunction.Contains("31001_1") || usedProps.BuildingFunction.Contains("31001_2") || usedProps.BuildingFunction.Contains("31001_3")))
      {
        try
        {
          functionCode = Convert.ToInt32(usedProps.BuildingFunction.Substring(6));
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }

        string usageProfileSetString = "";

        if (MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.DIN18599)
        {
          // TODO: Check if this code must be converted to use MyFuncViewModel
          //if (!(BuildingFunctionStats.UseBuildingFunction_1000 && usedProps.BuildingFunction.Contains("31001_1")) &&
          //    !(BuildingFunctionStats.UseBuildingFunction_2000 && usedProps.BuildingFunction.Contains("31001_2")) &&
          //    !(BuildingFunctionStats.UseBuildingFunction_3000 && usedProps.BuildingFunction.Contains("31001_3")) &&
          //    !BuildingFunctionStats.UseBuildingFunction_other && !BuildingFunctionStats.UseBuildingFunction_undefined)
          //{
          //  return false;
          //}

          usageProfileSetString = "DIN_18599_31001_" + Convert.ToString(functionCode / buildingFunctionProfileLevel);
        }
        else if (MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.SIA2024)
        {
          usageProfileSetString = "DIN_SIA2024_31001_" + Convert.ToString(functionCode / buildingFunctionProfileLevel);
        }

        if (!string.IsNullOrEmpty(usageProfileSetString))
        {
          usageProfileGroup = usageProfileList.FindUsageProfileSet(usageProfileSetString);
          enrichUsageProfiles = true;
        }
      }
      else
      {
        ComboBox comboBox = null;

        // TODO: Check if this code must be converted to use MyFuncViewModel
        //if (Check_ActivateFilter.IsChecked.Value == true && functionAutoEnrich.Check_Use_BuildingFunction.IsChecked == true)
        //{
        //  // check filter options
        //  if (BuildingFunctionStats.UseBuildingFunction_1000 && usedProps.BuildingFunction.Contains("31001_1"))
        //  {
        //    comboBox = functionAutoEnrich.Combo_BuildingFunction_1000;
        //  }
        //  else if (BuildingFunctionStats.UseBuildingFunction_2000 && usedProps.BuildingFunction.Contains("31001_2"))
        //  {
        //    comboBox = functionAutoEnrich.Combo_BuildingFunction_2000;
        //  }
        //  else if (BuildingFunctionStats.UseBuildingFunction_3000 && usedProps.BuildingFunction.Contains("31001_3"))
        //  {
        //    comboBox = functionAutoEnrich.Combo_BuildingFunction_3000;
        //  }
        //  else if (BuildingFunctionStats.UseBuildingFunction_other && usedProps.BuildingFunction != "")
        //  {
        //    comboBox = functionAutoEnrich.Combo_BuildingFunction_Other;
        //  }
        //  else if (BuildingFunctionStats.UseBuildingFunction_undefined && usedProps.BuildingFunction == "")
        //  {
        //    comboBox = functionAutoEnrich.Combo_BuildingFunction_Undefined;
        //  }
        //}

        if (comboBox != null)
        {
          var keyValuePair = (KeyValuePair<string, UsageProfileGroup>)comboBox.SelectedItem;
          usageProfileGroup = keyValuePair.Value;

          enrichUsageProfiles = true;
        }
      }

      if (usageProfileGroup != null && usageProfileGroup.UsageProfileGroupMembers.Count > 0)
      {
        foreach (UsageProfileGroupMember member in usageProfileGroup.UsageProfileGroupMembers)
        {
          if (member.UsageProfile is UsageProfileHeating)
          {
            usedProps.UsageProfileHeating = member.UsageProfile as UsageProfileHeating;
          }
          else if (member.UsageProfile is UsageProfileCooling)
          {
            usedProps.UsageProfileCooling = member.UsageProfile as UsageProfileCooling;
          }
          else if (member.UsageProfile is UsageProfileOccupancy)
          {
            usedProps.UsageProfileHeatGainOccupants = member.UsageProfile as UsageProfileOccupancy;
          }
          else if (member.UsageProfile is UsageProfileDevices)
          {
            usedProps.UsageProfileHeatGainElectricalDevices = member.UsageProfile as UsageProfileDevices;
          }
          else if (member.UsageProfile is UsageProfileLighting)
          {
            usedProps.UsageProfileLighting = member.UsageProfile as UsageProfileLighting;
          }
          else if (member.UsageProfile is UsageProfileVentilation)
          {
            usedProps.UsageProfileVentilation = member.UsageProfile as UsageProfileVentilation;
          }
          else if (member.UsageProfile is UsageProfileServiceHours)
          {
            usedProps.UsageProfileServiceHours = member.UsageProfile as UsageProfileServiceHours;
          }
          else if (member.UsageProfile is UsageProfileWindowShading)
          {
            usedProps.UsageProfileShading = member.UsageProfile as UsageProfileWindowShading;
          }
        }

        enrichUsageProfiles = true;
      }
      else
      {
        // Default Usage profiles
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileHeating) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileHeating))
        {
          usedProps.UsageProfileHeating = DefaultParameters.UsageProfileHeating;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileCooling) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileCooling))
        {
          usedProps.UsageProfileCooling = DefaultParameters.UsageProfileCooling;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileHeatGainOccupants) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileHeatGainOccupants))
        {
          usedProps.UsageProfileHeatGainOccupants = DefaultParameters.UsageProfileHeatGainOccupants;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileHeatGainElectricalDevices) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileHeatGainElectricalDevices))
        {
          usedProps.UsageProfileHeatGainElectricalDevices = DefaultParameters.UsageProfileHeatGainElectricalDevices;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileLighting) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileLighting))
        {
          usedProps.UsageProfileLighting = DefaultParameters.UsageProfileLighting;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileVentilation) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileVentilation))
        {
          usedProps.UsageProfileVentilation = DefaultParameters.UsageProfileVentilation;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.UsageProfileShading) && MyExtensions.IsNullOrDefault(usedProps.UsageProfileShading))
        {
          usedProps.UsageProfileShading = DefaultParameters.UsageProfileShading;
        }

        enrichUsageProfiles = true;
      }

      return enrichUsageProfiles;
    }

    private bool EnrichConstructionSingleBuilding()
    {
      bool enricheConstruction = false;

      if (Check_ActivateFilter.IsChecked.Value == true && MyYocViewModel.UseYearOfConstruction)
      {
        CityObjectGroup selectedConstructionSet = null;

        bool hasYearOfConstruction = false;
        int yearOfConstruction = 0;

        if (usedProps.YearOfConstruction.HasValue)
        {
          hasYearOfConstruction = true;
          yearOfConstruction = usedProps.YearOfConstruction.Value;
        }
        else if (usedProps.EthosBuildingProps != null && usedProps.EthosBuildingProps.YearOfConstruction.HasValue)
        {
          hasYearOfConstruction = true;
          yearOfConstruction = usedProps.EthosBuildingProps.YearOfConstruction.Value;
        }

        YocEntry yocEntry = null;
        if (hasYearOfConstruction)
        {
          // check filter options
          yocEntry = MyYocViewModel.YocEntries.First(e => e.PeriodStart <= yearOfConstruction && e.PeriodEnd >= yearOfConstruction);
        }
        else
        {
          yocEntry = MyYocViewModel.YocEntries.First(e => e.PeriodStart == 0 && e.PeriodEnd == 0);
        }

        if (yocEntry != null && yocEntry.SelectedConstructionIndex != -1)
        {
          selectedConstructionSet = yocEntry.AvailableConstructions.ElementAt(yocEntry.SelectedConstructionIndex).Value;
          enricheConstruction = true;
        }

        if (selectedConstructionSet != null && selectedConstructionSet.GroupMembers.Count > 0)
        {
          foreach (GroupMember member in selectedConstructionSet.GroupMembers)
          {
            string title = member.Title;
            Construction construction = member.Construction;

            if (member.Title == "Fassade")
            {
              usedProps.WallConstruction = member.Construction;
            }
            else if (member.Title == "Roof")
            {
              usedProps.RoofConstruction = member.Construction;
            }
            else if (member.Title == "GroundPlate")
            {
              usedProps.GroundPlateConstruction = member.Construction;
            }
            else if (member.Title == "Window")
            {
              usedProps.WindowConstruction = member.Construction;
            }
          }

          usedProps.useConstruction = true;
          enricheConstruction = true;
        }
      }
      else
      {
        usedProps.useConstruction = DefaultParameters.useConstruction;

        // Building Physics Parameters
        if (MyExtensions.IsNullOrDefault(usedProps.ConstructionWeight) || (usedProps.ConstructionWeight == ""))
        {
          usedProps.ConstructionWeight = DefaultParameters.ConstructionWeight;
        }
        if (MyExtensions.IsNullOrDefault(usedProps.UValueWalls))
        {
          usedProps.UValueWalls = DefaultParameters.UValueWalls;
        }
        if (MyExtensions.IsNullOrDefault(usedProps.UValueRoofs))
        {
          usedProps.UValueRoofs = DefaultParameters.UValueRoofs;
        }
        if (MyExtensions.IsNullOrDefault(usedProps.UValueGroundPlates))
        {
          usedProps.UValueGroundPlates = DefaultParameters.UValueGroundPlates;
        }
        if (MyExtensions.IsNullOrDefault(usedProps.UValueDoors))
        {
          usedProps.UValueDoors = DefaultParameters.UValueDoors;
        }
        if (MyExtensions.IsNullOrDefault(usedProps.UValueWindows))
        {
          usedProps.UValueWindows = DefaultParameters.UValueWindows;
        }

        if (!MyExtensions.IsNullOrDefault(DefaultParameters.WallConstruction) && MyExtensions.IsNullOrDefault(usedProps.WallConstruction))
        {
          usedProps.WallConstruction = DefaultParameters.WallConstruction;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.RoofConstruction) && MyExtensions.IsNullOrDefault(usedProps.RoofConstruction))
        {
          usedProps.RoofConstruction = DefaultParameters.RoofConstruction;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.GroundPlateConstruction) && MyExtensions.IsNullOrDefault(usedProps.GroundPlateConstruction))
        {
          usedProps.GroundPlateConstruction = DefaultParameters.GroundPlateConstruction;
        }
        if (!MyExtensions.IsNullOrDefault(DefaultParameters.WindowConstruction) && MyExtensions.IsNullOrDefault(usedProps.WindowConstruction))
        {
          usedProps.WindowConstruction = DefaultParameters.WindowConstruction;
        }
      }

      return enricheConstruction;
    }

    private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      isModified_auto = true;
      //CheckBoxFromUIElement(sender as FrameworkElement, (sender as TextBox).Text != String.Empty);
    }


    private void UserDefinedSettingsChanged(object sender, EventArgs e)
    {
      isModified_user = true;
    }

    private void AutoEnrichSettingsChanged(object sender, EventArgs e)
    {
      isModified_auto = true;
    }

    private void ComboBox_ConstructionSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      isModified_auto = true;

      // selected construction set
      var comboBox = sender as ComboBox;
      var grid = comboBox.Parent as Grid;

      if (comboBox == null || comboBox.SelectedItem == null) return;

      var keyValuePair = (KeyValuePair<string, CityObjectGroup>)comboBox.SelectedItem;
      CityObjectGroup selectedConstructionSet = keyValuePair.Value;

      userPhysics.WallConstructionList.SelectedIndex = -1;
      userPhysics.RoofConstructionList.SelectedIndex = -1;
      userPhysics.GroundPlateConstructionList.SelectedIndex = -1;
      userPhysics.WindowConstructionList.SelectedIndex = -1;

      if (selectedConstructionSet != null && selectedConstructionSet.GroupMembers.Count > 0)
      {
        myWallConstructions = new SortedDictionary<string, Construction>();
        myRoofConstructions = new SortedDictionary<string, Construction>();
        myGroundPlateConstructions = new SortedDictionary<string, Construction>();
        myWindowConstructions = new SortedDictionary<string, Construction>();

        foreach (GroupMember member in selectedConstructionSet.GroupMembers)
        {
          string title = member.Title;
          Construction construction = member.Construction;

          if (member.Title == "Fassade")
          {
            myWallConstructions.Add(member.Construction.Name, member.Construction);
          }
          else if (member.Title == "Roof")
          {
            myRoofConstructions.Add(member.Construction.Name, member.Construction);
          }
          else if (member.Title == "GroundPlate")
          {
            myGroundPlateConstructions.Add(member.Construction.Name, member.Construction);
          }
          else if (member.Title == "Window")
          {
            myWindowConstructions.Add(member.Construction.Name, member.Construction);
          }
        }

        userPhysics.WallConstructionList.ItemsSource = myWallConstructions;
        userPhysics.RoofConstructionList.ItemsSource = myRoofConstructions;
        userPhysics.GroundPlateConstructionList.ItemsSource = myGroundPlateConstructions;
        userPhysics.WindowConstructionList.ItemsSource = myWindowConstructions;

        //if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
        {
          if (myWallConstructions.Count >= 1)
            userPhysics.WallConstructionList.SelectedIndex = 0;

          if (myRoofConstructions.Count >= 1)
            userPhysics.RoofConstructionList.SelectedIndex = 0;

          if (myGroundPlateConstructions.Count >= 1)
            userPhysics.GroundPlateConstructionList.SelectedIndex = 0;

          if (myWindowConstructions.Count >= 1)
            userPhysics.WindowConstructionList.SelectedIndex = 0;
        }

        UpdateComboBoxAndButtonState(grid, userPhysics.WallConstructionList);
        UpdateComboBoxAndButtonState(grid, userPhysics.RoofConstructionList);
        UpdateComboBoxAndButtonState(grid, userPhysics.GroundPlateConstructionList);
        UpdateComboBoxAndButtonState(grid, userPhysics.WindowConstructionList);
      }
    }

    private void ComboBox_Construction_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      isModified_auto = true;

      // selected construction
      if (!(sender is ComboBox comboBox) || comboBox.SelectedItem == null) return;

      var keyValuePair = (KeyValuePair<string, Construction>)comboBox.SelectedItem;

      if (comboBox.Name == "WallConstructionList")
      {
        DefaultParameters.WallConstruction = keyValuePair.Value as Construction;
      }
      else if (comboBox.Name == "RoofConstructionList")
      {
        DefaultParameters.RoofConstruction = keyValuePair.Value as Construction;
      }
      else if (comboBox.Name == "GroundPlateConstructionList")
      {
        DefaultParameters.GroundPlateConstruction = keyValuePair.Value as Construction;
      }
      else if (comboBox.Name == "WindowConstructionList")
      {
        DefaultParameters.WindowConstruction = keyValuePair.Value as Construction;
      }

      var grid = comboBox.Parent as Grid;

      UpdateComboBoxAndButtonState(grid, userPhysics.WallConstructionList);
      UpdateComboBoxAndButtonState(grid, userPhysics.RoofConstructionList);
      UpdateComboBoxAndButtonState(grid, userPhysics.GroundPlateConstructionList);
      UpdateComboBoxAndButtonState(grid, userPhysics.WindowConstructionList);
    }

    private void ComboBox_UsageProfileSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      isModified_auto = true;

      // selected usage profile set
      if (!(sender is ComboBox comboBox) || comboBox.SelectedItem == null) return;

      var keyValuePair = (KeyValuePair<string, UsageProfileGroup>)comboBox.SelectedItem;
      UsageProfileGroup selectedUsageProfileSet = keyValuePair.Value;

      if (selectedUsageProfileSet != null && selectedUsageProfileSet.UsageProfileGroupMembers.Count > 0)
      {
        myUsageProfileHeating = new SortedDictionary<string, UsageProfile>();
        myUsageProfileCooling = new SortedDictionary<string, UsageProfile>();
        myUsageProfileHeatGainOccupants = new SortedDictionary<string, UsageProfile>();
        myUsageProfileHeatGainElectricalDevices = new SortedDictionary<string, UsageProfile>();
        myUsageProfileLighting = new SortedDictionary<string, UsageProfile>();
        myUsageProfileVentilation = new SortedDictionary<string, UsageProfile>();
        myUsageProfileShading = new SortedDictionary<string, UsageProfile>();
        myUsageProfileServiceHours = new SortedDictionary<string, UsageProfile>();

        foreach (UsageProfileGroupMember member in selectedUsageProfileSet.UsageProfileGroupMembers)
        {
          if (member.UsageProfile is UsageProfileHeating)
          {
            myUsageProfileHeating.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileCooling)
          {
            myUsageProfileCooling.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileOccupancy)
          {
            myUsageProfileHeatGainOccupants.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileDevices)
          {
            myUsageProfileHeatGainElectricalDevices.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileLighting)
          {
            myUsageProfileLighting.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileVentilation)
          {
            myUsageProfileVentilation.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileServiceHours)
          {
            myUsageProfileServiceHours.Add(member.UsageProfile.Name, member.UsageProfile);
          }
          else if (member.UsageProfile is UsageProfileWindowShading)
          {
            myUsageProfileShading.Add(member.UsageProfile.Name, member.UsageProfile);
          }
        }

        occupancyUserDef.UsageProfileHeatingList.ItemsSource = myUsageProfileHeating;
        occupancyUserDef.UsageProfileCoolingList.ItemsSource = myUsageProfileCooling;
        occupancyUserDef.UsageProfileHeatGainOccupantsList.ItemsSource = myUsageProfileHeatGainOccupants;
        occupancyUserDef.UsageProfileHeatGainElectricalDevicesList.ItemsSource = myUsageProfileHeatGainElectricalDevices;
        occupancyUserDef.UsageProfileLightingList.ItemsSource = myUsageProfileLighting;
        occupancyUserDef.UsageProfileVentilationList.ItemsSource = myUsageProfileVentilation;
        occupancyUserDef.UsageProfileShadingList.ItemsSource = myUsageProfileShading;
        //UsageProfileServiceHoursList.ItemsSource              = myUsageProfileServiceHours;

        //if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
        {
          if (myUsageProfileHeating.Count >= 1)
          {
            occupancyUserDef.UsageProfileHeatingList.SelectedIndex = 0;
          }

          if (myUsageProfileCooling.Count >= 1)
          {
            occupancyUserDef.UsageProfileCoolingList.SelectedIndex = 0;
          }

          if (myUsageProfileHeatGainOccupants.Count >= 1)
          {
            occupancyUserDef.UsageProfileHeatGainOccupantsList.SelectedIndex = 0;
          }

          if (myUsageProfileHeatGainElectricalDevices.Count >= 1)
          {
            occupancyUserDef.UsageProfileHeatGainElectricalDevicesList.SelectedIndex = 0;
          }

          if (myUsageProfileLighting.Count >= 1)
          {
            occupancyUserDef.UsageProfileLightingList.SelectedIndex = 0;
          }

          if (myUsageProfileVentilation.Count >= 1)
          {
            occupancyUserDef.UsageProfileVentilationList.SelectedIndex = 0;
          }

          if (myUsageProfileShading.Count >= 1)
          {
            occupancyUserDef.UsageProfileShadingList.SelectedIndex = 0;
          }

          //if (myUsageProfileServiceHours.Count >= 1)
          //  UsageProfileServiceHoursList.SelectedIndex = 0;
        }

        var grid = comboBox.Parent as Grid;

        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileHeatingList);
        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileCoolingList);
        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileHeatGainOccupantsList);
        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileHeatGainElectricalDevicesList);
        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileLightingList);
        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileVentilationList);
        //updateComboBoxAndButtonState(grid, UsageProfileServiceHoursList);
        UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileShadingList);
      }
    }

    private void ComboBox_UsageProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      isModified_auto = true;

      // selected usage profile
      if (!(sender is ComboBox comboBox) || comboBox.SelectedItem == null) return;

      var keyValuePair = (KeyValuePair<string, UsageProfile>)comboBox.SelectedItem;

      if (comboBox.Name == "UsageProfileHeatingList")
      {
        DefaultParameters.UsageProfileHeating = keyValuePair.Value as UsageProfileHeating;
      }
      else if (comboBox.Name == "UsageProfileCoolingList")
      {
        DefaultParameters.UsageProfileCooling = keyValuePair.Value as UsageProfileCooling;
      }
      else if (comboBox.Name == "UsageProfileHeatGainOccupantsList")
      {
        DefaultParameters.UsageProfileHeatGainOccupants = keyValuePair.Value as UsageProfileOccupancy;
      }
      else if (comboBox.Name == "UsageProfileHeatGainElectricalDevicesList")
      {
        DefaultParameters.UsageProfileHeatGainElectricalDevices = keyValuePair.Value as UsageProfileDevices;
      }
      else if (comboBox.Name == "UsageProfileLightingList")
      {
        DefaultParameters.UsageProfileLighting = keyValuePair.Value as UsageProfileLighting;
      }
      else if (comboBox.Name == "UsageProfileVentilationList")
      {
        DefaultParameters.UsageProfileVentilation = keyValuePair.Value as UsageProfileVentilation;
      }
      else if (comboBox.Name == "UsageProfileServiceHoursList")
      {
        DefaultParameters.UsageProfileServiceHours = keyValuePair.Value as UsageProfileServiceHours;
      }
      else if (comboBox.Name == "UsageProfileShadingList")
      {
        DefaultParameters.UsageProfileShading = keyValuePair.Value as UsageProfileWindowShading;
      }

      var grid = comboBox.Parent as Grid;

      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileHeatingList);
      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileCoolingList);
      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileHeatGainOccupantsList);
      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileHeatGainElectricalDevicesList);
      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileLightingList);
      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileVentilationList);
      //updateComboBoxAndButtonState(grid, UsageProfileServiceHoursList);
      UpdateComboBoxAndButtonState(grid, occupancyUserDef.UsageProfileShadingList);
    }

    private void UpdateBindings(UIElementCollection group)
    {
      foreach (var c in group)
      {
        if (c is TextBox)
        {
          var be = (c as TextBox).GetBindingExpression(TextBox.TextProperty);
          if (be != null)
          {
            be.UpdateTarget();
          }
        }

        if (c is ComboBox)
        {
          var be = (c as ComboBox).GetBindingExpression(ComboBox.TextProperty);
          if (be != null)
          {
            be.UpdateTarget();
          }
        }

        if (c is CheckBox)
        {
          var be = (c as CheckBox).GetBindingExpression(CheckBox.IsCheckedProperty);
          if (be != null)
          {
            be.UpdateTarget();
          }
        }
      }
    }

    private void EnableItems(bool bEnable, UIElementCollection items)
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

    private void CheckBox_ActivateFilter(object sender, RoutedEventArgs e)
    {
      if (functionAutoEnrich != null
        && functionAutoEnrich.gridBuildingFunction != null
        && functionAutoEnrich.gridFilterBuildingFunction != null
        && yocAutoEnrich.gridYearOfConstruction != null)
      {
        Text_NoBuildings.IsEnabled = Check_ActivateFilter.IsChecked.Value;
        NoBuildings.IsEnabled = Check_ActivateFilter.IsChecked.Value;
        Text_ConsideredBuildings.IsEnabled = Check_ActivateFilter.IsChecked.Value;
        ConsideredBuildings.IsEnabled = Check_ActivateFilter.IsChecked.Value;
        EnableItems(Check_ActivateFilter.IsChecked.Value, functionAutoEnrich.gridBuildingFunction.Children);
        //functionAutoEnrich.rbBuildingFunction_DIN18599.IsEnabled = (Check_ActivateFilter.IsChecked.Value && MyFuncViewModel.UseBuildingFunction);
        //functionAutoEnrich.rbBuildingFunction_SIA2024.IsEnabled = (Check_ActivateFilter.IsChecked.Value && MyFuncViewModel.UseBuildingFunction);
        //functionAutoEnrich.rbBuildingFunction_Individual.IsEnabled = (Check_ActivateFilter.IsChecked.Value && MyFuncViewModel.UseBuildingFunction);
        EnableItems(Check_ActivateFilter.IsChecked.Value && MyFuncViewModel.UseBuildingFunction, functionAutoEnrich.gridFilterBuildingFunction.Children);
        EnableItems(Check_ActivateFilter.IsChecked.Value, yocAutoEnrich.gridYearOfConstruction.Children);
        //EnableItems(Check_ActivateFilter.IsChecked.Value && MyYocViewModel.UseYearOfConstruction, yocAutoEnrich.gridFilterYearOfConstruction.Children);
      }
    }

    private void RadioButton_CalculationBasis(object sender, RoutedEventArgs e)
    {
      var level = MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.Individual
        ? 3 : MyFuncViewModel.BuildingFunctionLevel;
      CalculateBuildingFunctionStatistics(level, MyFuncViewModel);
      BuildUsageProfileList();
      SelectUsageProfiles();
      UpdateConsideredBuildings();
    }

    private void BuildUsageProfileList()
    {
      if (applicationDataPath == null)
        return;

      var filename = Path.Combine(applicationDataPath, "UsageProfileList.xml");

      try
      {
        UsageProfileDefinitionType type = UsageProfileDefinitionType.USER_DEFINED;

        if (MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.DIN18599)
        {
          type = UsageProfileDefinitionType.DIN_18599;
        }
        else if (MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.SIA2024)
        {
          type = UsageProfileDefinitionType.SIA_2024;
        }

        usageProfileList.ReadUsageProfileList(dataFactory_.Logger, filename, type);

        Func< KeyValuePair<string, UsageProfileGroup>, bool> filter;
        string filterSuffix = string.Empty;
        if (MyFuncViewModel.CalculationBasis != BuildingFunctionCalculationBasis.Individual)
        {
          switch (MyFuncViewModel.BuildingFunctionLevel)
          {
            case 0:
              filterSuffix = "000";
              filter = i =>
                type != UsageProfileDefinitionType.DIN_18599
                || (i.Key.Contains("DIN_18599") && i.Key.Contains("000-"));
              break;

            case 1:
              filterSuffix = "00";
              filter = i =>
                type != UsageProfileDefinitionType.DIN_18599
                || (i.Key.Contains("DIN_18599") && i.Key.Contains("00-"));
              break;

            case 2:
              filterSuffix = "0";
              filter = i =>
                type != UsageProfileDefinitionType.DIN_18599
                || (i.Key.Contains("DIN_18599") && i.Key.Contains("0-"));
              break;

            case 3:
            default:
              filter = i =>
                type != UsageProfileDefinitionType.DIN_18599
                || i.Key.Contains("DIN_18599");
              break;
          }
        }
        else
        {
          filter = i => true;
        }


        myUsageProfileHeating = new SortedDictionary<string, UsageProfile>();
        myUsageProfileCooling = new SortedDictionary<string, UsageProfile>();
        myUsageProfileHeatGainOccupants = new SortedDictionary<string, UsageProfile>();
        myUsageProfileHeatGainElectricalDevices = new SortedDictionary<string, UsageProfile>();
        myUsageProfileLighting = new SortedDictionary<string, UsageProfile>();
        myUsageProfileVentilation = new SortedDictionary<string, UsageProfile>();
        myUsageProfileShading = new SortedDictionary<string, UsageProfile>();
        myUsageProfileServiceHours = new SortedDictionary<string, UsageProfile>();

        foreach (KeyValuePair<string, UsageProfile> item in usageProfileList.UsageProfiles)
        {
          if (MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.DIN18599 && !item.Key.Contains("DIN_18599_"))
          {
            continue;
          }
          else if (MyFuncViewModel.CalculationBasis == BuildingFunctionCalculationBasis.SIA2024 && !item.Key.Contains("SIA_2024_"))
          {
            continue;
          }

          if (item.Value is UsageProfileHeating)
          {
            myUsageProfileHeating.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileCooling)
          {
            myUsageProfileCooling.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileOccupancy)
          {
            myUsageProfileHeatGainOccupants.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileDevices)
          {
            myUsageProfileHeatGainElectricalDevices.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileLighting)
          {
            myUsageProfileLighting.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileVentilation)
          {
            myUsageProfileVentilation.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileServiceHours)
          {
            myUsageProfileServiceHours.Add(item.Key, item.Value);
          }
          else if (item.Value is UsageProfileWindowShading)
          {
            myUsageProfileShading.Add(item.Key, item.Value);
          }
        }

        // Initialize the FunctionProfile view model with the loaded usage profiles
        MyFuncViewModel.FuncEntries.Clear();
        var availableProfileSet = myUsageProfileList.UsageProfileSets.Where(filter).ToArray();
        int idx = 0;
        foreach (var usageProfile in availableProfileSet)
        {
          var count = BuildingFunctionStats.BuildingCounts.Where(x => usageProfile.Key.Contains(x.Key + filterSuffix)).FirstOrDefault().Value;
          if (count > 0)
          {
            var newEntry = new FuncEntry();
            var key = usageProfile.Key ?? string.Empty;
            var pos = key.IndexOf('-');
            newEntry.Title = pos >= 0 ? key.Substring(pos + 1) : key;
            newEntry.IsEnabled = true;
            newEntry.UseThisEntry = true;
            newEntry.SelectedFunctionIndex = idx;
            newEntry.AvailableUsageProfiles = availableProfileSet;
            newEntry.Count = count;

            MyFuncViewModel.FuncEntries.Add(newEntry);
          }
          idx++;
        }

        // TODO: Hier hat MyFuncViewModel nur noch den Undefined-Eintrag. Die Suche nach den BuildingFunctionCounts war nicht erfolgreich????

        MyFuncViewModel.FuncEntries.Add(
          new FuncEntry()
          {
            Title = "Undefined",
            IsEnabled = true,
            UseThisEntry = false,
            SelectedFunctionIndex = -1,
            AvailableUsageProfiles = availableProfileSet,
            Count = BuildingFunctionStats.BuildingCounts[BuildingFunctionStatistics.UNDEFINED_KEY]
          });

        OnPropertyChanged(nameof(MyFuncViewModel));
        BuildingFunctionStats.UpdateStates(MyFuncViewModel);
      }
      catch (Exception ex)
      {
        dataFactory_.Logger.WriteError($"The usage profile list '{filename}' could not be read. Exception: {ex.Message}");
        MessageBox.Show(ex.Message, "Exception in usage profile list");
      }
    }

    private void SelectUsageProfiles()
    {
      // TODO: Check if this code must be converted to use MyFuncViewModel

      //if (functionAutoEnrich.Combo_BuildingFunction_1000 == null)
      //  return;

      //functionAutoEnrich.Combo_BuildingFunction_1000.SelectedIndex = -1;
      //functionAutoEnrich.Combo_BuildingFunction_2000.SelectedIndex = -1;
      //functionAutoEnrich.Combo_BuildingFunction_3000.SelectedIndex = -1;
      //functionAutoEnrich.Combo_BuildingFunction_Other.SelectedIndex = -1;
      //functionAutoEnrich.Combo_BuildingFunction_Undefined.SelectedIndex = -1;

      //if (functionAutoEnrich.rbBuildingFunction_DIN18599.IsChecked == true)
      //{
      //  SelectUsageProfile("DIN_18599_31001_1000", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_1000);
      //  SelectUsageProfile("DIN_18599_31001_2000", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_2000);
      //  SelectUsageProfile("DIN_18599_31001_3000", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_3000);
      //  SelectUsageProfile("DIN_18599_31001_", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_Other);
      //}
      //else if (functionAutoEnrich.rbBuildingFunction_SIA2024.IsChecked == true)
      //{
      //  SelectUsageProfile("SIA_2024_31001_1000", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_1000);
      //  SelectUsageProfile("SIA_2024_31001_2000", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_2000);
      //  SelectUsageProfile("SIA_2024_31001_3000", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_3000);
      //  SelectUsageProfile("SIA_2024_31001_", myUsageProfileList.UsageProfileSets, functionAutoEnrich.Combo_BuildingFunction_Other);
      //}

      //UpdateComboBoxState(functionAutoEnrich.Combo_BuildingFunction_1000, functionAutoEnrich.Check_BuildingFunction_1000, BuildingFunctionStats.BuildingFunction_1000);
      //BuildingFunctionStats.UseBuildingFunction_1000 = functionAutoEnrich.Check_BuildingFunction_1000.IsChecked.Value;
      //UpdateComboBoxState(functionAutoEnrich.Combo_BuildingFunction_2000, functionAutoEnrich.Check_BuildingFunction_2000, BuildingFunctionStats.BuildingFunction_2000);
      //BuildingFunctionStats.UseBuildingFunction_2000 = functionAutoEnrich.Check_BuildingFunction_2000.IsChecked.Value;
      //UpdateComboBoxState(functionAutoEnrich.Combo_BuildingFunction_3000, functionAutoEnrich.Check_BuildingFunction_3000, BuildingFunctionStats.BuildingFunction_3000);
      //BuildingFunctionStats.UseBuildingFunction_3000 = functionAutoEnrich.Check_BuildingFunction_3000.IsChecked.Value;
      //UpdateComboBoxState(functionAutoEnrich.Combo_BuildingFunction_Other, functionAutoEnrich.Check_BuildingFunction_Other, BuildingFunctionStats.BuildingFunction_other);
      //BuildingFunctionStats.UseBuildingFunction_other = functionAutoEnrich.Check_BuildingFunction_Other.IsChecked.Value;
      //UpdateComboBoxState(functionAutoEnrich.Combo_BuildingFunction_Undefined, functionAutoEnrich.Check_BuildingFunction_Undefined, BuildingFunctionStats.BuildingFunction_undefined);
      //BuildingFunctionStats.UseBuildingFunction_undefined = functionAutoEnrich.Check_BuildingFunction_Undefined.IsChecked.Value;
    }

    private void SelectUsageProfile(string findStr, SortedDictionary<string, UsageProfileGroup> dictionary, ComboBox comboBox)
    {
      int pos = 0;
      foreach (KeyValuePair<string, UsageProfileGroup> key in comboBox.Items)
      {
        if (key.Key.Contains(findStr))
        {
          comboBox.SelectedIndex = pos;
          return;
        }
        ++pos;
      }

      comboBox.SelectedIndex = -1;
    }

    private void ConstructionBasisChanged(object sender, ConstructionDefinitionType type)
    {
      BuildConstructionList(type);
      CalculateYearOfConstructionStatistics();
      SelectConstructions();
    }

    private void BuildConstructionList(ConstructionDefinitionType type = ConstructionDefinitionType.TABULA)
    {
      if (applicationDataPath == null)
        return;

      var filename = Path.Combine(applicationDataPath, "ConstructionList.gml");
      var database = Path.Combine(applicationDataPath, "NaiS.db");

      try
      {
        constructionList = new ConstructionList();
        if (type != ConstructionDefinitionType.TABULA)
        {
          constructionList.ReadConstructionList(dataFactory_.Logger, filename, type);
        }
        if (type != ConstructionDefinitionType.USER_DEFINED)
        {
          constructionList.Query(dataFactory_.Logger, database, sourceProps.CountryCode, type);
        }

        myWallConstructions = new SortedDictionary<string, Construction>(constructionList.Constructions);
        myRoofConstructions = new SortedDictionary<string, Construction>(constructionList.Constructions);
        myGroundPlateConstructions = new SortedDictionary<string, Construction>(constructionList.Constructions);
        myWindowConstructions = new SortedDictionary<string, Construction>(constructionList.Constructions);

        var yocRanges = constructionList.YearRanges;

        // Create time ranges from the data
        if (type == ConstructionDefinitionType.USER_DEFINED)
        {
          yocRanges = new List<(int firstYear, int lastYear)>();
          var minmax = dataFactory_.SelectedBuildings.Aggregate((firstYear: int.MaxValue, lastYear: int.MinValue), (acc, bldg) =>
          {
            var yoc = GetCacheProps(bldg)?.actProps.YearOfConstruction;
            if (yoc != null)
            {
              acc.firstYear = Math.Min(acc.firstYear, yoc.Value);
              acc.lastYear = Math.Max(acc.lastYear, yoc.Value);
            }
            return acc;
          });

          // Calculate ca. 8 intervalls, with at least 5y
          minmax.firstYear = (int)(Math.Floor(minmax.firstYear / 10.0) * 10);
          minmax.lastYear = (int)(Math.Ceiling(minmax.lastYear / 10.0) * 10);
          var interval = (int)(Math.Ceiling( (minmax.lastYear - minmax.firstYear) / 8.0 / 5.0) * 5);

          for ( var y = minmax.firstYear; y < minmax.lastYear; y += interval)
          {
            yocRanges.Add((y, y + interval));
          }
        }

        // Create the entries for the year ranges
        MyYocViewModel.YocEntries.Clear();
        foreach( var item in yocRanges)
        {
          var format = "Between {0} - {1}";
          if (item.firstYear == 0) format = "Before {1}";
          else if (item.lastYear == 0) format = "Since {0}";
          var title = string.Format(format, item.firstYear, item.lastYear);
          var selectedSets = constructionList.ConstructionSets.Where(g => 
            type == ConstructionDefinitionType.USER_DEFINED
            || (g.Value.PeriodStart >= item.firstYear && g.Value.PeriodEnd <= item.lastYear));
          var newEntry = new YocEntry { 
            Title = title, 
            IsEnabled = true, 
            PeriodStart = item.firstYear, 
            PeriodEnd = item.lastYear, 
            AvailableConstructions = selectedSets 
          };
          MyYocViewModel.YocEntries.Add(newEntry);
        }

        var newEntryDefault = new YocEntry { 
          Title = "Not Defined", 
          SelectedConstructionIndex = -1, 
          IsEnabled = true, 
          UseThisEntry = false
        };
        newEntryDefault.AvailableConstructions = constructionList.ConstructionSets.Where(g => true);
        MyYocViewModel.YocEntries.Add(newEntryDefault);

        // Give the construction sets to the YoC view model...
        MyYocViewModel.Constructions = new ObservableCollection<KeyValuePair<string, CityObjectGroup>>(myConstructionList.ConstructionSets);
        OnPropertyChanged(nameof(MyYocViewModel.Constructions));
        OnPropertyChanged(nameof(MyYocViewModel));

      }
      catch (Exception ex)
      {
        dataFactory_.Logger.WriteError($"The construction list '{filename}' could not be read. Exception: {ex.Message}");
        MessageBox.Show(ex.Message, "Exception in construction list");
      }
    }

    private void SelectConstructions()
    {
      foreach (var entry in MyYocViewModel.YocEntries)
      {
        entry.SelectedConstructionIndex = -1;
      }

      if (MyYocViewModel.ConstructionBasis == ConstructionDefinitionType.DENA)
      {
        //type = ConstructionDefinitionType.DENA;
      }
      else if (MyYocViewModel.ConstructionBasis == ConstructionDefinitionType.TABULA || MyYocViewModel.ConstructionBasis == ConstructionDefinitionType.USER_DEFINED)
      {
        foreach( var entry in MyYocViewModel.YocEntries)
        {
          entry.Count = YearOfConstructionStats.GetCount( entry.PeriodStart, entry.PeriodEnd);
          entry.UseThisEntry = entry.IsEnabled && entry.Count > 0;
          if (entry.Count != 0)
          {
            if (entry.AvailableConstructions != null 
              && entry.AvailableConstructions.Count() != 0
              && !(entry.PeriodStart == 0 && entry.PeriodEnd == 0))
            {
              entry.UseThisEntry = true;
              entry.SelectedConstructionIndex = entry.AvailableConstructions.Count() - 1;
            }
            else if (entry.SelectedConstructionIndex != -1)
            { 
              entry.UseThisEntry = false;
            }
          }

        }
      }
    }

    private void UpdateComboBoxState(ComboBox comboBox, CheckBox checkBox, int count)
    {
      if (count <= 0)
      {
        comboBox.SelectedIndex = -1;
      }

      if (comboBox.SelectedIndex == -1)
        checkBox.IsChecked = false;
      else
        checkBox.IsChecked = true;
    }

    private void RadioButton_UValue_Checked(object sender, RoutedEventArgs e)
    {
      if (DefaultParameters != null)
      {
        DefaultParameters.useConstruction = userPhysics.UseConstructions;
      }

      //EnableItems(true, gridPropertiesUValue.Children);
      //EnableItems(false, gridPropertiesConstruction.Children);
    }

    /// <summary>
    /// This method is called when a building function is checked or unchecked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BuildingFunctionChecked(object sender, RoutedEventArgs e)
    {
      if (BuildingFunctionStats != null)
      {
        BuildingFunctionStats.UpdateStates(MyFuncViewModel);

        UpdateConsideredBuildings();
      }
    }

    private void YearOfConstruction_SelectionChanged(object sender, RoutedEventArgs e)
    {
      if (YearOfConstructionStats != null)
      {
        YearOfConstructionStats.UpdateStates(MyYocViewModel);

        UpdateConsideredBuildings();
      }
    }

    private void UpdateConsideredBuildings()
    {
      if (MyFuncViewModel != null)
      {
        if (MyFuncViewModel.UseBuildingFunction && !MyYocViewModel.UseYearOfConstruction)
        {
          ConsideredBuildings.Text = BuildingFunctionStats.ConsideredBuildings.ToString();
        }
        else if (MyFuncViewModel.UseBuildingFunction && MyYocViewModel.UseYearOfConstruction)
        {
          ConsideredBuildings.Text = YearOfConstructionStats.ConsideredBuildings.ToString();
        }
        else if (MyFuncViewModel.UseBuildingFunction && MyYocViewModel.UseYearOfConstruction)
        {
          // attention is only a temporary solution as long as there is no real statistic for it!
          ConsideredBuildings.Text = Math.Min(BuildingFunctionStats.ConsideredBuildings, YearOfConstructionStats.ConsideredBuildings).ToString();
        }
        else
        {
          ConsideredBuildings.Text = "0";
        }
      }
    }

    private void RadioButton_Construction_Checked(object sender, RoutedEventArgs e)
    {
      if (DefaultParameters != null)
      {
        DefaultParameters.useConstruction = userPhysics.UseConstructions;
      }
      //EnableItems(false, gridPropertiesUValue.Children);
      //EnableItems(true, gridPropertiesConstruction.Children);

      //var radioButton = sender as RadioButton;
      //var grid = radioButton.Parent as Grid;

      //// disbale edit buttons
      //SetEditButtonState(grid, 2, false);
    }

    private void Image_ToolTipOpening(object sender, ToolTipEventArgs e)
    {
      //Windows.Data.
      if (e.Source is Image)
      {
        var img = e.Source as Image;

        // Create a HTML label if not already done so
        if (!(img.ToolTip is TheArtOfDev.HtmlRenderer.WPF.HtmlLabel))
        {
          try
          {
            img.ToolTip = new TheArtOfDev.HtmlRenderer.WPF.HtmlLabel();
          }
          catch
          {
          }
        }

        // Assign the HTML text to the label
        if (img.ToolTip is TheArtOfDev.HtmlRenderer.WPF.HtmlLabel)
        {
          var html = img.ToolTip as TheArtOfDev.HtmlRenderer.WPF.HtmlLabel;

          if (infoTextDictionary.ContainsKey(img.Tag.ToString()))
          {
            html.Text = infoTextDictionary[img.Tag.ToString()];
          }
          else
          {
            html.Text = String.Format("-- <i>no help available</i> -- (<b>{0}</b>)", img.Tag);
          }
        }
      }
    }

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]

    private void SetMapProperties(MapViewModel mapData)
    {
      foreach (var bldgid in dataFactory_.SelectedBuildings)
      {
        var bldg = GetCacheProps(bldgid).actProps;
        if (bldg.buildingGeometry != null)
        {
          foreach (var boundary in bldg.buildingGeometry)
          {
            var ar = boundary.boundaryGeometry.ToArray();
            var l = new PolygonItem();
            foreach (var item in ar)
            {
              l.Locations.Add(new Location(item.Lat, item.Lon));
            }
            if (ar.Length > 0)
            {
              l.Locations.Add(l.Locations.First());
              mapData.Polygons.Add(l);
            }
          }
        }
      }
    }

    /// <summary>
    /// Set a checkbox state in the same grid row as the given framework element to the value provided as argument
    /// </summary>
    /// <param name="el">The changed element</param>
    /// <param name="bSet">The checkbox to be checked</param>
    private void CheckBoxFromUIElement(FrameworkElement el, bool bSet)
    {
      Grid grid = el.Parent as Grid;
      int row = Grid.GetRow(el);

      foreach (var ui in grid.Children)
      {
        if (Grid.GetRow(ui as UIElement) == row && ui is CheckBox)
        {
          var checkbox = ui as CheckBox;
          checkbox.IsChecked = bSet;
        }
      }
    }


    /// <summary>
    /// Set or clear a checkbox depending on the selection state of a neighboring combo box
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="e">The event args</param>
    private void Combo_SelectionChanged_UpdateCheckbox(object sender, SelectionChangedEventArgs e)
    {
      CheckBoxFromUIElement (sender as FrameworkElement, (sender as ComboBox).SelectedIndex != -1);
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
            if (child is ComboBox)
            {
              var combobox = child as ComboBox;
              combobox.SelectedIndex = -1;
            }
            child.IsEnabled = bEnable;
          }
        }
      }
    }

    public bool IsWindowClosed
    {
      get
      {
        return m_isWindowClosed;
      }
    }

    private bool m_isWindowClosed = true;

    // An internal HTTP client to perform some HTTP requests
    private static HttpClient m_httpClient = new HttpClient(new HttpClientHandler { });
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(25);

    /// <summary>
    /// Walk over the buildings and generate the year of construction with external help...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void yocAutoEnrich_GenerateYearOfConstruction(object sender, RoutedEventArgs e)
    {
      // On the first call define the base URI
      // TODO: get the URI somehow from the host
      if (m_httpClient.BaseAddress == null)
      {
        m_httpClient.BaseAddress = new Uri("https://sdm-yoc.cloud.iai.kit.edu/find");
      }

      var tasks = new List<Task>();

      // Walk over all buildings
      var bldgStats = new InternalYocResults();
      foreach (var b in dataFactory_.SelectedBuildings)
      {
        // Check if this building exists, has no yoc and has a geometry
        var props = GetCacheProps(b);
        if (props != null 
          && props.actProps?.YearOfConstruction == null
          && props.actProps?.buildingGeometry != null
          && props.actProps?.CountryCode == "DE")
        {
          // Process the buildings in parallel, as limited by the semaphore
          await _semaphore.WaitAsync();
          try
          {
            tasks.Add(ProcessSingleBuilding(props, _semaphore, bldgStats));
          }
          finally
          {
            //_semaphore.Release();
          }
        }
        else
        {
          // Building has been ignored ...
          bldgStats.bldgIgnoredCount++;
        }
      }
      await Task.WhenAll(tasks);

      foreach (var b in dataFactory_.SelectedBuildings)
      {
        var props = GetCacheProps(b);
        if (props != null && props.actProps?.EthosBuildingProps != null)
        {
          MyYocViewModel.UseYearOfConstruction = true;
          break;
        }
      }

      // Update the statistics
      CalculateYearOfConstructionStatistics();

      // Reset the button text
      yocAutoEnrich.SetStatus(null);

      // Log some results
      dataFactory_.Logger.Write(
        $"Yoc Enrichment result: Total: {dataFactory_.SelectedBuildings.Count}, "
        + $"Ignored {bldgStats.bldgIgnoredCount}, "
        + $"Processed: {bldgStats.bldgCount}, "
        + $"Enriched: {bldgStats.bldgEnrichedCount}, "
        + $"Failed/NoData: {bldgStats.bldgNoDataCount}");
    }

    private async Task ProcessSingleBuilding(InternalBuildingProperties props, SemaphoreSlim semaphore, InternalYocResults stats)
    {
      // Count this call
      stats.bldgCount++;

      // Build the enclosing geometry...
      StringBuilder sb = new StringBuilder();
      sb.Append("?srs=4326&pts=");
      List<String> pts = new List<string>();
      foreach (var pt in props.actProps.buildingGeometry[0].boundaryGeometry)
      {
        pts.Add(String.Format(CultureInfo.InvariantCulture, "{0} {1}", pt.Lon, pt.Lat));
      }
      sb.Append(String.Join(", ", pts));

      try
      {
        // Give some feedback...
        Mouse.OverrideCursor = Cursors.Wait;
        yocAutoEnrich.SetStatus(String.Format("Processing {0} of {1}", stats.bldgCount, dataFactory_.SelectedBuildings.Count));

        // Perform the query for a single building...
        HttpResponseMessage response = await m_httpClient.GetAsync(sb.ToString());
        response.EnsureSuccessStatusCode();

        string result = response.Content.ReadAsStringAsync().Result.TrimEnd();
        dataFactory_.Logger.Write("yoc result: " + result);

        // If the response contains a positive result, extract the result into the properties
        dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(result);
        if (responseJson != null && responseJson.results != 0)
        {
          // Extract the WKT reference point from json
          Point ptRef = null;
          string strPt = responseJson.point;
          var parts = strPt.Split(new char[] { '(', ')', ' ', ',' });
          if (parts.Length >= 2)
          {
            float x, y;
            float.TryParse(parts[0], out x);
            float.TryParse(parts[1], out y);
            ptRef = new Point { EPSGCode = 3035, X = x, Y = y };
          }

          // Construct the porperties structure
          props.actProps.EthosBuildingProps = new EthosBuildingProperties
          {
            ExternalID = responseJson.id,
            GeneratorLineage = responseJson.yoc.lineage,
            GeneratorSource = responseJson.yoc.source,
            ReferencePoint = ptRef,
            YearOfConstruction = responseJson.yoc.value
          };

          // Count as successful result
          stats.bldgEnrichedCount++;
        }
        else
        {
          // Count the invalid response as failure
          stats.bldgNoDataCount++;
        }
      }
      catch (Exception ex)
      {
        dataFactory_.Logger.WriteWarning(ex.Message);
      }
      finally
      {
        Mouse.OverrideCursor = null;
        semaphore.Release();
      }
    }

    private void ProfileLevelSelChanged(object sender, SelectionChangedEventArgs e)
    {
      CalculateBuildingFunctionStatistics(MyFuncViewModel.BuildingFunctionLevel, MyFuncViewModel);
      BuildUsageProfileList();
      SelectUsageProfiles();
      UpdateConsideredBuildings();
    }
  }

}
