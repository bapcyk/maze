using System;
using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Maze;

namespace Alg1 {
    public class Edge : ICloneable {
        private readonly int isoline;
        private readonly Direct dir;
        private readonly Segment[] segments = new Segment[0];

        public Edge(int isoline, Segment[] segs, Direct dir) {
            Trace.Assert(segs.Length != 0, "Empty edge");
            this.isoline = isoline;
            this.segments = segs.OrderBy(e => e, Utils.SegCmp).ToArray();
            this.segments = segs;
            this.dir = dir;
        }

        ////////////////////////////// Factories //////////////////////////////////////
        public static Edge WithRandomDoor(int isoline, Segment bounds, Direct dir) {
            if (bounds.LinearSize() >= (3 * Utils.DOOR)) {
                int pt = Utils.Randomizer.Next(bounds.a + Utils.DOOR, bounds.b - 2 * Utils.DOOR);
                Edge ret = new Edge(isoline,
                    new Segment[]{ new Segment(bounds.a, pt, true),
                                   new Segment(pt, pt + Utils.DOOR, false),
                                   new Segment(pt + Utils.DOOR, bounds.b, true) },
                    dir);
                Trace.Assert(ret.Bounds().LinearSize() >= (3 * Utils.DOOR), "Edge with random door is too small");
                return ret;
            }
            else return null;
        }

        public static Edge WithEndDoor(int isoline, Segment bounds, Direct dir, End end) {
            if (bounds.LinearSize() >= (2 * Utils.DOOR)) {
                Edge ret = null;
                switch (end) {
                    case End.START:
                        ret = new Edge(isoline,
                            new Segment[] { new Segment(bounds.a, bounds.a + Utils.DOOR, false),
                                            new Segment(bounds.a + Utils.DOOR, bounds.b, true) },
                            dir);
                        break;
                    case End.END:
                        ret = new Edge(isoline,
                            new Segment[] { new Segment(bounds.a, bounds.b - Utils.DOOR, true),
                                            new Segment(bounds.b - Utils.DOOR, bounds.b, false) },
                            dir);
                        break;
                }
                Trace.Assert(ret.Bounds().LinearSize() >= (2 * Utils.DOOR), "Edge with end door is too small");
                return ret;
            }
            else return null;
        }

        //////////////////////////////// Predicates ///////////////////////////////////
        private bool IsPointInDoor(int pt) {
            Trace.Assert(Bounds().ContainsPoint(pt), "Point out of edge bounds");
            Segment? seg = SegmentWithPoint(pt);
            Trace.Assert(seg != null, "No segment with this point");
            return !seg?.visible?? true;
        }

        private bool IsDoor() => DoorsNumber() != 0;

        ///////////////////////////////// ToString ////////////////////////////////////
        public override string ToString() {
            string ss = String.Join(", ", segments.Select(s => s.ToString()));
            return $"<Edge {dir} {isoline}=>{ss} Edge>";
        }

        //////////////////////////////// ICloneable ///////////////////////////////////
        public object Clone() => new Edge(isoline, segments.Select(s => (Segment)s.Clone()).ToArray(), dir);

        ///////////////////////////// Other methods ///////////////////////////////////
        public void Draw(Canvas cnv) {
            foreach (Segment seg in segments) {
                Trace.Assert(!seg.IsEmpty(), "Attempt to draw empty segment");
                if (seg.visible) Utils.DrawLine(cnv, isoline, seg.a, seg.b, dir);
            }
        }

        private int DoorsNumber() => segments.Where(s => !s.visible).Count();

        private Segment Bounds() {
            (int a, int b) = (segments[0].a, segments.Last().b);
            Trace.Assert(b > a, "Abnormal bounds");
            return (a, b);
        }

        private Segment? SegmentWithPoint(int pt) {
            foreach (Segment seg in segments) {
                if (seg.ContainsPoint(pt)) return seg;
            }
            return null;
        }

        public Tuple<Edge, Edge> Split(int pt) {
            Trace.Assert(Bounds().ContainsPoint(pt), "Point out of edge bounds");
            LinkedList<Segment> segsOfPart1 = new LinkedList<Segment>(), segsOfPart2 = new LinkedList<Segment>();
            bool divided = false;
            foreach (Segment seg in segments) {
                if (divided) { // already divided, so add to 2nd part
                    segsOfPart2.AddLast(seg);
                }
                else if (seg.ContainsPoint(pt)) { // found segment with this point, split it to 2 parts
                    (Segment seg1, Segment seg2) = seg.Split(pt);
                    segsOfPart1.AddLast(seg1);
                    segsOfPart2.AddLast(seg2);
                    divided = true;
                }
                else { // not divided, not found point, so add to 1st part
                    segsOfPart1.AddLast(seg);
                }
            }
            return new Tuple<Edge, Edge>(new Edge(isoline, segsOfPart1.ToArray(), dir),
                                         new Edge(isoline, segsOfPart2.ToArray(), dir));
        }

        protected IEnumerable<Segment> SpacesForDivider() {
            // Is Pair OK: normal?
            bool IsPairOk(int x, int y) => x >= 0 && y >= 0 && y >= x;

            int a, b;
            if (segments.Length == 1) {
                Trace.Assert(segments[0].visible, "Edge cannot contain door only");
                (a, b) = (segments[0].a + Utils.DOOR, segments[0].b - Utils.DOOR);
                if (IsPairOk(a, b)) yield return new Segment(a, b, true);
            }
            int i = 0;
            foreach (Segment seg in segments) {
                if (i == 0) {
                    (a, b) = (seg.a + Utils.DOOR, seg.b);
                    Trace.Assert(seg.visible, "Edge's first segment cannot be door");
                    if (IsPairOk(a, b)) yield return new Segment(a, b, true);
                }
                else if (i == segments.Length - 1) {
                    (a, b) = (seg.a, seg.b - Utils.DOOR);
                    Trace.Assert(seg.visible, "Edge's last segment cannot be door");
                    if (IsPairOk(a, b)) yield return new Segment(a, b, true);
                }
                else if (seg.visible) {
                    yield return seg;
                }
                i++;
            }
        }

    }
}