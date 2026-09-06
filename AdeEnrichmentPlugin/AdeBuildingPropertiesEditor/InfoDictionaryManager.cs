using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace KIT.BuW.AdvEnrichment
{
  public class InfoDictionaryManager
  {
    /// <summary>
    /// A class to de-/serialize the tooltip help texts.
    /// </summary>
    /// The text field is written to a CData section so that
    /// it may contain arbitrary text, especially HTML formatted
    /// help texts including images and links.
    public class item
    {
      [XmlAttribute]
      public string id;
      [XmlIgnore]
      public string text;
      [XmlElement("value")]
      public System.Xml.XmlCDataSection CDATAvalue
      {
        get
        {
          return new System.Xml.XmlDocument().CreateCDataSection(this.text);
        }
        set
        {
          this.text = value.Value;
        }
      }
    }

    /// <summary>
    /// Load the info dictionary
    /// </summary>
    /// <param name="fileName">The name of the file containing the help texts</param>
    /// <returns>A dictionary containing the content from the file</returns>
    public static Dictionary<string, string> LoadInfoDictionary(string fileName)
    {
      try
      {
        XmlSerializer s = new XmlSerializer(typeof(item[]), new XmlRootAttribute() { ElementName = "items" });

        using (FileStream stream = new FileStream(fileName, FileMode.Open))
        {
          var items = (item[])s.Deserialize(stream);
          return items.ToDictionary(i => i.id, i => i.text);
        }
      }
      catch(Exception)
      {
        return new Dictionary<string, string>();
      }
    }

    /// <summary>
    /// Save the info dictionary
    /// </summary>
    /// <param name="fileName">The name of the file to be written</param>
    /// <param name="dict">The dictionary to be saved</param>
    /// This method is used only to generate an initial example of the serialized help texts
    public static void SaveInfoDictionary( string fileName, Dictionary<string, string> dict)
    {
      XmlSerializer s = new XmlSerializer(typeof(item[]), new XmlRootAttribute() { ElementName = "items" });

      using (FileStream stream = new FileStream(fileName, FileMode.Create))
      {
        s.Serialize(stream, dict.Select(kv => new item() { id = kv.Key, text = kv.Value }).ToArray());
      }
    }
  }
}
