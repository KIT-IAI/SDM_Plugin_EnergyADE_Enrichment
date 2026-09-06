using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIT.BuW.AdvEnrichment
{
  public enum UsageProfileDefinitionType
  {
    DIN_18599,
    SIA_2024,
    USER_DEFINED
  }

  public class UsageProfile
  {
    public UsageProfile() { Periods = new List<Period>(); }
    public string Name { get; set; }                                 // Name der Construction
    public string Description { get; set; }                          // Informelle Beschreibung
    public bool Active { get; set; }                                 // if a profile is active or not
    public List<Period> Periods { get; set; }                        // The period in which a profile is active
  }

  public class UsageProfileDevices : UsageProfile
  {
    public UsageProfileDevices() {}
    public double ConvectionFraction { get; set; }                   // Konvektiver Anteil der der Geräte-Energie
    public double SpecificRatedThermalCapacity { get; set; }         // Maximale Geräteenergie
  }

  public class UsageProfileHeating : UsageProfile
  {
    public UsageProfileHeating() {}
  }

  public class UsageProfileVentilation : UsageProfile
  {
    public UsageProfileVentilation() {}
  }

  public class UsageProfileCooling : UsageProfile
  {
    public UsageProfileCooling() {}
  }
  public class UsageProfileLighting : UsageProfile
  {
    public UsageProfileLighting() {}
    public double ConvectionFraction { get; set; }                   // Konvektiver Anteil der der Geräte-Energie
    public double SpecificRatedThermalCapacity { get; set; }         // Maximale Geräteenergie
    public double Efficiency { get; set; }                           // Lighting efficiency
    public double SpecificRatedLevelLux { get; set; }                // Maximale Helligkeit
  }

  public class UsageProfileOccupancy : UsageProfile
  {
    public UsageProfileOccupancy() {}
    public double ConvectionFraction { get; set; }                   // Konvektiver Anteil der Person-Energie
    public double HeatEmissionPerPerson { get; set; }                // Maximale Personenenergie
    public double SquareMeterPerPerson { get; set; }                 // Surface per person
    public int NoOfOccupants { get; set; }                           // Maximum number of persons
  }

  public class UsageProfileWindowShading : UsageProfile
  {
    public UsageProfileWindowShading() { }
    public Measure<double> MaximumCoverRatio { get; set; }           // The ratio to which a window surface is covered
    public string ShadingMaterial { get; set; }                      // The material used for the window cover
    public Measure<double> Thickness { get; set; }                   // Thickness of the material used for the window cover
    public Measure<double> GlassDistance { get; set; }               // Distance in between the window and the window cover
    public Measure<double> OutdoorTempSetpoint { get; set; }         // Outside temperature at which the covers can be pulled down automatically
    public Measure<double> SolarIntensitySetpoint { get; set; }      // Solar Intensity at which the covers can be pulled down automatically
  }

  public class UsageProfileServiceHours : UsageProfile
  {
    public UsageProfileServiceHours() { }
    public SystemType System { get; set; }                           // The system type for which the service hours are used
    public enum SystemType { cooling, heating };                     // Type of modelled ServiceHours systems
  }

  public class Period
  {
    public Period() { TimeSeries = new List<TimeSeries>(); }
    public int StartDay { get; set; }
    public int StartMonth { get; set; }
    public int EndDay { get; set; }
    public int EndMonth { get; set; }
    public List<TimeSeries> TimeSeries { get; set; }
  }

  public class TimeSeries
  {
    public TimeSeries() { Values = new MeasureList<double>(); }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double DefaultValue { get; set; }
    public enum EDayType { monday, tuesday, wednesday, thursday, friday, saturday, sunday, designDay, weekDay, weekEnd, typicalDay }; // Typ des modellierten Tages
    public EDayType DayType { get; set; }
    public MeasureList<double> Values { get; set; }
  }

  public class UsageProfileGroup
  {
    public UsageProfileGroup(string name) { UsageProfileGroupMembers = new List<UsageProfileGroupMember>(); }
    public string Name { get; set; }
    public List<UsageProfileGroupMember> UsageProfileGroupMembers { get; set; }
  }

  public class UsageProfileGroupMember
  {
    public string Name { get; set; }
    public UsageProfile UsageProfile { get; set; }
  }
}
