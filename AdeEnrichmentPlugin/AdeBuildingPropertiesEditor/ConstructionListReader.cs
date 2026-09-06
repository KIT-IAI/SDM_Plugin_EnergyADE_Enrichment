using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace KIT.BuW.AdvEnrichment
{
  class ConstructionListReader
  {
    IAdeMessageLogger m_Logger;
    System.Xml.XmlTextReader m_pReader = null;
    public SortedDictionary<String, Construction> m_Constructions;
    public SortedDictionary<String, CityObjectGroup> m_CityObjectGroups;
    public SortedDictionary<String, Material> m_Materials;
    public ConstructionDefinitionType m_Type;

    public ConstructionListReader(IAdeMessageLogger logger, string filename, ConstructionDefinitionType type = ConstructionDefinitionType.USER_DEFINED)
    {
      m_Logger           = logger;
      m_pReader          = new System.Xml.XmlTextReader(filename);
      m_Constructions    = new SortedDictionary<string, Construction>();
      m_Materials        = new SortedDictionary<string, Material>();
      m_CityObjectGroups = new SortedDictionary<string, CityObjectGroup>();
      m_Type             = type;
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
                  case "SolidMaterial":
                    {
                      ReadSolidMaterial();
                      break;
                    };
                  case "Gas":
                    {
                      ReadGas();
                      break;
                    };
                  case "Construction":
                    {
                      ReadConstruction();
                      break;
                    };
                  case "CityObjectGroup":
                    {
                      ReadConstructionGroup();
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

    public void ReadConstructionGroup()
    {
      string temporaryStringHolder, actElement;

      temporaryStringHolder = m_pReader.GetAttribute("name");

      var pCityObjectGroup = new CityObjectGroup();

      pCityObjectGroup.Name = temporaryStringHolder;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            pCityObjectGroup.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else if (actElement.CompareTo("intAttribute") == 0)
          {
            string name = "";
            int value = 0;

            ReadGenericIntAttribute(ref name, ref value);

            if (name == "periodStart")
            {
              pCityObjectGroup.PeriodStart = value;
            }
            else if (name == "periodEnd")
            {
              pCityObjectGroup.PeriodEnd = value;
            }

          }
          else if (actElement.CompareTo("stringAttribute") == 0)
          {
            string name = "";
            string value = "";

            ReadGenericStringAttribute(ref name, ref value);

            if (name == "definition")
            {
              if (value == "DENA")
              {
                pCityObjectGroup.Definition = ConstructionDefinitionType.DENA;
              }
              else
              if (value == "ENEV")
              {
                pCityObjectGroup.Definition = ConstructionDefinitionType.ENEV;
              }
              else
              if (value == "TABULA")
              {
                pCityObjectGroup.Definition = ConstructionDefinitionType.TABULA;
              }
              else
              {
                pCityObjectGroup.Definition = ConstructionDefinitionType.USER_DEFINED;
              }
            }
          }
          else
          if (actElement.CompareTo("groupMember") == 0)
          {
            var groupMember = new GroupMember();

            groupMember.Title = m_pReader.GetAttribute("xlink:title");

            temporaryStringHolder = m_pReader.GetAttribute("xlink:href");

            if (temporaryStringHolder.StartsWith("#"))
            {
              temporaryStringHolder = temporaryStringHolder.Substring(1);
            }

            if (m_Constructions.ContainsKey(temporaryStringHolder))
            {
              groupMember.Construction = m_Constructions[temporaryStringHolder];
            }
            else
            {
              m_Logger.Write($"Missing reference in construction definition: {temporaryStringHolder}");
              Debug.Assert(m_Constructions.ContainsKey(temporaryStringHolder));
            }
            pCityObjectGroup.GroupMembers.Add(groupMember);
          }
          else
          if (actElement.CompareTo("openingFractionRoof") == 0)
          {
            pCityObjectGroup.OpeningFractionRoof = new Measure<double>();
            pCityObjectGroup.OpeningFractionRoof.Uom = m_pReader.GetAttribute("uom");
            pCityObjectGroup.OpeningFractionRoof.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("openingFractionWall") == 0)
          {
            pCityObjectGroup.OpeningFractionWall = new Measure<double>();
            pCityObjectGroup.OpeningFractionWall.Uom = m_pReader.GetAttribute("uom");
            pCityObjectGroup.OpeningFractionWall.Value = m_pReader.ReadElementContentAsDouble();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("CityObjectGroup") == 0)
            break;
        }
      }

      if (m_Type == ConstructionDefinitionType.USER_DEFINED || pCityObjectGroup.Definition == m_Type)
      {
        m_CityObjectGroups[pCityObjectGroup.Name] = pCityObjectGroup;
      }
    }

    public void ReadConstruction()
    {
      string temporaryStringHolder, actElement;

      temporaryStringHolder = m_pReader.GetAttribute("gml:id");
      Construction pConstruction = new Construction(temporaryStringHolder);
      pConstruction.Name = temporaryStringHolder;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("description") == 0)
          {
            pConstruction.Description = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("name") == 0)
          {
            pConstruction.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("uValue") == 0)
          {
            pConstruction.UValue = new Measure<double>();
            pConstruction.UValue.Uom = m_pReader.GetAttribute("uom");
            pConstruction.UValue.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("glazingRatio") == 0)
          {
            pConstruction.GlazingRatio = new Measure<double>();
            pConstruction.GlazingRatio.Uom = m_pReader.GetAttribute("uom");
            pConstruction.GlazingRatio.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("outsideConvectionCoefficient") == 0)
          {
            pConstruction.OutsideConvectionCoefficient = new Measure<double>();
            pConstruction.OutsideConvectionCoefficient.Uom = m_pReader.GetAttribute("uom");
            pConstruction.OutsideConvectionCoefficient.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("insideConvectionCoefficient") == 0)
          {
            pConstruction.InsideConvectionCoefficient = new Measure<double>();
            pConstruction.InsideConvectionCoefficient.Uom = m_pReader.GetAttribute("uom");
            pConstruction.InsideConvectionCoefficient.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("opticalProperties") == 0)
          {
            pConstruction.hasOpticalParams = true;
            pConstruction.OpticalProperties = new OpticalProperties();

            while (m_pReader.Read())
            {
              actElement = m_pReader.LocalName;
              if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
              {
                if (actElement.CompareTo("transmittance") == 0)
                {
                  pConstruction.readTransmittance = true;
                  var transmittance = new Transmittance();
                  readtransmittance(transmittance);
                  pConstruction.OpticalProperties.Transmittances.Add(transmittance);
                }
              }
              else
              if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
              {
                actElement = m_pReader.LocalName;
                if (actElement.CompareTo("opticalProperties") == 0)
                  break;
              }
            }
          }
          else
          if (actElement.CompareTo("Layer") == 0)
          {
            readLayer(pConstruction);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("Construction") == 0)
            break;
        }

        addConstruction(pConstruction);
      }
    }

    public void ReadGas()
    {
      string temporaryStringHolder, actElement;

      temporaryStringHolder = m_pReader.GetAttribute("gml:id");
      Gas pGas = new Gas(temporaryStringHolder);
      pGas.Name = temporaryStringHolder;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("description") == 0)
          {
            pGas.Description = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("rValue") == 0)
          {
            pGas.RValue = new Measure<double>();
            pGas.RValue.Uom = m_pReader.GetAttribute("uom");
            pGas.RValue.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("isVentilated") == 0)
          {
            pGas.IsVentilated = m_pReader.ReadElementContentAsBoolean();
          }
          else
          if (actElement.CompareTo("gasType") == 0)
          {
            pGas.GasType = m_pReader.ReadElementContentAsString().Trim();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("Gas") == 0)
            break;
        }
      }

      m_Materials[pGas.Name] = pGas;
    }

    public void ReadSolidMaterial()
    {
      string temporaryStringHolder, actElement;

      temporaryStringHolder = m_pReader.GetAttribute("gml:id");
      SolidMaterial m_pMaterial = new SolidMaterial(temporaryStringHolder);
      m_pMaterial.GmlID = temporaryStringHolder;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("name") == 0)
          {
            m_pMaterial.Name = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("description") == 0)
          {
            m_pMaterial.Description = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("conductivity") == 0)
          {
            m_pMaterial.Lambda = new Measure<double>();
            m_pMaterial.Lambda.Uom = m_pReader.GetAttribute("uom");
            m_pMaterial.Lambda.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("density") == 0)
          {
            m_pMaterial.Density = new Measure<double>();
            m_pMaterial.Density.Uom = m_pReader.GetAttribute("uom");
            m_pMaterial.Density.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("specificHeat") == 0)
          {
            m_pMaterial.SpecificHeat = new Measure<double>();
            m_pMaterial.SpecificHeat.Uom = m_pReader.GetAttribute("uom");
            m_pMaterial.SpecificHeat.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("opticalProperties") == 0)
          {
            m_pMaterial.HasOpticalParams = true;
            m_pMaterial.OpticalProperties = new OpticalProperties();

            readOpticalPropertiesMaterial(m_pMaterial);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("SolidMaterial") == 0)
            break;
        }
      }

      m_Materials[m_pMaterial.GmlID] = m_pMaterial;
    }

    private void readOpticalPropertiesMaterial(SolidMaterial m_pMaterial)
    {
      string actElement;

      while (m_pReader.Read())
      {
        actElement = m_pReader.LocalName;
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          if (actElement.CompareTo("emissivity") == 0)
          {
            m_pMaterial.ReadEmissivity = true;
            var emissivity = new Emissivity();
            readEmissivity(emissivity);
            m_pMaterial.OpticalProperties.Emissivities.Add(emissivity);
          }
          else
          if (actElement.CompareTo("reflectance") == 0)
          {
            m_pMaterial.ReadReflectance = true;
            var reflectance = new Reflectance();
            readReflectance(reflectance);
            m_pMaterial.OpticalProperties.Reflectances.Add(reflectance);
          }
          else
          if (actElement.CompareTo("absorptance") == 0)
          {
            m_pMaterial.ReadAbsorptance = true;
            var absorptance = new Absorptance();
            readAbsorptance(absorptance);
            m_pMaterial.OpticalProperties.Absorptances.Add(absorptance);
          }
          else
          if (actElement.CompareTo("transmittance") == 0)
          {
            m_pMaterial.ReadTransmittance = true;
            var transmittance = new Transmittance();
            readtransmittance(transmittance);
            m_pMaterial.OpticalProperties.Transmittances.Add(transmittance);
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("opticalProperties") == 0)
            break;
        }
      }
    }

    private void readAbsorptance(Absorptance absorptance)
    {
      string actElement = "";

      while (m_pReader.Read())
      {
        actElement = m_pReader.LocalName;
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          if (actElement.CompareTo("fraction") == 0)
          {
            absorptance.Fraction = new Measure<double>();
            absorptance.Fraction.Uom = m_pReader.GetAttribute("uom");
            absorptance.Fraction.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("wavelengthRange") == 0)
          {
            absorptance.WavelengthRange = new Measure<string>();
            absorptance.WavelengthRange.Value = m_pReader.ReadElementContentAsString().Trim();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("Absorptance") == 0)
            break;
        }
      }
    }

    private void readReflectance(Reflectance reflectance)
    {
      string actElement;

      while (m_pReader.Read())
      {
        actElement = m_pReader.LocalName;
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          if (actElement.CompareTo("fraction") == 0)
          {
            reflectance.Fraction = new Measure<double>();
            reflectance.Fraction.Uom = m_pReader.GetAttribute("uom");
            reflectance.Fraction.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("surface") == 0)
          {
            reflectance.Surface = new Measure<string>();
            reflectance.Surface.Value = m_pReader.ReadElementContentAsString().Trim();
          }
          else
          if (actElement.CompareTo("wavelengthRange") == 0)
          {
            reflectance.WavelengthRange = new Measure<string>();
            reflectance.WavelengthRange.Value = m_pReader.ReadElementContentAsString().Trim();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("Reflectance") == 0)
            break;
        }
      }
    }

    private void readEmissivity(Emissivity emissivity)
    {
      string actElement;

      while (m_pReader.Read())
      {
        actElement = m_pReader.LocalName;
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          if (actElement.CompareTo("fraction") == 0)
          {
            emissivity.Fraction = new Measure<double>();
            emissivity.Fraction.Uom = m_pReader.GetAttribute("uom");
            emissivity.Fraction.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("surface") == 0)
          {
            emissivity.Surface = new Measure<string>();
            emissivity.Surface.Value = m_pReader.ReadElementContentAsString().Trim();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("Emissivity") == 0)
            break;
        }
      }
    }

    public void readtransmittance(Transmittance transmittance)
    {
      string actElement;

      while (m_pReader.Read())
      {
        actElement = m_pReader.LocalName;
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          if (actElement.CompareTo("fraction") == 0)
          {
            transmittance.Fraction = new Measure<double>();
            transmittance.Fraction.Uom = m_pReader.GetAttribute("uom");
            transmittance.Fraction.Value = m_pReader.ReadElementContentAsDouble();
          }
          else
          if (actElement.CompareTo("wavelengthRange") == 0)
          {
            transmittance.WavelengthRange = new Measure<string>();
            transmittance.WavelengthRange.Value = m_pReader.ReadElementContentAsString().Trim();
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("Transmittance") == 0)
            break;
        }
      }
    }

    public bool ExistConstruction(string name)
    {
      return m_Constructions.ContainsKey(name);
    }

    public bool addConstruction(Construction pConstruction)
    {
      if (ExistConstruction(pConstruction.Name))
      {
        return false;
      }
      else
      {
        m_Constructions[pConstruction.Name] = pConstruction;
        return true;
      }
    }

    public void readLayer(Construction construction)
    {
      string actElement;

      var layer = new Layer();

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("thickness") == 0)
          {
            layer.Thickness = new Measure<double>();
            layer.Thickness.Value = m_pReader.ReadElementContentAsDouble();
            layer.Thickness.Uom = m_pReader.GetAttribute("uom");
          }
          else
            if (actElement.CompareTo("LayerComponent") == 0)
          {
            var layerComponent = new LayerComponent();
            readlayerComponent(layerComponent);
            layer.LayerComponents.Add(layerComponent);
          }
        }
        else if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("layer") == 0)
            break;
        }
      }

      construction.Layers.Add(layer);
    }

    public void readlayerComponent(LayerComponent layerComponent)
    {
      string temporaryStringHolder, actElement;

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;

          if (actElement.CompareTo("areaFraction") == 0)
          {
            layerComponent.AreaFraction = new Measure<double>();
            layerComponent.AreaFraction.Uom = m_pReader.GetAttribute("uom");
            layerComponent.AreaFraction.Value = m_pReader.ReadElementContentAsDouble();
          }

          if (actElement.CompareTo("material") == 0)
          {
            temporaryStringHolder = m_pReader.GetAttribute("xlink:href");

            if (temporaryStringHolder.StartsWith("#"))
            {
              temporaryStringHolder = temporaryStringHolder.Substring(1);
            }

            if (m_Materials.ContainsKey(temporaryStringHolder))
            {
              layerComponent.Material = m_Materials[temporaryStringHolder];
            }
            else
            {
              m_Logger.Write($"Missing reference in material definition: {temporaryStringHolder}");
              Debug.Assert(m_Materials.ContainsKey(temporaryStringHolder));
            }
          }
        }
        else if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("LayerComponent") == 0)
            break;
        }
      }
    }

    public bool ReadGenericIntAttribute(ref string name, ref int value)
    {
      bool state = false;

      string actElement = "";

      name = m_pReader.GetAttribute("name");

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Text)
        {
          if (actElement.CompareTo("value") == 0)
          {
            value = m_pReader.ReadContentAsInt();
            state = true;
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("intAttribute") == 0)
            break;
        }
      }

      return state;
    }

    public bool ReadGenericStringAttribute(ref string name, ref string value)
    {
      bool state = false;

      string actElement = "";

      name = m_pReader.GetAttribute("name");

      while (m_pReader.Read())
      {
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Element)
        {
          actElement = m_pReader.LocalName;
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.Text)
        {
          if (actElement.CompareTo("value") == 0)
          {
            value = m_pReader.ReadContentAsString();
            state = true;
          }
        }
        else
        if (m_pReader.NodeType == System.Xml.XmlNodeType.EndElement)
        {
          actElement = m_pReader.LocalName;
          if (actElement.CompareTo("stringAttribute") == 0)
            break;
        }
      }
      return state;
    }
  }

}