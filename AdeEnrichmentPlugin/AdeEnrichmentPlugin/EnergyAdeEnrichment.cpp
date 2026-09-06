#include "EnergyAdeEnrichment.h"
#include "DotNetHelper.h"

using namespace KIT::BuW::AdvEnrichment;

EnergyAdeEnrichment::EnergyAdeEnrichment(KIT::BuW::AdvEnrichment::AdeBuildingProperties^ m_pBuildingProperties, IfcDB::Populationi* pDB, IfcDB::Feature* pFeature)
  : DataEnrichement(m_pBuildingProperties, pDB), m_pBuildingFeature(pFeature)
{
}

void EnergyAdeEnrichment::gatherBuildingParameter()
{
  m_pBuildingProperties->Guid = cDotNetHelper::ToString(m_pBuildingFeature->getGuid());

  m_pBuildingProperties->DisplayString = createDisplayString(m_pBuildingFeature);
  setGeoferenceInfo(m_pBuildingFeature);

  getGeometryStatistic(m_pBuildingFeature);
  calculateGeometryInformation();

  m_pBuildingProperties->GmlId = cDotNetHelper::ToString(m_pBuildingFeature->getGmlId());

  // Check for building parts
  if (m_pBuildingFeature->getType() == IfcDB::CITYGML_BUILDING)
  {
    IfcDB::IfcEntityList buildingParts;
    m_pDB->getChildren(IfcDB::CITYGML_BUILDING_PART, m_pBuildingFeature->getOid(), buildingParts);

    if (buildingParts.empty() == false)
    {
      m_pBuildingProperties->buildingParts = gcnew System::Collections::Generic::List<System::String^>();

      for (auto pBuildingPart : buildingParts)
      {
        m_pBuildingProperties->buildingParts->Add(cDotNetHelper::ToString(pBuildingPart->getGuid()));
      }
    }
  }

  if (m_pBuildingFeature->isTypeOf(IfcDB::CITYGML_BUILDING_PART) == true)
  {
    m_pBuildingProperties->isBuildingPart = true;
  }

  std::wstring namespaceCode(m_pBuildingFeature->getNamespaceCode() + _T(":"));

  getYearOfContruction(namespaceCode + _T("yearOfConstruction"));
  getBuildingFunction(namespaceCode + _T("function"));
  getStoreysAboveGround(namespaceCode + _T("storeysAboveGround"));
  getStoreysBelowGround(namespaceCode + _T("storeysBelowGround"));
  getMeasuredHeight(namespaceCode + _T("measuredHeight"));
}

System::String^ EnergyAdeEnrichment::createDisplayString(IfcDB::IfcEntity* pEntity)
{
  std::wstringstream displayString;

  IfcDB::Feature* pFeature = dynamic_cast<IfcDB::Feature*>(pEntity);

  if (pFeature)
  {
    if (pFeature->getName().empty())
    {
      displayString << pFeature->getGmlId();
    }
    else
    {
      displayString << pFeature->getName() << _T(" (") << pFeature->getGmlId() << _T(")");
    }
  }

  return cDotNetHelper::ToString(displayString.str());
}

void EnergyAdeEnrichment::getGeometryStatistic(IfcDB::Feature* pFeature)
{
  // hier müssen noch untergeordnete Elemente überprüft werden falls kein Solid vorhanden ist!

  for (const auto& representation : pFeature->getRepresentations())
  {
    std::wstring representationHash = representation.getRepresentationHash();

    if (representationHash.find(_T("lod0")) != std::wstring::npos)
    {
      m_pBuildingProperties->hasLoD0Geometry = true;
    }
    else if (representationHash.find(_T("lod1")) != std::wstring::npos)
    {
      m_pBuildingProperties->hasLoD1Geometry = true;
    }
    else if (representationHash.find(_T("lod2")) != std::wstring::npos)
    {
      m_pBuildingProperties->hasLoD2Geometry = true;
    }
    else if (representationHash.find(_T("lod3")) != std::wstring::npos)
    {
      m_pBuildingProperties->hasLoD3Geometry = true;
    }
    else if (representationHash.find(_T("lod4")) != std::wstring::npos)
    {
      m_pBuildingProperties->hasLoD4Geometry = true;
    }

    IfcDB::GeomType geometryType = representation.m_geometry->getType();

    if (geometryType == IfcDB::GML_SOLID ||
        geometryType == IfcDB::COMPOSITE_SOLID ||
        geometryType == IfcDB::MULTI_SOLID)
    {
      m_pBuildingProperties->hasBuildingSolid = true;

      // TODO: cSolidGeometryAnalyzer wieder aktivieren!
      // --> Instanz per Interface in PopulationContext beschaffen!
      IfcDB::cSolidGeometryAnalyzer solidGeometryAnalyzer(m_pDB);
      m_pBuildingProperties->hasCorrectSolid = solidGeometryAnalyzer.analyzeGeometry(representation.m_geometry);
      /*TODO: remove!*/ //m_pBuildingProperties->hasCorrectSolid = false;
    }
  }

  if (pFeature != nullptr)
  {
    std::vector<IfcDB::Feature*> boundarySurfaces;
    m_pDB->getRelatedFeatures(pFeature, _T("bldg:boundedBy"), boundarySurfaces);

    for (auto pBoundary : boundarySurfaces)
    {
      getGeometryStatistic(pBoundary);
    }
  }
}

void EnergyAdeEnrichment::calculateGeometryInformation()
{
  double area(0.0);
  double volume(0.0);
  double height(0.0);

  bool state = analyzeRepresentations(m_pBuildingFeature, area, volume, height);

  IfcDB::IfcEntityList buildingParts;
  m_pDB->getChildren(IfcDB::CITYGML_BUILDING_PART, m_pBuildingFeature->getOid(), buildingParts);

  if (!buildingParts.empty())
  {
    for (auto pBuildingPart : buildingParts)
    {
      double partHeight(0.0);

      state &= analyzeRepresentations(pBuildingPart, area, volume, partHeight);

      height = std::max(height, partHeight);
    }
  }

  if (volume > 0.0)
  {
    m_pBuildingProperties->GrossVolume = volume;
  }

  if (area > 0.0)
  {
    m_pBuildingProperties->GrossGroundArea = area;

    // has attribute storeysAboveGround?
    if (m_pBuildingProperties->StoreysOverground.HasValue)
    {
      area *= m_pBuildingProperties->StoreysOverground.Value;
    }

    m_pBuildingProperties->NetFloorArea = area;
    m_pBuildingProperties->GrossFloorArea = area;

    if (m_pBuildingProperties->FloorAreaRatio != nullptr)
    {
      m_pBuildingProperties->NetFloorArea = area * m_pBuildingProperties->FloorAreaRatio->Value;
    }
  }

  if (height > 0.0)
  {
    m_pBuildingProperties->BuildingHeightFromGeom = height;
  }
}

bool EnergyAdeEnrichment::analyzeRepresentations(IfcDB::IfcEntity* pEntity, double& area, double& volume, double& height)
{
  bool state(false);
  IfcDB::Geometry* pSolidGeometry = nullptr;
  IfcDB::Geometry* pFootPrintGeometry = nullptr;

  for (const auto& representation : pEntity->getRepresentations())
  {
    if (representation.m_representationIdentifier == L"lod0" && representation.m_representationType.find(L"FootPrint") != std::string::npos)
    {
      pFootPrintGeometry = representation.m_geometry;
    }
    else if (!pSolidGeometry && representation.m_representationIdentifier == L"lod1" && IfcDB::Geometry::isSolid(representation.m_geometry))
    {
      pSolidGeometry = representation.m_geometry;
    }
    else if (representation.m_representationIdentifier == L"lod2" && IfcDB::Geometry::isSolid(representation.m_geometry))
    {
      pSolidGeometry = representation.m_geometry;
    }
  }

  bool removeGeometry(false);
  IfcDB::utils::EnergyAdeExtender energyADEExtender(m_pDB);

  if (!pSolidGeometry)
  {
    removeGeometry = true;
    pSolidGeometry = energyADEExtender.createBuildingSolid(dynamic_cast<IfcDB::Feature*>(pEntity));
  }

  if (!pSolidGeometry)
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_004"), L"Missing building geometry", pEntity->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  IfcDB::Polylines polylines;

  if (pFootPrintGeometry)
  {
    polylines = energyADEExtender.createFootprint({ pFootPrintGeometry });
    setBoundaryGeometry(polylines);
  }
  else if(pSolidGeometry)
  {
    IfcDB::Point min, max;
    if (pSolidGeometry->getBoundingBox(min, max))
    {
      height = max.z - min.z;
    }

    polylines = energyADEExtender.createFootprint({ pSolidGeometry });
    setBoundaryGeometry(polylines);
  }
  else
  {
    assert(0);
  }

  volume = IfcDB::Geometry::calcVolume(pSolidGeometry);

  for (auto pPolyline : polylines)
  {
    area += IfcDB::Curve::calcArea(pPolyline);
    delete pPolyline;
  }

  if (pSolidGeometry && area > 0.0 && volume > 0.0)
  {
    state = true;
  }

  if (removeGeometry == true && pSolidGeometry)
  {
    removeGeometry = false;
    delete pSolidGeometry;
    pSolidGeometry = nullptr;
  }

  return state;
}

void EnergyAdeEnrichment::getYearOfContruction(std::wstring& attributeName)
{
  IfcDB::AbstractTime* pTime = m_pBuildingFeature->getDateAttributWert(attributeName);

  if (pTime != nullptr && pTime->getTimeType() == IfcDB::AbstractTime::TIME_POSITION)
  {
    IfcDB::TimePosition* pTimePosition = dynamic_cast<IfcDB::TimePosition*>(pTime);

    m_pBuildingProperties->YearOfConstruction = pTimePosition->getYear();
  }
}

void EnergyAdeEnrichment::getBuildingFunction(std::wstring& attributeName)
{
  std::wstring stringAttributeValue;

  if (m_pBuildingFeature->getStringAttributWert(attributeName, stringAttributeValue))
  {
    m_pBuildingProperties->AlkisFunction = cDotNetHelper::ToString(stringAttributeValue);
    m_pBuildingProperties->BuildingFunction = cDotNetHelper::ToString(stringAttributeValue);
  }
}

void EnergyAdeEnrichment::getStoreysAboveGround(std::wstring& attributeName)
{
  int64_t intAttributeValue;

  if (m_pBuildingFeature->getIntegerAttributWert(attributeName, intAttributeValue))
  {
    m_pBuildingProperties->StoreysOverground = (int)intAttributeValue;
  }
}

void EnergyAdeEnrichment::getStoreysBelowGround(std::wstring& attributeName)
{
  int64_t intAttributeValue;

  if (m_pBuildingFeature->getIntegerAttributWert(attributeName, intAttributeValue))
  {
    m_pBuildingProperties->StoreysUnderground = (int)intAttributeValue;
  }
}

void EnergyAdeEnrichment::getMeasuredHeight(std::wstring& attributeName)
{
  double doubleAttributeValue;

  if (m_pBuildingFeature->getDoubleAttributWert(attributeName, doubleAttributeValue))
  {
    m_pBuildingProperties->BuildingHeightFromModel = doubleAttributeValue;
  }
}

IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight toConstructionWeight(System::String^ constructionWeight)
{
  IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight ifcdbConstructionWeight(IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight::BC_Medium);

  if (constructionWeight == L"veryLight")
  {
    ifcdbConstructionWeight = IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight::BC_VeryLight;
  }
  else if (constructionWeight == L"light")
  {
    ifcdbConstructionWeight = IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight::BC_Light;
  }
  else if (constructionWeight == L"medium")
  {
    ifcdbConstructionWeight = IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight::BC_Medium;
  }
  else if (constructionWeight == L"heavy")
  {
    ifcdbConstructionWeight = IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight::BC_Heavy;
  }

  return ifcdbConstructionWeight;
}

IfcDB::utils::ThermalAnalysis::Construction toConstruction(KIT::BuW::AdvEnrichment::Construction^ construction)
{
  IfcDB::utils::ThermalAnalysis::Construction ifcdbConstruction;

  ifcdbConstruction.m_name = cDotNetHelper::FromString(construction->Name);
  ifcdbConstruction.m_description = cDotNetHelper::FromString(construction->Description);

  if (construction->UValue)
  {
    ifcdbConstruction.m_uValue = construction->UValue->Value;
  }

  if (construction->gValue)
  {
    ifcdbConstruction.m_gValue = construction->gValue->Value;
  }

  if (construction->GlazingRatio)
  {
    ifcdbConstruction.m_glazingRatio = construction->GlazingRatio->Value;
  }

  if (construction->InsideConvectionCoefficient)
  {
    ifcdbConstruction.m_insideConvectionCoefficient = construction->InsideConvectionCoefficient->Value;
  }

  if (construction->OutsideConvectionCoefficient)
  {
    ifcdbConstruction.m_outsideConvectionCoefficient = construction->OutsideConvectionCoefficient->Value;
  }

  if (construction->GlazingRatio)
  {
    ifcdbConstruction.m_glazingRatio = construction->GlazingRatio->Value;
  }

  for each (Layer^ layer in construction->Layers)
  {
    IfcDB::utils::ThermalAnalysis::Layer ifcdbLayer;

    ifcdbLayer.m_thickness = layer->Thickness->Value;

    for each (LayerComponent^ layerComponent in layer->LayerComponents)
    {
      IfcDB::utils::ThermalAnalysis::LayerComponent ifcdbLayerComponent;

      if (layerComponent->AreaFraction)
      {
        ifcdbLayerComponent.m_areaFraction = layerComponent->AreaFraction->Value;
      }

      std::wstring materialName = cDotNetHelper::FromString(layerComponent->Material->Name);

      if (layerComponent->Material->GetType() == SolidMaterial::typeid)
      {
        SolidMaterial^ solidMaterial = safe_cast<SolidMaterial^>(layerComponent->Material);

        IfcDB::utils::ThermalAnalysis::SolidMaterial ifcdbSolidMaterial;
        ifcdbSolidMaterial.m_name = materialName;
        ifcdbSolidMaterial.m_description = cDotNetHelper::FromString(solidMaterial->Description);
        ifcdbSolidMaterial.m_isTransparent = solidMaterial->IsTransparent;
        ifcdbSolidMaterial.m_lambda = solidMaterial->Lambda->Value;
        ifcdbSolidMaterial.m_density = solidMaterial->Density->Value;
        ifcdbSolidMaterial.m_specificHeat = solidMaterial->SpecificHeat->Value;

        ifcdbLayerComponent.m_material = std::make_shared<IfcDB::utils::ThermalAnalysis::SolidMaterial>(ifcdbSolidMaterial);
      }
      else if (layerComponent->Material->GetType() == Gas::typeid)
      {
        Gas^ gas = safe_cast<Gas^>(layerComponent->Material);

        IfcDB::utils::ThermalAnalysis::Gas ifcdbGas;

        if (gas->RValue)
        {
          ifcdbGas.m_rValue = gas->RValue->Value;
        }

        ifcdbGas.m_name = materialName;
        ifcdbGas.m_description = cDotNetHelper::FromString(gas->Description);
        ifcdbGas.m_isTransparent = gas->IsTransparent;
        ifcdbGas.m_isVentilated = gas->IsVentilated;
        ifcdbGas.m_gasType = cDotNetHelper::FromString(gas->GasType);

        ifcdbLayerComponent.m_material = std::make_shared<IfcDB::utils::ThermalAnalysis::Gas>(ifcdbGas);
      }

      ifcdbLayer.m_layerComponents.emplace_back(ifcdbLayerComponent);
    }

    ifcdbConstruction.m_layers.emplace_back(ifcdbLayer);
  }

  return std::move(ifcdbConstruction);
}


EnergyAdeEnrichmentDataUpdater::EnergyAdeEnrichmentDataUpdater(KIT::BuW::AdvEnrichment::AdeBuildingProperties^ m_pBuildingProperties, IfcDB::Populationi* pDB, IfcDB::Feature* pFeature)
  : EnrichmentDataUpdater(m_pBuildingProperties, pDB), m_pBuildingFeature(pFeature)
{
  m_CreatedEnrichmentComponents = gcnew System::Collections::Generic::List<System::String^>();
}

void EnergyAdeEnrichmentDataUpdater::updateBuildingParameter()
{
  if (m_pBuildingFeature != nullptr)
  {
    std::wstring namespaceKuerzel(m_pBuildingFeature->getNamespaceCode() + _T(":"));

    // yearOfConstruction
    if (m_pBuildingProperties->YearOfConstruction.HasValue == true)
    {
      setYearOfContruction(m_pBuildingFeature, namespaceKuerzel + _T("yearOfConstruction"), m_pBuildingProperties->YearOfConstruction.Value);
    }

    // constructionWeight
    if (m_pBuildingProperties->ConstructionWeight != nullptr)
    {
      setStringAttribute(m_pBuildingFeature, _T("energy:constructionWeight"), m_pBuildingProperties->ConstructionWeight);
    }

    // buildingSizeType
    if (m_pBuildingProperties->BuildingSizeType != nullptr)
    {
      setStringAttribute(m_pBuildingFeature, _T("energy:buildingSizeType"), m_pBuildingProperties->BuildingSizeType);
    }

    // update number of storeys
    if (m_pBuildingProperties->StoreyHeight != nullptr)
    {
      if (m_pBuildingProperties->BuildingHeightFromGeom.HasValue && m_pBuildingProperties->BuildingHeightFromModel.HasValue)
      {
        double deltaHeight = fabs(m_pBuildingProperties->BuildingHeightFromGeom.Value - m_pBuildingProperties->BuildingHeightFromModel.Value);

        if (fabs(deltaHeight > m_pBuildingProperties->BuildingHeightFromGeom.Value * 0.1))
        {
          std::wstringstream message;
          message << L"Height difference between model and geometry: " << deltaHeight;
          IfcDB::Message* pMessage = new IfcDB::Message(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_002"), message.str());
          m_pDB->addErrorMessage(pMessage);
        }
      }
    }

    // update floor area
    if (m_pBuildingProperties->GrossFloorArea.HasValue)
    {
      //// has attribute storeysAboveGround?
      //if (m_pBuildingProperties->StoreysOverground.HasValue)
      //{
      //  m_pBuildingProperties->GrossFloorArea = m_pBuildingProperties->GrossFloorArea.Value * m_pBuildingProperties->StoreysOverground.Value;
      //}

      m_pBuildingProperties->NetFloorArea = m_pBuildingProperties->GrossFloorArea.Value;

      if (m_pBuildingProperties->FloorAreaRatio != nullptr)
      {
        m_pBuildingProperties->NetFloorArea = m_pBuildingProperties->GrossFloorArea.Value * m_pBuildingProperties->FloorAreaRatio->Value;
      }
    }
  }
}

void EnergyAdeEnrichmentDataUpdater::setYearOfContruction(IfcDB::Feature* pFeature, const std::wstring& attributeName, int yearOfConstruction)
{
  IfcDB::AbstractTime* pTime = pFeature->getDateAttributWert(attributeName);

  if (pTime != nullptr && pTime->getTimeType() == IfcDB::AbstractTime::TIME_POSITION)
  {
    IfcDB::TimePosition* pTimePosition = dynamic_cast<IfcDB::TimePosition*>(pTime);

    pTimePosition->setYear(yearOfConstruction);
  }
  else
  {
    IfcDB::TimePosition* pTimePosition = new IfcDB::TimePosition(yearOfConstruction, 0, 0);
    pFeature->setDateAttribut(attributeName, pTimePosition);
  }
}

void EnergyAdeEnrichmentDataUpdater::setStringAttribute(IfcDB::Feature* pFeature, const std::wstring& attributeName, System::String^ attributeValue)
{
  std::wstring existingAttributeValue;
  if (pFeature->getStringAttributWert(attributeName, existingAttributeValue))
  {
    pFeature->deleteStringAttribut(attributeName);
  }
  else
  {
    pFeature->setStringAttribut(attributeName, cDotNetHelper::FromString(attributeValue));
  }
}

void EnergyAdeEnrichmentDataUpdater::setDoubleAttribute(IfcDB::Feature* pFeature, const std::wstring& attributeName, double attributeValue, IfcDB::UOM* pUOM)
{
  double existingAttributeValue;
  if (pFeature->getDoubleAttributWert(attributeName, existingAttributeValue))
  {
    pFeature->deleteDoubleAttribut(attributeName);
  }
  else
  {
    pFeature->setDoubleAttribut(attributeName, attributeValue, pUOM);
  }
}

// - Year of Construction
// - Construction Weight
// - Building Size Type
// Walls Opening Percentage
// Roof Opening Percentage
// Floor Area Ratio
// Storey Height
// - Air Infiltration Rate
// - U-Value Walls
// - U-Values Roofs
// - U-Values GroundPlate

void EnergyAdeEnrichmentDataUpdater::enrichBuilding()
{
  IfcDB::utils::EnergyAdeExtender energyADEExtender(m_pDB);

  if (m_pBuildingFeature != nullptr && m_pBuildingProperties->enrichBuildingData == true)
  {
    if (m_pBuildingProperties->EthosBuildingProps)
    {
      IfcDB::Attributes::ListAttribute* pListAttribute = new IfcDB::Attributes::ListAttribute(L"ETHOS-BUILDA");
      pListAttribute->addAttribute(new IfcDB::Attributes::StringAttribute(L"ExtrenalID", cDotNetHelper::FromString(m_pBuildingProperties->EthosBuildingProps->ExternalID)));
      pListAttribute->addAttribute(new IfcDB::Attributes::StringAttribute(L"GeneratorSource", cDotNetHelper::FromString(m_pBuildingProperties->EthosBuildingProps->GeneratorSource)));
      pListAttribute->addAttribute(new IfcDB::Attributes::StringAttribute(L"GeneratorLineage", cDotNetHelper::FromString(m_pBuildingProperties->EthosBuildingProps->GeneratorLineage)));

      if (m_pBuildingProperties->EthosBuildingProps->YearOfConstruction.HasValue)
      {
        pListAttribute->addAttribute(new IfcDB::Attributes::IntAttribute(L"YearOfConstruction", m_pBuildingProperties->EthosBuildingProps->YearOfConstruction.Value));
      }

      m_pBuildingFeature->addAttribute(pListAttribute);
    }

    // CityModel
    IfcDB::Feature* pCityModel = m_pDB->getFeature(m_pBuildingFeature->getParentOid());

    IfcDB::utils::ThermalAnalysis::BuildingConstructionWeight constructionWeight = toConstructionWeight(m_pBuildingProperties->ConstructionWeight);

    IfcDB::utils::ThermalAnalysis::Constructions constructions;
    constructions.m_constructionWeight = constructionWeight;

    if (m_pBuildingProperties->useConstruction == false)
    {
      if (m_pBuildingProperties->UValueWalls != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::Wall, { L"Default Wall Construction", m_pBuildingProperties->UValueWalls->Value });
      }

      if (m_pBuildingProperties->UValueRoofs != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::Roof, { L"Default Roof Construction", m_pBuildingProperties->UValueRoofs->Value });
      }

      if (m_pBuildingProperties->UValueGroundPlates != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::GroundSlab, { L"Default Base Plate Construction", m_pBuildingProperties->UValueWalls->Value });
      }

      if (m_pBuildingProperties->UValueWindows != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::Window, { L"Default Windows Construction", m_pBuildingProperties->UValueWindows->Value });
      }
    }
    else
    {
      if (m_pBuildingProperties->WallConstruction != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::Wall, toConstruction(m_pBuildingProperties->WallConstruction));
      }

      if (m_pBuildingProperties->RoofConstruction != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::Roof, toConstruction(m_pBuildingProperties->RoofConstruction));
      }

      if (m_pBuildingProperties->GroundPlateConstruction != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::GroundSlab, toConstruction(m_pBuildingProperties->GroundPlateConstruction));
      }

      if ((m_pBuildingProperties->OpeningPercentageWalls.HasValue || m_pBuildingProperties->OpeningPercentageRoofs.HasValue || m_pBuildingProperties->hasLoD3Geometry || m_pBuildingProperties->hasLoD4Geometry) && m_pBuildingProperties->WindowConstruction != nullptr)
      {
        constructions.addConstruction(IfcDB::utils::ThermalAnalysis::ElementType::Window, toConstruction(m_pBuildingProperties->WindowConstruction));
      }
    }

    if (!energyADEExtender.createConstructions(pCityModel, constructions))
    {
      System::Windows::MessageBox::Show("Failure while creating constructions!");

      return;
    }

    /*
        bool isExternal(true);
        std::wstring constructionWeight = cDotNetHelper::FromString(m_pBuildingProperties->ConstructionWeight);
        double uValue = m_pBuildingProperties->UValueWalls;
        double transition(0.0);

        if (isExternal == true)
        {
          transition = 1 / (Alpha_a + KONSTANTE_ALPHA_S) + 1 / (AlphaWert(self, Room) + KONSTANTE_ALPHA_S))
        }
        else
        {
          transition = 2 / (AlphaWert(self, Room) + KONSTANTE_ALPHA_S);
        }

        double Rges = 1 / uValue - transition;  // abzgl. Wärmeübergang

        HeatTransmission::calculateLamdaFromUValue(uValue, 0.0, IfcDB::CITYGML_WALL_SURFACE, true, 90.0);
    */

    bool createUsageZoneAndThermalZone(false);

    if ((m_pBuildingProperties->singleThermalZone == true && m_pBuildingProperties->isBuildingPart == false) ||
        (m_pBuildingProperties->singleThermalZone == false && (m_pBuildingProperties->isBuildingPart == true || m_pBuildingProperties->buildingParts == nullptr)))
    {
      createUsageZoneAndThermalZone = true;
    }

    IfcDB::IfcEntityList uasgeZones;
    m_pDB->getList(_T("energy:UsageZone"), uasgeZones, m_pBuildingFeature->getOid());

    if (uasgeZones.empty() == true)
    {
      if (createUsageZoneAndThermalZone == true)
      {
        std::wstring usageZoneType = cDotNetHelper::FromString(m_pBuildingProperties->UsageType);

        std::vector<IfcDB::utils::UsageProfile*> usageProfiles;
        IfcDB::Feature* pUsageZone = energyADEExtender.createUsageZone(m_pBuildingFeature, usageZoneType, usageProfiles);

        createUsageProfiles(energyADEExtender, pUsageZone);
      }
    }

    // Building
    if (m_pBuildingProperties->GrossFloorArea.HasValue == true)
    {
      energyADEExtender.createFloorArea(m_pBuildingFeature, m_pBuildingProperties->GrossFloorArea.Value);
    }

    if (m_pBuildingProperties->NetFloorArea.HasValue == true)
    {
      energyADEExtender.createFloorArea(m_pBuildingFeature, m_pBuildingProperties->NetFloorArea.Value, true);
    }

    if (m_pBuildingProperties->GrossVolume.HasValue == true)
    {
      energyADEExtender.createVolumeType(m_pBuildingFeature, m_pBuildingProperties->GrossVolume.Value);
    }

    // Thermal zones
    IfcDB::IfcEntityList thermalZones;
    m_pDB->getList(_T("energy:ThermalZone"), thermalZones, m_pBuildingFeature->getOid());

    if (thermalZones.empty() == true)
    {
      if (createUsageZoneAndThermalZone == true)
      {
        createThermalZone(m_pBuildingFeature, energyADEExtender);
      }
    }
  }
}

void EnergyAdeEnrichmentDataUpdater::createThermalZone(IfcDB::Feature* m_pFeature, IfcDB::utils::EnergyAdeExtender& energyADEExtender)
{
  IfcDB::utils::OpeningRatios openingRatios;
  openingRatios.CreateOpenings = m_pBuildingProperties->HasWindows;

  if (m_pBuildingProperties->OpeningPercentageWalls.HasValue)
  {
    openingRatios.OpeningRatioWall = m_pBuildingProperties->OpeningPercentageWalls.Value / 100;
  }

  if (m_pBuildingProperties->OpeningPercentageRoofs.HasValue)
  {
    openingRatios.OpeningRatioRoof = m_pBuildingProperties->OpeningPercentageRoofs.Value / 100;
  }

  if (!m_pBuildingProperties->WallConstruction || !m_pBuildingProperties->GroundPlateConstruction ||
      !m_pBuildingProperties->RoofConstruction || !m_pBuildingProperties->WindowConstruction)
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, L"EnergyAdeEnrichment_001", L"Failed to create thermal zone (missing constructions) for " + m_pFeature->getName(), m_pFeature->getOid());
    m_pDB->addErrorMessage(pMessage);

    return;
  }

  IfcDB::utils::ConstructionNames constructionNames;
  constructionNames.wallConstruction = cDotNetHelper::FromString(m_pBuildingProperties->WallConstruction->Name);
  constructionNames.groundPlateConstruction = cDotNetHelper::FromString(m_pBuildingProperties->GroundPlateConstruction->Name);
  constructionNames.roofConstruction = cDotNetHelper::FromString(m_pBuildingProperties->RoofConstruction->Name);
  constructionNames.windowConstruction = cDotNetHelper::FromString(m_pBuildingProperties->WindowConstruction->Name);

  if (m_pBuildingProperties->singleThermalZone)
  {
    IfcDB::Feature* pThermalZone = energyADEExtender.createThermalZone(m_pFeature);

    energyADEExtender.createThermalBoundaries(m_pFeature, pThermalZone, constructionNames, openingRatios);
    energyADEExtender.createThermalZoneGeometry(pThermalZone);

    // infiltrationRate
    if (m_pBuildingProperties->AirInfiltrationRate)
    {
      setDoubleAttribute(pThermalZone, _T("energy:infiltrationRate"), m_pBuildingProperties->AirInfiltrationRate->Value, m_pDB->getUomList()->getUOMFromName(_T("AIR_CHANGE_RATE_HOUR")));
    }

    int noOfStoreys(1);

    if (m_pBuildingProperties->StoreysOverground.HasValue)
    {
      noOfStoreys = m_pBuildingProperties->StoreysOverground.Value;
    }

    if (m_pBuildingProperties->StoreyHeight && m_pBuildingProperties->StoreysOverground.HasValue == false)
    {
      double height(0.0);

      if (m_pBuildingProperties->BuildingHeightFromGeom.HasValue)
      {
        height = m_pBuildingProperties->BuildingHeightFromGeom.Value;
      }
      else if (m_pBuildingProperties->BuildingHeightFromModel.HasValue)
      {
        height = m_pBuildingProperties->BuildingHeightFromModel.Value;
      }

      noOfStoreys = std::max(1, static_cast<int>(std::floor(height / m_pBuildingProperties->StoreyHeight->Value)));
    }

    if (m_pBuildingProperties->GrossFloorArea.HasValue == true)
    {
      energyADEExtender.createFloorArea(pThermalZone, m_pBuildingProperties->GrossFloorArea.Value * noOfStoreys);
    }

    if (m_pBuildingProperties->NetFloorArea.HasValue == true)
    {
      energyADEExtender.createFloorArea(pThermalZone, m_pBuildingProperties->NetFloorArea.Value * noOfStoreys, true);
    }

    if (m_pBuildingProperties->GrossVolume.HasValue == true)
    {
      energyADEExtender.createVolumeType(pThermalZone, m_pBuildingProperties->GrossVolume.Value);
    }
  }
  else
  {
    energyADEExtender.createThermalZones(m_pFeature, openingRatios);
  }
}

bool EnergyAdeEnrichmentDataUpdater::createDefaultUsageProfiles(std::vector<IfcDB::utils::UsageProfile*>& usageProfiles)
{
  bool state(false);

  // TODO: Enable this code
  //usageProfiles.emplace_back(BuildingSimulation::UsageProfile::createDefaultHeatingProfile());
  //usageProfiles.emplace_back(BuildingSimulation::UsageProfile::createDefaultPersonProfile());

  return state;
}

void EnergyAdeEnrichmentDataUpdater::createUsageProfiles(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone)
{
  if (m_pBuildingProperties->UsageProfileHeating != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileHeating->Name) == false)
    {
      IfcDB::UOM* pUOM = m_pDB->getUomList()->getUOMFromName(_T("DEGREE_CELSIUS"));
      IfcDB::Feature* pDailyPatternSchedule = createDailyPatternSchedule(energyADEExtender, pUsageZone, m_pBuildingProperties->UsageProfileHeating->Periods,
                                                                         cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeating->Name),
                                                                         cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeating->Description), _T("Nominal temperature heating"), pUOM);
      m_pDB->createObjRelation(pUsageZone->getOid(), pDailyPatternSchedule->getOid(), _T("energy:heatingSchedule"), pUsageZone->getModelInfo());

      energyADEExtender.registerHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeating->Name), pDailyPatternSchedule);
      m_CreatedEnrichmentComponents->Add(m_pBuildingProperties->UsageProfileHeating->Name);
    }
    else
    {
      IfcDB::utils::EnergyAdeExtender::HRef hRef = energyADEExtender.getHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeating->Name));
      energyADEExtender.createHRef(pUsageZone, _T("energy:heatingSchedule"), hRef);
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_005"), L"Usage profile heating missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  if (m_pBuildingProperties->UsageProfileCooling != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileCooling->Name) == false)
    {
      IfcDB::UOM* pUOM = m_pDB->getUomList()->getUOMFromName(_T("DEGREE_CELSIUS"));
      IfcDB::Feature* pDailyPatternSchedule = createDailyPatternSchedule(energyADEExtender, pUsageZone, m_pBuildingProperties->UsageProfileCooling->Periods,
                                                                         cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileCooling->Name),
                                                                         cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileCooling->Description), _T("Nominal temperature cooling"), pUOM);
      m_pDB->createObjRelation(pUsageZone->getOid(), pDailyPatternSchedule->getOid(), _T("energy:coolingSchedule"), pUsageZone->getModelInfo());

      energyADEExtender.registerHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileCooling->Name), pDailyPatternSchedule);
      m_CreatedEnrichmentComponents->Add(m_pBuildingProperties->UsageProfileCooling->Name);
    }
    else
    {
      IfcDB::utils::EnergyAdeExtender::HRef hRef = energyADEExtender.getHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileCooling->Name));
      energyADEExtender.createHRef(pUsageZone, _T("energy:coolingSchedule"), hRef);
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_006"), L"Usage profile cooling missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  if (m_pBuildingProperties->UsageProfileHeatGainElectricalDevices != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileHeatGainElectricalDevices->Name) == false)
    {
      IfcDB::Feature* pElectricalAppliances = createElectricalAppliances(energyADEExtender, pUsageZone, m_pBuildingProperties->UsageProfileHeatGainElectricalDevices);

      energyADEExtender.registerHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeatGainElectricalDevices->Name), pElectricalAppliances);
      m_CreatedEnrichmentComponents->Add(m_pBuildingProperties->UsageProfileHeatGainElectricalDevices->Name);
    }
    else
    {
      IfcDB::utils::EnergyAdeExtender::HRef hRef = energyADEExtender.getHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeatGainElectricalDevices->Name));
      energyADEExtender.createHRef(pUsageZone, _T("energy:equippedWith"), hRef);
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_007"), L"Usage profile electrical devices missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  if (m_pBuildingProperties->UsageProfileVentilation != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileVentilation->Name) == false)
    {
      IfcDB::UOM* pUOM = m_pDB->getUomList()->getUOMFromName(_T("AIR_CHANGE_RATE_HOUR"));
      IfcDB::Feature* pDailyPatternSchedule = createDailyPatternSchedule(energyADEExtender, pUsageZone, m_pBuildingProperties->UsageProfileVentilation->Periods,
                                                                         cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileVentilation->Name),
                                                                         cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileVentilation->Description), _T("Nominal ventilation flow rate"), pUOM);
      m_pDB->createObjRelation(pUsageZone->getOid(), pDailyPatternSchedule->getOid(), _T("energy:ventilationSchedule"), pUsageZone->getModelInfo());

      energyADEExtender.registerHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileVentilation->Name), pDailyPatternSchedule);
      m_CreatedEnrichmentComponents->Add(m_pBuildingProperties->UsageProfileVentilation->Name);
    }
    else
    {
      IfcDB::utils::EnergyAdeExtender::HRef hRef = energyADEExtender.getHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileVentilation->Name));
      energyADEExtender.createHRef(pUsageZone, _T("energy:ventilationSchedule"), hRef);
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_008"), L"Usage profile ventilation missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  if (m_pBuildingProperties->UsageProfileLighting != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileLighting->Name) == false)
    {
      IfcDB::Feature* pLightingFacilities = createLightingFacilities(energyADEExtender, pUsageZone, m_pBuildingProperties->UsageProfileLighting);

      energyADEExtender.registerHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileLighting->Name), pLightingFacilities);
      m_CreatedEnrichmentComponents->Add(m_pBuildingProperties->UsageProfileLighting->Name);
    }
    else
    {
      IfcDB::utils::EnergyAdeExtender::HRef hRef = energyADEExtender.getHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileLighting->Name));
      energyADEExtender.createHRef(pUsageZone, _T("energy:equippedWith"), hRef);
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_009"), L"Usage profile lighting missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  if (m_pBuildingProperties->UsageProfileHeatGainOccupants != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileHeatGainOccupants->Name) == false)
    {
      IfcDB::Feature* pOccupants = createOccupants(energyADEExtender, pUsageZone);

      energyADEExtender.registerHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeatGainOccupants->Name), pOccupants);
      m_CreatedEnrichmentComponents->Add(m_pBuildingProperties->UsageProfileHeatGainOccupants->Name);
    }
    else
    {
      IfcDB::utils::EnergyAdeExtender::HRef hRef = energyADEExtender.getHRef(cDotNetHelper::FromString(m_pBuildingProperties->UsageProfileHeatGainOccupants->Name));
      energyADEExtender.createHRef(pUsageZone, _T("energy:occupiedBy"), hRef);
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_010"), L"Usage profile Occupants missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  /*
  if (m_pBuildingProperties->UsageProfileServiceHours != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileServiceHours->Name) == false)
    {
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_011"), L"Usage profile service hours missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }

  if (m_pBuildingProperties->UsageProfileShading != nullptr)
  {
    if (m_CreatedEnrichmentComponents->Contains(m_pBuildingProperties->UsageProfileShading->Name) == false)
    {
    }
  }
  else
  {
    IfcDB::Message_Object* pMessage = new IfcDB::Message_Object(IfcDB::MT_ERROR, _T("EnergyAdeEnrichment_012"), L"Usage profile shading missing", pUsageZone->getOid());
    m_pDB->addErrorMessage(pMessage);
  }
  */
}

IfcDB::Feature* EnergyAdeEnrichmentDataUpdater::createDailyPatternSchedule(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone, System::Collections::Generic::List<KIT::BuW::AdvEnrichment::Period^>^ periods, const std::wstring& name, const std::wstring& description, const std::wstring& thematicDescription, IfcDB::UOM* pUOM)
{
  IfcDB::Feature* pDailyPatternSchedule = energyADEExtender.createDailyPatternSchedule(pUsageZone, name, description);

  for each (Period ^ period in periods)
  {
    IfcDB::TimePeriod* pTimePeriod = energyADEExtender.createTimePeriod(2022, period->StartMonth, period->StartDay, 2022, period->EndMonth, period->EndDay);
    IfcDB::Feature* pPeriodOfYear = energyADEExtender.createPeriodOfYear(pDailyPatternSchedule, pTimePeriod);

    for each (TimeSeries ^ timeSeries in period->TimeSeries)
    {
      std::wstring dayType = cDotNetHelper::FromString(System::Enum::GetName(KIT::BuW::AdvEnrichment::TimeSeries::EDayType::typeid, timeSeries->DayType));

      std::vector<double> values;

      for each (double value in timeSeries->Values->Values)
      {
        values.emplace_back(value);
      }

      IfcDB::DailyTimeSeries* pDailyTimeSeries = energyADEExtender.createDailyTimeSeries(dayType, values);
      pDailyTimeSeries->setAquisitionMethod(_T("estimation"));
      pDailyTimeSeries->m_minValue = timeSeries->MinValue;
      pDailyTimeSeries->m_maxValue = timeSeries->MaxValue;
      pDailyTimeSeries->m_defaultValue = 0.5 * (timeSeries->MinValue + timeSeries->MaxValue);
      pDailyTimeSeries->m_dayType = IfcDB::DailyTimeSeries::getDayTypFromString(dayType);

      IfcDB::TimeIntervalLength* pTimeIntervalLength = new IfcDB::TimeIntervalLength();
      pTimeIntervalLength->setValue(1);
      pTimeIntervalLength->setUnit(IfcDB::TimeIntervalLength::HOUR);
      pDailyTimeSeries->setTimeIntervalLength(pTimeIntervalLength);

      pDailyTimeSeries->setThematicDescription(thematicDescription);
      pDailyTimeSeries->setUOM(pUOM);

      IfcDB::Feature* pDailySchedule = energyADEExtender.createDailySchedule(pPeriodOfYear, dayType, pDailyTimeSeries);
    }
  }

  return pDailyPatternSchedule;
}

IfcDB::Feature* EnergyAdeEnrichmentDataUpdater::createElectricalAppliances(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone, KIT::BuW::AdvEnrichment::UsageProfileDevices^ usageProfileDevices)
{
  IfcDB::Feature* pElectricalAppliances = energyADEExtender.createElectricalAppliances(pUsageZone, cDotNetHelper::FromString(usageProfileDevices->Name), usageProfileDevices->ConvectionFraction, usageProfileDevices->SpecificRatedThermalCapacity);

  IfcDB::UOM* pUOM = m_pDB->getUomList()->getUOMFromName(_T("SCALE"));

  IfcDB::Feature* pDailyPatternSchedule = createDailyPatternSchedule(energyADEExtender, pElectricalAppliances, usageProfileDevices->Periods, cDotNetHelper::FromString(usageProfileDevices->Name),
                                                                     cDotNetHelper::FromString(usageProfileDevices->Description), _T("Facility usage"), pUOM);
  m_pDB->createObjRelation(pElectricalAppliances->getOid(), pDailyPatternSchedule->getOid(), _T("energy:operationSchedule"), pUsageZone->getModelInfo());

  return pElectricalAppliances;
}

IfcDB::Feature* EnergyAdeEnrichmentDataUpdater::createLightingFacilities(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone, KIT::BuW::AdvEnrichment::UsageProfileLighting^ usageProfileLighting)
{
  IfcDB::Feature* pLightingFacilities = energyADEExtender.createLightingFacilities(pUsageZone, cDotNetHelper::FromString(usageProfileLighting->Name), usageProfileLighting->ConvectionFraction, usageProfileLighting->SpecificRatedThermalCapacity, usageProfileLighting->SpecificRatedLevelLux, usageProfileLighting->Efficiency);

  IfcDB::UOM* pUOM = m_pDB->getUomList()->getUOMFromName(_T("SCALE"));

  IfcDB::Feature* pDailyPatternSchedule = createDailyPatternSchedule(energyADEExtender, pLightingFacilities, usageProfileLighting->Periods, cDotNetHelper::FromString(usageProfileLighting->Name),
                                                                     cDotNetHelper::FromString(usageProfileLighting->Description), _T("Lighting usage"), pUOM);
  m_pDB->createObjRelation(pLightingFacilities->getOid(), pDailyPatternSchedule->getOid(), _T("energy:operationSchedule"), pUsageZone->getModelInfo());

  return pLightingFacilities;
}

IfcDB::Feature* EnergyAdeEnrichmentDataUpdater::createOccupants(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone)
{
  KIT::BuW::AdvEnrichment::UsageProfileOccupancy^ usageProfileOccupancy = m_pBuildingProperties->UsageProfileHeatGainOccupants;

  double maxPerson(0);

  if (m_pBuildingProperties->NetFloorArea.HasValue == true || m_pBuildingProperties->GrossFloorArea.HasValue == true)
  {
    if (usageProfileOccupancy->SquareMeterPerPerson > 0.0)
    {
      if (m_pBuildingProperties->singleThermalZone == true && m_pBuildingProperties->NetFloorArea.HasValue)
      {
        maxPerson = m_pBuildingProperties->NetFloorArea.Value / usageProfileOccupancy->SquareMeterPerPerson;
      }
    }
    else if (usageProfileOccupancy->NoOfOccupants > 0)
    {
      maxPerson = usageProfileOccupancy->NoOfOccupants;
    }
  }

  IfcDB::Feature* pOccupants = energyADEExtender.createOccupants(pUsageZone, cDotNetHelper::FromString(usageProfileOccupancy->Name), usageProfileOccupancy->ConvectionFraction, usageProfileOccupancy->HeatEmissionPerPerson, int(maxPerson + 0.5), usageProfileOccupancy->SquareMeterPerPerson);

  IfcDB::UOM* pUOM = m_pDB->getUomList()->getUOMFromName(_T("SCALE"));

  IfcDB::Feature* pDailyPatternSchedule = createDailyPatternSchedule(energyADEExtender, pOccupants, usageProfileOccupancy->Periods, cDotNetHelper::FromString(usageProfileOccupancy->Name),
                                                                     cDotNetHelper::FromString(usageProfileOccupancy->Description), _T("Presence of occupants"), pUOM);
  m_pDB->createObjRelation(pOccupants->getOid(), pDailyPatternSchedule->getOid(), _T("energy:occupancyRate"), pUsageZone->getModelInfo());

  return pOccupants;
}
