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
    public enum End {
        START, END
    }

    public struct Segment : ICloneable {
        public readonly int a;
        public readonly int b;
        public bool visible;
        public Segment(int a, int b, bool visible=true) {
            Trace.Assert(a != b, "Empty segment");
            if (b < a) {
                this.a = b;
                this.b = a;
            } else {
                this.a = a;
                this.b = b;
            }
            this.visible = visible;
        }
        //////////////////////////// Deconstruct to pair //////////////////////////////
        public void Deconstruct(out int ad, out int bd) {
            ad = a;
            bd = b;
        }
        ////////////////////////////////// ToString ///////////////////////////////////
        public override string ToString() => $"{a}:{b}";

        //////////////////////////////// Predicates ///////////////////////////////////
        public bool IsNormal() => b >= a;
        public bool IsEmpty() => b == a;
        public bool ContainsPoint(int pt) => a <= pt && pt <= b;

        ////////////////////////////// Other methods //////////////////////////////////
        public int LinearSize() => b - a + 1;

        public Tuple<Segment, Segment> Split(int pt) {
            Trace.Assert(ContainsPoint(pt), "Point is out of segment bounds");
            Trace.Assert(pt != a, "Point on start point");
            Trace.Assert(pt != b, "Point on end point");
            return new Tuple<Segment, Segment>(new Segment(a, pt, visible), new Segment(pt, b, visible));
        }
        //////////////////////////////// IClonable ////////////////////////////////////
        public object Clone() => new Segment(a, b, visible);

        ////////////////////////////////// Cast ///////////////////////////////////////
        public static implicit operator Segment((int, int) v) => new Segment(v.Item1, v.Item2);

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