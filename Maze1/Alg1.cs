using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Maze;

namespace Alg1 {

    public class Edge : ICloneable {
        public int isoline;
        public Segment[] segments;
        public Direct direct;

        public Edge(int i, Segment[] ps, Direct d) {
            isoline = i;
            segments = ps.OrderBy(e => e, Utils.SegCmp).ToArray();
            direct = d;
        }
        public static Edge MakeWithDoor(int i, Segment bounds, Direct d) {
            if (bounds.b - bounds.a + 1 >= (3*Utils.DOOR)) {
                int pt = Utils.Randomizer.Next(bounds.a + Utils.DOOR, bounds.b - 2 * Utils.DOOR);
                return new Edge(i, new Segment[] { (bounds.a, pt), (pt + Utils.DOOR, bounds.b) }, d);
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
            int i = 0, len = segments.Length;
            foreach (Segment seg in segments) {
                if (len == 1) {
                    Segment p = new Segment(seg.a + Utils.DOOR, seg.b - Utils.DOOR);
                    if (p.IsNormal() && !p.IsEmpty()) yield return p;
                }
                else if (i == 0) {
                    // 1st seg need margin from the wall (for a motion near the wall)
                    Segment p = new Segment(seg.a + Utils.DOOR, seg.b);
                    if (p.IsNormal() && !p.IsEmpty()) yield return p;
                }
                else if (i == len - 1) {
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

        public Tuple<Edge, Edge> Divide(int pt) {
            Segment[] nosegments = new Segment[]{};
            // segments of 1st edge (before division point) and 2nd edge (after the division point)
            List<Segment> ps1 = new List<Segment>(), ps2 = new List<Segment>();
            Edge e1 = new Edge(isoline, nosegments, direct), e2 = new Edge(isoline, nosegments, direct);
            bool divided = false;
            foreach ((var a, var b) in segments) {
                if (a <= pt && pt <= b) {
                    ps1.Add(new Segment(a, pt));
                    ps2.Add(new Segment(pt, b));
                    divided = true;
                }
                else {
                    if (divided) ps1.Add(new Segment(a, b));
                    else ps2.Add(new Segment(a, b));
                }
            }
            if (divided) {
                e1.segments = ps1.ToArray();
                e2.segments = ps2.ToArray();
                return new Tuple<Edge, Edge>(e1, e2);
            }
            else return null;
        }

        public Tuple<Edge, Edge> Divide() {
            foreach (Segment seg in SpacesForDivider()) {
                int pt = Utils.Randomizer.Next(seg.a, seg.b);
                Tuple<Edge, Edge> edges = Divide(pt);
                if (edges != null) return edges;
            }
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

        public Tuple<Room, Room> Divide() {
            Room room1 = (Room)Clone(), room2 = (Room)Clone();
            int dividedEdgeIndex = -1, i = 0;
            Tuple<Edge, Edge> dividedEdges = null, dividedOppositeEdges = null;
            foreach (Edge edge in edges) {
                dividedEdges = edge.Divide();
                if (dividedEdges != null) {
                    dividedEdgeIndex = i;
                    break;
                }
                i++;
            }
            if (dividedEdges == null) return null;
            else {
                // found place for divider and edge was divided
                int dividedOppositeEdgeIndex = OppositeEdgeIndex(dividedEdgeIndex);
                int dividerPt = dividedEdges.Item1.Bounds().b;
                dividedOppositeEdges = edges[dividedOppositeEdgeIndex].Divide(dividerPt);
                if (dividedOppositeEdges == null) return null;
                else {
                    int dividedNextEdgeIndex = NextEdgeIndex(dividedEdgeIndex);
                    room1.edges[dividedEdgeIndex] = dividedEdges.Item1;
                    room2.edges[dividedEdgeIndex] = dividedEdges.Item2;
                    room1.edges[dividedOppositeEdgeIndex] = dividedOppositeEdges.Item1;
                    room2.edges[dividedOppositeEdgeIndex] = dividedOppositeEdges.Item2;
                    Edge dividerProto = edges[dividedNextEdgeIndex];
                    //Edge divider = new Edge(dividerPt,
                    //new Segment[] { dividerProto.Bounds() },
                    //dividerProto.direct);
                    Edge divider = Edge.MakeWithDoor(dividerPt, dividerProto.Bounds(), dividerProto.direct);
                    if (divider == null) return null;
                    else {
                        room1.edges[dividedNextEdgeIndex] = divider;
                        room2.edges[OppositeEdgeIndex(dividedNextEdgeIndex)] = (Edge)divider.Clone();
                        // TODO make door
                        return new Tuple<Room, Room>(room1, room2);
                    }
                }
            }
        }
    }

}