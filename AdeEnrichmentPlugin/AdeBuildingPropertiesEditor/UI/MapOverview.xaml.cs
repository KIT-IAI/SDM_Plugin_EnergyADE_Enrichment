using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapControl;


namespace KIT.BuW.AdvEnrichment.UI
{
  /// <summary>
  /// Interaktionslogik für MapOverview.xaml
  /// </summary>
  public partial class MapOverview : UserControl
  {
    public MapViewModel MapViewModel { get; set; }

    public MapOverview()
    {
      InitializeComponent();
      MapViewModel = new MapViewModel();
      DataContext = MapViewModel;
    }

    private void ResetHeadingButtonClick(object sender, RoutedEventArgs e)
    {
    }

    private void MapItemTouchDown(object sender, TouchEventArgs e)
    {
      var mapItem = (MapItem)sender;
      mapItem.IsSelected = !mapItem.IsSelected;
      e.Handled = true;
    }

  }
}
