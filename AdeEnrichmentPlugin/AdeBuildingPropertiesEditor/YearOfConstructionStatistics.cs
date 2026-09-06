using KIT.BuW.AdvEnrichment.UI;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace KIT.BuW.AdvEnrichment
{
  public class YearOfConstructionStatistics
  {
    protected class TimeRangeStats
    {
      public int YearStart { get; set; }
      public int YearEnd { get; set; }
      public int Count { get; set; }
    }

    protected System.Collections.Generic.List<TimeRangeStats> TimeRanges;

    public void AddTimeRange( int Start, int End)
    {
      if (TimeRanges == null)
      {
        TimeRanges = new System.Collections.Generic.List<TimeRangeStats>();
      }

      if (TimeRanges.Find( e => (Start == e.YearStart && End == e.YearEnd) ) == null)
      {
        TimeRanges.Add(new TimeRangeStats { YearStart = Start, YearEnd = End });
      }
    }

    public int GetCount( int Start, int End)
    {
      var tr = TimeRanges.Find(e => (Start == e.YearStart && End == e.YearEnd));
      if (tr != null)
      {
        return tr.Count;
      }

      return 0;
    }

    public int ConsideredBuildings { get; private set; } = 0;
    public bool HasGermanBuildings { get; set; } = false;
    public Visibility UIVisibility { get; set; } = Visibility.Hidden;

    public void CopyCounts(YearOfConstructionStatistics other)
    {
      if (other != null)
      {
        ConsideredBuildings = other.ConsideredBuildings;
        TimeRanges = other.TimeRanges;
      }
    }

    public void processBuilding(AdeBuildingProperties src)
    {
      void classifyYearOfConstruction(int yoc)
      {
        var tr = TimeRanges.Find( e => (e.YearStart == 0 || e.YearStart <= yoc) && (e.YearEnd == 0 || yoc <= e.YearEnd) );
        if (tr != null)
        {
          tr.Count++;
        }
      };

      if (src.YearOfConstruction.HasValue)
      {
        classifyYearOfConstruction(src.YearOfConstruction.Value);
      }
      else if (src.EthosBuildingProps != null && src.EthosBuildingProps.YearOfConstruction.HasValue)
      {
        classifyYearOfConstruction(src.EthosBuildingProps.YearOfConstruction.Value);
      }
      else
      {
        //_yearOfConstruction_undefined++;
        var tr = TimeRanges.Find(e => (e.YearStart == 0 && e.YearEnd == 0));
        if (tr != null)
        {
          tr.Count++;
        }
      }
    }

    public void UpdateStates(YocViewModel y)
    {
      ConsideredBuildings = 0;

      foreach ( var e in y.YocEntries)
      {
        if ( e.UseThisEntry)
        {
          ConsideredBuildings += e.Count;
        }
      }
    }
  }

  public partial class BuildingPropertiesEditor: INotifyPropertyChanged
  {
    private void CalculateYearOfConstructionStatistics()
    {
      var newYocStats = new YearOfConstructionStatistics();
      //foreach (var c in myConstructionList.ConstructionSets)
      //{
      //  newYocStats.AddTimeRange(c.Value.PeriodStart, c.Value.PeriodEnd);
      //}
      //newYocStats.AddTimeRange(0, 0);
      foreach (var c in MyYocViewModel.YocEntries)
      {
        newYocStats.AddTimeRange(c.PeriodStart, c.PeriodEnd);
      }

      //if (YearOfConstructionStats == null)
      {
        YearOfConstructionStats = newYocStats;
      }

      foreach (var id in dataFactory_.SelectedBuildings)
      {
        var bldg = GetCacheProps(id).actProps;

        if (bldg.isBuildingPart == false)
        {
          newYocStats.processBuilding(bldg);
        }

        if (bldg.CountryCode == "DE")
        {
          newYocStats.HasGermanBuildings = true;
          newYocStats.UIVisibility = Visibility.Visible;
        }
      }

      YearOfConstructionStats.CopyCounts(newYocStats);
      YearOfConstructionStats.UpdateStates(MyYocViewModel);
      UpdateConsideredBuildings();

      OnPropertyChanged(nameof(YearOfConstructionStats));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
