using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;

namespace KIT.BuW.AdvEnrichment
{
  public class ConstructionList
  {
    internal SortedDictionary<string, CityObjectGroup> ConstructionSets_ = new SortedDictionary<string, CityObjectGroup>();
    internal SortedDictionary<string, Construction> Constructions_       = new SortedDictionary<string, Construction>();
    internal SortedDictionary<string, Material> Materials_               = new SortedDictionary<string, Material>();
    internal SortedDictionary<string, YearOfConstruction> YearOfConstructions_ = new SortedDictionary<string, YearOfConstruction>();
    internal List<(int firstYear, int lastYear)> YearRanges_ = new List<(int firstYear, int lastYear)>();

    public Construction GetConstruction(string constructionName)
    {
      if(Constructions.ContainsKey(constructionName) == true)
      {
        return Constructions[constructionName];
      }

      return null;
    }

    public SortedDictionary<string, CityObjectGroup> ConstructionSets
    {
      get
      {
        return ConstructionSets_;
      }
    }

    public SortedDictionary<string, Construction> Constructions
    {
      get
      {
        return Constructions_;
      }
    }

    public SortedDictionary<string, Material> Materials
    {
      get
      {
        return Materials_;
      }
    }

    public List<(int firstYear, int lastYear)> YearRanges
    {
      get
      {
        return YearRanges_;
      }
    }

    public void ReadConstructionList(IAdeMessageLogger logger, string constructionList, ConstructionDefinitionType type = ConstructionDefinitionType.USER_DEFINED)
    {
      ConstructionListReader reader = new ConstructionListReader(logger, constructionList, type);
      reader.Read();

      Constructions_    = reader.m_Constructions;
      Materials_        = reader.m_Materials;
      ConstructionSets_ = reader.m_CityObjectGroups;
    }

    public void Query(IAdeMessageLogger logger, string database, string countryCode, ConstructionDefinitionType type = ConstructionDefinitionType.USER_DEFINED)
    {
      try
      {
        SQLitePCL.Batteries.Init();
        ConstructionListDB constructionDB = new ConstructionListDB(logger, type);
        constructionDB.Connect(database);

        constructionDB.VersionLog();

        YearRanges_ = constructionDB.getYearRanges(countryCode);

        constructionDB.Query(ConstructionDefinitionType.TABULA, countryCode);

        foreach (KeyValuePair<string, Construction> kvp in constructionDB.m_Constructions)
        {
          Constructions_.Add(kvp.Key, kvp.Value);
        }

        foreach (KeyValuePair<string, Material> kvp in constructionDB.m_Materials)
        {
          Materials_.Add(kvp.Key, kvp.Value);
        }

        foreach (KeyValuePair<string, CityObjectGroup> kvp in constructionDB.m_CityObjectGroups)
        {
          ConstructionSets_.Add(kvp.Key, kvp.Value);
        }

        foreach(KeyValuePair<string, YearOfConstruction> kvp in constructionDB.m_yearOfConstruction)
        {
          YearOfConstructions_.Add(kvp.Key, kvp.Value);
        }
      }
      catch (Exception ex)
      {
        logger.WriteError($"The construction list '{database}' could not be read. Exception: {ex.Message}");
        var inner = ex.InnerException;
        while (inner != null)
        {
          logger.WriteError($"  Innner exception: {inner.Message}");
          inner = inner.InnerException;
        }
        logger.Write($"  Current path is {Path.GetFullPath(database)}");
      }
    }

    public void WriteConstructionList(string constructionList)
    {
      //ConstructionListWriter writer = new ConstructionListWrit(constructionList, Constructions);
      //writer.Write();
    }
  }
}
