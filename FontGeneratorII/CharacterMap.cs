using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontGeneratorII
{
  public class CharacterMap
  {
    private int x;
    private int y;
    private byte[,] bytes;

    public int Width { get { return x; } }
    public int Pages { get { return y; } }
    public byte[,] Bytes { get { return bytes; } }
    
    public CharacterMap()
    {
      x = 1;
      y = 1;

      bytes = new byte[y, x];
    }

    public CharacterMap(byte[,] data)
    {
      x = data.GetLength(1);
      y = data.GetLength(0);

      bytes = data;
    }

    public CharacterMap(byte[] data)
    {
      ParseBytes(data);
    }

    public byte[] ToBytes()
    {
      int index = 2;
      byte[] output = new byte[(x * y) + 2];

      output[0] = (byte)y;
      output[1] = (byte)x;

      for(int j = 0; j < y;  j++)
      {
        for(int i = 0; i < x;  i++)
        {
          output[index++] = bytes[j, i];
        }
      }
      return output;
    }

    public byte[] DataBytes()
    {
      int index = 0;
      byte[] output = new byte[x * y];

      for(int j = 0; j < y;  j++)
      {
        for(int i = 0; i < x;  i++)
        {
          output[index++] = bytes[j, i];
        }
      }
      return output;
    }

    public bool ParseBytes(byte[] input)
    {
      if ( input.Length < 3 )
        return false;

      int index = 2;

      y = input[0];
      x = input[1];

      if ( input.Length != ((y * x) + 2) )
        return false;

      bytes = new byte[y, x];

      for(int j = 0; j < y;  j++)
      {
        for(int i = 0; i < x;  i++)
        {
          bytes[j, i] = input[index++];
        }
      }
      return true;
    }

    public string DataString()
    {
      string str = "";

      foreach(byte b in DataBytes())
      {
        str += "0x" + b.ToString("X2") + ", ";
      }
      str = str.Substring(0, str.Length - 2);
      return str;
    }
  }
}
