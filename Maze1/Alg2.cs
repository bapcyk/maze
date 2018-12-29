using Maze;
using System;

namespace Alg2 {
    class Line : ICloneable {
        public Direct Dir { get; }
        public Segment seg = new Segment();

        ///////////////////////////// ICloneable //////////////////////////////////////
        public object Clone() {
            throw new NotImplementedException();
        }

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