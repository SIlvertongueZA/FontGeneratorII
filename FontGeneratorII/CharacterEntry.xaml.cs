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

namespace FontGeneratorII
{
  /// <summary>
  /// Interaction logic for CharacterEntry.xaml
  /// </summary>
  public partial class CharacterEntry : Window
  {
    public char Character
    {
      get { return char.Parse(txtEntry.Text); }
      set { txtEntry.Text = value.ToString(); }
    }
    public CharacterEntry()
    {
      InitializeComponent();
    }

    private void btnOkay_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
    }

    private void txtEntry_KeyDown(object sender, KeyEventArgs e)
    {
      if(e.Key == Key.Enter)
      {
        this.DialogResult = true;
        return;
      }

      txtEntry.Text = "";
    }
  }
}
