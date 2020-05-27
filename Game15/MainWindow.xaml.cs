using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Game15
{
    public partial class MainWindow : Window
    {
        enum Direction { UP, RIGHT, DOWN, LEFT }

        const int mapSize = 4;
        Random rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 4; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int counter = 1;
            for (int row = 0; row < mapSize; row++)
            {
                for (int col = 0; col < mapSize; col++)
                {
                    TextBlock text = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = this.Height / 8,
                        TextWrapping = TextWrapping.Wrap,
                        Text = counter.ToString()
                    };
                    if (col == 3 && row == 3) text.Text = "";
                    MainGrid.Children.Add(text);
                    Grid.SetRow(text, row);
                    Grid.SetColumn(text, col);
                    counter++;
                }
            }

            Shuffle();
        }

        private void Shuffle()
        {
            List<TextBlock> blocks = MainGrid.Children.Cast<TextBlock>().ToList();

            for (int i = 0; i < 20; i++)
            {
                int id1 = rand.Next(0, blocks.Count);
                int id2 = rand.Next(0, blocks.Count);
                SwapBlocks(blocks[id1], blocks[id2]);
            }
        }

        private void SwapBlocks(TextBlock block1, TextBlock block2)
        {
            if (block1 == null || block2 == null) return;

            int row1 = Grid.GetRow(block1);
            int row2 = Grid.GetRow(block2);
            int col1 = Grid.GetColumn(block1);
            int col2 = Grid.GetColumn(block2);

            if (!InRange(row1, col1) || !InRange(row2, col2)) return;

            Grid.SetRow(block1, row2);
            Grid.SetRow(block2, row1);
            Grid.SetColumn(block1, col2);
            Grid.SetColumn(block2, col1);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int row = 0, col = 0;
            GetClickCoordinates(out row, out col);
            if (!InRange(row, col)) return;

            TextBlock block = MainGrid.Children.Cast<TextBlock>().FirstOrDefault(t => Grid.GetRow(t) == row && Grid.GetColumn(t) == col);
            if (block == null) return;

            TryMoveBlock(block, row, col);

            if (EndOfGame())
                if (MessageBox.Show("Do you want to start a new game?", "End of game", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    NewGame_Click(null, null);
        }

        private void TryMoveBlock(TextBlock block, int row, int col)
        {
            TextBlock empty = MainGrid.Children.Cast<TextBlock>().FirstOrDefault(t => t.Text == "");
            int eRow = Grid.GetRow(empty), eCol = Grid.GetColumn(empty);
            if (eRow != row && eCol != col) return;

            Direction dir = Direction.RIGHT;
            int steps = 0;
            if (eRow == row && eCol > col)
            {
                dir = Direction.RIGHT;
                steps = Math.Abs(eCol - col);
            }
            else if (eRow == row && eCol < col)
            {
                dir = Direction.LEFT;
                steps = Math.Abs(eCol - col);
            }
            else if (eCol == col && eRow > row)
            {
                dir = Direction.DOWN;
                steps = Math.Abs(eRow - row);
            }
            else if (eCol == col && eRow < row)
            {
                dir = Direction.UP;
                steps = Math.Abs(eRow - row);
            }

            MoveStack(steps, dir);
        }

        private void MoveStack(int steps, Direction dir)
        {
            for (int i = 0; i < steps; i++)
            {
                TextBlock empty = MainGrid.Children.Cast<TextBlock>().FirstOrDefault(t => t.Text == "");
                if (empty == null) continue;

                switch (dir)
                {
                    case Direction.UP:
                        SwapBlocks(empty, GetNear(empty, 1, 0));
                        break;
                    case Direction.DOWN:
                        SwapBlocks(empty, GetNear(empty, -1, 0));
                        break;
                    case Direction.RIGHT:
                        SwapBlocks(empty, GetNear(empty, 0, -1));
                        break;
                    case Direction.LEFT:
                        SwapBlocks(empty, GetNear(empty, 0, 1));
                        break;
                }
            }
        }

        private TextBlock GetNear(TextBlock empty, int row, int col)
        {
            return MainGrid.Children.Cast<TextBlock>()
                            .FirstOrDefault(t => Grid.GetRow(t) == Grid.GetRow(empty) + row && Grid.GetColumn(t) == Grid.GetColumn(empty) + col);
        }

        private bool InRange(int row, int col)
        {
            return row < mapSize && row > -1 && col < mapSize && col > -1;
        }

        private bool EndOfGame()
        {
            int counter = 1;
            for (int row = 0; row < mapSize; row++)
            {
                for (int col = 0; col < mapSize; col++)
                {
                    TextBlock block = MainGrid.Children.Cast<TextBlock>().FirstOrDefault(t => Grid.GetRow(t) == row && Grid.GetColumn(t) == col);
                    if (block == null) continue;

                    string text = block.Text;
                    string original = counter.ToString();
                    if (row == mapSize - 1 && col == mapSize - 1) original = "";
                    if (text != original)
                        return false;
                    counter++;
                }
            }

            return true;
        }

        private void GetClickCoordinates(out int row, out int col)
        {
            col = row = 0;
            var point = Mouse.GetPosition(MainGrid);
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;
            foreach (var rowDefinition in MainGrid.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y)
                    break;
                row++;
            }
            foreach (var columnDefinition in MainGrid.ColumnDefinitions)
            {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X)
                    break;
                col++;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var block in MainGrid.Children.Cast<TextBlock>())
            {
                block.FontSize = this.Height / 8;
            }
        }
    }
}