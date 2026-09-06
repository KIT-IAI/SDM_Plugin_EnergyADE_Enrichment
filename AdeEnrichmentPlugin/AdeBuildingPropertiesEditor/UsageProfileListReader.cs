using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace KIT.BuW.AdvEnrichment
{
  class UsageProfileReader
  {
    IAdeMessageLogger m_Logger;
    System.Xml.XmlTextReader m_pReader = null;
    public SortedDictionary<String, UsageProfile> m_UsageProfiles;
    public SortedDictionary<String, UsageProfileGroup> m_UsageProfileGroups;
    public UsageProfileDefinitionType m_Type = UsageProfileDefinitionType.USER_DEFINED;

    public UsageProfileReader(IAdeMessageLogger logger, string filename, UsageProfileDefinitionType type = UsageProfileDefinitionType.USER_DEFINED)
    {
      m_Logger             = logger;
      m_pReader            = new System.Xml.XmlTextReader(filename);
      m_UsageProfiles      = new SortedDictionary<string, UsageProfile>();
      m_UsageProfileGroups = new SortedDictionary<string, UsageProfileGroup>();
      m_Type               = type;
    }

    public void Read()
    {
      try
      {
        m_pReader.MoveToContent();

        while (m_pReader.Read())
        {
          switch (m_pReader.NodeType)
          {
            case XmlNodeType.Element:
              {
                switch (m_pReader.LocalName)
                {
                  case "NutzungsprofilGeraete":
                  {
                    ReadUseProfileDevices();
                    break;
                  };
                  case "NutzungsprofilHeizung":
                  {
                    ReadUseProfileHeating();
                    break;
                  };
                  case "NutzungsprofilLueftung":
                  {
                    ReadUseProfileVentilation();
                    break;
                  };
                  case "NutzungsprofilKuehlung":
                  {
                    ReadUseProfileCooling();
                    break;
                  };
                  case "NutzungsprofilBeleuchtung":
                  {
                    ReadUseProfileLighting();
                    break;
                  };
                  case "NutzungsprofilPersonen":
                  {
                    ReadUseProfileOccupancy();
                    break;
                  };
                  case "NutzungsprofilBetrieb":
                  {
                    ReadUseProfileServiceHours();
                    break;
                  };
                  case "NutzungsprofilVerschattung":
                  {
                    ReadUseProfileWindowShading();
                    break;
                  };
                  case "NutzungsprofilGruppe":
                  {
                    ReadUseProfileGroups();
                    break;
                  };
                  default:
                    break;
                }
              }
              break;
            //case XmlNodeType.Text:
            //  break;
            case XmlNodeType.EndElement:
              break;
            default:
              break;
          }
        }
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"XML parser error in line: { m_pReader.LineNumber } at position: { m_pReader.LinePosition }", ex);
      }
    }

    public void ReadUseProfileGroups()
    {
      string temporaryStringHolder, actElement;

      temporaryStringHolder = m_pReader.GetAttribute("name");

      if (m_Type == UsageProfileDefinitionType.DIN_18599)
      {
        if (temporaryStringHolder.Contains("DIN_18599_") != true)
          return;
      }
      else if (m_Type == UsageProfileDefinitionType.SIA_2024)
      {
        if (temporaryStringHolder.Contains("SIA_2024_") != true)
          return;
      }

      UsageProfileGroup pUsageProfileGroup = new UsageProfileGroup(temporaryStringHolder);

      pUsageProfileGroup.Name = temporaryStringHolder;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          temporaryStringHolder = m_pReader.GetAttribute("ref");

          var pUsageProfileGroupMember = new UsageProfileGroupMember();

          pUsageProfileGroupMember.Name = temporaryStringHolder;

          if (m_UsageProfiles.ContainsKey(temporaryStringHolder))
          {
            pUsageProfileGroupMember.UsageProfile = m_UsageProfiles[temporaryStringHolder];
          }
          else
          {
            m_Logger.Write($"Missing reference in usage profile group definition: {temporaryStringHolder}");
          }
          pUsageProfileGroup.UsageProfileGroupMembers.Add(pUsageProfileGroupMember);
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilGruppe") == 0)
            break;
        }
      }

      m_UsageProfileGroups[pUsageProfileGroup.Name] = pUsageProfileGroup;     
    }

    public void ReadUseProfileDevices()
    {
      string actElement;

      UsageProfileDevices m_pUseProfile = new UsageProfileDevices();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
          else
          if (actElement.CompareTo("konvektAnteil") == 0)
          {
            m_pUseProfile.ConvectionFraction = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("maxWaermeleistung") == 0)
          {
            m_pUseProfile.SpecificRatedThermalCapacity = m_pReader.ReadElementContentAsDouble();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilGeraete") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileHeating()
    {
      string actElement;

      UsageProfileHeating m_pUseProfile = new UsageProfileHeating();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilHeizung") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileVentilation()
    {
      string actElement;

      UsageProfileVentilation m_pUseProfile = new UsageProfileVentilation();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilLueftung") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileCooling()
    {
      string actElement;

      UsageProfileCooling m_pUseProfile = new UsageProfileCooling();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilKuehlung") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileLighting()
    {
      string actElement;

      UsageProfileLighting m_pUseProfile = new UsageProfileLighting();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("automatisch") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
          else
          if (actElement.CompareTo("konvektAnteil") == 0)
          {
            m_pUseProfile.ConvectionFraction = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("maxWaermeleistung") == 0)
          {
            m_pUseProfile.SpecificRatedThermalCapacity = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("maxHelligkeit") == 0)
          {
            m_pUseProfile.SpecificRatedLevelLux = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("effizienz") == 0)
          {
            m_pUseProfile.Efficiency = m_pReader.ReadElementContentAsDouble();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilBeleuchtung") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileOccupancy()
    {
      string actElement;

      UsageProfileOccupancy m_pUseProfile = new UsageProfileOccupancy();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
          else
          if (actElement.CompareTo("konvektAnteil") == 0)
          {
            m_pUseProfile.ConvectionFraction = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("waermeleistungPerson") == 0)
          {
            m_pUseProfile.HeatEmissionPerPerson = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("flaecheProPerson") == 0)
          {
            m_pUseProfile.SquareMeterPerPerson = m_pReader.ReadElementContentAsDouble();

            if (Double.IsInfinity(m_pUseProfile.SquareMeterPerPerson))
              m_pUseProfile.SquareMeterPerPerson = 0.0;
          }
          else
          if (actElement.CompareTo("maxAnzahlPersonen") == 0)
          {
            m_pUseProfile.NoOfOccupants = m_pReader.ReadElementContentAsInt();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilPersonen") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileServiceHours()
    {
      string temporaryStringHolder, actElement;

      UsageProfileServiceHours m_pUseProfile = new UsageProfileServiceHours();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
          else
          if (actElement.CompareTo("system") == 0)
          {
            temporaryStringHolder = m_pReader.ReadElementContentAsString().Trim();

            if (Enum.IsDefined(typeof(UsageProfileServiceHours.SystemType), temporaryStringHolder))
            {
              if (temporaryStringHolder.CompareTo("heating") == 0)
              {
                m_pUseProfile.System = UsageProfileServiceHours.SystemType.heating;
              }
              else
              if (temporaryStringHolder.CompareTo("cooling") == 0)
              {
                m_pUseProfile.System = UsageProfileServiceHours.SystemType.cooling;
              }
            }
            else
            {
              m_Logger.Write($"Unsupported timeseries profile service hours type: {temporaryStringHolder}");
            }
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilBetrieb") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }

    public void ReadUseProfileWindowShading()
    {
      string temporaryStringHolder, actElement;

      UsageProfileWindowShading m_pUseProfile = new UsageProfileWindowShading();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pUseProfile.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("aktiv") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("automatisch") == 0)
          {
            m_pUseProfile.Active = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("periode") == 0)
          {
            var vPeriod = new Period();

            readPeriod(vPeriod);

            m_pUseProfile.Periods.Add(vPeriod);
          }
          else
          if (actElement.CompareTo("maximumCoverRatio") == 0)
          {
            m_pUseProfile.MaximumCoverRatio = new Measure<double>();
            m_pUseProfile.MaximumCoverRatio.Uom = m_pReader.GetAttribute("uom");
            m_pUseProfile.MaximumCoverRatio.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("shadingMaterial") == 0)
          {
            temporaryStringHolder = m_pReader.GetAttribute("xlink:href");

            if (temporaryStringHolder.StartsWith("#"))
            {
              temporaryStringHolder = temporaryStringHolder.Substring(1);
            }

            m_pUseProfile.ShadingMaterial = temporaryStringHolder;
          }
          else
          if (actElement.CompareTo("thickness") == 0)
          {
            m_pUseProfile.Thickness = new Measure<double>();
            m_pUseProfile.Thickness.Uom = m_pReader.GetAttribute("uom");
            m_pUseProfile.Thickness.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("glassDistance") == 0)
          {
            m_pUseProfile.GlassDistance = new Measure<double>();
            m_pUseProfile.GlassDistance.Uom = m_pReader.GetAttribute("uom");
            m_pUseProfile.GlassDistance.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("outdoorTempSetpoint") == 0)
          {
            m_pUseProfile.OutdoorTempSetpoint = new Measure<double>();
            m_pUseProfile.OutdoorTempSetpoint.Uom = m_pReader.GetAttribute("uom");
            m_pUseProfile.OutdoorTempSetpoint.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("solarIntensitySetpoint") == 0)
          {
            m_pUseProfile.SolarIntensitySetpoint = new Measure<double>();
            m_pUseProfile.SolarIntensitySetpoint.Uom = m_pReader.GetAttribute("uom");
            m_pUseProfile.SolarIntensitySetpoint.Value = m_pReader.ReadElementContentAsDouble();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("NutzungsprofilVerschattung") == 0)
            break;
        }
      }

      m_UsageProfiles[m_pUseProfile.Name] = m_pUseProfile;
    }


    public void readPeriod(Period vPeriod)
    {
      string actElement;

      vPeriod.StartDay   = XmlConvert.ToInt16(m_pReader.GetAttribute("startDay"));
      vPeriod.StartMonth = XmlConvert.ToInt16(m_pReader.GetAttribute("startMonth"));
      vPeriod.EndDay     = XmlConvert.ToInt16(m_pReader.GetAttribute("endDay"));
      vPeriod.EndMonth   = XmlConvert.ToInt16(m_pReader.GetAttribute("endMonth"));

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("zeitreihe") == 0)
          {
            var vTimeSeries = new TimeSeries();

            vTimeSeries.MinValue = XmlConvert.ToDouble(m_pReader.GetAttribute("minValue"));
            vTimeSeries.MaxValue = XmlConvert.ToDouble(m_pReader.GetAttribute("maxValue"));
            vTimeSeries.DefaultValue = XmlConvert.ToDouble(m_pReader.GetAttribute("defaultValue"));

            readTimeSeries(vTimeSeries);

            vPeriod.TimeSeries.Add(vTimeSeries);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("periode") == 0)
            break;
        }
      }
    }

    public void readTimeSeries(TimeSeries vTimeSeries)
    {
      string temporaryStringHolder, actElement;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("day") == 0)
          {
            temporaryStringHolder = m_pReader.ReadElementContentAsString().Trim();

            if (Enum.IsDefined(typeof(TimeSeries.EDayType), temporaryStringHolder))
            {
              if (temporaryStringHolder.CompareTo("monday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.monday;
              }
              else
              if (temporaryStringHolder.CompareTo("tuesday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.tuesday;
              }
              else
              if (temporaryStringHolder.CompareTo("wednesday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.wednesday;
              }
              else
              if (temporaryStringHolder.CompareTo("thursday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.thursday;
              }
              else
              if (temporaryStringHolder.CompareTo("friday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.friday;
              }
              else
              if (temporaryStringHolder.CompareTo("saturday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.saturday;
              }
              else
              if (temporaryStringHolder.CompareTo("sunday") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.sunday;
              }
              else
              if (temporaryStringHolder.CompareTo("designDay") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.designDay;
              }
              else
              if (temporaryStringHolder.CompareTo("weekDay") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.weekDay;
              }
              else
              if (temporaryStringHolder.CompareTo("weekEnd") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.weekEnd;
              }
              else
              if (temporaryStringHolder.CompareTo("typicalDay") == 0)
              {
                vTimeSeries.DayType = TimeSeries.EDayType.typicalDay;
              }
            }
            else
            {
              m_Logger.Write($"Unsupported timeseries day type: {temporaryStringHolder}");
            }
          }
          else
          if (actElement.CompareTo("uom") == 0)
          {
            vTimeSeries.Values.Uom = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("values") == 0)
          {
            string[] elements = m_pReader.ReadElementContentAsString()
                                  .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            foreach (var element in elements)
            {
              vTimeSeries.Values.Values.Add(XmlConvert.ToDouble(element)); //.vValues.Values.Add();
            }
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("zeitreihe") == 0)
            break;
        }
      }
    }
  }
}
