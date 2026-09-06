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

namespace KIT.BuW.AdvEnrichment.UI
{
  /// <summary>
  /// Interaktionslogik für DataOverview.xaml
  /// </summary>
  public partial class DataOverview : UserControl
  {
    public DataOverview()
    {
      InitializeComponent();
    }

    private void CbMultiComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      foreach (KeyValuePair<string, string> item in e.AddedItems)
      {
        SetItemHilite(item.Value, true);
      }

      foreach (KeyValuePair<string, string> item in e.RemovedItems)
      {
        SetItemHilite(item.Value, false);
      }
    }

    private void ResetHeadingButtonClick(object sender, RoutedEventArgs e)
    {

    }


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
