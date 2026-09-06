using MapControl;
using System;

namespace KIT.BuW.AdvEnrichment
{
  public class BuildingStatistics
  {
    private int _totalBuildings = 0;
    private int _buildingsWithParts = 0;
    private int _buildingsWithSolid = 0;
    private int _buildingsWithCorrectSolid = 0;
    private int _buildingsWithSurface = 0;
    private int _buildingsWithLod0 = 0;
    private int _buildingsWithLod1 = 0;
    private int _buildingsWithLod2 = 0;
    private int _buildingsWithLod3 = 0;
    private int _buildingsWithLod4 = 0;
    private int _buildingsNoGeometry = 0;
    private int _buildingsWithFunction = 0;
    private int _buildingsWithYear = 0;
    private int _buildingParts = 0;
    private int _maxThermalZones = 0;

    public int TotalBuildings => _totalBuildings;
    public int BuildingsWithParts => _buildingsWithParts;
    public int BuildingsWithSolid => _buildingsWithSolid;
    public int BuildingsWithCorrectSolid => _buildingsWithCorrectSolid;
    public int BuildingsWithSurface => _buildingsWithSurface;
    public int BuildingsWithLod0 => _buildingsWithLod0;
    public int BuildingsWithLod1 => _buildingsWithLod1;
    public int BuildingsWithLod2 => _buildingsWithLod2;
    public int BuildingsWithLod3 => _buildingsWithLod3;
    public int BuildingsWithLod4 => _buildingsWithLod4;
    public int BuildingsNoGeometry => _buildingsNoGeometry;
    public int BuildingsWithFunction => _buildingsWithFunction;
    public int BuildingsWithYear => _buildingsWithYear;
    public int BuildingParts => _buildingParts;
    public int MaxThermalZones => _maxThermalZones;

    public void processBuilding(AdeBuildingProperties src)
    {
      if (!src.isBuildingPart)
      {
        _totalBuildings++;
        if (src.buildingParts != null && src.buildingParts.Count > 0) _buildingsWithParts++;
      }
      else
      {
        _buildingParts++;
      }

      if (src.buildingParts != null)
      {
        _buildingParts += src.buildingParts.Count;
        _maxThermalZones += src.buildingParts.Count;
      }
      else
      {
        _maxThermalZones++;
      }

      if (src.hasBuildingSolid) _buildingsWithSolid++;
      if (src.hasCorrectSolid) _buildingsWithCorrectSolid++;
      if (src.hasSolidFromSurface) _buildingsWithSurface++;
      if (src.hasLoD0Geometry) _buildingsWithLod0++;
      if (src.hasLoD1Geometry) _buildingsWithLod1++;
      if (src.hasLoD2Geometry) _buildingsWithLod2++;
      if (src.hasLoD3Geometry) _buildingsWithLod3++;
      if (src.hasLoD4Geometry) _buildingsWithLod4++;
      if (src.hasLoD0Geometry == false && src.hasLoD1Geometry == false && src.hasLoD2Geometry == false && src.hasLoD3Geometry == false && src.hasLoD4Geometry == false) _buildingsNoGeometry++;
      if ((src.YearOfConstruction ?? 0) != 0) _buildingsWithYear++;
      if (!string.IsNullOrWhiteSpace(src.AlkisFunction)) _buildingsWithFunction++;
    }
  }

  public partial class BuildingPropertiesEditor
  {
    private void CalculateBuildingStatistics()
    {
      var blgdStats = new BuildingStatistics();
      var partStats = new BuildingStatistics();

      foreach (var id in dataFactory_.SelectedBuildings)
      {
        var bldg = GetCacheProps(id).actProps;

        if (!bldg.isBuildingPart)
        {
          blgdStats.processBuilding(bldg);
        }
        else
        {
          partStats.processBuilding(bldg);
        }
      }
      BuildingStats = blgdStats;
      BuildingPartStats = partStats;
    }

    private GeoPoint CalculateSceneProperties(out BoundingBox bb)
    {
      bool hasBuildingGeometry = false;

      var bboxpt1 = new GeoPoint() { Lat = double.MaxValue, Lon = double.MaxValue, Alt = double.MaxValue };
      var bboxpt2 = new GeoPoint() { Lat = double.MinValue, Lon = double.MinValue, Alt = double.MinValue };
      foreach (var id in dataFactory_.SelectedBuildings)
      {
        var bldg = GetCacheProps(id).actProps;

        if (bldg.buildingGeometry == null)
          continue;

        hasBuildingGeometry = true;

        foreach (var boundary in bldg.buildingGeometry)
        {
          foreach (var pt in boundary.boundaryGeometry)
          {
            bboxpt1.Lat = Math.Min(bboxpt1.Lat, pt.Lat);
            bboxpt2.Lat = Math.Max(bboxpt2.Lat, pt.Lat);
            bboxpt1.Lon = Math.Min(bboxpt1.Lon, pt.Lon);
            bboxpt2.Lon = Math.Max(bboxpt2.Lon, pt.Lon);
            bboxpt1.Alt = Math.Min(bboxpt1.Alt, pt.Alt);
            bboxpt2.Alt = Math.Max(bboxpt2.Alt, pt.Alt);
          }
        }
      }

      if (hasBuildingGeometry)
      {
        bb = new BoundingBox(bboxpt1.Lat, bboxpt1.Lon, bboxpt2.Lat, bboxpt2.Lon);
        return new GeoPoint()
        {
          Lat = (bboxpt1.Lat + bboxpt2.Lat) / 2,
          Lon = (bboxpt1.Lon + bboxpt2.Lon) / 2,
          Alt = (bboxpt1.Alt + bboxpt2.Alt) / 2
        };
      }

      bb = null;
      return null;
    }
  }
}
