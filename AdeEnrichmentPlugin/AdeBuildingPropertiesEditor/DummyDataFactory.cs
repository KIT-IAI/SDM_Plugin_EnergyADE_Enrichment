using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIT.BuW.AdvEnrichment;

namespace KIT.BuW.AdvEnrichment.Dummy
{
    class DummyLogger : IAdeMessageLogger
    {
        public void Write(string sMsg) {}
        public void WriteWarning(string sMsg) {}
        public void WriteError(string sMsg) {}
    }

    class DummyDataFactory : IAdeDataFactory
    {
    internal AdeApplicationSettings dummyApplicationSettings = new AdeApplicationSettings();
    internal AdeDataSourceProperties dummySourceProps = new AdeDataSourceProperties();
    internal IAdeMessageLogger dummyLogger = new DummyLogger();
    internal IAdeDataUpdater dummyUpdater = null;
    internal Dictionary<string, AdeBuildingProperties> dummyBuildingProps = new Dictionary<string, AdeBuildingProperties>();
    internal List<string> dummySelection = new List<string>() {"guid-id1", "guid-id3", "guid-id4", "guid-id4a", "guid-id2", "guid-id5", "guid-id6" };

    public DummyDataFactory()
    {
      dummyApplicationSettings.DataDirectory = ".";

      dummySourceProps.SourceName = "irgend/ein/datei/name.gml";
      dummySourceProps.Description = "a Description";
      dummySourceProps.CountryCode = "DE";
      dummySourceProps.NumItems = 42;
      dummySourceProps.LOD = 2;
      dummySourceProps.MapLocation = new GeoPoint() { Lon = 8.43309, Lat = 49.09592 };

      var props = new AdeBuildingProperties()
      {
        Guid = "guid-id1",
        GmlId = "id1",
        DisplayString = "A named building",
        Name = "A simple building",
        PosX = 3333,
        PosY = 5555,
        PosZ = 666,
        SrsName = "EPSG:4711",
        //YearOfConstruction = 1973,
        AlkisFunction = "200_2333",
        UsageType = "Research",
        BuildingSizeType = "small",
        ConstructionWeight = "Heavy",
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
        isHeated = false,
        isCooled = false,
        isMechanicallyVentilated = false,
        hasLoD2Geometry = true,
        BuildingFunction = "DIN_18599_31001_1000"
      };
      dummyBuildingProps[props.Guid] = props;

      props = new AdeBuildingProperties()
      {
        Guid = "guid-id2",
        GmlId = "id2",
        DisplayString = "Building B",
        Name = "Another building",
        PosX = 12345.6789,
        PosY = 87543.45879,
        PosZ = 155.345,
        SrsName = "EPSG:42",
        CountryCode = "DE",
        //YearOfConstruction = 2001,
        AlkisFunction = "100_2000",
        UsageType = "Education",
        BuildingSizeType = "Medium",
        ConstructionWeight = "Heavy",
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
        hasLoD2Geometry = true,
        BuildingFunction = "DIN_18599_31001_3010",
        buildingGeometry = new List<GeoBoundary> {
          new GeoBoundary() {
            boundaryGeometry = new List<GeoPoint> {
              new GeoPoint() { Lat = 49.09608, Lon = 8.43390, Alt = 0 },
              new GeoPoint() { Lat = 49.09612, Lon = 8.43374, Alt = 0 },
              new GeoPoint() { Lat = 49.09558, Lon = 8.43352, Alt = 0 },
              new GeoPoint() { Lat = 49.09556, Lon = 8.43367, Alt = 0 }
            }
          }
        }
      };
      dummyBuildingProps[props.Guid] = props;

      props = new AdeBuildingProperties()
      {
        Guid = "guid-id3",
        GmlId = "id3",
        Name = "C: No data building",
        PosX = 12345.6789,
        PosY = 87543.45879,
        PosZ = 155.345,
        SrsName = "EPSG:42",
        CountryCode = "DE",
        AlkisFunction = "100_2000",
        UsageType = "Education",
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
        buildingGeometry = new List<GeoBoundary> {
          new GeoBoundary() {
            boundaryGeometry = new List<GeoPoint> {
              new GeoPoint() { Lat = 49.09576, Lon = 8.43261, Alt = 0 },
              new GeoPoint() { Lat = 49.09599, Lon = 8.43269, Alt = 0 },
              new GeoPoint() { Lat = 49.09585, Lon = 8.43346, Alt = 0 },
              new GeoPoint() { Lat = 49.09563, Lon = 8.43335, Alt = 0 }
            }
          }
        }
      };
      dummyBuildingProps[props.Guid] = props;

      props = new AdeBuildingProperties()
      {
        Guid = "guid-id4",
        GmlId = "id4",
        DisplayString = "d: B4",
        Name = "d: No ADE props",
        BuildingFunction = "DIN_18599_31001_3013"
      };
      dummyBuildingProps[props.Guid] = props;

      props = new AdeBuildingProperties()
      {
        Guid = "guid-id4a",
        GmlId = "id4a",
        DisplayString = "d: B4a",
        Name = "d: No ADE props",
        UsageType = "Research",
        BuildingFunction = "DIN_18599_31001_3010"
      };
      dummyBuildingProps[props.Guid] = props;

      props = new AdeBuildingProperties()
      {
        Guid = "guid-id5",
        GmlId = "id5",
        Name = "E: No data building",
        PosX = 12345.6789,
        PosY = 87543.45879,
        PosZ = 155.345,
        SrsName = "EPSG:42",
        CountryCode = "DE",
        AlkisFunction = "100_3000",
        UsageType = "Residential",
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
        buildingGeometry = new List<GeoBoundary> {
          new GeoBoundary() {
            boundaryGeometry = new List<GeoPoint> {
              new GeoPoint() { Lat = 49.098311, Lon = 8.404792, Alt = 0 },
              new GeoPoint() { Lat = 49.098290, Lon = 8.404899, Alt = 0 },
              new GeoPoint() { Lat = 49.098392, Lon = 8.404942, Alt = 0 },
              new GeoPoint() { Lat = 49.098409, Lon = 8.404835, Alt = 0 }
            }
          }
        }
      };
      dummyBuildingProps[props.Guid] = props;

      props = new AdeBuildingProperties()
      {
        Guid = "guid-id6",
        GmlId = "id6",
        DisplayString = "A named building",
        Name = "An old building",
        PosX = 3355,
        PosY = 5533,
        PosZ = 666,
        SrsName = "EPSG:4711",
        //YearOfConstruction = 1850,
        AlkisFunction = "200_2444",
        UsageType = "Residential",
        BuildingSizeType = "small",
        ConstructionWeight = "Heavy",
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
        isHeated = false,
        isCooled = false,
        isMechanicallyVentilated = false,
        hasLoD2Geometry = true
      };
      dummyBuildingProps[props.Guid] = props;

    }

    public AdeApplicationSettings ApplicationSettings => dummyApplicationSettings;

    public AdeDataSourceProperties DataSourceProperties => dummySourceProps;

    public IAdeMessageLogger Logger => dummyLogger;

    public IReadOnlyList<string> SelectedBuildings => dummySelection;

    public IAdeDataUpdater UpdateCallbacks => dummyUpdater;

    public AdeBuildingProperties GetBuildingProperties(string id)
    {
      return dummyBuildingProps[id];
    }
  }
}
