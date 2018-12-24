using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;

namespace Maze {
    public enum Direct {
        H, V
    }
    public enum Side {
        TOP, RIGHT, BOTTOM, LEFT
    }
    public enum CmpResult {
        GT, LT, EQ, LE, GE
    }

    public struct Segment {
        public int a;
        public int b;
        public Segment(int ac, int bc) {
            if (bc < ac) {
                a = bc;
                b = ac;
            } else {
                a = ac;
                b = bc;
            }
        }
        public void Deconstruct(out int ad, out int bd) {
            ad = a;
            bd = b;
        }
        public bool IsNormal() => b >= a;
        public bool IsEmpty() => b == a;
        public int LinearSize() => b - a + 1;
        public static implicit operator Segment((int, int) v) => new Segment(v.Item1, v.Item2);
        public override string ToString() => $"({a}, {b})";
    }

    public class SegmentsComparer : IComparer<Segment> {
        public int Compare(Segment x, Segment y) {
            int r = x.a.CompareTo(y.a);
            if (r == 0) r = x.b.CompareTo(y.b);
            return r;
        }
    }

    public static class Utils {
        public const int DOOR = 20;
        public static Random Randomizer = new Random();
        public static SegmentsComparer SegCmp = new SegmentsComparer();
        public static TraceSource Tracer = new TraceSource("Maze");
        public static Side OppositeSide(Side s) {
            switch (s) {
                case Side.TOP: return Side.BOTTOM;
                case Side.BOTTOM: return Side.TOP;
                case Side.LEFT: return Side.RIGHT;
                case Side.RIGHT: return Side.LEFT;
                default: throw new ArgumentException("Invalid value");
            }
        }
        public static void DrawLine(Canvas cnv, int x1, int y1, int x2, int y2) {
            var myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Black;
            myLine.X1 = x1;
            myLine.Y1 = y1;
            myLine.X2 = x2;
            myLine.Y2 = y2;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Top;
            myLine.StrokeThickness = 1;
            cnv.Children.Add(myLine);
        }

        public static void DrawLine(Canvas cnv, int i, int a, int b, Direct d) {
            if (d == Direct.H) DrawLine(cnv, a, i, b, i);
            else DrawLine(cnv, i, a, i, b);
        }
    }

}