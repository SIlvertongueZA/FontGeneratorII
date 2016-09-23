using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace FontGeneratorII
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public FontFile font { get; set; }
    private Timer timer;

    public List<string> Test { get; set; }

    char SelectedCharacter
    {
      get
      {
        if ( Characters.SelectedValue == null )
          return (char)(0xFF);

        return char.Parse(Characters.SelectedValue.ToString());
      }
    }

    public MainWindow()
    {
      Test = new List<string>();
      Test.Add("e");
      Test.Add("f");
      Test.Add("g");
      Test.Add("h");

      InitializeComponent();

      timer = new Timer();
      timer.Interval = 2000;
      timer.Elapsed += Timer_Elapsed;
      timer.Enabled = true;
      timer.Start();

      font = new FontFile(null);

      DataContext = this;

    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if(screen.Dirty)
      {
      }
    }

    private void btnClear_Click(object sender, RoutedEventArgs e)
    {
      screen.Clear(); 
    }

    private void btnPrint_Click(object sender, RoutedEventArgs e)
    {
      byte[] map = screen.CharMap.ToBytes();

      textBox.Text = "";
      foreach(byte b in map )
      {
        this.textBox.Text += "0x" + b.ToString("X2") + " ";
      }
    }

    private void btnLoad_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog fileDialog = new OpenFileDialog();
      bool? res = fileDialog.ShowDialog();

      font.load(fileDialog.FileName);
      Properties.Settings.Default.font_file = font.Path;

      Properties.Settings.Default.Save();
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      font.save();
    }

    private void btnSaveAs_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog fileDialog = new SaveFileDialog();
      bool? res = fileDialog.ShowDialog();

      if ( res == true )
      {
        string file = fileDialog.FileName;
        font.save(file);
      }
    }

    private void btnExit_Click(object sender, RoutedEventArgs e)
    {
      Application.Current.Shutdown();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
      CharacterEntry entry = new CharacterEntry();

      bool? result = entry.ShowDialog();

      if(result == true)
      {
        CommitChar(SelectedCharacter);
        screen.Clear();
        font.SetCharmap(entry.Character, screen.CharMap);
      }
    }

    private void populateComboBox()
    {
      Characters.Items.Clear();
      foreach(char c in font.Characters)
        Characters.Items.Add(c);
    }

    private void Characters_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if ( Characters.SelectedValue == null )
        return;

      CharacterMap map = font.GetCharmap(SelectedCharacter);
      if ( map != null )
      {
        screen.CharMap = map;
      }
      else
      {
        screen.Clear();
        screen.CanvasWidth = 5;
        screen.CanvasHeight = 1;
      }
    }

    private void CommitChar(char c)
    {
      if ( c == 0xFF )
        return;

      font.SetCharmap(c, screen.CharMap);
    }

    private void Characters_DropDownOpened(object sender, EventArgs e)
    {
      /*
      Characters.Items.Clear();
      foreach(char c in font )
      {
        Characters.Items.Add(c);
      }
      */
//      CommitChar(SelectedCharacter);
    }

    private void btnCommit_Click(object sender, RoutedEventArgs e)
    {
      CommitChar(SelectedCharacter);
    }
  }
}
