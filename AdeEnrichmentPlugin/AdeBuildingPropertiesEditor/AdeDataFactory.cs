using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIT.BuW.AdvEnrichment
{
    // The main interface that has to be provided by the application
    // The editor uses this interface to gain access to the information
    // from the applications data model.
    public interface IAdeDataFactory
    {
        // Overall properties of the current data source (metadata)
        AdeDataSourceProperties DataSourceProperties { get; }

        // Gerenal settings
        AdeApplicationSettings ApplicationSettings { get; }

        // List of currently selected objects. This list cannot be modified
        // by the editor, but it might be changed by the application.
        IReadOnlyList<string> SelectedBuildings { get; }

        // Getter to retrieve (building) properties for a given object.
        // The object is identified by its uuid (or gmlid).
        AdeBuildingProperties GetBuildingProperties(string id);

        // A logger that receives log messages from the editor
        IAdeMessageLogger Logger { get; }

        // This interface provides methods to update the application model
        IAdeDataUpdater UpdateCallbacks { get; }
    }

    // General settings
    [Serializable]
    public class AdeApplicationSettings
    {
        public string DataDirectory { get; set; }
    }

    // A simple 3D point with a specified SRS
    [Serializable]
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int EPSGCode { get; set; }
    }

    // A 3D point in geographic coordinates
    [Serializable]
    public class GeoPoint
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
    }

    [Serializable]
    public class GeoBoundary
    {
        public List<GeoPoint> boundaryGeometry { get; set; }
    }

    // Meta data of the data source
    [Serializable]
    public class AdeDataSourceProperties
    {
        public string SourceName { get; set; }
        public string Description { get; set; }
        public int NumItems { get; set; }
        public int LOD { get; set; }
        public GeoPoint MapLocation { get; set; }
        public string CountryCode { get; set; }
        public bool HasWindows { get; set; }
    }

  // A measure class for the properties
  [Serializable]
    public class Measure<T>
    {
        public T Value { get; set; }
        public string Uom { get; set; }
        public bool IsFixed { get; set; }
    }

    [Serializable]
    public class MeasureList<T>
    {
        public MeasureList() { Values = new List<T>(); }
        public List<T> Values { get; set; }
        public string Uom { get; set; }
        public bool IsFixed { get; set; }
    }

    [Serializable]
    public class EthosBuildingProperties
    {
        public string ExternalID { get; set; }
        public string GeneratorSource { get; set; }
        public string GeneratorLineage { get; set; }
        public int? YearOfConstruction { get; set; }
        public Point ReferencePoint { get; set; }
    }

    // The building properties provided by the application
    [Serializable]
    public class AdeBuildingProperties : IDataErrorInfo
    {
        public AdeBuildingProperties()
        {
        }

        public AdeBuildingProperties(AdeBuildingProperties buildingProperties)
        {
            this.Guid = string.Copy(buildingProperties.Guid ?? string.Empty);
            this.GmlId = string.Copy(buildingProperties.GmlId ?? string.Empty);
            this.DisplayString = string.Copy(buildingProperties.DisplayString ?? string.Empty);
            this.Name = string.Copy(buildingProperties.Name ?? string.Empty);
            this.PosX = buildingProperties.PosX;
            this.PosY = buildingProperties.PosY;
            this.PosZ = buildingProperties.PosZ;
            this.SrsName = string.Copy(buildingProperties.SrsName ?? string.Empty);
            this.CountryCode = string.Copy(buildingProperties.CountryCode ?? string.Empty);
            this.AlkisFunction = string.Copy(buildingProperties.AlkisFunction ?? string.Empty);
            this.UsageType = string.Copy(buildingProperties.UsageType ?? string.Empty);
            this.RoofType = buildingProperties.RoofType;
            this.StoreysOverground = buildingProperties.StoreysOverground;
            this.StoreysUnderground = buildingProperties.StoreysUnderground;
            this.BuildingHeightFromGeom = buildingProperties.BuildingHeightFromGeom;
            this.BuildingHeightFromModel = buildingProperties.BuildingHeightFromModel;
            this.GrossVolume = buildingProperties.GrossVolume;
            this.GrossGroundArea = buildingProperties.GrossGroundArea;
            this.GrossFloorArea = buildingProperties.GrossFloorArea;
            this.NetFloorArea = buildingProperties.NetFloorArea;
            this.AirInfiltrationRate = buildingProperties.AirInfiltrationRate;

            //parameters used in enrichment with DefaultParameters
            this.BuildingFunction = string.Copy(buildingProperties.BuildingFunction ?? string.Empty);
            this.YearOfConstruction = buildingProperties.YearOfConstruction;
            this.ConstructionWeight = string.Copy(buildingProperties.ConstructionWeight ?? string.Empty);
            this.BuildingSizeType = string.Copy(buildingProperties.BuildingSizeType ?? string.Empty);

            //special parameters for generated building properties
            this.EthosBuildingProps = buildingProperties.EthosBuildingProps;

            //new parameters introduced for DefaultParams
            this.OpeningPercentageWalls = buildingProperties.OpeningPercentageWalls;
            this.OpeningPercentageRoofs = buildingProperties.OpeningPercentageRoofs;
            this.FloorAreaRatio = buildingProperties.FloorAreaRatio;
            this.StoreyHeight = buildingProperties.StoreyHeight;

            //physical parameters
            this.UValueWalls = buildingProperties.UValueWalls;
            this.UValueRoofs = buildingProperties.UValueRoofs;
            this.UValueGroundPlates = buildingProperties.UValueGroundPlates;
            this.useConstruction = buildingProperties.useConstruction;
            this.WallConstruction = buildingProperties.WallConstruction;
            this.RoofConstruction = buildingProperties.RoofConstruction;
            this.GroundPlateConstruction = buildingProperties.GroundPlateConstruction;
            this.WindowConstruction = buildingProperties.WindowConstruction;
            this.DoorConstruction = buildingProperties.DoorConstruction;

            // usage profiles
            this.UsageProfileHeating = buildingProperties.UsageProfileHeating;
            this.UsageProfileCooling = buildingProperties.UsageProfileCooling;
            this.UsageProfileHeatGainOccupants = buildingProperties.UsageProfileHeatGainOccupants;
            this.UsageProfileHeatGainElectricalDevices = buildingProperties.UsageProfileHeatGainElectricalDevices;
            this.UsageProfileLighting = buildingProperties.UsageProfileLighting;
            this.UsageProfileVentilation = buildingProperties.UsageProfileVentilation;
            this.UsageProfileShading = buildingProperties.UsageProfileShading;

            // The geometry will not be deep-copied, since it will never be changed by the enrichtment
            this.buildingGeometry = buildingProperties.buildingGeometry;

            //parameters used for building statistic
            this.hasBuildingSolid = buildingProperties.hasBuildingSolid;
            this.hasCorrectSolid = buildingProperties.hasCorrectSolid;
            this.hasLoD0Geometry = buildingProperties.hasLoD0Geometry;
            this.hasLoD1Geometry = buildingProperties.hasLoD1Geometry;
            this.hasLoD2Geometry = buildingProperties.hasLoD2Geometry;
            this.hasLoD3Geometry = buildingProperties.hasLoD3Geometry;
            this.hasLoD4Geometry = buildingProperties.hasLoD4Geometry;

            this.isBuildingPart = buildingProperties.isBuildingPart;

            //parameters used for building part statistic
            if (buildingProperties.buildingParts != null)
            {
                this.buildingParts = buildingProperties.buildingParts.ToList();
            }

            //parameters used for building simulation
            this.enrichBuildingData = buildingProperties.enrichBuildingData;
            this.singleThermalZone = buildingProperties.singleThermalZone;
            this.isHeated = buildingProperties.isHeated;
            this.isCooled = buildingProperties.isCooled;
            this.isMechanicallyVentilated = buildingProperties.isMechanicallyVentilated;
        }


        public string Error
        {
          get { return null; }
        }

        public string this[string columnName]
        {
          get
          {
            switch (columnName)
            {
              case "OpeningPercentageWalls":
                if (this.OpeningPercentageWalls < 0 || this.OpeningPercentageWalls > 100)
                  return "OpeningPercentageWalls must be between 0 and 100";
                break;
            }

            return string.Empty;
          }
        }


    public string Guid { get; set; }
        public string GmlId { get; set; }
        public string DisplayString { get; set; }
        public string Name { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public string SrsName { get; set; }
        public string CountryCode { get; set; }
        public string BuildingFunction { get; set; }
        public int? YearOfConstruction { get; set; }
        public string AlkisFunction { get; set; }
        public string UsageType { get; set; }
        public string RoofType { get; set; }
        public string BuildingSizeType { get; set; }
        public string ConstructionWeight { get; set; }
        public int? StoreysOverground { get; set; }
        public int? StoreysUnderground { get; set; }
        public double? BuildingHeightFromGeom { get; set; }
        public double? BuildingHeightFromModel { get; set; }
        public double? GrossVolume { get; set; }
        public double? GrossGroundArea { get; set; }
        public double? GrossFloorArea { get; set; }
        public double? NetFloorArea { get; set; }
        public Measure<double> AirInfiltrationRate { get; set; }

        //new parameters introduced for DefaultParams
        public double? OpeningPercentageWalls { get; set; }
        public double? OpeningPercentageRoofs { get; set; }
        public Measure<double> FloorAreaRatio { get; set; }
        public Measure<double> StoreyHeight { get; set; }
        public bool HasWindows { get; set; } = false;

        //physical parameters
        public Measure<double> UValueWalls { get; set; }
        public Measure<double> UValueRoofs { get; set; }
        public Measure<double> UValueGroundPlates { get; set; }
        public Measure<double> UValueWindows { get; set; }
        public Measure<double> UValueDoors { get; set; }
        public bool useConstruction { get; set; } = false;
        public Construction WallConstruction { get; set; }
        public Construction RoofConstruction { get; set; }
        public Construction GroundPlateConstruction { get; set; }
        public Construction WindowConstruction { get; set; }
        public Construction DoorConstruction { get; set; }

        // usage profiles
        public UsageProfileHeating UsageProfileHeating { get; set; }
        public UsageProfileCooling UsageProfileCooling { get; set; }
        public UsageProfileOccupancy UsageProfileHeatGainOccupants { get; set; }
        public UsageProfileDevices UsageProfileHeatGainElectricalDevices { get; set; }
        public UsageProfileLighting UsageProfileLighting { get; set; }
        public UsageProfileVentilation UsageProfileVentilation { get; set; }
        public UsageProfile UsageProfileShading { get; set; }
        public UsageProfile UsageProfileServiceHours { get; set; }

        //parameters used for building statistic
        public bool hasBuildingSolid { get; set; } = false;
        public bool hasCorrectSolid { get; set; } = false;
        public bool hasSolidFromSurface { get; set; } = false;
        public bool hasLoD0Geometry { get; set; } = false;
        public bool hasLoD1Geometry { get; set; } = false;
        public bool hasLoD2Geometry { get; set; } = false;
        public bool hasLoD3Geometry { get; set; } = false;
        public bool hasLoD4Geometry { get; set; } = false;
        public bool isBuildingPart { get; set; } = false;
        public List<string> buildingParts { get; set; }

        //parameters used for building simulation
        public bool enrichBuildingData { get; set; } = false;
        public bool singleThermalZone { get; set; } = true;
        public bool buildingPartThermalZone { get; set; } = false;
        public bool createStoreys { get; set; } = false;
        public bool isHeated { get; set; } = true;
        public bool isCooled { get; set; } = true;
        public bool isMechanicallyVentilated { get; set; } = true;

        // Building geometry
        public List<GeoBoundary> buildingGeometry { get; set; }

        // Generated building parameters for ETHOS_BUILDA
        public EthosBuildingProperties EthosBuildingProps { get; set; }
    }

    // Interface for logging operations
    // The Editor uses this interface to provide about progress and events
    public interface IAdeMessageLogger
    {
        void Write(string sMsg);
        void WriteWarning(string msg);
        void WriteError(string msg);
    }

    // Interface for update operations
    // This interface defined the methods that may be called by the editor
    // in order to update properties of the data model.
    // The methods are implemented by the application (IFCExplorer/IfcDB)
    // and are called from the editor.
    public interface IAdeDataUpdater
    {
        ApplicationException UpdateBuildingProperties(IList<AdeBuildingProperties> changedAdeBuildingProperties);
    }
}
