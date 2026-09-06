#pragma once

ref class KIT::BuW::AdvEnrichment::AdeBuildingProperties;

namespace IfcDB::utils
{
  class PopulationStates;
}

[System::Serializable]
ref class MessageLoggerProxy : public KIT::BuW::AdvEnrichment::IAdeMessageLogger
{
  public:
    // Geerbt über IAdeMessageLogger
    virtual void Write(System::String^ sMsg);
    virtual void WriteWarning(System::String^ sMsg);
    virtual void WriteError(System::String^ sMsg);

    // Implementation
    void setLogger(sdm::plugin::LiveLogInterface* pLogger) { m_pLogger = pLogger; }

  protected:
    sdm::plugin::LiveLogInterface* m_pLogger = nullptr;
};

[System::Serializable]
ref class DataFactoryProxy : public KIT::BuW::AdvEnrichment::IAdeDataFactory
{
  public:
    DataFactoryProxy(IfcDB::Populationi* pDB, IfcDB::utils::PopulationStates* pPopStates, sdm::plugin::AppFeatureHelper* pAppFeature)
      : m_pDB(pDB), m_pPopStates(pPopStates), m_pAppFeature(pAppFeature)
    {
      IfcDB::cSrsManager::setBaseZone(pDB->getGeoReference().getBaseZone());
    };

    // Geerbt über IAdeDataFactory
    virtual property KIT::BuW::AdvEnrichment::AdeDataSourceProperties^ DataSourceProperties;
    virtual property KIT::BuW::AdvEnrichment::AdeApplicationSettings^ ApplicationSettings;
    virtual property System::Collections::Generic::IReadOnlyList<System::String^>^ SelectedBuildings;
    virtual property KIT::BuW::AdvEnrichment::IAdeMessageLogger^ Logger;
    virtual property KIT::BuW::AdvEnrichment::IAdeDataUpdater^ UpdateCallbacks;
    virtual KIT::BuW::AdvEnrichment::AdeBuildingProperties^ GetBuildingProperties(System::String^ id);

    bool getData(System::Collections::Generic::List<System::String^>^ sel);

  protected:
    bool getBuildings(System::Collections::Generic::List<System::String^>^ sel);
    KIT::BuW::AdvEnrichment::AdeBuildingProperties^ DataFactoryProxy::getBuildingData(IfcDB::IfcEntity* pEntity);

  protected:
    IfcDB::Populationi* m_pDB = nullptr;
    IfcDB::utils::PopulationStates* m_pPopStates = nullptr;
    sdm::plugin::AppFeatureHelper* m_pAppFeature = nullptr;
};

//[System::Serializable]
//ref class DataUpdateProxy : public KIT::BuW::AdvEnrichment::IAdeDataUpdater
//{
//public:
//    DataUpdateProxy(DataFactoryProxy^ pFactory, IfcDB::Populationi* pDB);
//
//    // Geerbt über IAdeDataUpdater
//    virtual System::ApplicationException^ UpdateBuildingProperties(System::Collections::Generic::IList<KIT::BuW::AdvEnrichment::AdeBuildingProperties^>^ changedAdeBuildingProperties);
//
//protected:
//    void createNeighboringInfo();
//    void updateBuildingParameter(IfcDB::IfcEntity* pEntity, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);
//    void enrichBuilding(IfcDB::IfcEntity* pEntity, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties, EnergyAdeExtender& energyADEExtender);
//
//protected:
//    DataFactoryProxy^ m_pFactory = nullptr;
//    IfcDB::Populationi* m_pDB = nullptr;
//    System::Collections::Generic::List<System::String^>^ m_CreatedEnrichmentComponents;
//};

[System::Serializable]
ref class AppEnrichmentDataUpdater : public KIT::BuW::AdvEnrichment::IAdeDataUpdater
{
  public:
    AppEnrichmentDataUpdater(/*AppEnrichmentFactory*/DataFactoryProxy^ pFactory, IfcDB::Populationi* pDB, IfcDB::utils::PopulationStates* pPopObserver, sdm::plugin::MainFrameInterface* pMainFrameInterface);
    virtual ~AppEnrichmentDataUpdater();

  public:
    virtual System::ApplicationException^ UpdateBuildingProperties(System::Collections::Generic::IList<KIT::BuW::AdvEnrichment::AdeBuildingProperties^>^ changedAdeBuildingProperties);

  private:
    void createNeighboringInfo();

  private:
    /*AppEnrichmentFactory*/DataFactoryProxy^ m_pFactory = nullptr;
    IfcDB::Populationi* m_pDB = nullptr;
    IfcDB::utils::PopulationStates* m_pPopStates = nullptr;
    sdm::plugin::MainFrameInterface* m_pMainFrameInterface = nullptr;
};

//ref class AppEnrichmentFactory : public KIT::BuW::AdvEnrichment::IAdeDataFactory
//{
//  public:
//    AppEnrichmentFactory(/*CMasterDoc* pDoc,*/ IfcDB::Populationi* pDB);
//    virtual ~AppEnrichmentFactory();
//
//public:
    // Geerbt über IAdeDataFactory
    //property KIT::BuW::AdvEnrichment::AdeApplicationSettings^ ApplicationSettings
    //{
    //  public:
    //    virtual KIT::BuW::AdvEnrichment::AdeApplicationSettings^ get() { return m_pApplicationSettings; }
    //}

    //property KIT::BuW::AdvEnrichment::AdeDataSourceProperties^ DataSourceProperties
    //{
    //  public:
    //    virtual KIT::BuW::AdvEnrichment::AdeDataSourceProperties^ get() { return m_pDataSourceProperties; }
    //}

    //property System::Collections::Generic::IReadOnlyList<System::String^>^ SelectedBuildings
    //{
    //  public:
    //    virtual System::Collections::Generic::IReadOnlyList<System::String^>^ get() { return m_pSelectedBuildings; }
    //}

    //property KIT::BuW::AdvEnrichment::IAdeMessageLogger^ Logger
    //{
    //  public:
    //    virtual KIT::BuW::AdvEnrichment::IAdeMessageLogger^ get() { return m_pDataLogger; }
    //}

    //property KIT::BuW::AdvEnrichment::IAdeDataUpdater^ UpdateCallbacks
    //{
    //  public:
    //    virtual KIT::BuW::AdvEnrichment::IAdeDataUpdater^ get() { return m_pDataUpdater; }
    //}

    //virtual KIT::BuW::AdvEnrichment::AdeBuildingProperties^ GetBuildingProperties(System::String^ id);

//  public:
    //void updateData();
    //bool getData();
    //CMasterDoc* getDocument() { return m_pDoc; }

//  private:
    //bool getBuildings();
    //KIT::BuW::AdvEnrichment::AdeBuildingProperties^ getBuildingData(IfcDB::IfcEntity* pEntity);
    //void getYearOfContruction(IfcDB::Feature* pFeature, std::wstring& attributeName, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);
    //void getBuildingFunction(IfcDB::Feature* pFeature, std::wstring& attributeName, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);
    //void getStoreysAboveGround(IfcDB::Feature* pFeature, std::wstring& attributeName, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);
    //void getStoreysBelowGround(IfcDB::Feature* pFeature, std::wstring& attributeName, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);
    //void getMeasuredHeight(IfcDB::Feature* pFeature, std::wstring& attributeName, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);

    //void getGeometryStatistic(IfcDB::IfcEntity* pEntity, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);
    //void calculateGeometryInformation(IfcDB::Feature* pFeature, KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties);

    //std::wstring createDisplayString(IfcDB::IfcEntity* pEntity);

    //void setBoundaryGeometry(KIT::BuW::AdvEnrichment::AdeBuildingProperties^ pBuildingProperties, const IfcDB::Polylines& polylines);

    //void deletePolylines(IfcDB::Polylines& polylines);

//  private:
    //KIT::BuW::AdvEnrichment::AdeApplicationSettings^ m_pApplicationSettings = nullptr;
    //KIT::BuW::AdvEnrichment::AdeDataSourceProperties^ m_pDataSourceProperties = nullptr;
    //System::Collections::Generic::List<System::String^>^ m_pSelectedBuildings = nullptr;
    //AppEnrichmentLogger^ m_pDataLogger = nullptr;
    //AppEnrichmentDataUpdater^ m_pDataUpdater = nullptr;

    //CMasterDoc* m_pDoc = nullptr;
    //IfcDB::Populationi* m_pDB = nullptr;
//};

//ref class UsageProfileFactory
//{
//  BuildingSimulation::Period* createPerion(KIT::BuW::AdvEnrichment::Period^ period);
//  BuildingSimulation::TimeSeries* createTimeSeries(KIT::BuW::AdvEnrichment::TimeSeries^ timeSeries);
//  BuildingSimulation::UsageProfile* createHeatingProfile(KIT::BuW::AdvEnrichment::UsageProfileHeating^ usageProfileHeating);
//  BuildingSimulation::UsageProfile* createCoolingProfile(KIT::BuW::AdvEnrichment::UsageProfileCooling^ usageProfileCooling);
//  BuildingSimulation::UsageProfile* createVentilationProfile(KIT::BuW::AdvEnrichment::UsageProfileVentilation^ usageProfileVentilation);
//  BuildingSimulation::UsageProfile* createSunShadingProfile(KIT::BuW::AdvEnrichment::UsageProfileWindowShading^ usageProfileWindowShading);
//  BuildingSimulation::UsageProfile* createLightingProfile(KIT::BuW::AdvEnrichment::UsageProfileLighting^ usageProfileLighting);
//  BuildingSimulation::UsageProfile* createPersonProfile(KIT::BuW::AdvEnrichment::UsageProfileOccupancy^ usageProfileOccupancy);
//  BuildingSimulation::UsageProfile* createDeviceProfile(KIT::BuW::AdvEnrichment::UsageProfileDevices^ usageProfileDevices);
//};
