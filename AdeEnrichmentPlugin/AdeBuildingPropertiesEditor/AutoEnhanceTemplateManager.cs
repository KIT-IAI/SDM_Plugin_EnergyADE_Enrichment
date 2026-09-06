using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIT.BuW.AdvEnrichment
{
  public class AutoEnhanceTemplateManager
  {
    public static Dictionary<string, AdeBuildingProperties> AutoEnhanceTemplates = new Dictionary<string, AdeBuildingProperties>();

    public static void LoadTemplates()
    {
      var props = new AdeBuildingProperties()
      {
        GmlId = "id1",
        Name = "Alpha building",
        PosX = 3333,
        PosY = 5555,
        PosZ = 666,
        SrsName = "EPSG:4711",
        YearOfConstruction = 1977,
        AlkisFunction = "200_2333",
        UsageType = "Research",
        BuildingSizeType = "small",
        ConstructionWeight = "heavy",
        StoreysOverground = 2,
        StoreysUnderground = 1,
        BuildingHeightFromGeom = 8,
        BuildingHeightFromModel = 10,
        GrossVolume = 1124,
        GrossGroundArea = 190,
        GrossFloorArea = 452,
        NetFloorArea = 401,
        AirInfiltrationRate = new Measure<double>() { Value = 0.4, Uom = "1/h" },
        RoofType = "flat roof",
        OpeningPercentageWalls = 30,
        OpeningPercentageRoofs = 0,
        FloorAreaRatio = new Measure<double>() { Value = 0.8, Uom = "%" },
        StoreyHeight = new Measure<double>() { Value = 3.0, Uom = "m" },
        UValueWalls = new Measure<double>() { Value = 0.558, Uom = "W/(m²·K)" },
        UValueRoofs = new Measure<double>() { Value = 0.484, Uom = "W/(m²·K)" },
        UValueGroundPlates = new Measure<double>() { Value = 0.393, Uom = "W/(m²·K)" },
        isHeated = true,
        isCooled = true,
        isMechanicallyVentilated = true,
      };

      AutoEnhanceTemplates[props.Name] = props;

      props = new AdeBuildingProperties()
      {
        GmlId = "id2",
        Name = "Beta building",
        PosX = 12345.6789,
        PosY = 87543.45879,
        PosZ = 155.345,
        SrsName = "EPSG:42",
        YearOfConstruction = 2001,
        AlkisFunction = "100_2000",
        UsageType = "Education",
        BuildingSizeType = "medium",
        ConstructionWeight = "light",
        StoreysOverground = 3,
        StoreysUnderground = 1,
        BuildingHeightFromGeom = 18,
        BuildingHeightFromModel = 20,
        GrossVolume = 17240,
        GrossGroundArea = 1900,
        GrossFloorArea = 4520,
        NetFloorArea = 4010,
        AirInfiltrationRate = new Measure<double>() { Value = 0.4, Uom = "1/h" },
        RoofType = "gabled roof",
        OpeningPercentageWalls = 35,
        OpeningPercentageRoofs = 15,
        FloorAreaRatio = new Measure<double>() { Value = 0.85, Uom = "%" },
        StoreyHeight = new Measure<double>() { Value = 3.5, Uom = "m" },
        UValueWalls = new Measure<double>() { Value = 0.277, Uom = "W/(m²·K)" },
        UValueRoofs = new Measure<double>() { Value = 0.356, Uom = "W/(m²·K)" },
        UValueGroundPlates = new Measure<double>() { Value = 0.193, Uom = "W/(m²·K)" },
        isHeated = true,
        isCooled = true,
        isMechanicallyVentilated = true,
      };

      AutoEnhanceTemplates[props.Name] = props;

      props = new AdeBuildingProperties()
      {
        GmlId = "id3",
        Name = "Gamma building",
        PosX = 1111,
        PosY = 8888,
        PosZ = 999,
        SrsName = "EPSG:4711",
        YearOfConstruction = 2022,
        AlkisFunction = "120_4444",
        UsageType = "Education",
        BuildingSizeType = "medium",
        ConstructionWeight = "medium",
        StoreysOverground = 4,
        StoreysUnderground = 0,
        BuildingHeightFromGeom = 15,
        BuildingHeightFromModel = 10,
        GrossVolume = 58924,
        GrossGroundArea = 490,
        GrossFloorArea = 852,
        NetFloorArea = 8401,
        AirInfiltrationRate = new Measure<double>() { Value = 0.1, Uom = "1/h" },
        RoofType = "flat roof",
        OpeningPercentageWalls = 30,
        OpeningPercentageRoofs = 0,
        FloorAreaRatio = new Measure<double>() { Value = 0.9, Uom = "%" },
        StoreyHeight = new Measure<double>() { Value = 4.0, Uom = "m" },
        UValueWalls = new Measure<double>() { Value = 0.778, Uom = "W/(m²·K)" },
        UValueRoofs = new Measure<double>() { Value = 0.984, Uom = "W/(m²·K)" },
        UValueGroundPlates = new Measure<double>() { Value = 0.793, Uom = "W/(m²·K)" },
        isHeated = false,
        isCooled = false,
        isMechanicallyVentilated = false,
      };

      AutoEnhanceTemplates[props.Name] = props;
     
    }
  }
}
