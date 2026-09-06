using System;
using System.Collections.Generic;
using System.Windows;

namespace KIT.BuW.AdvEnrichment
{
  public enum ConstructionDefinitionType
  {
    DENA,
    ENEV,
    TABULA,
    USER_DEFINED
  }

  public class Material
  {
    public enum MATERIAL_TYPE { SOLID_MATERIAL, GAS };

    public Material(string nameP) { }

    public virtual bool IsValid() { return true; }

    public string GmlID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsTransparent { get; set; }
  }

  public class SolidMaterial : Material
  {
    public SolidMaterial(string name) : base(name) { }

    public override bool IsValid()
    {
      bool state = false;

      if (Density.Value != 0.0 && Lambda.Value != 0.0 && SpecificHeat.Value != 0.0)
      {
        state = true;
      }

      return state;
    }

    public Measure<double> Lambda { get; set; }                       // Wärmeleitfähigkeit
    public Measure<double> Density { get; set; }                      // Physikalische Dichte
    public Measure<double> SpecificHeat { get; set; }                 // Spezifische Wärme
    public bool HasOpticalParams { get; set; }                        // Confirms it has optical parameters
    public bool ReadEmissivity { get; set; }                          // Confirms it has emissivity
    public bool ReadReflectance { get; set; }                         // Confirms it has reflectance
    public bool ReadAbsorptance { get; set; }                         // Confirms it has absorptance
    public bool ReadTransmittance { get; set; }                       // Confirms it has transmittance
    
    public OpticalProperties OpticalProperties { get; set; }          // Optische Eigenschaften
  }

  public class Gas : Material
  {
    public Gas(string name) : base(name) { }

    public override bool IsValid()
    {
      bool state = false;

      if (RValue.Value != 0.0)
      {
        state = true;
      }

      return state;
    }

    public Measure<double> RValue { get; set; }                       // R-Wert (Inverser U-Wert) der Gas-Schicht
    public bool IsVentilated { get; set; }                            // Gigt an, of eine Luftschicht hinterlüftet ist (derzeit nicht umgesetzt)
    public string GasType { get; set; }                               // Gas-Typ bei Mehrschicht-Fenstern
  }

  public class Construction
  {
    public Construction(string name) { Layers = new List<Layer>(); }

    public bool IsValid(bool checkLayer = false)
    {
      bool state = false;

      if (checkLayer)
      {
        bool layerState = true;

        foreach (var layer in Layers)
        {
          layerState &= layer.IsValid();
        }

        state = layerState;
      }
      else
      {
        if (UValue != null && UValue.Value > 0.0)
        {
          state = true;
        }
      }

      return state;
    }

    public string Name { get; set; }                                  // Name der Construction
    public string Description { get; set; }                           // Informelle Beschreibung
    //public string ElementType {  get; set; }                        // Typ von Bauelement: Roof, Wall, Window, Door, Floor
    public Measure<double> UValue { get; set; }                       // U-Wert der Construction
    public Measure<double> gValue { get; set; }                       // G-Wert (Transparanz zwischen 0 und 1) der gesamten Construction
    public Measure<double> GlazingRatio { get; set; }                 // Anteil der transparenten Fläche des zugehörigen Bauteils
    public Measure<double> OutsideConvectionCoefficient { get; set; } // Äußerer Wärmeübergangs-Koeffizient Luft --> Bauteil
    public Measure<double> InsideConvectionCoefficient { get; set; }  // Innrere Wärmeübergangs-Koeffizient Bauteil -->Luft
    public OpticalProperties OpticalProperties { get; set; }          // Optische Eigenschaften
    public bool hasOpticalParams { get; set; }                        // Confirms it has optical parameters
    public bool readTransmittance { get; set; }                       // Confirms it has emissivity
    public bool hasTransparentLayers { get; set; }

    public List<Layer> Layers { get; set; }                           // Geordnete Liste der Material-Schichten, von außen nach innen
  }

  public class Layer
  {
    public Layer() { LayerComponents = new List<LayerComponent>(); }

    public bool IsValid()
    {
      bool state = false;

      if (Thickness != null && Thickness.Value > 0.0)
      {
        state = true;
      }

      bool layerComponentState = true;

      foreach (var layerComponent in LayerComponents)
      {
        layerComponentState &= layerComponent.IsValid();
      }

      state &= layerComponentState;

      return state;
    }

    public List<LayerComponent> LayerComponents { get; set; }         // Layer components
    public Measure<double> Thickness { get; set; }                    // Dicke des Layers
  }

  public class LayerComponent
  {
    public bool IsValid()
    {
      if (Material != null)
      {
        return Material.IsValid();
      }

      return false;
    }

    public Measure<double> AreaFraction { get; set; }                 // Fraction ( zwischen 0 und 1) der Layer Component
    //public string xLink { get; set; }                               // Zugehöriges Material
    public Material Material { get; set; }
  }

  public class CityObjectGroup
  {
    public CityObjectGroup(string name) { GroupMembers = new List<GroupMember>(); }
    public CityObjectGroup () { GroupMembers = new List<GroupMember>(); }

    public bool IsValid(bool checkLayer = false)
    {
      bool state = true;

      bool groupMemberState = true;
      bool hasFassadeConstruction = false;
      bool hasRoofConstruction = false;
      bool hasGroundPlateConstruction = false;
      bool hasWindowConstruction = false;

      foreach (var groupMember in GroupMembers)
      {
        if (groupMember.Title == "Fassade")
        {
          hasFassadeConstruction = true;
        }
        else if (groupMember.Title == "Roof")
        {
          hasRoofConstruction = true;
        }
        else if (groupMember.Title == "GroundPlate")
        {
          hasGroundPlateConstruction = true;
        }
        else if (groupMember.Title == "Window")
        {
          hasWindowConstruction = true;
        }

        groupMemberState &= groupMember.IsValid(checkLayer);
      }

      state &= groupMemberState;

      if (!hasFassadeConstruction || !hasRoofConstruction || !hasGroundPlateConstruction || !hasWindowConstruction)
      {
        state &= false;
      }

      return state;
    }

    public List<GroupMember> GroupMembers { get; set; }

    public string Name { get; set; }
    public int PeriodStart { get; set; }
    public int PeriodEnd { get; set; }
    public ConstructionDefinitionType Definition { get; set; } = ConstructionDefinitionType.USER_DEFINED;
    public Measure<double> OpeningFractionWall { get; set; }          // Fraction ( zwischen 0 und 1) der CityObjectGroup
    public Measure<double> OpeningFractionRoof { get; set; }          // Fraction ( zwischen 0 und 1) der CityObjectGroup
  }

  public class GroupMember
  {
    public bool IsValid(bool checkLayer = false)
    {
      return Construction.IsValid(checkLayer);
    }

    public string Title { get; set; }
    public Construction Construction { get; set; }
  }

  public class Absorptance
  {
    public Absorptance() { }

    public Measure<double> Fraction {get; set; }                      // Anteil (zw. 0 und 1) der emittierten Strahlung im Vergleich zur ankommenden Strahlung
    public Measure<string> WavelengthRange { get; set; }              // Wellenlängenbereich der Strahlung
  }

  public class Emissivity
  {
    public Emissivity() { }

    public Measure<double> Fraction {get; set; }                      // Anteil (zw. 0 und 1) der emittierten Strahlung im Vergleich zur ankommenden Strahlung
    public Measure<string> Surface {get; set; }                       // Emission an Innen- oder Außenseite
  }

  public class Reflectance
  {
    public Reflectance() { }

    public Measure<double> Fraction {get; set; }                      // Anteil (zw. 0 und 1) der emittierten Strahlung im Vergleich zur ankommenden Strahlung
    public Measure<string> Surface {get; set; }                       // Emission an Innen- oder Außenseite
    public Measure<string> WavelengthRange { get; set; }              // Wellenlängenbereich der Strahlung
  }

  public class Transmittance
  {
    public Transmittance() { }

    public Measure<double> Fraction { get; set; }                     // Anteil (zw. 0 und 1) der durchgelassenen Strahlung im Vergleich zur ankommenden Strahlung
    public Measure<string> WavelengthRange { get; set; }              // Wellenlängenbereich der Strahlung
  }

  public class OpticalProperties
  {
    public OpticalProperties() { Emissivities = new List<Emissivity>(); Reflectances = new List<Reflectance>(); Absorptances = new List<Absorptance>(); Transmittances = new List<Transmittance>(); }

    public List<Absorptance> Absorptances { get; set; }               // Absorptanz in verschiedenen Wellenlängenbereichen
    public List<Emissivity> Emissivities { get; set; }                // Emissivität in verschiedenen Wellenlängenbereichen
    public List<Reflectance> Reflectances { get; set; }               // Reflektivität in verschiedenen Wellenlängenbereichen
    public List<Transmittance> Transmittances { get; set; }           // Transmissivität in verschiedenen Wellenlängenbereichen*/
  }

  public class YearOfConstruction
  {
    public YearOfConstruction() { firstAndLastYear = new List<(int, int)>(); }
    
    public string countryCode { get; set; }
    public List<(int, int)> firstAndLastYear { get; set; }
  }

}
