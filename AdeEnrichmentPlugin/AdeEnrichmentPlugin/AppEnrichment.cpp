#include "AdeEnrichmentPlugin.h"
#include "AppEnrichment.h"
#include "EnergyAdeEnrichment.h"
#include "IfcEnrichment.h"
#include "GbxmlEnrichment.h"
#include "DotNetHelper.h"
#include "DataAnalyzer.h"
#include <utils/HeatTransmission.h>

#define JC_VORONOI_IMPLEMENTATION
#include <voronoi/src/jc_voronoi.h>
#include <msclr/marshal_cppstd.h>
#include <vcclr.h>

using namespace KIT::BuW::AdvEnrichment;


///************************************************************************/
///* MessageLoggerProxy                                                   */
///************************************************************************/
void MessageLoggerProxy::Write(System::String^ sMsg)
{
  if (m_pLogger)
  {
    std::string msg = msclr::interop::marshal_as<std::string>(sMsg);
    m_pLogger->log("Comment", msg);
  }
}

void MessageLoggerProxy::WriteWarning(System::String^ sMsg)
{
  if (m_pLogger)
  {
    std::string msg = msclr::interop::marshal_as<std::string>(sMsg);
    m_pLogger->log(sdm::plugin::MessageLevel::Warning, "Warning", msg);
  }
}

void MessageLoggerProxy::WriteError(System::String^ sMsg)
{
  if (m_pLogger)
  {
    std::string msg = msclr::interop::marshal_as<std::string>(sMsg);
    m_pLogger->log(sdm::plugin::MessageLevel::Error, "Error", msg);
  }
}


/************************************************************************/
/* DataUpdateProxy                                                      */
/************************************************************************/
AppEnrichmentDataUpdater::AppEnrichmentDataUpdater(DataFactoryProxy^ pFactory, IfcDB::Populationi* pDB, IfcDB::utils::PopulationStates* pPopStates, sdm::plugin::MainFrameInterface* pMainFrameInterface)
  : m_pFactory(pFactory), m_pDB(pDB), m_pPopStates(pPopStates), m_pMainFrameInterface(pMainFrameInterface)
{
  createNeighboringInfo();
}

AppEnrichmentDataUpdater::~AppEnrichmentDataUpdater()
{
}

System::ApplicationException^ AppEnrichmentDataUpdater::UpdateBuildingProperties(System::Collections::Generic::IList<AdeBuildingProperties^>^ changedAdeBuildingProperties)
{
  for each (auto pBuildingProperties in changedAdeBuildingProperties)
  {
    if (pBuildingProperties->enrichBuildingData == false)
    {
      continue;
    }

    IfcDB::IfcEntity* pBuilding = m_pDB->get(cDotNetHelper::FromString(pBuildingProperties->Guid));

    if (pBuilding != nullptr)
    {
      if (pBuilding->getModelInfo()->m_ModelInfoType == IfcDB::ModelInfo::MT_GML)
      {
        EnergyAdeEnrichmentDataUpdater enrichmentUpdater(pBuildingProperties, m_pDB, dynamic_cast<IfcDB::Feature*>(pBuilding));
        enrichmentUpdater.updateBuildingParameter();
        enrichmentUpdater.enrichBuilding();
      }
      else if (pBuilding->getModelInfo()->m_ModelInfoType == IfcDB::ModelInfo::MT_IFC)
      {
        IfcEnrichmentDataUpdater enrichmentUpdater(pBuildingProperties, m_pDB, pBuilding);
        enrichmentUpdater.updateBuildingParameter();
        enrichmentUpdater.enrichBuilding();
      }
      else if (pBuilding->getModelInfo()->m_ModelInfoType == IfcDB::ModelInfo::MT_GBXML)
      {
        GbxmlEnrichmentDataUpdater enrichmentUpdater(pBuildingProperties, m_pDB, pBuilding);
        enrichmentUpdater.updateBuildingParameter();
        enrichmentUpdater.enrichBuilding();
      }
      else
      {
        throw std::logic_error("not implemented");
      }
    }
  }

  m_pDB->incrementSerialNo();

  m_pPopStates->compile();

  m_pMainFrameInterface->setSaveFileFlag(true);

  return nullptr;
}

void AppEnrichmentDataUpdater::createNeighboringInfo()
{
  IfcDB::IfcEntityList buildings;
  m_pDB->getList(IfcDB::CITYGML_BUILDING, buildings);

  std::vector<jcv_point> points;
  std::vector<IfcDB::NeighboringInfo> neighboringInfos;

  for (auto pBuilding : buildings)
  {
    if (m_pDB->getNeighboringInfo(pBuilding->getOid()) != nullptr)
    {
      continue;
    }

    IfcDB::Feature* pBuildingFeature = dynamic_cast<IfcDB::Feature*>(pBuilding);

    if (pBuildingFeature == nullptr)
    {
      continue;
    }

    IfcDB::Point centroid;
    double volume(0.0);

    // try to get centroid from building
    IfcDB::Geometry* pGeometryBuilding = pBuildingFeature->getGeometry();

    if (pGeometryBuilding != nullptr)
    {
      IfcDB::Geometry::calcCentroid(pGeometryBuilding, centroid, volume);
    }

    // try to get centroid from building parts
    if (volume == 0.0)
    {
      std::vector<IfcDB::Feature*> buildingParts;
      m_pDB->getRelatedFeatures(pBuildingFeature, _T("bldg:consistsOfBuildingPart"), buildingParts);

      for (auto pBuildingPart : buildingParts)
      {
        IfcDB::Geometry* pGeometryBuildingPart = pBuildingPart->getGeometry();

        if (pGeometryBuildingPart != nullptr)
        {
          double volumeGeom(0.0);
          IfcDB::Point centroidGeom;

          IfcDB::Geometry::calcCentroid(pGeometryBuildingPart, centroidGeom, volumeGeom);
          volume += volumeGeom;
          centroid += (centroidGeom * volumeGeom);
        }
      }

      centroid /= volume;
    }

    // try to get centroid from thermal zones
    if (volume == 0.0)
    {
      std::vector<IfcDB::Feature*> vThermalBoundaries;

      std::vector<IfcDB::Feature*> vZones;
      m_pDB->getRelatedFeatures(pBuildingFeature, _T("energy:thermalZone"), vZones);

      for (auto pZone : vZones)
      {
        IfcDB::Geometry* pGeometryZone = pZone->getGeometry();

        if (pGeometryZone != nullptr)
        {
          double volumeGeom(0.0);
          IfcDB::Point centroidGeom;

          IfcDB::Geometry::calcCentroid(pGeometryZone, centroidGeom, volumeGeom);
          volume += volumeGeom;
          centroid += (centroidGeom * volumeGeom);
        }
      }

      centroid /= volume;
    }

    if (volume > 0.0)
    {
      points.emplace_back(jcv_point{ (jcv_real)centroid.x, (jcv_real)centroid.y });
      neighboringInfos.emplace_back(IfcDB::NeighboringInfo(pBuilding->getOid(), centroid));
    }
  }

  jcv_diagram diagram;
  memset(&diagram, 0, sizeof(jcv_diagram));
  jcv_diagram_generate((int)points.size(), points.data(), 0, 0, &diagram);

  const jcv_edge* edge = jcv_diagram_get_edges(&diagram);
  while (edge)
  {
    edge = jcv_diagram_get_next_edge(edge);
  }

  const jcv_site* sites = jcv_diagram_get_sites(&diagram);
  for (int i = 0; i < diagram.numsites; ++i)
  {
    const jcv_site* site = &sites[i];

    IfcDB::NeighboringInfo neighboringInfo = neighboringInfos[site->index];

    std::vector<const jcv_site*> neighbors;
    std::unordered_set<const jcv_site*> ignoreList;

    ignoreList.emplace(site);

    double maxDist;

    const jcv_graphedge* edge = site->edges;
    while (edge)
    {
      if (edge->neighbor != nullptr)
      {
        neighbors.emplace_back(edge->neighbor);
        ignoreList.emplace(edge->neighbor);

        neighboringInfo.neighbors.emplace_back(neighboringInfos[edge->neighbor->index].oid);

        double currentDist = IfcDB::lengthOf(neighboringInfo.centroid - IfcDB::Point(edge->neighbor->p.x, edge->neighbor->p.y));
        maxDist = std::max(maxDist, currentDist);
      }

      edge = edge->next;
    }

    // identify 2nd level neighbors
    for (auto pNeighbor: neighbors)
    {
      const jcv_graphedge* neighborEdge = pNeighbor->edges;
      while (neighborEdge)
      {
        if (neighborEdge->neighbor != nullptr)
        {
          auto it = ignoreList.find(neighborEdge->neighbor);

          if (it == ignoreList.end())
          {
            double neighborDist = IfcDB::lengthOf(neighboringInfo.centroid - IfcDB::Point(neighborEdge->neighbor->p.x, neighborEdge->neighbor->p.y));

            if (neighborDist < maxDist)
            {
              ignoreList.emplace(neighborEdge->neighbor);
              neighboringInfo.neighbors.emplace_back(neighboringInfos[neighborEdge->neighbor->index].oid);
            }
          }
        }

        neighborEdge = neighborEdge->next;
      }
    }

    m_pDB->addNeighboringInfo(neighboringInfo);
  }

  jcv_diagram_free(&diagram);

/*
  // for debugging with geometry creation
  IfcDB::MultiPoint* pMultiPoint = nullptr;
  IfcDB::SetOfCurve* pSetOfCurve = nullptr;
  IfcDB::SetOfGeometry* pSetOfGeometry = nullptr;
  IfcDB::Face* pFace = nullptr;
  IfcDB::Polyloop* pPolyloop = nullptr;

  IfcDB::IfcEntityList buildings;
  m_pDB->getList(IfcDB::CITYGML_BUILDING, buildings);

  std::vector<jcv_point> points;
  std::vector<IfcDB::NeighboringInfo> neighboringInfos;

  for (auto pBuilding : buildings)
  {
    if (m_pDB->getNeighboringInfo(pBuilding->getOid()) != nullptr)
    {
      continue;
    }

    IfcDB::Feature* pBuildingFeature = dynamic_cast<IfcDB::Feature*>(pBuilding);

    if (pBuildingFeature == nullptr)
    {
      continue;
    }

    IfcDB::Point centroid;
    double volume(0.0);

    // try to get centroid from building
    IfcDB::Geometry* pGeometryBuilding = pBuildingFeature->getGeometry();

    if (pGeometryBuilding != nullptr)
    {
      IfcDB::Geometry::calcCentroid(pGeometryBuilding, centroid, volume);
    }

    // try to get centroid from thermal zones
    if (volume == 0.0)
    {
      std::vector<IfcDB::Feature*> vThermalBoundaries;

      std::vector<IfcDB::Feature*> vZones;
      m_pDB->getRelatedFeatures(pBuildingFeature, _T("energy:thermalZone"), vZones);

      for (auto pZone : vZones)
      {
        IfcDB::Geometry* pGeometryZone = pZone->getGeometry();

        if (pGeometryZone != nullptr)
        {
          double volumeGeom(0.0);
          IfcDB::Point centroidGeom;

          IfcDB::Geometry::calcCentroid(pGeometryZone, centroidGeom, volumeGeom);
          volume += volumeGeom;
          centroid += (centroidGeom * volumeGeom);
        }
      }

      centroid /= volume;
    }

    if (volume > 0.0)
    {
      points.emplace_back(jcv_point{ (jcv_real)centroid.x, (jcv_real)centroid.y });
      neighboringInfos.emplace_back(IfcDB::NeighboringInfo(pBuilding->getOid(), centroid));
    }
  }

  jcv_diagram diagram;
  memset(&diagram, 0, sizeof(jcv_diagram));
  jcv_diagram_generate((int)points.size(), points.data(), 0, 0, &diagram);

  pSetOfCurve = new IfcDB::SetOfCurve();

  const jcv_edge* edge = jcv_diagram_get_edges(&diagram);
  while (edge)
  {
    IfcDB::Line* pLine = new IfcDB::Line(IfcDB::Point(edge->pos[0].x, edge->pos[0].y), IfcDB::Point(edge->pos[1].x, edge->pos[1].y));
    pSetOfCurve->addCurve(pLine);

    edge = jcv_diagram_get_next_edge(edge);
  }

  pMultiPoint = new IfcDB::MultiPoint();
  pSetOfGeometry = new IfcDB::SetOfGeometry();

  const jcv_site* sites = jcv_diagram_get_sites(&diagram);
  for (int i = 0; i < diagram.numsites; ++i)
  {
    const jcv_site* site = &sites[i];

    IfcDB::NeighboringInfo& neighboringInfo = neighboringInfos[site->index];

    pMultiPoint->addGeometry(new IfcDB::cVertex(neighboringInfo.centroid));

    pFace = new IfcDB::Face();
    pPolyloop = new IfcDB::Polyloop();

    std::vector<const jcv_site*> neighbors;
    std::unordered_set<const jcv_site*> ignoreList;

    ignoreList.emplace(site);

    double maxDist;

    const jcv_graphedge* edge = site->edges;
    while (edge)
    {
      pPolyloop->addPoint(edge->pos[0].x, edge->pos[0].y);

      if (edge->neighbor != nullptr)
      {
        neighbors.emplace_back(edge->neighbor);
        ignoreList.emplace(edge->neighbor);

        neighboringInfo.neighbors.emplace_back(neighboringInfos[edge->neighbor->index].oid);

        double currentDist = IfcDB::lengthOf(neighboringInfo.centroid - IfcDB::Point(edge->neighbor->p.x, edge->neighbor->p.y));
        maxDist = std::max(maxDist, currentDist);
      }

      edge = edge->next;
    }

    // identify 2nd level neighbors
    for (auto pNeighbor: neighbors)
    {
      const jcv_graphedge* neighborEdge = pNeighbor->edges;
      while (neighborEdge)
      {
        if (neighborEdge->neighbor != nullptr)
        {
          auto it = ignoreList.find(neighborEdge->neighbor);

          if (it == ignoreList.end())
          {
            double neighborDist = IfcDB::lengthOf(neighboringInfo.centroid - IfcDB::Point(neighborEdge->neighbor->p.x, neighborEdge->neighbor->p.y));

            if (neighborDist < maxDist)
            {
              ignoreList.emplace(neighborEdge->neighbor);
              neighboringInfo.neighbors.emplace_back(neighboringInfos[neighborEdge->neighbor->index].oid);
            }
          }
        }

        neighborEdge = neighborEdge->next;
      }
    }

    pFace->addOuterLoop(pPolyloop);
    pSetOfGeometry->addGeometry(pFace);

    m_pDB->addNeighboringInfo(neighboringInfo);
  }

  IfcDB::IfcEntity* pEntity = new IfcDB::IfcEntity();
  pEntity->setModelInfo(pEntity->getModelInfo());
  m_pDB->add(pEntity);

  IfcDB::sRepresentation representationSite(_T("Voronoi_Site"), _T("Face"), pSetOfGeometry);
  pEntity->addRepresentation(representationSite);

  IfcDB::sRepresentation representationEdge(_T("Voronoi_Edge"), _T("Line"), pSetOfCurve);
  pEntity->addRepresentation(representationEdge);

  IfcDB::sRepresentation representationPoint(_T("Voronoi_Point"), _T("Point"), pMultiPoint);
  pEntity->addRepresentation(representationPoint);

  pEntity->setState(IfcDB::STATE_INITIAL_GEOMETRY_TRANSFORMATION);

  CMasterDoc* pDoc = m_pFactory->getDocument();

  pDoc->getStates().registerEntityHash(pEntity->getOid(), _T("Voronoi_Site"), _T("Face"), {});
  pDoc->getStates().registerEntityHash(pEntity->getOid(), _T("Voronoi_Edge"), _T("Line"), {});
  pDoc->getStates().registerEntityHash(pEntity->getOid(), _T("Voronoi_Point"), _T("Point"), {});

  jcv_diagram_free(&diagram);
*/
}

/************************************************************************/
/* AppEnrichmentFactory                                                */
/************************************************************************/
//AppEnrichmentFactory::AppEnrichmentFactory(/*CMasterDoc* pDoc,*/ IfcDB::Populationi* pDB)
//  : /*m_pDoc(pDoc),*/ m_pDB(pDB)
//{
//  m_pApplicationSettings = gcnew AdeApplicationSettings();
//  m_pDataSourceProperties = gcnew AdeDataSourceProperties();
//  m_pSelectedBuildings = gcnew System::Collections::Generic::List<System::String^>;
//  //m_pDataLogger = gcnew AppEnrichmentLogger();
//  //m_pDataUpdater = gcnew AppEnrichmentDataUpdater(this, m_pDB);
//
//  m_pApplicationSettings->DataDirectory = cDotNetHelper::ToString(m_pDB->getDataPath()) + _T("\\ADE_Enrichment\\");
//}

//AppEnrichmentFactory::~AppEnrichmentFactory()
//{
//}

KIT::BuW::AdvEnrichment::AdeBuildingProperties^ DataFactoryProxy::GetBuildingProperties(System::String^ id)
{
  IfcDB::IfcEntity* pBuilding = m_pDB->get(cDotNetHelper::FromString(id));

  if (pBuilding != nullptr)
  {
    return getBuildingData(pBuilding);
  }

  return nullptr;
}

//void AppEnrichmentFactory::updateData()
//{
//  m_pDoc->prepareLists(true);
//  m_pDoc->IncrementChanges();
//  m_pDoc->SetSaveFileFlag(TRUE);
//  m_pDoc->UpdateAllViews(NULL, makeLPARAM(MasterViewUpdateHint::GEOMETRY_CHANGED), NULL);
//}

bool DataFactoryProxy::getData(System::Collections::Generic::List<System::String^>^ sel)
{
    bool bBuildings = getBuildings(sel);

    if (sel->Count > 0)
    {
      DataSourceProperties->HasWindows = false;

      auto pBuildingProperties = GetBuildingProperties(sel[0]);

      if (pBuildingProperties && pBuildingProperties->buildingGeometry->Count > 0 && pBuildingProperties->buildingGeometry[0]->boundaryGeometry->Count > 0)
      {
        auto geoPoint = pBuildingProperties->buildingGeometry[0]->boundaryGeometry[0];

        // identify contry code
        IfcDB::Point latLon(geoPoint->Lat, geoPoint->Lon);
        
        std::vector<std::wstring> results;
        m_pAppFeature->requestGoogleGeocode(latLon, results);

        if (!results.empty())
        {
          std::wstring countryCode = results.front();

          DataSourceProperties->CountryCode = cDotNetHelper::ToString(countryCode);
        }
      }
    }

    std::vector<IfcDB::ModelInfo*> models;
    m_pDB->getModelInfo(models);

    bool bSupportedModels(false);

    // identify supported model
    for (auto pModel : models)
    {
        if (pModel->m_ModelInfoType == IfcDB::ModelInfo::MT_GML)
        {
            bSupportedModels = true;

            if (System::String::IsNullOrEmpty(DataSourceProperties->SourceName) == false)
            {
                DataSourceProperties->SourceName += _T(", ");
            }

            if (System::String::IsNullOrEmpty(DataSourceProperties->Description) == false)
            {
                DataSourceProperties->Description += _T(", ");
            }

            DataSourceProperties->SourceName += cDotNetHelper::ToString(pModel->m_ModelName);
            DataSourceProperties->Description += cDotNetHelper::ToString(pModel->m_ModelLocation);
        }
    }

    DataSourceProperties->NumItems = sel->Count;

    return (bSupportedModels && bBuildings);
}

bool DataFactoryProxy::getBuildings(System::Collections::Generic::List<System::String^>^ sel)
{
    IfcDB::IfcEntityList buildings;
    IfcDB::IfcEntityList entities;

    auto selctedEntities = m_pPopStates->getSelectedEntities();

    for (auto oid : selctedEntities)
    {
      entities.push_back(m_pDB->get(oid));
    }

    //// Identify context
    //States::Context context(States::Context::All_Entities);

    //States& states = m_pDoc->getStates();

    //if (states.hasImageSelection() == true)
    //{
    //  context = States::Context::Selected_Entities;
    //}
    //else if (states.getSelectionStates().treeSelection.empty() == false)
    //{
    //  context = States::Context::Visible_Entities;
    //}

    //m_pDoc->getStates().getEntitiesInContext(context, entities);

    if (entities.empty() == false)
    {
        std::set<IfcDB::ifcOid> buildingsSet;

        for (auto pEntity : entities)
        {
            IfcDB::ifcOid buildingOid = 0;

            IfcDB::ifcOid oid = pEntity->getOid();

            if (pEntity &&
                (pEntity->isTypeOf(IfcDB::IFC_BUILDING)     ||
                 pEntity->isTypeOf(IfcDB::CITYGML_BUILDING) ||
                 pEntity->isTypeOf(IfcDB::GBXML_BUILDING)))
            {
                buildingsSet.insert(oid);
            }
            else if (m_pDB->hasEntityParentOfType(IfcDB::IFC_BUILDING, oid, buildingOid) == true     ||
                     m_pDB->hasEntityParentOfType(IfcDB::CITYGML_BUILDING, oid, buildingOid) == true ||
                     m_pDB->hasEntityParentOfType(IfcDB::GBXML_BUILDING, oid, buildingOid) == true)
            {
                buildingsSet.insert(buildingOid);
            }
        }

        for (auto oid : buildingsSet)
        {
            buildings.push_back(m_pDB->get(oid));
        }
    }
    else
    {
        m_pDB->getList(IfcDB::CITYGML_BUILDING, buildings);
        m_pDB->getList(IfcDB::IFC_BUILDING, buildings);
        m_pDB->getList(IfcDB::GBXML_BUILDING, buildings);
    }

    for (auto pBuilding : buildings)
    {
        std::wstring guid = pBuilding->getGuid();
        sel->Add(cDotNetHelper::ToString(guid));

        // collect BuildingParts
        IfcDB::Feature* pBuildingFeature = dynamic_cast<IfcDB::Feature*>(pBuilding);

        if (pBuildingFeature != nullptr)
        {
            std::vector<IfcDB::Feature*> buildingParts;
            m_pDB->getRelatedFeatures(pBuildingFeature, _T("bldg:consistsOfBuildingPart"), buildingParts);

            for (auto pBuildingPart : buildingParts)
            {
                std::wstring guidPart = pBuildingPart->getGuid();
                sel->Add(cDotNetHelper::ToString(guidPart));
            }
        }
    }

    return (sel->Count > 0);
}

AdeBuildingProperties^ DataFactoryProxy::getBuildingData(IfcDB::IfcEntity* pEntity)
{
  KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties = gcnew KIT::BuW::AdvEnrichment::AdeBuildingProperties();

  pBuildingProperties->CountryCode = DataSourceProperties->CountryCode;

  if (pEntity->getModelInfo()->m_ModelInfoType == IfcDB::ModelInfo::MT_GML)
  {
    IfcDB::IfcEntityList windows;
    if (!DataSourceProperties->HasWindows)
    {
      m_pDB->getChildren(IfcDB::CITYGML_WINDOW, pEntity->getOid(), windows);

      if (!DataSourceProperties->HasWindows)
      {
        DataSourceProperties->HasWindows = true;
      }
    }

    pBuildingProperties->HasWindows = DataSourceProperties->HasWindows;

    EnergyAdeEnrichment enrichment(pBuildingProperties, m_pDB, dynamic_cast<IfcDB::Feature*>(pEntity));
    enrichment.gatherBuildingParameter();
  }
  else if (pEntity->getModelInfo()->m_ModelInfoType == IfcDB::ModelInfo::MT_IFC)
  {
    IfcEnrichment enrichment(pBuildingProperties, m_pDB, pEntity);
    enrichment.gatherBuildingParameter();
  }
  else if (pEntity->getModelInfo()->m_ModelInfoType == IfcDB::ModelInfo::MT_GBXML)
  {
    GbxmlEnrichment enrichment(pBuildingProperties, m_pDB, pEntity);
    enrichment.gatherBuildingParameter();
  }
  else
  {
    throw std::logic_error("not implemented");
  }

  return pBuildingProperties;
}

//BuildingSimulation::Period* UsageProfileFactory::createPerion(Period^ period)
//{
//  BuildingSimulation::Period* pPeriod = new BuildingSimulation::Period();
//  pPeriod->m_startDay   = period->StartDay;
//  pPeriod->m_startMonth = period->StartMonth;
//  pPeriod->m_endDay     = period->EndDay;
//  pPeriod->m_endMonth   = period->EndMonth;
//
//  for each (TimeSeries^ timeSeries in period->TimeSeries)
//  {
//    BuildingSimulation::TimeSeries* pTimeSeries = createTimeSeries(timeSeries);
//    pPeriod->m_timeSeries.emplace_back(pTimeSeries);
//  }
//
//  return pPeriod;
//}
//
//BuildingSimulation::TimeSeries* UsageProfileFactory::createTimeSeries(TimeSeries^ timeSeries)
//{
//  BuildingSimulation::TimeSeries* pTimeSeries = new BuildingSimulation::TimeSeries();
//  pTimeSeries->m_minValue = timeSeries->MinValue;
//  pTimeSeries->m_maxValue = timeSeries->MaxValue;
//  pTimeSeries->m_defaultValue = timeSeries->DefaultValue;
//  pTimeSeries->m_dayType = IfcDB::DailyTimeSeries::getDayTypFromString(cDotNetHelper::FromString(timeSeries->DayType.ToString()));
//
//  for each (double value in timeSeries->Values->Values)
//  {
//    pTimeSeries->m_values.emplace_back(BuildingSimulation::HourValue(value, true));
//  }
//
//  return pTimeSeries;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createHeatingProfile(UsageProfileHeating^ usageProfileHeating)
//{
//  BuildingSimulation::UsageProfileHeating* pHeatingProfile = new BuildingSimulation::UsageProfileHeating();
//  pHeatingProfile->m_name = cDotNetHelper::FromString(usageProfileHeating->Name);
//  pHeatingProfile->m_description = cDotNetHelper::FromString(usageProfileHeating->Description);
//
//  for each (Period^ period in usageProfileHeating->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pHeatingProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pHeatingProfile;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createCoolingProfile(UsageProfileCooling^ usageProfileCooling)
//{
//  BuildingSimulation::UsageProfileCooling* pCoolingProfile = new BuildingSimulation::UsageProfileCooling();
//  pCoolingProfile->m_name = cDotNetHelper::FromString(usageProfileCooling->Name);
//  pCoolingProfile->m_description = cDotNetHelper::FromString(usageProfileCooling->Description);
//
//  for each (Period^ period in usageProfileCooling->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pCoolingProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pCoolingProfile;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createVentilationProfile(UsageProfileVentilation^ usageProfileVentilation)
//{
//  BuildingSimulation::UsageProfileVentilation* pVentilationProfile = new BuildingSimulation::UsageProfileVentilation();
//  pVentilationProfile->m_name = cDotNetHelper::FromString(usageProfileVentilation->Name);
//  pVentilationProfile->m_description = cDotNetHelper::FromString(usageProfileVentilation->Description);
//
//  for each (Period^ period in usageProfileVentilation->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pVentilationProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pVentilationProfile;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createSunShadingProfile(UsageProfileWindowShading^ usageProfileWindowShading)
//{
//  BuildingSimulation::UsageProfileSunShading* pSunShadingProfile = new BuildingSimulation::UsageProfileSunShading();
//  pSunShadingProfile->m_name = cDotNetHelper::FromString(usageProfileWindowShading->Name);
//  pSunShadingProfile->m_description = cDotNetHelper::FromString(usageProfileWindowShading->Description);
//
//  for each (Period^ period in usageProfileWindowShading->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pSunShadingProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pSunShadingProfile;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createLightingProfile(UsageProfileLighting^ usageProfileLighting)
//{
//  BuildingSimulation::UsageProfileLighting* pLightingProfile = new BuildingSimulation::UsageProfileLighting();
//  pLightingProfile->m_name = cDotNetHelper::FromString(usageProfileLighting->Name);
//  pLightingProfile->m_description = cDotNetHelper::FromString(usageProfileLighting->Description);
//
//  for each (Period^ period in usageProfileLighting->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pLightingProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pLightingProfile;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createPersonProfile(UsageProfileOccupancy^ usageProfileOccupancy)
//{
//  BuildingSimulation::UsageProfileOccupancy* pPersonProfile = new BuildingSimulation::UsageProfileOccupancy();
//  pPersonProfile->m_name = cDotNetHelper::FromString(usageProfileOccupancy->Name);
//  pPersonProfile->m_description = cDotNetHelper::FromString(usageProfileOccupancy->Description);
//
//  for each (Period^ period in usageProfileOccupancy->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pPersonProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pPersonProfile;
//}
//
//BuildingSimulation::UsageProfile* UsageProfileFactory::createDeviceProfile(UsageProfileDevices^ usageProfileDevices)
//{
//  BuildingSimulation::UsageProfileDevices* pDevicesProfile = new BuildingSimulation::UsageProfileDevices();
//  pDevicesProfile->m_name = cDotNetHelper::FromString(usageProfileDevices->Name);
//  pDevicesProfile->m_description = cDotNetHelper::FromString(usageProfileDevices->Description);
//
//  for each (Period^ period in usageProfileDevices->Periods)
//  {
//    BuildingSimulation::Period* pPeriod = createPerion(period);
//    pDevicesProfile->m_periods.emplace_back(pPeriod);
//  }
//
//  return pDevicesProfile;
//}
