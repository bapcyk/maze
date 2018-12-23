using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maze;
using Alg1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze.Tests {
    [TestClass()]
    public class segTests {
        public void Cmp(int a, int b, int c, int d, CmpResult res) {
            Segment p1 = new Segment(a, b), p2 = new Segment(c, d);
            SegmentsComparer cmp = new SegmentsComparer();
            switch (res) {
                case CmpResult.GT: Assert.IsTrue(cmp.Compare(p1, p2) > 0);
                    break;
                case CmpResult.LT: Assert.IsTrue(cmp.Compare(p2, p1) > 0);
                    break;
                case CmpResult.GE: Assert.IsTrue(cmp.Compare(p1, p2) >= 0);
                    break;
                case CmpResult.LE: Assert.IsTrue(cmp.Compare(p2, p1) >= 0);
                    break;
                case CmpResult.EQ: Assert.IsTrue(cmp.Compare(p2, p1) == 0);
                    break;
            }
        }
        [TestMethod()]
        public void Cmpsegments1() { Cmp(3, 7, 1, 5, CmpResult.GT); }
        [TestMethod()]
        public void Cmpsegments2() { Cmp(1, 7, 1, 5, CmpResult.GT); }
        [TestMethod()]
        public void Cmpsegments3() { Cmp(1, 7, 3, 5, CmpResult.LT); }
        [TestMethod()]
        public void Cmpsegments4() { Cmp(3, 7, 3, 7, CmpResult.EQ); }
        [TestMethod()]
        public void Cmpsegments5() { Cmp(1, 7, 3, 8, CmpResult.LT); }
        [TestMethod()]
        public void Cmpsegments6() { Cmp(1, 7, 3, 7, CmpResult.LT); }
    }
}