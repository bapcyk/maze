using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Maze;

namespace Maze1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Algorithms.Items.Add("Dummy maze 1");
            Algorithms.Items.Add("Maze 2");
        }

        private void Algorithms_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Alg1.Room r = new Alg1.Room(new Alg1.Edge[] {
                new Alg1.Edge(10, new Segment[] { (10, 500) }, Direct.H),
                new Alg1.Edge(500, new Segment[] { (10, 200) }, Direct.V),
                new Alg1.Edge(200, new Segment[] { (10, 500) }, Direct.H),
                new Alg1.Edge(10, new Segment[] { (10, 200) }, Direct.V)
            });
            (Alg1.Room r1, Alg1.Room r2) = r.Divide();
            //(Alg1.Room r3, Alg1.Room r4) = r1.Divide();
            //(Alg1.Room r5, Alg1.Room r6) = r2.Divide();
            r1.Draw(Canvas);
            r2.Draw(Canvas);
            //r3.Draw(Canvas);
            //r4.Draw(Canvas);
            //r5.Draw(Canvas);
            //r6.Draw(Canvas);
            //Utils.DrawLine(Canvas, 50, 10, 100, Direct.V);
        }
    }
}
