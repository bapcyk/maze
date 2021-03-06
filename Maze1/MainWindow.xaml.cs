﻿using System;
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
            Algorithms.Items.Add("Random divided rooms");
            Algorithms.Items.Add("Random turtle");
        }

        private void ClearMaze() {
            for (int i = Canvas.Children.Count - 1; i >= 0; i--) {
                Canvas.Children.Remove(Canvas.Children[i]); // FIXME does not clear all
            }
        }

        private async void Algorithms_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            switch (Algorithms.SelectedIndex) {
                case 0:
                    ClearMaze();
                    Alg1.Room room = Alg1.Room.Initial(10, 10, (int)Canvas.Width - 10, (int)Canvas.Height - 20);
                    Alg1.Room[] rooms = Alg1.Room.Maze(room).ToArray();
                    int roomsNumber = rooms.Length + 1; // +1 for original room
                    room.Draw(Canvas);
                    int i = 1;
                    DrawnPercent.Content = $"Drawn: {i}/{roomsNumber}";
                    foreach (Alg1.Room r in rooms.OrderByDescending(r => r.Area())) {
                        r.Draw(Canvas);
                        await Task.Delay(300);
                        i++;
                        DrawnPercent.Content = $"Drawn: {i}/{roomsNumber}";
                    }
                    DrawnPercent.Content = $"Done: {i}/{roomsNumber}";
                    break;

                case 1:
                    ClearMaze();
                    Alg2.Grid grid = new Alg2.Grid((int)Canvas.Width, (int)Canvas.Height, 10, 10);
                    for (int j=0; j<999; j++) {
                        grid.Dir = Utils.RandomSide();
                        grid.Forward(1);
                    }
                    grid.Draw(Canvas);
                    break;
            }
        }
    }
}
