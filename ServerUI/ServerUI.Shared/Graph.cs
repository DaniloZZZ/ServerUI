using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ServerUI
{
    public class Graph
    {
        public int[] Values { get; set; }
        public string Title { get { return txt.Text; } set { txt.Text = value; } }

        public Color LineColor { get { return ((SolidColorBrush)Line.Stroke).Color; } set { Line.Stroke =  new SolidColorBrush(value); } }

        public int Thickness = 2;
        public int Length;
        public Grid Dom;
        private int[] Data;
        private Polyline Line;
        private TextBlock txt;
        public Graph(int[] vals) {
            Values = vals;
            Length = vals.Length;
        }
        public Graph(Grid dom) {
            Dom = dom;
            Grid rect = new Grid();
            rect.Margin = new Windows.UI.Xaml.Thickness(10, 10, 0, 0);
            rect.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            rect.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            rect.Background = new SolidColorBrush(Color.FromArgb(50, 10, 10, 10));
            
            txt = new TextBlock();
            txt.Margin = new Windows.UI.Xaml.Thickness(5, 3, 5, 3);
            txt.FontSize = 14;
            rect.Children.Add(txt);
            Line = new Polyline();
            Line.StrokeThickness = Thickness;
            Line.Stroke = new SolidColorBrush(Colors.DarkRed);

            Dom.Children.Add(Line);
            Dom.Children.Add(rect);
        }
        public void Draw(int[] data)
        {
            Data = data;
            Length = data.Length;
            
            double dx = (Dom.ActualWidth-10)/ Length;
            double dy = (Dom.ActualHeight- 10) / max(data);

            double h = Dom.ActualHeight;
            var Points = new PointCollection();
            for (int i = 0; i < Length; i++)
            {
                Points.Add(new Windows.Foundation.Point(5+dx * i, (h-5-dy * data[i])));
            }
            Line.Points = Points;
        }

        private double max(int[] data)
        {
            double m = data[0];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > m) m = data[i];
            }
            return m;
        }

    }
}
