using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PerryMikeGOL
{
    public partial class Form1 : Form
    {
        //methods for creating needed arrays
        public void CreateUniverse(int sizeOfUniverse)
        {
            universe = new bool[sizeOfUniverse, sizeOfUniverse];
            scratchPad = new bool[sizeOfUniverse, sizeOfUniverse];
            neighborCountArray = new int[sizeOfUniverse, sizeOfUniverse];
        }
        public void CreateUniverse(int xsize, int ysize)
        {
            universe = new bool[xsize, ysize];
            scratchPad = new bool[xsize, ysize];
            neighborCountArray = new int[xsize, ysize];
        }
        //public getters
        public int GetXSize()
        {
            return universe.GetLength(0);
        }
        public int GetYSize()
        {
            return universe.GetLength(1);
        }

        //the default universe members
        public bool[,] universe;
        public bool[,] scratchPad;
        public int[,] neighborCountArray;

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;

        //other members
        bool DisplayNeighbors = false;
        bool DisplayGrid = true;
        bool CellCount = true;
        bool GenerationsOn = true;
        int generations = 0;
        int livingCellsCount;
        int livingCells;
        int cellWidth = 0;
        int cellHeight = 0;
        //the form itself
        public Form1()
        {
            //default universe of 25
            CreateUniverse(25);
            InitializeComponent();

            // Setup the timer
        }

        #region methods
        //Calculate the next generation of cells. The event called by the timer every Interval milliseconds.
        private void NextGeneration()
        {
            //CountNeighbors();
            for (int i = 0; i < universe.GetLength(0); i++)
            {
                for (int j = 0; j < universe.GetLength(1); j++)
                {
                    neighborCountArray[i, j] = CountNeighborsToroidal(i, j);
                }
            }
            livingCells = Survive();
            // Increment generation count
            generations++;

            // Update status strip generations
            UpdateDisplay();

        }
        //paint method that was given
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {

                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // Outline the cell with a pen if option is toggled on
                    if (DisplayGrid)
                    {
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    }
                    // display neighbors on the cell if option is toggled on
                    if (DisplayNeighbors)
                    {
                        Font font = new Font("Arial", 20f);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        Rectangle rect = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                        int neighbors = CountNeighborsToroidal(x, y);

                        e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Black, rect, stringFormat);
                    }
                }
            }
            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }
        //how many living cells
        public int CountCells()
        {
            livingCellsCount = 0;
            for (int i = 0; i < neighborCountArray.GetLength(0); i++)
            {
                for (int j = 0; j < neighborCountArray.GetLength(1); j++)
                {

                    if (universe[i, j] == true)
                    {
                        // iterates through the array, increasing the living cell count for each true boolean value
                        livingCellsCount++;
                    }
                }
            }
            return livingCellsCount;
        }
        //logic for surviving
        public int Survive()
        {
            livingCellsCount = 0;
            for (int i = 0; i < neighborCountArray.GetLength(0); i++)
            {
                for (int j = 0; j < neighborCountArray.GetLength(1); j++)
                {
                    if (universe[i, j] && (neighborCountArray[i, j] < 2 || neighborCountArray[i, j] > 3))
                    {
                        scratchPad[i, j] = false;
                    }
                    else if (!universe[i, j] && neighborCountArray[i, j] == 3)
                    {
                        scratchPad[i, j] = true;
                        livingCellsCount++;
                    }
                    else if (universe[i,j] && (neighborCountArray[i,j] == 2 || neighborCountArray[i,j] == 3))
                    {
                        scratchPad[i, j] = true;
                        livingCellsCount++;
                    }
                    else if (universe[i, j] == true)
                    {
                        livingCellsCount++;
                    }
                }
            }
            universe = scratchPad;
            return livingCellsCount;
            //for (int i = 0; i < universe.GetLength(0); i++)
            //{
            //    for (int j = 0; j < universe.GetLength(1); j++)
            //    {
            //        universe[i, j] = scratchPad[i, j];
            //    }
            //}
        }
        //count neighbors from class
        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;
                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    // if xCheck is less than 0 then set to xLen - 1
                    else if (xCheck < 0)
                    {
                        xCheck = xLen - 1;
                    }
                    // if yCheck is less than 0 then set to yLen - 1
                    else if (yCheck < 0)
                    {
                        yCheck = yLen - 1;
                    }
                    // if xCheck is greater than or equal too xLen then set to 0
                    else if (xCheck >= xLen)
                    {
                        xCheck = 0;
                    }
                    // if yCheck is greater than or equal too yLen then set to 0
                    else if (yCheck >= yLen)
                    {
                        yCheck = 0;
                    }
                    else if (universe[xCheck, yCheck]) count++;
                }
            }
            return count;
        }
        //my attempt at counting neighbors

        //public void CountNeighbors()
        //{
        //    //iterate through the columns
        //    for (int y = 0; y < universe.GetLength(1); y++)
        //    //iterate through the rows
        //    {
        //        for (int x = 0; x < universe.GetLength(0); x++)
        //        {
        //            int howManyNeighbors = 0;


        //            for (int i = (x - 1); i <= (x + 1); i++)
        //            {
        //                for (int j = (y - 1); j <= (y + 1); j++)
        //                {
        //                    if (i < 0 || i >= universe.GetLength(0) || j < 0 || j >= universe.GetLength(1))
        //                    {
        //                        continue;
        //                    }
        //                    try
        //                    {
        //                        if (universe[i, j])
        //                        {
        //                            howManyNeighbors++;
        //                        }
        //                    }
        //                    catch (IndexOutOfRangeException)
        //                    {
        //                        continue;
        //                    }


        //                }
        //                neighborCountArray[x, y] = howManyNeighbors;

        //            }

        //        }
        //    }
        //}
        //random universe
        private void RandomUniverse(bool[,] universe)
        {
            Random rand = new Random();
            int randint;
            generations = 0;
            for (int i = 0; i < universe.GetLength(0); i++)
            {
                for (int j = 0; j < universe.GetLength(1); j++)
                {
                    randint = rand.Next(1, 3);
                    if (randint == 1)
                    {
                        universe[i, j] = true;
                    }
                    else
                    {
                        universe[i, j] = false;
                    }
                }
            }
            UpdateDisplay();
        }
        //method to make updating easier
        public void UpdateDisplay()
        {
            if (CellCount && GenerationsOn)
            {
                toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString() + " Living Cells " + CountCells();
            }
            else if (!CellCount && GenerationsOn)
            {
                toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            }
            else if (CellCount && !GenerationsOn)
            {
                toolStripStatusLabelGenerations.Text = " Living Cells " + CountCells();
            }
            else if (!CellCount && !GenerationsOn)
            {
                toolStripStatusLabelGenerations.Text = " ";
            }
            graphicsPanel1.Invalidate();

        }
        //reset universe method
        private void ResetUniverse(bool[,] universe)
        {
            generations = 0;
            for (int i = 0; i < universe.GetLength(0); i++)
            {
                for (int j = 0; j < universe.GetLength(1); j++)
                {
                    universe[i, j] = false;
                }
            }
            UpdateDisplay();
        }
        //save method
        public void Save()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                writer.WriteLine($"!{GetYSize()}\n!{GetXSize()}\n!{SandOfTime.Interval}");
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.

                // Iterate through the universe one row at a time.
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;

                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        // If the universe[x,y] is alive then append 'O' (capital O)
                        // to the row string.
                        if (universe[x, y])
                        {
                            currentRow += "O";
                        }
                        else
                        {
                            currentRow += ".";

                        }
                        // Else if the universe[x,y] is dead then append '.' (period)
                        // to the row string.
                    }
                    writer.WriteLine(currentRow);
                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }
        //open method
        public void Open()
        {

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 1;
                int setInterval;
                bool readheight = false;
                bool readwidth = false;
                bool readinterval = false;

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    for (int y = 0; y < maxHeight; y++)
                    {
                        string row = reader.ReadLine();
                        if (row[0] == '!')
                        {
                            if (!readheight)
                            {
                                readheight = true;

                                int.TryParse(row.Trim('!'), out maxHeight);
                                y--;
                                continue;
                            }
                            else if (!readwidth)
                            {
                                readwidth = true;
                                int.TryParse(row.Trim('!'), out maxWidth);
                                y--;
                                continue;
                            }
                            else if (!readinterval)
                            {
                                readinterval = true;
                                int.TryParse(row.Trim('!'), out setInterval);
                                CreateUniverse(maxWidth, maxHeight);
                                SandOfTime.Interval = setInterval;
                                y--;
                                continue;
                            }
                        }
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            // If row[xPos] is a 'O' (capital O) then
                            if (row[xPos] == 'O')
                            {
                                universe[xPos, y] = true;
                            }
                            else
                            {
                                universe[xPos, y] = false;
                            }
                            // set the corresponding cell in the universe to alive.

                            // If row[xPos] is a '.' (period) then
                            // set the corresponding cell in the universe to dead.
                        }
                    }

                }

                // Close the file.
                reader.Close();
            }
        }

        #endregion

        #region buttons
        //start
        private void Start_Click(object sender, EventArgs e)
        {
            SandOfTime.Interval = 100;
            SandOfTime.Enabled = true;
        }
        //timer
        private void SandOfTime_Tick(object sender, EventArgs e)
        {
            NextGeneration();
            if (livingCells == 0)
            {
                SandOfTime.Enabled = false;
            }
        }
        //pause
        private void Pause_Click(object sender, EventArgs e)
        {
            SandOfTime.Enabled = false;
        }
        //random universe, sets timer to off
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            RandomUniverse(universe);
            SandOfTime.Enabled = false;
        }
        // new universe using reset universe method, also turns off timer
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetUniverse(universe);
            SandOfTime.Enabled = false;
        }
        //activates cells on click
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];
                UpdateDisplay();
                // Tell Windows you need to repaint
            }
        }
        //toggle neighbors
        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayNeighbors = !DisplayNeighbors;
            UpdateDisplay();
        }
        //change to gray
        private void grayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cellColor = Color.Gray;
            UpdateDisplay();
        }
        //pick the living cell color
        private void youPickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog color = new ColorDialog();
            color.Color = graphicsPanel1.BackColor;

            if (DialogResult.OK == color.ShowDialog())
            {
                cellColor = color.Color;
                UpdateDisplay();
            }
        }
        //pick the outline color
        private void youPickToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ColorDialog color = new ColorDialog();
            color.Color = graphicsPanel1.BackColor;

            if (DialogResult.OK == color.ShowDialog())
            {
                gridColor = color.Color;
                UpdateDisplay();
            }
        }
        //options menu for changing timer, height and width
        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Options optionMenu = new Options();
            optionMenu.TimerTick = SandOfTime.Interval;
            optionMenu.CellHeight = GetYSize();
            optionMenu.CellWidth = GetXSize();
            if (DialogResult.OK == optionMenu.ShowDialog())
            {
                SandOfTime.Interval = optionMenu.TimerTick;
                cellHeight = optionMenu.CellHeight;
                cellWidth = optionMenu.CellWidth;
            }
            CreateUniverse(cellHeight, cellWidth);
            UpdateDisplay();
        }
        //open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
            UpdateDisplay();
        }
        //save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }
        //reset
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            ResetUniverse(universe);
        }
        //another open
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            Open();
            UpdateDisplay();
        }
        //another save
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            Save();
        }
        //toggle grid
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayGrid = !DisplayGrid;
            UpdateDisplay();
        }
        // single generation button
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }
        //10x10 universe option
        private void x10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateUniverse(10);
            UpdateDisplay();
        }
        //25x25 universe option
        private void x25ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateUniverse(25);
            UpdateDisplay();
        }
        //change cells to blue
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cellColor = Color.Blue;
            UpdateDisplay();
        }
        // change cells to green
        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cellColor = Color.Green;
            UpdateDisplay();
        }
        //change cell outline to black
        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridColor = Color.Black;
            UpdateDisplay();

        }
        //chance cell outline to red
        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridColor = Color.Red;
            UpdateDisplay();

        }
        //toggle cell count
        private void cellCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CellCount = !CellCount;
            UpdateDisplay();
        }
        //toggle generations
        private void generationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerationsOn = !GenerationsOn;
            UpdateDisplay();
        }
        #endregion

    }
}



