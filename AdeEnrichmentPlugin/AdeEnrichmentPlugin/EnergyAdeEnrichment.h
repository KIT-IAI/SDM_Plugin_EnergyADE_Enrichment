#pragma once

#include "DataEnrichment.h"
#include <IfcDbInclude.h>

class IfcDB::utils::EnergyAdeExtender;

namespace IfcDB::utils
{
  struct Period;
  struct TimeSeries;
  struct UsageProfile;
  struct Construction;
  class PopulationStates;
}

ref class EnergyAdeEnrichment : public DataEnrichement
{
  public:
    EnergyAdeEnrichment(KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties, IfcDB::Populationi* pDB, IfcDB::Feature* pFeature);

  public:
    void gatherBuildingParameter() override;
    System::String^ createDisplayString(IfcDB::IfcEntity* pEntity) override;

  private:
    void getGeometryStatistic(IfcDB::Feature* pFeature);
    void calculateGeometryInformation();
    bool analyzeRepresentations(IfcDB::IfcEntity* pEntity, double& area, double& volume, double& height);
    void getYearOfContruction(std::wstring& attributeName);
    void getBuildingFunction(std::wstring& attributeName);
    void getStoreysAboveGround(std::wstring& attributeName);
    void getStoreysBelowGround(std::wstring& attributeName);
    void getMeasuredHeight(std::wstring& attributeName);

  private:
    IfcDB::Feature* m_pBuildingFeature = nullptr;
};

public ref class EnergyAdeEnrichmentDataUpdater : public EnrichmentDataUpdater
{
  public:
    EnergyAdeEnrichmentDataUpdater(KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties, IfcDB::Populationi* pDB, IfcDB::Feature* pFeature);

  public:
    void updateBuildingParameter() override;
    void enrichBuilding() override;

  private:
    void setYearOfContruction(IfcDB::Feature* pFeature, const std::wstring& attributeName, int yearOfConstruction);
    void setStringAttribute(IfcDB::Feature* pFeature, const std::wstring& attributeName, System::String^ attributeValue);
    void setDoubleAttribute(IfcDB::Feature* pFeature, const std::wstring& attributeName, double attributeValue, IfcDB::UOM* pUOM);

    void createThermalZone(IfcDB::Feature* pBuilding, IfcDB::utils::EnergyAdeExtender& energyADEExtender);
    bool createDefaultUsageProfiles(std::vector<IfcDB::utils::UsageProfile*>& usageProfiles);
    void createUsageProfiles(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone);
    IfcDB::Feature* createDailyPatternSchedule(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone, System::Collections::Generic::List<KIT::BuW::AdvEnrichment::Period^>^ periods, const std::wstring& name, const std::wstring& description, const std::wstring& thematicDescription, IfcDB::UOM* pUOM);
    IfcDB::Feature* createElectricalAppliances(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone, KIT::BuW::AdvEnrichment::UsageProfileDevices^ usageProfileDevices);
    IfcDB::Feature* createLightingFacilities(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone, KIT::BuW::AdvEnrichment::UsageProfileLighting^ usageProfileLighting);
    IfcDB::Feature* createOccupants(IfcDB::utils::EnergyAdeExtender& energyADEExtender, IfcDB::Feature* pUsageZone);

  private:
    IfcDB::Feature* m_pBuildingFeature = nullptr;
    System::Collections::Generic::List<System::String^>^ m_CreatedEnrichmentComponents;
};
