using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace FontGeneratorII
{
  public class FontFile : IEnumerable<char>, INotifyCollectionChanged
  {
    public string Name;
    public string Path;

    private SortedDictionary<char, CharacterMap> font_map;

    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public List<char> Characters
    {
      get
      {
        List<char> retval = new List<char>();
        foreach(char c in font_map.Keys)
        {
          retval.Add(c);
        }
        return retval;
      }
    }

    public FontFile(string font_file)
    {
      if ( font_file == null )
        Name = "Unnamed";
      else
        Name = font_file;

      font_map = new SortedDictionary<char, CharacterMap>();

      if ( font_file == null )
        font_file = "temp.csv";

      if ( File.Exists(font_file) )
        load(font_file);
      else
      {
        foreach(char c in CharacterSets.All)
        {
          font_map.Add(c, null);
        }
      }
    }

    public CharacterMap GetCharmap(char c)
    {
      if ( font_map.ContainsKey(c) )
        return font_map[c];
      else
      {
        font_map.Add(c, null);
        return null;
      }
    }

    public void SetCharmap(char c, CharacterMap map)
    {
      if ( font_map.ContainsKey(c) )
        font_map.Remove(c);

      font_map.Add(c, map);
      if(PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs("charList"));


      if ( CollectionChanged != null )
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public char[] Contents()
    {
      return font_map.Keys.ToArray();
    }

    public void load()
    {
      StreamReader reader = new StreamReader(Path);

      string line;
      while ( (line = reader.ReadLine()) != null )
      {
        parse_line(line);
      }
      reader.Close();
    }

    public void load(string path)
    {
      Path = path;
      load();
    }

    public void save()
    {
      if ( File.Exists(Path) )
        File.Delete(Path);

      using ( StreamWriter sw = File.AppendText(Path) )
      {
        foreach ( KeyValuePair<char, CharacterMap> map in font_map )
        {
          string line = "";
          line += map.Key;
          line += " : ";
          line += "H = " + map.Value.Pages;
          line += ", W = " + map.Value.Width;
          line += ", Data = ";

          foreach ( byte b in map.Value.DataBytes() )
          {
            line += "0x" + b.ToString("X2");
            line += ", ";
          }
          line = line.Substring(0, line.Length - 2);
          sw.WriteLine(line);
        }
      }
    }

    public void save(string path)
    {
      Path = path;
      save();
    }

    private void parse_line(string line)
    {
      List<byte> bytes = new List<byte>();

      string[] separators = { " : H = ", ", W = ", ", Data = "};
      string[] parts = line.Split(separators, StringSplitOptions.None);

      char c = char.Parse(parts[0]);
      byte height = byte.Parse(parts[1]);
      byte width = byte.Parse(parts[2]);

      bytes.Add(height);
      bytes.Add(width);

      parts[3] = parts[3].Replace(" ", "");
      parts = parts[3].Split(',');

      foreach(string s in parts )
      {
        bytes.Add(Convert.ToByte(s, 16));
      }

      SetCharmap(c, new CharacterMap(bytes.ToArray()));
    }

    public IEnumerator<char> GetEnumerator()
    {
      return ((IEnumerable<char>)Characters).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<char>)Characters).GetEnumerator();
    }

    public List<Block> Print()
    {
      List<Block> blocks = new List<Block>();

      Paragraph title = new Paragraph();
      title.Inlines.Add(new Bold(new Run(Name)));

      Paragraph characters = new Paragraph();

      foreach ( KeyValuePair<char, CharacterMap> kvp in font_map )
      {
        characters.Inlines.Add(new Run(kvp.Key.ToString() + " : Width = " + kvp.Value.Width.ToString() + " Height = " + kvp.Value.Pages.ToString()));
        characters.Inlines.Add(new Run(" Data = " + kvp.Value.DataString() + "\n"));
      }

      blocks.Add(title);
      blocks.Add(characters);

      return blocks;
    }

    public Paragraph PrintCCode()
    {
      Paragraph characters = new Paragraph();

      foreach ( KeyValuePair<char, CharacterMap> kvp in font_map )
      {
        characters.Inlines.Add("const U8 ascii_char_");
        characters.Inlines.Add("0x" + Convert.ToByte(kvp.Key).ToString("X2"));
        characters.Inlines.Add("[] PROGMEM = {");
        characters.Inlines.Add(kvp.Value.Pages.ToString());
        characters.Inlines.Add(", ");
        characters.Inlines.Add(kvp.Value.Width.ToString());
        characters.Inlines.Add(", ");
        characters.Inlines.Add(kvp.Value.DataString());
        characters.Inlines.Add("}; /* " + kvp.Key.ToString() + " */\n");
      }

      return characters;
    }


  } /* End Class */
} /* Namespace */
