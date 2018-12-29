using Maze;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;

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
            Trace.Assert(VerifyPoints((x1, y1), (x2, y2)), "Only horizontal and vertical lines are allowed");
            P1 = (x1, y1);
            P2 = (x2, y2);
        }

        public Line(Point p1, Point p2) {
            Trace.Assert(VerifyPoints(p1, p2), "Only horizontal and vertical lines are allowed");
            P1 = p1;
            P2 = p2;
        }

        //////////////////////////// Deconstruct to pair //////////////////////////////
        public void Deconstruct(out Point x, out Point y) {
            x = P1;
            y = P2;
        }

        //////////////////////////////// Other methods ////////////////////////////////
        private static bool VerifyPoints(Point p1, Point p2) => p1.X == p2.X || p1.Y == p2.Y;

    }

    public class Grid {
        private readonly int FullWidth;
        private readonly int FullHeight;
        private readonly int Width;
        private readonly int Height;
        private readonly int ColsNum;
        private readonly int RowsNum;
        private readonly int SquareSide;
        public int XCor { get; protected set; } = 0;
        public int YCor { get; protected set; } = 0;
        public Side Dir = Side.TOP; // direction (like angle)
        private LinkedList<Line> Lines = new LinkedList<Line>();

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

        public bool Forward(int step) {
            int newX = XCor, newY = YCor;
            switch (Dir) {
                case Side.TOP:
                    newY = YCor - step;
                    break;
                case Side.BOTTOM:
                    newY = YCor + step;
                    break;
                case Side.RIGHT:
                    newX = XCor + step;
                    break;
                case Side.LEFT:
                    newX = XCor - step;
                    break;
            }
            if (newX >= 0 && newX < ColsNum && newY >= 0 && newY < RowsNum) {
                Lines.AddLast(new Line(XCor, YCor, newX, newY));
                XCor = newX;
                YCor = newY;
                return true;
            }
            else {
                return true;
            }
        }

        public void Draw(Canvas cnv) {
            foreach (Line ln in Lines) {
                ((int x1, int y1), (int x2, int y2)) = ln;
                x1 *= SquareSide;
                x2 *= SquareSide;
                y1 *= SquareSide;
                y2 *= SquareSide;
                Utils.DrawLine(cnv, x1, y1, x2, y2);
            }
        }

    }
}