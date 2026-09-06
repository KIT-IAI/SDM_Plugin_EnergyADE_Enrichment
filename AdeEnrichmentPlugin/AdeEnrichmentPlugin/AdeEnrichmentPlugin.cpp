#include "AdeEnrichmentPlugin.h"
#include "AppEnrichment.h"
#include "stdutils/executable.hpp"
#include <msclr/marshal_cppstd.h>

#include "IFCDBInterfaceVersion.hpp"
#include "PluginInterfaceVersion.hpp"
#include "ComponentRegistry.hpp"

//using namespace std;
using namespace sdm::plugin;
using namespace System;

#using <mscorlib.dll>
#using "System.dll"
#using "System.Xaml.dll"

IMPLEMENT_PLUGIN(AdeEnrichmentPlugin);

System::String^ getDLLDirectory()
{
    auto dllPath = stdutils::findDLLFileName();
    dllPath.remove_filename();
    return gcnew System::String(dllPath.wstring().c_str());
}

System::Reflection::Assembly^ InvokeResolveHandler(System::Object^ sender, System::ResolveEventArgs^ args)
{
    std::string unmanaged = msclr::interop::marshal_as<std::string>(args->Name);
    std::string dir = msclr::interop::marshal_as<std::string>(getDLLDirectory());
    std::cout << " -- resolving -- " << unmanaged << " / " << dir << std::endl;

    if (args->Name->StartsWith("XamlApp"))
    {
        return System::Reflection::Assembly::LoadFile(System::String::Concat(getDLLDirectory(), "XamlApp.exe"));
    }
    else if (args->Name->StartsWith("AdeBuildingPropertyEditor.resources"))
    {
      return System::Reflection::Assembly::LoadFile(System::String::Concat(getDLLDirectory(), "AdeBuildingPropertyEditor.exe"));
    }
    else if (args->Name->StartsWith("AdeBuildingPropertyEditor"))
    {
        return System::Reflection::Assembly::LoadFile(System::String::Concat(getDLLDirectory(), "AdeBuildingPropertyEditor.exe"));
    }
    else if (args->Name->StartsWith("WpfApp1"))
    {
      return System::Reflection::Assembly::LoadFile(System::String::Concat(getDLLDirectory(), "AdeBuildingPropertyEditor.exe"));
    }
    else if (args->Name->StartsWith("KIT"))
    {
      return System::Reflection::Assembly::LoadFile(System::String::Concat(getDLLDirectory(), "AdeBuildingPropertyEditor.exe"));
    }
    else if (args->Name->StartsWith("CefSharp"))
    {
        return System::Reflection::Assembly::LoadFile(System::String::Concat(getDLLDirectory(), "CefSharp.dll"));
    }

    return nullptr;
}


DummyAction::DummyAction(const std::string& name/*, IAppDomainProvider& rAppDP*/)
  : ActionFeatureHelper(name)
  //, m_rAppDP(rAppDP)
{
}

void DummyAction::execute() const
{
  m_pLogger->log("Comment", "Executing Dummy Plugin Action");

  if (static_cast<XamlApp::AppProxyImpl^>(m_appProxy) == nullptr)
  {
    //auto myDomain = m_rAppDP.GetAppDomain();
    //auto oinstance = myDomain->CreateInstanceAndUnwrap("XamlApp", "XamlApp.AppProxyImpl");
    //auto instance = (XamlApp::AppProxyImpl^)oinstance;
    //m_appProxy = instance;
    m_appProxy = gcnew XamlApp::AppProxyImpl();
  }

  if (static_cast<XamlApp::AppProxyImpl^>(m_appProxy) != nullptr)
  {
    m_appProxy->Start();
  }
}


AdeEnrichmentAction::AdeEnrichmentAction(const std::string& name, IAppDomainProvider& rAppDP, sdm::plugin::AppFeatureHelper* pAppFeature)
  : ActionFeatureHelper(name)
  , m_rAppDP(rAppDP)
  , m_pAppFeature(pAppFeature)
{
}

AdeEnrichmentAction::~AdeEnrichmentAction()
{
}

void AdeEnrichmentAction::execute() const
{
    m_pLogger->log("Comment", "Executing AdeBuildingPropertyEditor Plugin Action");

    if (static_cast<KIT::BuW::AdvEnrichment::BPEditorProxyImpl^>(m_appProxy) == nullptr)
    {
        //array<Object^>^ args = gcnew array<Object^>(1);

        auto myDomain = m_rAppDP.GetAppDomain();
        auto oinstance = myDomain->CreateInstanceAndUnwrap(
            "AdeBuildingPropertyEditor",
            "KIT.BuW.AdvEnrichment.BPEditorProxyImpl");
        auto instance = (KIT::BuW::AdvEnrichment::BPEditorProxyImpl^)oinstance;
        m_appProxy = instance;
        //m_appProxy = gcnew KIT::BuW::AdvEnrichment::BPEditorProxyImpl();
    }

    if (static_cast<KIT::BuW::AdvEnrichment::BPEditorProxyImpl^>(m_appProxy) != nullptr)
    {
        // Create a proxy logger that forwards the messages to the application logger
        m_loggerProxy = gcnew MessageLoggerProxy;
        m_loggerProxy->setLogger(m_pLogger);

        // Create the proxy data factory
        auto theFactory = gcnew DataFactoryProxy(m_pDB, m_pStates, m_pAppFeature);
        auto theSelectedBuildings = gcnew System::Collections::Generic::List<System::String^>;
        m_factoryProxy = theFactory;
        m_factoryProxy->Logger = m_loggerProxy;
        m_factoryProxy->ApplicationSettings = gcnew KIT::BuW::AdvEnrichment::AdeApplicationSettings;
        m_factoryProxy->ApplicationSettings->DataDirectory = getDLLDirectory();
        m_factoryProxy->SelectedBuildings = theSelectedBuildings;
        m_factoryProxy->DataSourceProperties = gcnew KIT::BuW::AdvEnrichment::AdeDataSourceProperties;
        m_factoryProxy->UpdateCallbacks = gcnew AppEnrichmentDataUpdater(m_factoryProxy, m_pDB, m_pStates, m_pMainFrameInterface);

        // Update the data from the model database
        theFactory->getData(theSelectedBuildings);

        // Show the window
        m_appProxy->Show(m_factoryProxy, IntPtr(m_pMainFrameInterface->getParentWnd()));
    }
}

void AdeEnrichmentAction::setDB(IfcDB::Populationi* pDB)
{
  m_pDB = pDB;
}

bool AdeEnrichmentAction::isActive() const
{
  if (m_pDB &&
      (m_pDB->hasModelInfo(IfcDB::ModelInfo::MT_GML) ||
       m_pDB->hasModelInfo(IfcDB::ModelInfo::MT_OSM) ||
       m_pDB->hasModelInfo(IfcDB::ModelInfo::MT_IFC) ||
       m_pDB->hasModelInfo(IfcDB::ModelInfo::MT_GBXML)) && !m_appProxy->IsEditorVisible)
  {
    return true;
  }

  return false;
}

AdeEnrichmentPlugin::AdeEnrichmentPlugin()
    : m_pluginName("ADE Enrichment Plugin")
    , m_EnrichmentAction("EnergyADE Enrichment", *this, &m_appFeature)
{
    CreateAppDomain();

    info.name = m_pluginName;
    info.description = "";
    info.version = {1, 0};

    features.emplace_back(&m_liveLogObserver);
    features.emplace_back(&m_documentObserver);
    features.emplace_back(&m_EnrichmentAction);
    features.emplace_back(&m_mainFrameObserver);
    features.emplace_back(&m_appFeature);

    m_liveLogObserver.attach([this](LiveLogInterface* pLogger)
    {
        m_EnrichmentAction.setLogger(pLogger);
    });
    m_documentObserver.attach([this](IfcDB::Populationi* pDB)
    {
        m_EnrichmentAction.setDB(pDB);
        IfcDB::assignGlobalStates(pDB);
    });
    m_documentObserver.attach([this](IfcDB::utils::PopulationSubject* pStates)
    {
        m_EnrichmentAction.setStates(dynamic_cast<IfcDB::utils::PopulationStates*>(pStates));
    });
    m_mainFrameObserver.attach([this](sdm::plugin::MainFrameInterface* pMainFrameInterface)
    {
        m_EnrichmentAction.setMainFrameInterface(pMainFrameInterface);
    });
}

void AdeEnrichmentPlugin::CreateAppDomain()
{
    System::AppDomain::CurrentDomain->AssemblyResolve += gcnew System::ResolveEventHandler(&InvokeResolveHandler);

    const auto dllDir = getDLLDirectory();

    System::AppDomainSetup^ domaininfo = gcnew System::AppDomainSetup();
    domaininfo->ApplicationBase = dllDir;

    m_myDomain = System::AppDomain::CreateDomain("MyDomain", nullptr, domaininfo);
}

ComponentInfo AdeEnrichmentPlugin::getComponentInfo(const RequiredComponent& requiredComponent) const
{
    ComponentRegistry availableComponents;
    availableComponents.addAvailable(IFCDB_INTERFACE_COMPONENT_NAME, IFCDB_INTERFACE_COMPONENT_VERSION, IFCDB_INTERFACE_COMPONENT_HINT, std::atoi(IFCDB_INTERFACE_COMPONENT_DATE.data()));
    availableComponents.addAvailable(PLUGIN_INTERFACE_COMPONENT_NAME, PLUGIN_INTERFACE_COMPONENT_VERSION, PLUGIN_INTERFACE_COMPONENT_HINT, std::atoi(PLUGIN_INTERFACE_COMPONENT_DATE.data()));

    return availableComponents.getInfo(requiredComponent);
}

const InitializationState& AdeEnrichmentPlugin::getInitializationState() const
{
    return m_initState;
}

System::AppDomain^ AdeEnrichmentPlugin::GetAppDomain()
{
    return m_myDomain;
}
