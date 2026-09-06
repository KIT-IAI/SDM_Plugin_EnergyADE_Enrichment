using Newtonsoft.Json;
using System;
using System.Windows;

namespace KIT.BuW.AdvEnrichment
{
  public partial class BuildingPropertiesEditor
  {
    public class MapViewParams
    {
      public GeoPoint center { get; set; }
      public int zoom { get; set; }
    }

    public class GeometryParams
    {
      public GeoPoint[] points { get; set; }
      public string id { get; set; }
    }

    //private void SetMapCenter(GeoPoint mapLocation, int zoomVal)
    //{
    //  var pars = new MapViewParams()
    //  {
    //    center = sourceProps.MapLocation,
    //    zoom = zoomVal
    //  };
    //  var json = JsonConvert.SerializeObject(pars);
    //  //wbMap.ExecuteScriptAsync("setViewParams", new object[] { json });
    //}

    //private void AddMapPolygon(GeoPoint[] pts, string gmlid)
    //{
    //  var geom = new GeometryParams()
    //  {
    //    points = pts,
    //    id = gmlid
    //  };
    //  var json = JsonConvert.SerializeObject(geom);
    //  //wbMap.ExecuteScriptAsync("addModelPoligon", new object[] { json });
    //}

    /// <summary>
    /// Change the display state of a feature item
    /// </summary>
    /// <param name="id">The ID of the layer to be changed</param>
    /// <param name="bHighlight">true: hilite, false default</param>
    private void SetItemHilite(string id, bool bHighlight)
    {
      //wbMap.ExecuteScriptAsync("setItemHilite", new object[] { id, bHighlight });
    }

  }
}