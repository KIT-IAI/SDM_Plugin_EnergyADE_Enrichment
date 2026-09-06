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
using System.Windows.Shapes;

using KIT.BuW.AdvEnrichment;

namespace KIT.BuW.AdvEnrichment.AdeBuildingPropertyEditor
{
  /// <summary>
  /// Interaktionslogik für ConstructionEditor.xaml
  /// </summary>
  public partial class ConstructionEditor : Window
  {
    public Construction m_Construction { get; set; }

    public ConstructionEditor(Construction construction)
    {
      DataContext = this;

      m_Construction = construction;

      InitializeComponent();
    }
  }
}
