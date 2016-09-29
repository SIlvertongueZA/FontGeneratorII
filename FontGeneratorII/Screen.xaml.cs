using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FontGeneratorII
{
  /// <summary>
  /// Interaction logic for Screen.xaml
  /// </summary>
  public partial class Screen : UserControl
  {
    #region events

    public event EventHandler Update;

    #endregion

    #region enums

    private enum Actions
    {
      AddStroke,
      RemoveStroke,
      ErasePoint,
      ShiftLeft,
      ShiftRight,
      Clear,
    }

    private enum PenSize
    {
      Small,
      Medium,
      Large,
    };

    #endregion

    #region Pens

    private PenSize pen;

    DrawingAttributes inkDA;
    DrawingAttributes penSmall;
    DrawingAttributes penMedium;
    DrawingAttributes penLarge;
    DrawingAttributes penErase;

    #endregion

    #region Scaling

    ScaleTransform scale;
    double ScalingFactor = 4;

    private int desiredScaleX;
    private int desiredScaleY;

    public int CanvasWidth
    {
      get { return (int)canvas.Width; }

      set
      {
        if ( value > 128 )
          value = 128;
        if ( value < 1 )
          value = 1;

        canvas.Width = value;

        wSlider.Value = canvas.Width;
        wText.Text = canvas.Width.ToString();

        ReScale();
      }
    }

    public int CanvasHeight
    {
      get { return (int)canvas.Height; }
      set
      {
        if ( value > 8 )
          value = 8;
        if ( value < 1 )
          value = 1;

        value *= 8;

        canvas.Height = value;
     
        hSlider.Value = canvas.Height / 8;
        hText.Text = (canvas.Height /8 ).ToString();

        ReScale();
      }
    }

    public void ReScale()
    {
      desiredScaleY = (int)(this.canvasRow.ActualHeight / canvas.Height);
      desiredScaleX = (int)(this.ActualWidth / canvas.Width);

      /* Which axis is limitting */
      if ( desiredScaleX > desiredScaleY )
      {
        ScalingFactor = scale.ScaleY;
        scale.ScaleX = desiredScaleY;
        scale.ScaleY = desiredScaleY;
      }
      else
      {
        ScalingFactor = scale.ScaleX;
        scale.ScaleX = desiredScaleX;
        scale.ScaleY = desiredScaleX;
      }
    }

    #endregion

    #region History

    private List<Actions> ActionHistory;
    private List<int> UndoPoints;
    private List<Stroke> RedoList;

    #endregion

    #region Data

    private bool dirty;
    public bool Dirty
    {
      get { bool temp = dirty; dirty = false; return temp; }
    }

    public CharacterMap CharMap
    {
      get
      {
        return new CharacterMap(ToBytes());
      }
      set
      {
        CanvasWidth = value.Width;
        CanvasHeight = value.Pages;
        Clear();

        FromBytes(value.Bytes);
      }
    }

    #endregion

    #region Constructor

    public Screen()
    {
      InitializeComponent();

      scale = new ScaleTransform();
      scale.ScaleX = 8;
      scale.ScaleY = 8;
      desiredScaleX = (int)scale.ScaleX;
      desiredScaleY = (int)scale.ScaleY;

      TransformGroup canvasTransforms = new TransformGroup();
      canvasTransforms.Children.Add(scale);
      canvas.RenderTransform = canvasTransforms;

      penSmall = new DrawingAttributes();
      penSmall.Color = Colors.Black;
      penSmall.Height = 1;
      penSmall.Width = 1;
      penSmall.FitToCurve = false;
      penSmall.StylusTip = StylusTip.Rectangle;

      penMedium = new DrawingAttributes();
      penMedium.Color = Colors.Black;
      penMedium.Height = 2;
      penMedium.Width = 2;
      penMedium.FitToCurve = false;
      penMedium.StylusTip = StylusTip.Rectangle;

      penLarge = new DrawingAttributes();
      penLarge.Color = Colors.Black;
      penLarge.Height = 4;
      penLarge.Width = 4;
      penLarge.FitToCurve = false;
      penLarge.StylusTip = StylusTip.Rectangle;

      penErase = new DrawingAttributes();
      penErase.Color = Colors.YellowGreen;
      penErase.Height = 1;
      penErase.Width = 1;
      penErase.FitToCurve = false;
      penErase.StylusTip = StylusTip.Rectangle;

      inkDA = penSmall;

      canvas.DefaultDrawingAttributes = inkDA;
      canvas.EditingMode = InkCanvasEditingMode.Ink;

      wSlider.ValueChanged += wSlider_ValueChanged;
      hSlider.ValueChanged += hSlider_ValueChanged;

      UndoPoints = new List<int>();
      RedoList = new List<Stroke>();
      ActionHistory = new List<Actions>();
    }

    #endregion

    #region Controls

    private void wSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      wText.Text = wSlider.Value.ToString();
      CanvasWidth = (int)wSlider.Value;
    }

    private void wText_KeyDown(object sender, KeyEventArgs e)
    {
      if(e.Key == Key.Enter)
      {
        int val = int.Parse(wText.Text);
        if ( val > 128 )
          val = 128;
        if ( val < 1 )
          val = 1;
        wSlider.Value = val;
      }
    }

    private static bool IsTextAllowed(string text)
    {
      Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
      return !regex.IsMatch(text);
    }

    private void numericPreview(object sender, TextCompositionEventArgs e)
    {
      e.Handled = !IsTextAllowed(e.Text);
    }

    private void hSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      hText.Text = hSlider.Value.ToString();
      CanvasHeight = (int)hSlider.Value;
    }

    private void hText_KeyDown(object sender, KeyEventArgs e)
    {
      if(e.Key == Key.Enter)
      {
        int val = int.Parse(hText.Text);
        if ( val > 8 )
          val = 8;
        if ( val < 1 )
          val = 1;
        hSlider.Value = val;
      }
    }

    #endregion

    #region Strokes

    public void Clear()
    {
      StylusPointCollection spc = new StylusPointCollection();

      foreach(Stroke st in canvas.Strokes)
      {
        spc.Add(st.StylusPoints[0]);
      }

      if(spc.Count != 0)
      {
        RedoList.Add(new Stroke(spc));
        UndoPoints.Add(spc.Count);
        ActionHistory.Add(Actions.Clear);
      }

      this.canvas.Strokes.Clear();
      dirty = true;
    }

    private void canvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
      Point canvasPos = canvas.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));

      Stroke st = canvas.Strokes[canvas.Strokes.Count - 1];
      canvas.Strokes.Remove(st);

      AddStrokeAsPoints(st, pen);

      /*
      st = new Stroke(spc);
      st.DrawingAttributes = inkDA;

      canvas.Strokes.Add(st);
      */
    }

    private void AddStrokeAsPoints(Stroke stroke, PenSize size)
    {
      int count = 0;

      StylusPointCollection spc = new StylusPointCollection();

      foreach(StylusPoint sp in stroke.StylusPoints)
      {
        int scale = 1;

        if ( size == PenSize.Medium )
          scale = 2;
        if ( size == PenSize.Large )
          scale = 4;

        double x = sp.X - (sp.X % scale) + 0.5;
        double y = sp.Y - (sp.Y % scale) + 0.5;

        StylusPoint point = new StylusPoint(x, y);
        if ( !spc.Contains(point) )
          spc.Add(point);

        if((size == PenSize.Medium) || (size == PenSize.Large))
        {
          point = new StylusPoint(x + 1, y);
          if ( !spc.Contains(point) )
            spc.Add(point);
          point = new StylusPoint(x, y + 1);
          if ( !spc.Contains(point) )
            spc.Add(point);
          point = new StylusPoint(x + 1, y + 1);
          if ( !spc.Contains(point) )
            spc.Add(point);

          if(size == PenSize.Large)
          {
            point = new StylusPoint(x + 2, y);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 3, y);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 2, y + 1);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 3, y + 1);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x, y + 2);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 1, y + 2);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 2, y + 2);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 3, y + 2);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x, y + 3);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 1, y + 3);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 2, y + 3);
            if ( !spc.Contains(point) )
              spc.Add(point);
            point = new StylusPoint(x + 3, y + 3);
            if ( !spc.Contains(point) )
              spc.Add(point);
          }
        }

        if ( !spc.Contains(point) )
          spc.Add(point);

        Stroke st = new Stroke(spc);
        st.DrawingAttributes = inkDA;

        if ( !canvas.Strokes.Contains(st) )
        {
          canvas.Strokes.Add(st);
          count++;
        }
        spc = new StylusPointCollection();
      }
      ActionHistory.Add(Actions.AddStroke);
      UndoPoints.Add(count);
      dirty = true;
    }

    private void AddStrokeAsPoints(Stroke stroke)
    {
      int count = 0;

      StylusPointCollection spc = new StylusPointCollection();

      foreach(StylusPoint sp in stroke.StylusPoints)
      {
        double x = sp.X - (sp.X % 1) + (inkDA.Width / 2);
        double y = sp.Y - (sp.Y % 1) + (inkDA.Height / 2);

        StylusPoint point = new StylusPoint(x, y);

        if ( !spc.Contains(point) )
          spc.Add(point);

        Stroke st = new Stroke(spc);
        st.DrawingAttributes = inkDA;

        if ( !canvas.Strokes.Contains(st) )
        {
          canvas.Strokes.Add(st);
          count++;
        }
        spc = new StylusPointCollection();
      }
      ActionHistory.Add(Actions.AddStroke);
      UndoPoints.Add(count);
      dirty = true;
    }

    private void canvas_Gesture(object sender, InkCanvasGestureEventArgs e)
    {

    }

    #endregion

    #region Parsing

    private byte[,] ToBytes()
    {
      byte[,] bytes = new byte[CanvasHeight / 8, CanvasWidth];

      foreach(Stroke stroke in canvas.Strokes)
      {
        foreach(StylusPoint point in stroke.StylusPoints)
        {
          int x = (int)(point.X);
          int y = (int)(point.Y);

          int page = y / 8;
          int bit = y % 8;

          if((bytes.GetLength(0) > page) && (bytes.GetLength(1) > x))
            bytes[page, x] |= (byte)(0x01 << bit);
        }
      }

      return bytes;
    }

    private void FromBytes(byte[,] bytes)
    {
      StylusPointCollection spc = new StylusPointCollection();

      for(int page = 0; page < bytes.GetLength(0); page++)
      {
        for(int x = 0; x < bytes.GetLength(1); x++ )
        {
          for(int bit_pos = 0; bit_pos < 8; bit_pos++ )
          {
            if(((bytes[page,x] >> bit_pos) & 0x01) == 0x01)
            {
              spc.Add(new StylusPoint(x + 0.5, (page * 8) + bit_pos + 0.5));
            }
          }
        }
      }

      if ( spc.Count > 0 )
      {
        Stroke st = new Stroke(spc);
        AddStrokeAsPoints(st);
      }
    }

    #endregion

    #region Undo and Redo

    private void undoStroke()
    {
      if ( UndoPoints.Count == 0 )
        return;

      StylusPointCollection spc = new StylusPointCollection();
      for(int i = 0; i < UndoPoints[UndoPoints.Count - 1]; i++ )
      {
        Stroke st = canvas.Strokes[canvas.Strokes.Count - 1];
        canvas.Strokes.Remove(st);

        spc.Add(st.StylusPoints[0]);
      }

      RedoList.Add(new Stroke(spc));
      UndoPoints.RemoveAt(UndoPoints.Count - 1);
    }

    private void undoErase()
    {

    }

    private void undoClear()
    {
      Stroke st = RedoList[RedoList.Count - 1];

      AddStrokeAsPoints(st);
      ActionHistory.RemoveAt(ActionHistory.Count - 1);

      UndoPoints.RemoveAt(UndoPoints.Count - 1);
      UndoPoints.RemoveAt(UndoPoints.Count - 1);
      RedoList.RemoveAt(RedoList.Count - 1);
    }

    private void btnUndo_Click(object sender, RoutedEventArgs e)
    {
      if ( ActionHistory.Count <= 0 )
        return;

      switch(ActionHistory[ActionHistory.Count - 1])
      {
        case Actions.AddStroke:
          undoStroke();
          break;

        case Actions.Clear:
          undoClear();
          break;

        case Actions.ErasePoint:
          undoErase();
          break;

        case Actions.RemoveStroke:
          break;
      }

      ActionHistory.RemoveAt(ActionHistory.Count - 1);
    }

    private void btnRedo_Click(object sender, RoutedEventArgs e)
    {
      if ( RedoList.Count > 0 )
      {
        AddStrokeAsPoints(RedoList[RedoList.Count - 1]);
        RedoList.RemoveAt(RedoList.Count - 1);
      }
    }

    #endregion

    #region Pens

    private void btnClear_Click(object sender, RoutedEventArgs e)
    {
      Clear();
    }

    private void btnSmallPen_Click(object sender, RoutedEventArgs e)
    {
      pen = PenSize.Small;
      canvas.EditingMode = InkCanvasEditingMode.Ink;
    }

    private void btnMediumPen_Click(object sender, RoutedEventArgs e)
    {
      pen = PenSize.Medium;
      canvas.EditingMode = InkCanvasEditingMode.Ink;
    }

    private void btnLargePen_Click(object sender, RoutedEventArgs e)
    {
      pen = PenSize.Large;
      canvas.EditingMode = InkCanvasEditingMode.Ink;
    }

    private void btnErasure_Click(object sender, RoutedEventArgs e)
    {
      canvas.EraserShape = new RectangleStylusShape(inkDA.Width, inkDA.Height); 
      canvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
    }

    #endregion

    private void shiftLeft_Click(object sender, RoutedEventArgs e)
    {
      Stroke temp;
      StylusPointCollection spc = new StylusPointCollection();

      foreach ( Stroke stroke in canvas.Strokes )
      {
        temp = stroke;

        foreach ( StylusPoint point in temp.StylusPoints )
        {
          StylusPoint pt = new StylusPoint();
          pt = point;
          pt.X -= 1;
          spc.Add(pt);
        }
      }
      canvas.Strokes.Clear();
      AddStrokeAsPoints(new Stroke(spc));
    }

    private void shiftRight_Click(object sender, RoutedEventArgs e)
    {
      Stroke temp;
      StylusPointCollection spc = new StylusPointCollection();

      CanvasWidth = CanvasWidth + 1;

      foreach ( Stroke stroke in canvas.Strokes )
      {
        temp = stroke;

        foreach ( StylusPoint point in temp.StylusPoints )
        {
          StylusPoint pt = new StylusPoint();
          pt = point;
          pt.X += 1;
          spc.Add(pt);
        }
      }
      canvas.Strokes.Clear();
      AddStrokeAsPoints(new Stroke(spc));
    }

    private void shiftUp_Click(object sender, RoutedEventArgs e)
    {
      Stroke temp;
      StylusPointCollection spc = new StylusPointCollection();

      foreach ( Stroke stroke in canvas.Strokes )
      {
        temp = stroke;

        foreach ( StylusPoint point in temp.StylusPoints )
        {
          StylusPoint pt = new StylusPoint();
          pt = point;
          pt.Y -= 1;
          if(pt.Y >= 0)
            spc.Add(pt);
        }
      }
      canvas.Strokes.Clear();
      AddStrokeAsPoints(new Stroke(spc));
    }

    private void shiftDown_Click(object sender, RoutedEventArgs e)
    {
      Stroke temp;
      StylusPointCollection spc = new StylusPointCollection();

      foreach ( Stroke stroke in canvas.Strokes )
      {
        temp = stroke;

        foreach ( StylusPoint point in temp.StylusPoints )
        {
          StylusPoint pt = new StylusPoint();
          pt = point;
          pt.Y += 1;
          spc.Add(pt);
        }
      }
      canvas.Strokes.Clear();
      AddStrokeAsPoints(new Stroke(spc));
    }
  }
}
