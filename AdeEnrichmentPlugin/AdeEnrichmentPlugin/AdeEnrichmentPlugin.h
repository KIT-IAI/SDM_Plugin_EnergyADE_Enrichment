#pragma once

#include <Plugin.hpp>
#include <ActionFeatureHelper.hpp>
#include <DocumentObserver.hpp>
#include <MainFrameObserverImpl.hpp>
#include <LiveLogFeature.hpp>
#include <LogDialogFeature.hpp>
#include <LogDialogFeatureHelper.hpp>
#include <AppFeatureHelper.hpp>
#include <IfcDbInclude.h>
#include "AppEnrichment.h"
#include <utils/PopulationObserver.hpp>
#include <vcclr.h>


class IAppDomainProvider
{
public:
  virtual System::AppDomain^ GetAppDomain() = 0;
};

class DummyAction : public sdm::plugin::ActionFeatureHelper
{
  public:
    DummyAction(const std::string& name /*, IAppDomainProvider& rAppDP*/);
    DummyAction() = delete;
    ~DummyAction() override = default;

    void execute() const override;

    void setLogger(sdm::plugin::LiveLogInterface* pLogger) { m_pLogger = pLogger; }

  protected:
    sdm::plugin::LiveLogInterface* m_pLogger = nullptr;
    //IAppDomainProvider& m_rAppDP;
    mutable gcroot<XamlApp::AppProxyImpl^> m_appProxy;
};


class AdeEnrichmentAction : public sdm::plugin::ActionFeatureHelper
{
  public:
    AdeEnrichmentAction(const std::string& name, IAppDomainProvider& rAppDP, sdm::plugin::AppFeatureHelper* pAppFeature);
    AdeEnrichmentAction() = delete;
    ~AdeEnrichmentAction() override;

    void execute() const override;

    void setLogger(sdm::plugin::LiveLogInterface* pLogger) { m_pLogger = pLogger; }
    void setDB(IfcDB::Populationi* pDB);
    void setStates(IfcDB::utils::PopulationStates* pSates) { m_pStates = pSates; }
    void setMainFrameInterface(sdm::plugin::MainFrameInterface* pMainFrameInterface) { m_pMainFrameInterface = pMainFrameInterface; }
    bool isActive() const override;

  protected:
    sdm::plugin::LiveLogInterface* m_pLogger = nullptr;
    IfcDB::Populationi* m_pDB = nullptr;
    IfcDB::utils::PopulationStates* m_pStates = nullptr;
    IAppDomainProvider& m_rAppDP;
    sdm::plugin::MainFrameInterface* m_pMainFrameInterface = nullptr;
    sdm::plugin::AppFeatureHelper* m_pAppFeature = nullptr;

    // The proxy objects used to interface with the application
    mutable gcroot<KIT::BuW::AdvEnrichment::BPEditorProxyImpl^> m_appProxy;
    mutable gcroot<MessageLoggerProxy^> m_loggerProxy;
    mutable gcroot<DataFactoryProxy^> m_factoryProxy;
};


class AdeEnrichmentPlugin : public sdm::plugin::Plugin, public IAppDomainProvider
{
  public:
    AdeEnrichmentPlugin();
    ~AdeEnrichmentPlugin() override = default;

    // Inherited by Plugin
    sdm::plugin::Version getInterfaceVersion() const override { return sdm::plugin::Version(); }
    sdm::plugin::PluginInfo getInfo() const override { return info; }
    std::vector<sdm::plugin::Feature*> getFeatures() const override { return features; }
    sdm::plugin::ComponentInfo getComponentInfo(const sdm::plugin::RequiredComponent& requiredComponent) const override;
    const sdm::plugin::InitializationState& getInitializationState() const override;
    sdm::plugin::AppFeatureHelper* getAppFeature() { return &m_appFeature; }

    IfcDB::Populationi* m_pDB = nullptr;
    IfcDB::utils::PopulationStates* m_States = nullptr;

  protected:
    System::AppDomain^ GetAppDomain() override;
    void CreateAppDomain();

  private:
    const std::string& m_pluginName;

    sdm::plugin::DocumentObserverImpl m_documentObserver;
    sdm::plugin::LiveLogObserver m_liveLogObserver;
    sdm::plugin::MainFrameObserverImpl m_mainFrameObserver;
    sdm::plugin::AppFeatureHelper m_appFeature;
    AdeEnrichmentAction m_EnrichmentAction;

    std::vector<sdm::plugin::Feature*> features;
    sdm::plugin::PluginInfo info;

    gcroot<System::AppDomain^> m_myDomain;

    sdm::plugin::InitializationState m_initState;
};
