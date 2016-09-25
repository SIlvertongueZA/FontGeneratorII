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
      InitializeComponent();

      timer = new Timer();
      timer.Interval = 2000;
      timer.Elapsed += Timer_Elapsed;
      timer.Enabled = true;
      timer.Start();

      font = new FontFile();

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
      /*
      byte[] map = screen.CharMap.ToBytes();

      outputText.Document.Blocks.Clear();

      Paragraph header = new Paragraph();
      header.Inlines.Add(new Bold(new Run("Character: " + Characters.SelectedValue.ToString())));
      outputText.Document.Blocks.Add(header);
            
      Paragraph line = new Paragraph();
      foreach(byte b in map )
      {
        line.Inlines.Add(new Run("0x" + b.ToString("X2") + " "));
      }
      outputText.Document.Blocks.Add(line);
      */

      /*
      outputText.Document.Blocks.Clear();
      foreach(Block b in font.Print())
      {
        outputText.Document.Blocks.Add(b);
      }
      */

      outputText.Document.Blocks.Clear();
      outputText.Document.Blocks.Add(font.PrintCCode());
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
      CommitChar(SelectedCharacter);
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
      CommitChar(SelectedCharacter);
    }

    private void btnCommit_Click(object sender, RoutedEventArgs e)
    {
      CommitChar(SelectedCharacter);
    }

    private void btnNew_Click(object sender, RoutedEventArgs e)
    {
      font = new FontFile();
      Properties.Settings.Default.font_file = font.Path;

      Properties.Settings.Default.Save();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      screen.ReScale();
    }
  }
}
