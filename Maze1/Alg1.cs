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
        public static Edge Simple(int isoline, int a, int b, Direct dir) {
            return new Edge(isoline, new Segment[] { new Segment(a, b, true) }, dir);
        }

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
                if (IsPairOk(a, b)) yield return new Segment(a, b, segments[0].visible);
            }
            int i = 0;
            foreach (Segment seg in segments) {
                if (i == 0) {
                    (a, b) = (seg.a + Utils.DOOR, seg.b);
                    Trace.Assert(seg.visible, "Edge's first segment cannot be door");
                    if (IsPairOk(a, b)) yield return new Segment(a, b, seg.visible);
                }
                else if (i == segments.Length - 1) {
                    (a, b) = (seg.a, seg.b - Utils.DOOR);
                    Trace.Assert(seg.visible, "Edge's last segment cannot be door");
                    if (IsPairOk(a, b)) yield return new Segment(a, b, seg.visible);
                }
                else if (seg.visible) {
                    yield return seg;
                }
                i++;
            }
        }

        protected Tuple<Edge, Edge> Divide() {
            Segment[] spaces = SpacesForDivider().ToArray();
            if (spaces.Length == 0) {
                return null; // no more spaces for division
            }
            else {
                int dividerSegmentIdx = Utils.Randomizer.Next(spaces.Length);
                Segment dividerSpace = spaces[dividerSegmentIdx];
                int dividerPt = Utils.Randomizer.Next(dividerSpace.a, dividerSpace.b + 1);
                return Split(dividerPt);
            }
        }

    }

    public class Room : ICloneable {
        private readonly Edge[] edges = new Edge[4];

        public Room(Edge[] edges) {
            Trace.Assert(edges.Length == 4, "Room needs 4 edges");
            this.edges = edges;
        }

        public Room(Edge top, Edge right, Edge bottom, Edge left) {
            this.edges[0] = top;
            this.edges[1] = right;
            this.edges[2] = bottom;
            this.edges[3] = left;
        }

        //////////////////////////////// Factories ////////////////////////////////////
        public static Room Initial(int x1, int y1, int x2, int y2) =>
            new Room(Edge.WithRandomDoor(y1, (Segment)(x1, x2), Direct.H),
                     Edge.Simple(x2, y1, y2, Direct.V),
                     Edge.WithRandomDoor(y2, (Segment)(x1, x2), Direct.H),
                     Edge.Simple(x1, y1, y2, Direct.V));

        //////////////////////////////// ToString /////////////////////////////////////
        public override string ToString() {
            string es = String.Join(", ", edges.Select(s => s.ToString()));
            return $"<Room {es} Room>";
        }

        /////////////////////////////// IClonable /////////////////////////////////////
        public object Clone() {
            return new Room(edges.Select(e => (Edge)e.Clone()).ToArray());
        }

        /////////////////////////////// Other methods /////////////////////////////////
        public void Draw(Canvas cnv) {
            foreach (Edge e in edges) e.Draw(cnv);
        }

        protected int OppositeEdgeIndex(int edgeIndex) => (edgeIndex + 2) % 4;
        protected int NextEdgeIndex(int edgeIndex) => (edgeIndex + 1) % 4;

        //protected Tuple<Room, Room> Divide() {
        //    ;
        //}

    }
}