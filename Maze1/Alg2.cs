using Maze;
using System;
using System.Diagnostics;

namespace Alg2 {
    struct Line {
        public Direct Dir {
            get {
                if (P1.X == P2.X) return Direct.V;
                else return Direct.H; // otherwise they can be only H
            }
        }
        public Point[] ToArray { get => new Point[] { P1, P2 }; }

        private readonly Point P1, P2;


        public Line(int x1, int y1, int x2, int y2) {
            Trace.Assert(AssertPoints((x1, y1), (x2, y2)), "Only horizontal and vertical lines are allowed");
            P1 = (x1, y1);
            P2 = (x2, y2);
        }

        public Line(Point p1, Point p2) {
            Trace.Assert(AssertPoints(p1, p2), "Only horizontal and vertical lines are allowed");
            P1 = p1;
            P2 = p2;
        }


        //////////////////////////////// Other methods ////////////////////////////////
        private static bool AssertPoints(Point p1, Point p2) => p1.X == p2.X || p1.Y == p2.Y;

    }

    public class Grid {
        private readonly int FullWidth;
        private readonly int FullHeight;
        private readonly int Width;
        private readonly int Height;
        private readonly int ColsNum;
        private readonly int RowsNum;
        private readonly int SquareSide;
        public int XCor { get; set; } = 0;
        public int YCor { get; set; } = 0;

        public Grid(int width, int height, int margin, int squareSide) {
            FullWidth = width;
            FullHeight = height;
            Width = width - margin;
            Height = height - margin;
            SquareSide = squareSide;
            ColsNum = Width / SquareSide;
            RowsNum = Height / SquareSide;
            XCor = ColsNum / 2;
            YCor = RowsNum / 2;
        }

        public void Forward(int step) {
            ;
        }

    }
}