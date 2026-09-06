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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KIT.BuW.AdvEnrichment;


namespace KIT.BuW.AdvEnrichment.Dummy
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IAdeMessageLogger, IAdeDataUpdater
  {
    public MainWindow()
    {
      InitializeComponent();

      //var x = new Dictionary<string, string>();
      //x["Blah"] = "Fasel";
      //x["test"] = "Das <b>ist</b> <i>ein</i> Test";
      //InfoDictionaryManager.SaveInfoDictionary("t.t", x);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var app = Application.Current as App;
      app.theFactory.dummyLogger = this;
      app.theFactory.dummyUpdater = this;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var hlp = new WindowInteropHelper(this);
      var dlg = new BuildingPropertiesEditor(((App)Application.Current).theFactory, hlp.Handle);
      dlg.Show();
    }

    public void Write(string sMsg)
    {
      var s = new StringBuilder(txtLogging.Text);
      s.AppendLine(sMsg);
      txtLogging.Text = s.ToString();
    }

    public void WriteWarning(string sMsg)
    {
      var s = new StringBuilder(txtLogging.Text);
      s.Append("Warning: ");
      s.AppendLine(sMsg);
      txtLogging.Text = s.ToString();
    }

    public void WriteError(string sMsg)
    {
      var s = new StringBuilder(txtLogging.Text);
      s.Append("ERROR: ");
      s.AppendLine(sMsg);
      txtLogging.Text = s.ToString();
    }

    public class DummyException : ApplicationException
    {
      public DummyException() : base("This is a dummy result")
      { }
    }

    public ApplicationException UpdateBuildingProperties(IList<AdeBuildingProperties> changedAdeBuildingProperties)
    {
      foreach(var item in changedAdeBuildingProperties)
      {
        Write(string.Format("Updating item: {0}", item.GmlId));
      }

      DummyException result = null;
      if ( changedAdeBuildingProperties.Count != 0
        && changedAdeBuildingProperties.FirstOrDefault(x => x.GmlId == "id2") != null)
      {
        result = new DummyException();
        result.Source = "DummyApplication";
        result.Data.Add("OperationResult", "Failure");
        result.Data.Add("OperationDate", DateTime.Now);
        result.Data.Add("Failures", changedAdeBuildingProperties[0].GmlId);
      }
      return result;
    }

    private void Image_ToolTipOpening(object sender, ToolTipEventArgs e)
    {
      if (e.Source is Image)
      {
        var img = e.Source as Image;
        //img.ToolTip = "Blah Fasel <Bold>Fett</Bold>";
        if (img.ToolTip != null && img.ToolTip is TheArtOfDev.HtmlRenderer.WPF.HtmlLabel)
        {
          var l = img.ToolTip as TheArtOfDev.HtmlRenderer.WPF.HtmlLabel;
          l.Text = "Blah Fasel <b>Fett</b>";
        }
      }
    }
  }
}
