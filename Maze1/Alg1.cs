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

        public Edge(int iso, Segment[] segs, Direct d) {
            Trace.Assert(segs.Length != 0, "Empty edge");
            isoline = iso;
            segments = segs;
            dir = d;
        }

        //////////////////////////////// Predicates ///////////////////////////////////
        private bool IsPointInDoor(int pt) {
            Trace.Assert(Bounds().ContainsPoint(pt), "Point out of edge bounds");
            Segment? seg = SegmentWithPoint(pt);
            Trace.Assert(seg != null, "No segment with this point");
            return !seg?.visible?? true;
        }

        ///////////////////////////////// ToString ////////////////////////////////////
        public override string ToString() {
            string ss = String.Join(", ", segments.Select(s => s.ToString()));
            return $"<Edge {dir} {isoline}=>{ss} Edge>";
        }

        //////////////////////////////// ICloneable ///////////////////////////////////
        public object Clone() => new Edge(isoline, segments.Select(s => (Segment)s.Clone()).ToArray(), dir);

        ///////////////////////////// Other methods ///////////////////////////////////
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

    }
}