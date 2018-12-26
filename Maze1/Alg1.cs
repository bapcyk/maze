using System;
using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Maze;

namespace Alg1 {
    struct EdgeOfDivider {
        public readonly int EdgeIdx;
        public readonly int Point;
        public readonly Tuple<Edge, Edge> Edges;
        public EdgeOfDivider(int edgeIdx, int point, Tuple<Edge, Edge> edges) {
            EdgeIdx = edgeIdx;
            Point = point;
            Edges = edges;
        }
    }

    class Edge : ICloneable {
        public int Isoline { get; }
        public Direct Dir { get; }
        public Segment[] Segments { get; } = new Segment[0];

        public Edge(int isoline, Segment[] segs, Direct dir) {
            Trace.Assert(segs.Length != 0, "Empty edge");
            this.Isoline = isoline;
            this.Segments = segs.OrderBy(e => e, Utils.SegCmp).ToArray();
            this.Segments = segs;
            this.Dir = dir;
        }

        ////////////////////////////// Factories //////////////////////////////////////
        public static Edge Simple(int isoline, int a, int b, Direct dir) {
            return new Edge(isoline, new Segment[] { new Segment(a, b, true) }, dir);
        }

        public static Edge WithRandomDoor(int isoline, Segment bounds, Direct dir) {
            if (bounds.LinearSize() > (3 * Utils.DOOR)) {
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
            if (bounds.LinearSize() > (2 * Utils.DOOR)) {
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
        public bool IsPointInDoor(int pt) {
            Trace.Assert(Bounds().ContainsPoint(pt), "Point out of edge bounds");
            Segment? seg = SegmentWithPoint(pt);
            Trace.Assert(seg != null, "No segment with this point");
            return !seg?.visible?? true;
        }

        private bool IsDoor() => DoorsNumber() != 0;

        ///////////////////////////////// ToString ////////////////////////////////////
        public override string ToString() {
            string ss = String.Join(", ", Segments.Select(s => s.ToString()));
            return $"<Edge {Dir} {Isoline} % {ss} Edge>";
        }

        //////////////////////////////// ICloneable ///////////////////////////////////
        public object Clone() => new Edge(Isoline, Segments.Select(s => (Segment)s.Clone()).ToArray(), Dir);

        ///////////////////////////// Other methods ///////////////////////////////////
        public void Draw(Canvas cnv) {
            foreach (Segment seg in Segments) {
                Trace.Assert(!seg.IsEmpty(), "Attempt to draw empty segment");
                if (seg.Visible) Utils.DrawLine(cnv, Isoline, seg.a, seg.b, Dir);
            }
        }

        private int DoorsNumber() => Segments.Where(s => !s.Visible).Count();

        public Segment Bounds() {
            (int a, int b) = (Segments[0].a, Segments.Last().b);
            Trace.Assert(b > a, "Abnormal bounds");
            return (a, b);
        }

        private Segment? SegmentWithPoint(int pt) {
            foreach (Segment seg in Segments) {
                if (seg.ContainsPoint(pt) || pt == seg.a || pt == seg.b) return seg;
            }
            return null;
        }

        public Tuple<Edge, Edge> Split(int pt) {
            Trace.Assert(Bounds().ContainsPoint(pt), "Point out of edge bounds");
            LinkedList<Segment> segsOfPart1 = new LinkedList<Segment>(), segsOfPart2 = new LinkedList<Segment>();
            bool divided = false;
            foreach (Segment seg in Segments) {
                if (divided) { // already divided, so add to 2nd part
                    segsOfPart2.AddLast(seg);
                }
                else if (pt == seg.a) {
                    segsOfPart2.AddLast(seg);
                    divided = true;
                }
                else if (pt == seg.b) {
                    segsOfPart1.AddLast(seg);
                    divided = true;
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
            //if (0==segsOfPart1.Count())
            Trace.Assert(segsOfPart1.Count() != 0, "First part of split is empty");
            //if (0==segsOfPart2.Count())
            Trace.Assert(segsOfPart2.Count() != 0, "Second part of split is empty");
            return new Tuple<Edge, Edge>(new Edge(Isoline, segsOfPart1.ToArray(), Dir),
                                         new Edge(Isoline, segsOfPart2.ToArray(), Dir));
        }

        protected IEnumerable<Segment> SpacesForDivider() {
            // Is Pair OK: normal?
            bool IsPairOk(int x, int y) => x >= 0 && y >= 0 && y - x > 1;

            int a, b;
            if (Segments.Length == 1) {
                Trace.Assert(Segments[0].Visible, "Edge cannot contain door only");
                (a, b) = (Segments[0].a + Utils.DOOR, Segments[0].b - Utils.DOOR);
                if (IsPairOk(a, b)) yield return new Segment(a, b, Segments[0].Visible);
            }
            int i = 0;
            foreach (Segment seg in Segments) {
                if (i == 0) {
                    (a, b) = (seg.a + Utils.DOOR, seg.b - Utils.DOOR);
                    //Trace.Assert(seg.Visible, "Edge's first segment cannot be door");
                    if (IsPairOk(a, b)) yield return new Segment(a, b, seg.Visible);
                }
                else if (i == Segments.Length - 1) {
                    (a, b) = (seg.a + Utils.DOOR, seg.b - Utils.DOOR);
                    //Trace.Assert(seg.Visible, "Edge's last segment cannot be door");
                    if (IsPairOk(a, b)) yield return new Segment(a, b, seg.Visible);
                }
                else if (seg.Visible) {
                    yield return seg;
                }
                i++;
            }
        }

        public Tuple<Edge, Edge> Divide() {
            Segment[] spaces = SpacesForDivider().ToArray();
            if (spaces.Length == 0) {
                return null; // no more spaces for division
            }
            else {
                int divSegmentIdx = Utils.Randomizer.Next(spaces.Length);
                Segment divSpace = spaces[divSegmentIdx];
                int divPt = Utils.Randomizer.Next(divSpace.a + 1, divSpace.b);
                Utils.Log($"Random {divPt} in space {divSpace}; edge {this}");
                return Split(divPt);
            }
        }

    }

    class Room : ICloneable {
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

        protected bool RandomDivider(out int edgeIdx, out int divPt, out Tuple<Edge, Edge> divEdges) {
            IEnumerable<EdgeOfDivider> PossibleDividers() {
                int i = 0, pt;
                Tuple<Edge, Edge> divedges = null;
                foreach (Edge edge in edges) {
                    divedges = edge.Divide();
                    if (divedges != null) {
                        pt = divedges.Item1.Bounds().b;
                        yield return new EdgeOfDivider(i, pt, divedges);
                    }
                    i++;
                }
            }

            EdgeOfDivider[] possibleDividers = PossibleDividers().ToArray();
            if (possibleDividers.Length == 0) {
                edgeIdx = 0;
                divPt = 0;
                divEdges = null;
                return false;
            }
            else {
                EdgeOfDivider found = possibleDividers[Utils.Randomizer.Next(0, possibleDividers.Length)];
                edgeIdx = found.EdgeIdx;
                divPt = found.Point;
                divEdges = found.Edges;
                return true;
            }
        }

        protected Tuple<Room, Room> Divide() {
            if (!RandomDivider(out int edgeIdx, out int divPt, out Tuple<Edge, Edge> divEdges)) {
                return null;
            }
            else {
                // random edge for divider was found, so try to unconditionally divide opposite edge
                int oppEdgeIdx = OppositeEdgeIndex(edgeIdx); // index of edge opposite to edge where divider begins
                Tuple<Edge, Edge> oppDivEdges = edges[oppEdgeIdx].Split(divPt);
                if (oppDivEdges == null) return null; // if it's impossible, divide of room is impossible
                else {
                    Edge[] edges1 = new Edge[4], edges2 = new Edge[4]; // edges of new rooms
                    Edge divider = null;
                    int nextEdgeIdx = NextEdgeIndex(edgeIdx); // edge index next to the edge where divider begins
                    Edge nextEdge = edges[nextEdgeIdx]; // edge next to the divider origin edge
                    // solve length of divider (if opposite edge is door, then make it shorter)
                    if (edges[oppEdgeIdx].IsPointInDoor(divPt)) {
                        // got in door - make divider shorter
                        if (edgeIdx == 0 || edgeIdx == 3) {
                            // Orientation of divider leads to door at end
                            divider = Edge.WithEndDoor(divPt, nextEdge.Bounds(), nextEdge.Dir, End.END);
                        }
                        else {
                            // Orientation of divider leads to door at start
                            divider = Edge.WithEndDoor(divPt, nextEdge.Bounds(), nextEdge.Dir, End.START);
                        }
                    }
                    else {
                        // no door, so make divider with random placed door
                        divider = Edge.WithRandomDoor(divPt, nextEdge.Bounds(), nextEdge.Dir);
                    }
                    if (divider == null) {
                        // no space for door
                        return null;
                    }
                    else {
                        int lastEdgeIdx = NextEdgeIndex(oppEdgeIdx); // last edge of room1,2
                        // room1
                        edges1[edgeIdx] = divEdges.Item1;
                        edges1[nextEdgeIdx] = divider;
                        edges1[oppEdgeIdx] = oppDivEdges.Item1;
                        edges1[lastEdgeIdx] = edges[lastEdgeIdx];
                        // room2
                        edges2[edgeIdx] = divEdges.Item2;
                        edges2[nextEdgeIdx] = edges[nextEdgeIdx];
                        edges2[oppEdgeIdx] = oppDivEdges.Item2;
                        edges2[lastEdgeIdx] = divider;
                        return new Tuple<Room, Room>(new Room(edges1), new Room(edges2));
                    }
                }
            }
        }

        public static IEnumerable<Room> Maze(Room room) {
            Tuple<Room, Room> dividedRooms = null;
            Utils.Log($"Maze({room})");
            dividedRooms = room.Divide();
            if (dividedRooms == null) yield return room;
            else {
                foreach (var r in Maze(dividedRooms.Item1)) yield return r;
                foreach (var r in Maze(dividedRooms.Item2)) yield return r;
            }
        }

    }
}