using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConwaysGameOfLife
{
    public class Grid
    {
        public Grid(List<Cell> cells)
        {
            this.Cells = cells;
        }
        public List<Cell> Cells { get; set; }
    }

    public class Cell
    {
        public Cell(int rowId, int columnId)
        {
            this.RowId = rowId;
            this.ColumnId = columnId;
            this.IsAlive = false;
        }

        public int RowId { get; set; }
        public int ColumnId { get; set; }
        public bool IsAlive { get; set; }
        public bool NextGenAlive { get; set; }
        public List<Cell> Neighbors { get; set; }
    }

    public partial class ConwaysGameOfLifeForm : Form
    {
        Grid grid;
        int numberOfCellsHorizontal = 30;
        int numberOfCellsVertical = 20;
        int cellSize = 20;
        Random rand = new Random();
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token;
        public ConwaysGameOfLifeForm()
        {
            this.token = tokenSource.Token;
            InitializeComponent();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            List<Cell> cells = new List<Cell>(numberOfCellsHorizontal * numberOfCellsVertical);

            for (int v = 0; v < numberOfCellsVertical; v++)
            {
                for (int i = 0; i < numberOfCellsHorizontal; i++)
                {
                    Cell newCell = new Cell(v, i);

                    RandomizeInitialAliveCell(newCell);

                    cells.Add(newCell);
                }
            }

            SetCellNeighbors(cells);

            this.grid = new Grid(cells);
        }

        private void RandomizeInitialAliveCell(Cell newCell)
        {
            if (this.rand.Next(1, 9) % 3 == 0)
            {
                newCell.IsAlive = true;
            }
        }

        private static void SetCellNeighbors(List<Cell> cells)
        {
            //initialize cell neighbors
            foreach (var cell in cells)
            {
                List<Cell> cellNeighbors = new List<Cell>();
                int[] neighborOffsets = {-1, 1};

                foreach(int i in neighborOffsets)
                {
                    //horz and vert
                    cellNeighbors.AddRange(cells.Where(x => x.ColumnId == cell.ColumnId + i && x.RowId == cell.RowId).ToList());
                    cellNeighbors.AddRange(cells.Where(x => x.RowId == cell.RowId + i && x.ColumnId == cell.ColumnId).ToList());

                    //diag
                    cellNeighbors.AddRange(cells.Where(x => x.RowId == cell.RowId + i && x.ColumnId == cell.ColumnId + i).ToList());
                    cellNeighbors.AddRange(cells.Where(x => x.RowId == cell.RowId + i && x.ColumnId == cell.ColumnId + -i).ToList());
                }

                cell.Neighbors = cellNeighbors;
            }
        }

        private void DrawCells()
        {
            foreach(var c in grid.Cells)
            {
                Pen myPen = new Pen(Color.Green);
                Graphics formGraphics;
                SolidBrush cellFillColor = new SolidBrush(c.IsAlive ? Color.Blue : Color.Yellow);

                formGraphics = this.CreateGraphics();

                var rectangle = new Rectangle(c.ColumnId * cellSize, c.RowId * cellSize, cellSize, cellSize);

                formGraphics.FillRectangle(cellFillColor, rectangle);

                formGraphics.DrawRectangle(myPen, rectangle);

                myPen.Dispose();
                cellFillColor.Dispose();
                formGraphics.Dispose();
            }
        }

        private void ProcessNextGen()
        {
            //LIVE CELLS
            var liveCells = this.grid.Cells.Where(x => x.IsAlive).ToList();
            var deadCells = this.grid.Cells.Where(x => !x.IsAlive).ToList();


            //live cells with two or three live neighbors, lives
            foreach (var liveCell in liveCells)
            {
                int countOfAlive = liveCell.Neighbors.Count(x => x.IsAlive);

                liveCell.NextGenAlive = countOfAlive == 2 || countOfAlive == 3;
            }

            //DEAD CELLS
            //dead cells with exactly 3 live neighbors, lives
            foreach (var deadCell in deadCells)
            {
                int countOfAlive = deadCell.Neighbors.Count(x => x.IsAlive);

                deadCell.NextGenAlive = countOfAlive == 3;
            }

            foreach(var cell in this.grid.Cells)
            {
                cell.IsAlive = cell.NextGenAlive;
            }
        }

        private void ConwaysGameOfLifeForm_OnClosing(object sender, FormClosingEventArgs obj)
        {
            this.tokenSource.Cancel();
        }

        private async void ConwaysGameOfLifeForm_Paint(object sender, PaintEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                await ProcessNextGenAsync(this.token);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

            MessageBox.Show("Game Over");
            this.Close();
        }

        private async Task ProcessNextGenAsync(CancellationToken cancellationToken)
        {
            await Task.Run( async () =>
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            DrawCells();
                            await Task.Delay(200);
                            ProcessNextGen();
                        }
                    },
                    cancellationToken
            );
        }
    }
}
