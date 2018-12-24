using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Maze;

namespace Alg1 {

    // (index of edge, divider point coord, edges)
    using DividedEdges = Tuple<int, int, Tuple<Edge, Edge>>;

    public class Edge : ICloneable {
        public int isoline;
        public Segment[] segments;
        public Direct direct;

        public override string ToString() {
            string segs = String.Join(", ", segments.Select(s => s.ToString()));
            return $"<Edge {isoline}|{segs}|{direct} Edge>";
        }

        public Edge(int i, Segment[] ps, Direct d) {
            isoline = i;
            segments = ps.OrderBy(e => e, Utils.SegCmp).ToArray();
            direct = d;
        }

        public static Edge WithRandomDoor(int i, Segment bounds, Direct d) {
            if (bounds.LinearSize() >= (3*Utils.DOOR)) {
                int pt = Utils.Randomizer.Next(bounds.a + Utils.DOOR, bounds.b - 2 * Utils.DOOR);
                return new Edge(i, new Segment[] { (bounds.a, pt), (pt + Utils.DOOR, bounds.b) }, d);
            }
            else return null;
        }

        public static Edge WithEndDoor(int i, Segment bounds, Direct d) {
            if (bounds.LinearSize() >= (2*Utils.DOOR)) {
                return new Edge(i, new Segment[] { (bounds.a, bounds.b - Utils.DOOR) }, d);
            }
            else return null;
        }

        public void Draw(Canvas cnv) {
            foreach ((var a, var b) in segments) {
                if (a != b) Utils.DrawLine(cnv, isoline, a, b, direct);
            }
        }

        public bool PointInDoor(int pt) {
            bool foundWall = false;
            foreach ((var a, var b) in segments) {
                if (a <= pt && pt <= b) {
                    foundWall = true;
                    break;
                }
            }
            return !foundWall;
        }

        protected IEnumerable<Segment> SpacesForDivider() {
            int i = 0;
            if (segments.Length == 1) {
                Segment p = new Segment(segments[0].a + Utils.DOOR, segments[0].b - Utils.DOOR);
                if (p.IsNormal() && !p.IsEmpty()) yield return p;
            }
            else {
                foreach (Segment seg in segments) {
                    if (i == 0) {
                            // 1st seg need margin from the wall (for a motion near the wall)
                            Segment p = new Segment(seg.a + Utils.DOOR, seg.b);
                            if (p.IsNormal() && !p.IsEmpty()) yield return p;
                        }
                        else if (i == segments.Length - 1) {
                            // last seg also need margin from the wall (for a motion near the wall)
                            Segment p = new Segment(seg.a, seg.b + Utils.DOOR);
                            if (p.IsNormal() && !p.IsEmpty()) yield return p;
                        }
                        else {
                            // other segments don't need margins
                            yield return seg;
                        }
                    i++;
                }
            }
        }

        // FIXME leads to error
        public Tuple<Edge, Edge> ForceDivide(int pt) {
            bool divided = false;
            LinkedList<Segment> ps1 = new LinkedList<Segment>(), ps2 = new LinkedList<Segment>();
            foreach (Segment seg in segments) {
                if (divided) {
                    ps2.AddLast(seg);
                }
                else {
                    if (seg.a <= pt && pt <= seg.b) {
                        ps1.AddLast(new Segment(seg.a, pt));
                        ps2.AddLast(new Segment(pt, seg.b));
                        divided = true;
                    }
                    if (pt <= seg.a) {
                        ps2.AddLast(seg);
                        divided = true;
                    }
                }
            }
            if (divided) {
                return new Tuple<Edge, Edge>(new Edge(isoline, ps1.ToArray(), direct),
                                             new Edge(isoline, ps2.ToArray(), direct));
            }
            else {
                return null;
            }
        }

        public Tuple<Edge, Edge> Divide(int pt) {
            //Contract.Requires(Bounds().ContainsPoint(pt)); // FIXME fails
            //Contract.Ensures(Contract.Result<Tuple<Edge, Edge>>() );
            // segments of 1st edge (before division point) and 2nd edge (after the division point)
            LinkedList<Segment> ps1 = new LinkedList<Segment>(), ps2 = new LinkedList<Segment>();
            bool divided = false;
            foreach ((var a, var b) in segments) {
                if (a <= pt && pt <= b) {
                    ps1.AddLast(new Segment(a, pt));
                    ps2.AddLast(new Segment(pt, b));
                    divided = true;
                }
                else {
                    if (divided) ps1.AddLast(new Segment(a, b));
                    else ps2.AddLast(new Segment(a, b));
                }
            }
            if (divided) {
                return new Tuple<Edge, Edge>(new Edge(isoline, ps1.ToArray(), direct),
                                             new Edge(isoline, ps2.ToArray(), direct));
            }
            else {
                return null;
            }
        }

        public Tuple<Edge, Edge> Divide(out int dividerPoint) {
            foreach (Segment seg in SpacesForDivider()) {
                dividerPoint = Utils.Randomizer.Next(seg.a, seg.b + 1);
                Tuple<Edge, Edge> edges = Divide(dividerPoint);
                if (edges != null) return edges;
            }
            dividerPoint = 0;
            return null;
        }

        public Segment Bounds() {
            Debug.Assert(segments.Length > 0);
            return new Segment(segments[0].a, segments.Last().b);
        }

        public object Clone() {
            Segment[] ps = new Segment[segments.Length];
            Array.Copy(segments, ps, segments.Length);
            return new Edge(isoline, ps, direct);
        }
    }

    public class Room : ICloneable {
        public Edge[] edges;

        public override string ToString() {
            string es = String.Join(", ", edges.Select(s => s.ToString()));
            return $"<Room {es} Room>";
        }
        public Room(Edge[] es) {
            edges = es;
        }

        public object Clone() {
            Edge[] es = new Edge[edges.Length];
            Array.Copy(edges.Select(e => e.Clone()).ToArray(), es, edges.Length);
            return new Room(es);
        }

        public void Draw(Canvas cnv) {
            foreach (Edge e in edges) e.Draw(cnv);
        }

        protected int OppositeEdgeIndex(int edgeIndex) => (edgeIndex + 2) % 4;
        protected int NextEdgeIndex(int edgeIndex) => (edgeIndex + 1) % 4;

        protected IEnumerable<DividedEdges> DividersEdges() {
            int i = 0;
            Tuple<Edge, Edge> dividedEdges = null;
            foreach (Edge edge in edges) {
                dividedEdges = edge.Divide(out int pt);
                if (dividedEdges != null) {
                    yield return new DividedEdges(i, pt, dividedEdges);
                }
                i++;
            }
        }

        // FIXME compare with null with ReferenctialEquals()
        public Tuple<Room, Room> Divide() {
            Room room1 = (Room)Clone(), room2 = (Room)Clone();
            DividedEdges[] allDividers = DividersEdges().ToArray();
            if (allDividers.Length == 0)
                return null;
            else {
                (var dividedEdgeIndex, var dividerPt, var dividedEdges) =
                    allDividers[Utils.Randomizer.Next(allDividers.Length)];
                // found place for divider and edge was divided
                Tuple<Edge, Edge> dividedOppositeEdges = null;
                int dividedOppositeEdgeIndex = OppositeEdgeIndex(dividedEdgeIndex);
                dividedOppositeEdges = edges[dividedOppositeEdgeIndex].ForceDivide(dividerPt);
                if (dividedOppositeEdges == null)
                    return null;
                else {
                    int dividedNextEdgeIndex = NextEdgeIndex(dividedEdgeIndex);
                    room1.edges[dividedEdgeIndex] = dividedEdges.Item1;
                    room2.edges[dividedEdgeIndex] = dividedEdges.Item2;
                    room1.edges[dividedOppositeEdgeIndex] = dividedOppositeEdges.Item1;
                    room2.edges[dividedOppositeEdgeIndex] = dividedOppositeEdges.Item2;

                    Edge dividerProto = edges[dividedNextEdgeIndex];
                    Edge divider = null;
                    if (edges[dividedOppositeEdgeIndex].PointInDoor(dividerPt)) {
                        // FIXME what if door must be at begining too?
                        divider = Edge.WithEndDoor(dividerPt, dividerProto.Bounds(), dividerProto.direct);
                    }
                    else {
                        divider = Edge.WithRandomDoor(dividerPt, dividerProto.Bounds(), dividerProto.direct);
                    }

                    if (divider == null)
                        return null;
                    else {
                        room1.edges[dividedNextEdgeIndex] = divider;
                        room2.edges[OppositeEdgeIndex(dividedNextEdgeIndex)] = (Edge)divider.Clone();
                        return new Tuple<Room, Room>(room1, room2);
                    }
                }
            }
        }

        public static IEnumerable<Room> Maze(Room room) {
            Tuple<Room, Room> dividedRooms = null;
            Utils.Tracer.TraceEvent(TraceEventType.Information, 0, "Maze({0})", room);
            dividedRooms = room.Divide();
            if (dividedRooms == null) yield return room;
            else {
                foreach (var r in Maze(dividedRooms.Item1)) yield return r;
                foreach (var r in Maze(dividedRooms.Item2)) yield return r;
            }
        }
    }

}