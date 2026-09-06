using KIT.BuW.AdvEnrichment.UI;
using MapControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KIT.BuW.AdvEnrichment
{
  public class BuildingFunctionStatistics
  {
    public static readonly string UNDEFINED_KEY = "_undefined";
    public BuildingFunctionStatistics()
    {
      BuildingCounts[UNDEFINED_KEY] = 0;
    }

    public int ConsideredBuildings { get; set; } = 0;

    public Dictionary<string, int> BuildingCounts { get; set; } = new Dictionary<string, int>();

    public void processBuilding(AdeBuildingProperties src, int level)
    {
      string key = null;
      if (!string.IsNullOrEmpty(src.BuildingFunction))
      {
        int removeCount = 0;
        switch( level)
        {
          case 0: removeCount = 3; break;
          case 1: removeCount = 2; break;
          case 2: removeCount = 1; break;
          default: break;
        }
        removeCount = Math.Min(removeCount, src.BuildingFunction.Length);
        key = src.BuildingFunction.Substring(0, src.BuildingFunction.Length - removeCount);
      }
      if (string.IsNullOrEmpty(key)) { key = UNDEFINED_KEY; }
      if (!BuildingCounts.ContainsKey(key))
      {
        BuildingCounts[key] = 0;
      }
      BuildingCounts[key]++;
    }

    // Update the considered buildings count based on the count values in the FuncViewModel
    // TODO: Move this code to the FuncViewModel class and make it a method of that class,
    //       so that the BuildingFunctionStatistics class does not need to know about the FuncViewModel class.
    public void UpdateStates(FuncViewModel myViewModel)
    {
      int _consideredBuildings = 0;

      if (myViewModel != null)
      {
        foreach (var i in myViewModel.FuncEntries)
        {
          if (i.UseThisEntry)
          {
            _consideredBuildings += i.Count;
          }
        }
      }

      ConsideredBuildings = _consideredBuildings;
    }
  }

  public partial class BuildingPropertiesEditor
  {
    private void CalculateBuildingFunctionStatistics(int level, FuncViewModel myViewModel)
    {
      var blgdFunctionStats = new BuildingFunctionStatistics();

      if (level < 1 || level > 3)
      {
        level = 0;
      }

      foreach (var id in dataFactory_.SelectedBuildings)
      {
        var bldg = GetCacheProps(id).actProps;

        if (bldg.isBuildingPart == false)
        {
          blgdFunctionStats.processBuilding(bldg, level);
        }
      }

      blgdFunctionStats.UpdateStates(myViewModel);

      BuildingFunctionStats = blgdFunctionStats;
    }
  }
}
