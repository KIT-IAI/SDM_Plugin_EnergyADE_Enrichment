using MapControl;
using System.Collections.Generic;

namespace KIT.BuW.AdvEnrichment
{
    public class PointItem
    {
        public string Name { get; set; }

        public Location Location { get; set; }
    }

    public class PolylineItem
    {
        public LocationCollection Locations { get; set; } = new LocationCollection();
    }

    public class PolygonItem
    {
        public LocationCollection Locations { get; set; } = new LocationCollection();
    }

    public class MapViewModel
    {
        public List<PointItem> Points { get; } = new List<PointItem>();
        public List<PointItem> Pushpins { get; } = new List<PointItem>();
        public List<PolylineItem> Polylines { get; } = new List<PolylineItem>();
        public List<PolygonItem> Polygons { get; } = new List<PolygonItem>();

        public MapViewModel()
        {}
    }
}
