using System;
using System.Collections.Generic;

namespace KIT.BuW.AdvEnrichment
{  
  public class UsageProfileList
  {
    internal SortedDictionary<string, UsageProfileGroup> UsageProfileSets_ = new SortedDictionary<string, UsageProfileGroup>();
    internal SortedDictionary<string, UsageProfile> UsageProfiles_         = new SortedDictionary<string, UsageProfile>();

    public UsageProfile GetUsageProfile(string profileName)
    {
      if (UsageProfiles.ContainsKey(profileName) == true )
      {
        return UsageProfiles[profileName];
      }

      return null;
    }

    public UsageProfileGroup GetUsageProfileSet(string profileSetName)
    {
      if (UsageProfileSets.ContainsKey(profileSetName) == true)
      {
        return UsageProfileSets[profileSetName];
      }

      return null;
    }

    public UsageProfileGroup FindUsageProfileSet(string profileSetName)
    {
      foreach (var keyValuePair in UsageProfileSets)
      {
        if (keyValuePair.Key.Contains(profileSetName) == true)
        {
          return keyValuePair.Value;
        }
      }

      return null;
    }

    public SortedDictionary<string, UsageProfileGroup> UsageProfileSets
    {
      get
      {
        return UsageProfileSets_;
      }
    }

    public SortedDictionary<string, UsageProfile> UsageProfiles
    {
      get
      {
        return UsageProfiles_;
      }
    }

    public void ReadUsageProfileList(IAdeMessageLogger logger, string usageProfileList, UsageProfileDefinitionType type = UsageProfileDefinitionType.USER_DEFINED)
    {
      UsageProfileReader reader = new UsageProfileReader(logger, usageProfileList, type);
      reader.Read();

      UsageProfiles_    = reader.m_UsageProfiles;
      UsageProfileSets_ = reader.m_UsageProfileGroups;
    }

    public void WriteUsageProfileList()
    {

    }
  }
}
