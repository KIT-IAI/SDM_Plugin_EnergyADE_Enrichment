using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace KIT.BuW.AdvEnrichment
{
  class ConstructionListDB
  {
    Microsoft.Data.Sqlite.SqliteConnection m_connection = null;
    public SortedDictionary<String, Construction> m_Constructions;
    public SortedDictionary<String, CityObjectGroup> m_CityObjectGroups;
    public SortedDictionary<String, Material> m_Materials;
    public SortedDictionary<String, YearOfConstruction> m_yearOfConstruction;
    IAdeMessageLogger m_Logger;
    public ConstructionDefinitionType m_Type;

    public ConstructionListDB(IAdeMessageLogger logger, ConstructionDefinitionType type = ConstructionDefinitionType.USER_DEFINED)
    {
      m_Constructions = new SortedDictionary<string, Construction>();
      m_Materials = new SortedDictionary<string, Material>();
      m_CityObjectGroups = new SortedDictionary<string, CityObjectGroup>();
      m_yearOfConstruction = new SortedDictionary<string, YearOfConstruction>();
			m_Logger = logger;
      m_Type = type;
    }

    ~ConstructionListDB()
    {
      if (m_connection != null)
      {
        m_connection.Close();
      }
    }

    public void Connect(string database)
    {
      string connStr = $"Data Source={database};Mode=ReadOnly;";
      m_connection = new SqliteConnection(connStr);
      m_connection.Open();
    }

    public void VersionLog()
    {
      var command = m_connection.CreateCommand();

      command.CommandText = @"SELECT data_entry, version FROM naisDB_data_source WHERE name IS 'Nais DB'";
      var reader = command.ExecuteReader();

      if(reader.HasRows)
      {
        while(reader.Read())
        {
          m_Logger.Write($"NaiS data base version: {reader.GetString(1)} ({reader.GetString(0)})");
        }
      }

      reader.Close();

			command.CommandText = @"SELECT data_entry, version FROM naisDB_data_source WHERE name IS 'TABULA'";
			reader = command.ExecuteReader();

			if (reader.HasRows)
			{
				while (reader.Read())
				{
					m_Logger.Write($"Tabula version: {reader.GetString(1)} ({reader.GetString(0)})");
				}
			}
		}

    public void Query(ConstructionDefinitionType type, string countryCode)
    {
      var command = m_connection.CreateCommand();

      command.CommandText = @"SELECT * FROM tabula_year_class WHERE id NOTNULL AND country_code IS $country";

      command.Parameters.AddWithValue("$country", countryCode);

      using (var reader = command.ExecuteReader())
      {
        if (!reader.HasRows)
        {
          m_Logger.Write($"No data available for requested country code: {countryCode}");
        }

        int count = reader.FieldCount;

        while (reader.Read())
        {
          var pCityObjectGroup = new CityObjectGroup();

          var year_class = reader.GetString(1);
          var first_year = reader.GetInt16(3);
          var last_year = reader.GetInt16(4);

          if (first_year == 0 && last_year == 0) { continue; }

          var yoc = new YearOfConstruction();
          yoc.countryCode = countryCode;
          yoc.firstAndLastYear.Add((first_year, last_year));          
          m_yearOfConstruction[countryCode] = yoc;

          pCityObjectGroup.Name = $"TABULA_{countryCode}_{first_year}-{last_year}";
          pCityObjectGroup.Definition = ConstructionDefinitionType.TABULA;
          pCityObjectGroup.PeriodStart = first_year;
          pCityObjectGroup.PeriodEnd = last_year;

          m_CityObjectGroups[pCityObjectGroup.Name] = pCityObjectGroup;

          List<double> roofs_u = new List<double>();
          List<double> floors_u = new List<double>();
          List<double> walls_u = new List<double>();
          List<double> windows_u = new List<double>();
          List<double> windows_g = new List<double>();
          List<double> doors_u = new List<double>();
          List<double> doors_g = new List<double>();

          var command_buildingType = m_connection.CreateCommand();

          command_buildingType.CommandText = @"SELECT code FROM tabula_building_type WHERE id NOTNULL AND year_class IS $year_class AND year_class_ext ISNULL";
          command_buildingType.Parameters.AddWithValue("$year_class", year_class);

          using (var reader_building_type = command_buildingType.ExecuteReader())
          {
            while (reader_building_type.Read())
            {
              var building_type = reader_building_type.GetString(0);

              var command_building = m_connection.CreateCommand();

              command_building.CommandText = @"SELECT * FROM tabula_building WHERE id NOTNULL AND building_type IS $building_type AND data_type IS $data_type";

              command_building.Parameters.AddWithValue("$building_type", building_type);
              command_building.Parameters.AddWithValue("$data_type", "ReEx");

              using (var reader_building = command_building.ExecuteReader())
              {
                while(reader_building.Read())
                {
                  var pCityObjectGroupIndividual = new CityObjectGroup();

                  pCityObjectGroupIndividual.Name = $"TABULA_Building_{reader_building.GetString(1)}_{first_year}-{last_year}";
                  pCityObjectGroupIndividual.PeriodStart = first_year;
                  pCityObjectGroupIndividual.PeriodEnd = last_year;
                  pCityObjectGroupIndividual.Definition = ConstructionDefinitionType.TABULA;

                  m_CityObjectGroups[pCityObjectGroupIndividual.Name] = pCityObjectGroupIndividual;

                  List<double> roofs_u_i = new List<double>();
                  List<double> floors_u_i = new List<double>();
                  List<double> walls_u_i = new List<double>();
                  List<double> windows_u_i = new List<double>();
                  List<double> windows_g_i = new List<double>();
                  List<double> doors_u_i = new List<double>();

                  for (var i = 6; i < 16; ++i)
                  {
                    if(reader_building.IsDBNull(i))
                    {
                      continue;
                    }

                    var construction_code = reader_building.GetString(i);

                    var command_construction = m_connection.CreateCommand();

                    command_construction.CommandText = $"SELECT * FROM tabula_construction WHERE id NOTNULL and code IS $code";

                    command_construction.Parameters.AddWithValue("$code", construction_code);

                    using (var reader_construction = command_construction.ExecuteReader())
                    {
                      while(reader_construction.Read())
                      {
                        switch (reader_construction.GetString(4))
                        {
                          case "Roof":
                            if (!reader_construction.IsDBNull(11))
                            {
                              roofs_u.Add(reader_construction.GetFloat(11));
                              roofs_u_i.Add(reader_construction.GetFloat(11));
                            }
                            break;
                          case "Wall":
                            if (!reader_construction.IsDBNull(11))
                            {
                              walls_u.Add(reader_construction.GetFloat(11));
                              walls_u_i.Add(reader_construction.GetFloat(11));
                            }
                            break;
                          case "Floor":
                            if (!reader_construction.IsDBNull(11))
                            {
                              floors_u.Add(reader_construction.GetFloat(11));
                              floors_u_i.Add(reader_construction.GetFloat(11));
                            }
                            break;
                          case "Door":
                            if (!reader_construction.IsDBNull(11))
                            {
                              doors_u.Add(reader_construction.GetFloat(11));
                              doors_u_i.Add(reader_construction.GetFloat(11));
                            }
                            break;
                          case "Window":
                            if (!reader_construction.IsDBNull(11))
                            {
                              windows_u.Add(reader_construction.GetFloat(11));
                              windows_u_i.Add(reader_construction.GetFloat(11));
                            }
                            if (!reader_construction.IsDBNull(13))
                            {
                              windows_g.Add(reader_construction.GetFloat(13));
                              windows_g_i.Add(reader_construction.GetFloat(13));
                            }
                            break;
                          default:
                            break;
                        }
                      }
                    }
                  }

                  var pConstruction_Roof_Indi = createConstruction("Roof", pCityObjectGroupIndividual.Name, roofs_u_i, new List<double>());

                  var groupMemberRoof_Indi = new GroupMember();
                  groupMemberRoof_Indi.Title = "Roof";
                  groupMemberRoof_Indi.Construction = pConstruction_Roof_Indi;
                  pCityObjectGroupIndividual.GroupMembers.Add(groupMemberRoof_Indi);

                  var pConstruction_Floor_Indi = createConstruction("GroundPlate", pCityObjectGroupIndividual.Name, floors_u_i, new List<double>());

                  var groupMemberFloor_Indi = new GroupMember();
                  groupMemberFloor_Indi.Title = "GroundPlate";
                  groupMemberFloor_Indi.Construction = pConstruction_Floor_Indi;
                  pCityObjectGroupIndividual.GroupMembers.Add(groupMemberFloor_Indi);

                  var pConstruction_Wall_Indi = createConstruction("Fassade", pCityObjectGroupIndividual.Name, walls_u_i, new List<double>());

                  var groupMemberWall_Indi = new GroupMember();
                  groupMemberWall_Indi.Title = "Fassade";
                  groupMemberWall_Indi.Construction = pConstruction_Wall_Indi;
                  pCityObjectGroupIndividual.GroupMembers.Add(groupMemberWall_Indi);

                  var pConstruction_Door_Indi = createConstruction("Door", pCityObjectGroupIndividual.Name, doors_u_i, new List<double>());

                  var groupMemberDoor_Indi = new GroupMember();
                  groupMemberDoor_Indi.Title = "Door";
                  groupMemberDoor_Indi.Construction = pConstruction_Door_Indi;
                  pCityObjectGroupIndividual.GroupMembers.Add(groupMemberDoor_Indi);

                  var pConstruction_Window_Indi = createConstruction("Window", pCityObjectGroupIndividual.Name, windows_u_i, windows_g_i);

                  var groupMemberWindow_Indi = new GroupMember();
                  groupMemberWindow_Indi.Title = "Window";
                  groupMemberWindow_Indi.Construction = pConstruction_Window_Indi;
                  pCityObjectGroupIndividual.GroupMembers.Add(groupMemberWindow_Indi);

                  if (!pCityObjectGroupIndividual.IsValid())
                  {
                    m_Logger.Write($"Construction set not valid: {pCityObjectGroupIndividual.Name}");
                  }
                }
              }
            }
          }

          var pConstruction_Roof = createConstruction("Roof", pCityObjectGroup.Name, roofs_u, new List<double>());

          var groupMemberRoof = new GroupMember();
          groupMemberRoof.Title = "Roof";
          groupMemberRoof.Construction = pConstruction_Roof;
          pCityObjectGroup.GroupMembers.Add(groupMemberRoof);

          var pConstruction_Wall = createConstruction("Fassade", pCityObjectGroup.Name, walls_u, new List<double>());

          var groupMemberWall = new GroupMember();
          groupMemberWall.Title = "Fassade";
          groupMemberWall.Construction = pConstruction_Wall;
          pCityObjectGroup.GroupMembers.Add(groupMemberWall);

          var pConstruction_Floor = createConstruction("GroundPlate", pCityObjectGroup.Name, floors_u, new List<double>());

          var groupMemberFloor = new GroupMember();
          groupMemberFloor.Title = "GroundPlate";
          groupMemberFloor.Construction = pConstruction_Floor;
          pCityObjectGroup.GroupMembers.Add(groupMemberFloor);

          var pConstruction_Door = createConstruction("Door", pCityObjectGroup.Name, doors_u, new List<double>());

          var groupMemberDoor = new GroupMember();
          groupMemberDoor.Title = "Door";
          groupMemberDoor.Construction = pConstruction_Door;
          pCityObjectGroup.GroupMembers.Add(groupMemberDoor);

          var pConstructionWindow = createConstruction("Window", pCityObjectGroup.Name, windows_u, windows_g);

          var groupMemberWindow = new GroupMember();
          groupMemberWindow.Title = "Window";
          groupMemberWindow.Construction = pConstructionWindow;
          pCityObjectGroup.GroupMembers.Add(groupMemberWindow);

          if (!pCityObjectGroup.IsValid())
          {
            m_Logger.Write($"Construction set not valid: {pCityObjectGroup.Name}");
          }
        }
      }
    }

    public List<(int firstYear, int lastYear)> getYearRanges(string countryCode)
    {
      var result = new List<(int firstYear, int lastYear)>();

      var command = m_connection.CreateCommand();
      command.CommandText = @"SELECT * FROM tabula_year_class WHERE id NOTNULL AND country_code IS $country";
      command.Parameters.AddWithValue("$country", countryCode);

      using (var reader = command.ExecuteReader())
      {
        if(!reader.HasRows)
        {
          m_Logger.Write($"No data available for requested country code: {countryCode}");
        }

        while(reader.Read())
        {
          int first = reader.GetInt16(3);
          int last = reader.GetInt16(4);

          if(first == 0 && last == 0){ continue; }

          result.Add((first, last));
        }
      }
      return result;
    }

    public Construction createConstruction(string elementType, string name, List<double> uValues, List<double> gValues)
    {
      var pConstruction = new Construction($"{elementType}-{name}");
      pConstruction.Name = $"{elementType}-{name}";
      pConstruction.Description = $"Construction {pConstruction.Name}";
      pConstruction.UValue = new Measure<double>();
      pConstruction.UValue.Uom = "W/K*m2";
      pConstruction.UValue.Value = uValues.Count > 0 ? uValues.Average() : 0.0;

      if (elementType == "Window")
      {
        pConstruction.gValue = new Measure<double>();
        //pConstruction_Roof.gValue.Uom = "W/K*m2";
        pConstruction.gValue.Value = gValues.Count > 0 ? gValues.Average() : 0.0;
      }

      if (uValues.Count == 0)
      {
        m_Logger.Write($"Missing u-value in {elementType} construction definition: {name}");
      }

      return pConstruction;
    }

    public SortedDictionary<string, double> GetBuildingSizeAndArea(ConstructionDefinitionType type, string countryCode)
    {
      var results = new SortedDictionary<string, double>();

      var command = m_connection.CreateCommand();

      command.CommandText = @"SELECT * FROM year_class WHERE id NOTNULL AND country_code IS $country";

      command.Parameters.AddWithValue("$country", countryCode);

      using (var reader = command.ExecuteReader())
      {
        if (!reader.HasRows)
        {
          m_Logger.Write($"No data available for requested country code: {countryCode}");
        }        

				while (reader.Read())
        {
					List<double> sfh_areas = new List<double>();
					List<double> mfh_areas = new List<double>();
					List<double> ab_areas = new List<double>();
					List<double> th_areas = new List<double>();

					var year_class = reader.GetString(1);
          var first_year = reader.GetInt16(3);
          var last_year = reader.GetInt16(4);

          if (first_year == 0 && last_year == 0) { continue; }

          var command_buildingType = m_connection.CreateCommand();

          command_buildingType.CommandText = @"SELECT code, building_size_class, average_area FROM building_type WHERE id NOTNULL AND year_class IS $year_class AND year_class_ext ISNULL AND building_size_class_ext ISNULL";
          command_buildingType.Parameters.AddWithValue("$year_class", year_class);

          using (var reader_buildingType = command_buildingType.ExecuteReader())
          {
            if(!reader_buildingType.HasRows) 
            {
              m_Logger.Write($"No data available.");
            }

            while (reader_buildingType.Read())
            {
              var building_size_class = reader_buildingType.GetString(1);
              var average_area = reader_buildingType.GetDouble(2);

              if (building_size_class == "SFH")
              {
                sfh_areas.Add(average_area);
              }
              else if (building_size_class == "MFH")
              {
                mfh_areas.Add(average_area);
              }
              else if (building_size_class == "AB")
              {
                ab_areas.Add(average_area);
              }
              else if (building_size_class == "TH")
              {
                th_areas.Add(average_area);
              }
            }            
          }

          if(sfh_areas.Count > 0) 
          {
            var name = year_class + ".SFH." + first_year.ToString() + "-" + last_year.ToString();
            var average_area = sfh_areas.Average();

            results.Add(name, average_area);
          }

          if(mfh_areas.Count > 0) 
          {
            var name = year_class + ".MFH." + first_year.ToString() + "-" + last_year.ToString();
            var average_area = mfh_areas.Average();

            results.Add(name, average_area);
          }

          if(ab_areas.Count > 0)
          {
            var name = year_class + ".AB." + first_year.ToString() + "-" + last_year.ToString();
            var average_area = ab_areas.Average();

            results.Add(name, average_area);
          }

          if(th_areas.Count > 0)
          {
            var name = year_class + ".TH." + first_year.ToString() + "-" + last_year.ToString();
            var average_area = th_areas.Average();

            results.Add(name, average_area);
          }
        }
      }

      return results;
    }
  
    public CityObjectGroup GetConstructionsFromBuilding(ConstructionDefinitionType type, string countryCode, int year, string buildingSizeClass)
    {     
      var pCityObjectGroup = new CityObjectGroup();

      var command = m_connection.CreateCommand();

      command.CommandText = @"SELECT code FROM year_class WHERE id NOTNULL AND country_code IS $country AND first_year <= $year AND last_year >= $year";
      command.Parameters.AddWithValue("$country", countryCode);
      command.Parameters.AddWithValue("$year", year);

      using (var reader = command.ExecuteReader()) 
      {
        if (!reader.HasRows)
        {
          m_Logger.Write($"No data available for the requested year: {year} or country: {countryCode}.");
        }

        while (reader.Read())
        {
          var year_class = reader.GetString(0);

          var command_building_type = m_connection.CreateCommand();

          command_building_type.CommandText = @"SELECT code FROM building_type WHERE id NOTNULL and building_size_class IS $buildingSizeClass AND year_class IS $year_class AND building_size_class_ext ISNULL and year_class_ext ISNULL";
          command_building_type.Parameters.AddWithValue("$buildingSizeClass", buildingSizeClass);
          command_building_type.Parameters.AddWithValue("$year_class", year_class);

          using (var reader_type = command_building_type.ExecuteReader())
          {
            if (!reader_type.HasRows)
            {
							m_Logger.Write($"No data available for the requested building size class: {buildingSizeClass} or year_class: {year_class}.");
						}	

            while (reader_type.Read())
            {
							var building_type_name = countryCode + '.' + year.ToString() + '.' + buildingSizeClass;
							
							pCityObjectGroup.Name = building_type_name;
							pCityObjectGroup.Definition = ConstructionDefinitionType.TABULA;

							List<double> roofs_u = new List<double>();
							List<double> floors_u = new List<double>();
							List<double> walls_u = new List<double>();
							List<double> windows_u = new List<double>();
							List<double> windows_g = new List<double>();
							List<double> doors_u = new List<double>();

							var type_code = reader_type.GetString(0);

              var command_building = m_connection.CreateCommand();

              command_building.CommandText = @"SELECT * FROM building WHERE id NOTNULL AND data_type IS 'ReEx' AND building_type IS $building_type";
              command_building.Parameters.AddWithValue("$building_type", type_code);

              using (var reader_building = command_building.ExecuteReader())
              {
                if(!reader_building.HasRows)
                {
                  m_Logger.Write($"No data available for the requested building type: {type_code}");
                }

                while(reader_building.Read()) 
                {
                  for(int i = 6; i < 16; ++i)
                  {
                    if(reader_building.IsDBNull(i))
                    {
                      continue;
                    }

                    var construction_code = reader_building.GetString(i);

                    var command_construction = m_connection.CreateCommand();

                    command.CommandText = @"SELECT element_type, u_value, g_value FROM construction WHERE id NOTNULL and code IS $code";
                    command.Parameters.AddWithValue("$code", construction_code);

                    using (var reader_construction = command_construction.ExecuteReader())
                    {
                      if (!reader_construction.HasRows)
                      {
                        m_Logger.Write($"No data avaialble for the requested construction: {construction_code}");
                      }

                      while (reader_construction.Read())
                      {
                        switch (reader_construction.GetString(0))
                        {
                          case "Roof":
                            if (!reader_construction.IsDBNull(1))
                            {
                              roofs_u.Add(reader_construction.GetDouble(1));                              
                            }
														break;
                          case "Floor":
														if (!reader_construction.IsDBNull(1))
														{
															floors_u.Add(reader_construction.GetDouble(1));
														}
                            break;
													case "Wall":
														if (!reader_construction.IsDBNull(1))
														{
															walls_u.Add(reader_construction.GetDouble(1));
														}
														break;
													case "Door":
														if (!reader_construction.IsDBNull(1))
														{
															doors_u.Add(reader_construction.GetDouble(1));
														}
														break;
													case "Window":
														if (!reader_construction.IsDBNull(1))
														{
															windows_u.Add(reader_construction.GetDouble(1));
														}
														if (!reader_construction.IsDBNull(2))
														{
															windows_g.Add(reader_construction.GetDouble(1));
														}
														break;
													default:
                            break;
                        }
                      }
                    }
                  }
                }
              }

              var pConstruction_Roof = createConstruction("Roof", pCityObjectGroup.Name, roofs_u, new List<double>());
              var groupMemberRoof = new GroupMember();
              groupMemberRoof.Title = "Roof";
              groupMemberRoof.Construction = pConstruction_Roof;
              pCityObjectGroup.GroupMembers.Add(groupMemberRoof);

              var pConstruction_Floor = createConstruction("Floor", pCityObjectGroup.Name, floors_u, new List<double>());
              var groupMemberFloor = new GroupMember();
              groupMemberFloor.Title = "Floor";
              groupMemberFloor.Construction = pConstruction_Floor;
              pCityObjectGroup.GroupMembers.Add(groupMemberFloor);

              var pConstruction_Wall = createConstruction("Wall", pCityObjectGroup.Name, walls_u, new List<double>());
              var groupMemberWall = new GroupMember();
              groupMemberWall.Title = "Wall";
              groupMemberWall.Construction = pConstruction_Wall;
              pCityObjectGroup.GroupMembers.Add(groupMemberWall);

              var pConstruction_Door = createConstruction("Door", pCityObjectGroup.Name, doors_u, new List<double>());
              var groupMemberDoor = new GroupMember();
              groupMemberDoor.Title = "Door";
              groupMemberDoor.Construction = pConstruction_Door;
              pCityObjectGroup.GroupMembers.Add(groupMemberDoor);

              var pConstruction_Window = createConstruction("Window", pCityObjectGroup.Name, windows_u, windows_g);
              var groupMemberWindow = new GroupMember();
              groupMemberWindow.Title = "Window";
              groupMemberWindow.Construction = pConstruction_Window;
              pCityObjectGroup.GroupMembers.Add(groupMemberWindow); 
            }
					}
				}
      }
      return pCityObjectGroup;
    }    
  }
}
