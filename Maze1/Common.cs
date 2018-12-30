using System;
using System.Numerics;
using V4 = System.Numerics.Vector4;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace Maze {
    public enum Direct {
        H, V
    }

    public enum Side {
        TOP=1, RIGHT=2, BOTTOM=4, LEFT=8
    }

    public enum CmpResult {
        GT, LT, EQ, LE, GE
    }
    public enum End {
        START, END
    }

    public struct Point {
        public readonly int X;
        public readonly int Y;

        public Point(int x, int y) {
            X = x;
            Y = y;
        }

        //////////////////////////// Deconstruct to pair //////////////////////////////
        public void Deconstruct(out int x, out int y) {
            x = X;
            y = Y;
        }

        ////////////////////////////////// ToString ///////////////////////////////////
        public override string ToString() => $"{X}:{Y}";

        ////////////////////////////////// Cast ///////////////////////////////////////
        public static implicit operator Point((int, int) v) => new Point(v.Item1, v.Item2);

    }

    public struct Segment {
        public readonly int A;
        public readonly int B;
        private readonly bool? visible;
        public bool Visible {
            get {
                Trace.Assert(visible != null, "Try to get visible attribute without to setup it");
                return (bool)visible;
            }
        }
        public Segment(int a, int b) {
            Trace.Assert(a != b, "Empty segment");
            if (b < a) {
                A = b;
                B = a;
            } else {
                A = a;
                B = b;
            }
            visible = null;
        }

        public Segment(int a, int b, bool visible) {
            Trace.Assert(a != b, "Empty segment");
            if (b < a) {
                A = b;
                B = a;
            } else {
                A = a;
                B = b;
            }
            this.visible = visible;
        }

        //////////////////////////// Deconstruct to pair //////////////////////////////
        public void Deconstruct(out int a, out int b) {
            a = A;
            b = B;
        }

        ////////////////////////////////// ToString ///////////////////////////////////
        public override string ToString() => $"{A}:{B}";

        //////////////////////////////// Predicates ///////////////////////////////////
        public bool IsNormal() => B > A;
        public bool IsEmpty() => B == A;
        public bool ContainsPoint(int pt) => A < pt && pt < B;

        ////////////////////////////// Other methods //////////////////////////////////
        public int LinearSize() => B - A + 1;

        public Tuple<Segment, Segment> Split(int pt) {
            Trace.Assert(ContainsPoint(pt), "Point is out of segment bounds");
            Trace.Assert(pt != A, "Point on start point");
            Trace.Assert(pt != B, "Point on end point");
            return new Tuple<Segment, Segment>(new Segment(A, pt, Visible), new Segment(pt, B, Visible));
        }

        ////////////////////////////////// Cast ///////////////////////////////////////
        public static implicit operator Segment((int, int) v) => new Segment(v.Item1, v.Item2);

    }

    public class SegmentsComparer : IComparer<Segment> {
        public int Compare(Segment x, Segment y) {
            int r = x.A.CompareTo(y.A);
            if (r == 0) r = x.B.CompareTo(y.B);
            return r;
        }
    }

    public static class Utils {
        public const int SidesNum = 4;
        public const int DOOR = 15;
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

        public static void Log(string msg) => Tracer.TraceEvent(TraceEventType.Information, 0, msg);

        public static void DrawLine(Canvas cnv, int x1, int y1, int x2, int y2, int thickness=1, UInt32 color=0) {
            byte r = (byte)(color >> 16),
                 g = (byte)((color >> 8) & 0xFF),
                 b = (byte)(color & 0xFF);
            SolidColorBrush stroke = new SolidColorBrush(Color.FromRgb(r, g, b));
            var myLine = new Line {
                Stroke = stroke,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = thickness
            };
            cnv.Children.Add(myLine);
        }

        public static void DrawLine(Canvas cnv, int i, int a, int b, Direct d) {
            if (d == Direct.H) DrawLine(cnv, a, i, b, i);
            else DrawLine(cnv, i, a, i, b);
        }

        public static Side RandomSide(byte enabled=0xFF) {
            Array allSides = Enum.GetValues(typeof(Side));
            List<Side> enabledSides = new List<Side>(SidesNum);
            foreach (Side side in allSides) {
                if (0 != ((byte)side & enabled)) {
                    enabledSides.Add(side);
                }
            }
            if (enabledSides.Count > 0) {
                Side[] arr = enabledSides.ToArray();
                return arr[Randomizer.Next(arr.Length)];
            }
            else {
                throw new ArgumentException($"Value {enabled} disables all Side values");
            }
        }

        public static Side RandomSide(float[] freqs) {
            Array allSides = Enum.GetValues(typeof(Side));
            Trace.Assert(freqs.Length == allSides.Length, $"Frequency array must have {allSides.Length} items");
            ulong totals = 0;
            V4[] counters = new V4[] { new V4(0), new V4(0), new V4(0), new V4(0) };
            for (int i = 0; i < freqs.Length; i++) {
                ;
            }
            // TODO
            return Side.TOP;
        }

    }

}