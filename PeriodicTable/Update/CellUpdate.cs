using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using PeriodicTableConstructorLibrary;
using System.Threading;

namespace PeriodicTable
{
    class CellUpdate
    {
        private static Display form;

        private static int[] cell = new int[2];

        public static void setupCellUpdater(Display formDisplay) {
            form = formDisplay;

            //form.pictureBox.
            form.pictureBox.MouseWheel += mouseWheel;

            Thread t3 = new Thread(() => cellHover());
            //t3.Start();

        }

        //Current Cell
        private static void cellHover() {
            cell = new int[]{ 1, 2};//getCell(form.PointToClient(Cursor.Position).X, form.PointToClient(Cursor.Position).Y);

            activateMouseMove(0, 0);

            //Thread.Sleep(3);
        }

        //Mouse Move
        private static int[] lastEnteredCell = {-1, -1}; //row index, cell index
        private static int[] lastExitedCell =  {-1, -1}; //row index, cell index
        private static void mouseMove(object sender, MouseEventArgs e)
        {
            //activateMouseMove(e.X, e.Y);
        }
        private static void activateMouseMove(int x, int y) {
            //int[] cell = getCell(x, y);

            bool executeEnter = false;
            bool executeExit = false;

            int[] enterCell = { };
            int[] exitCell = { };

            //Check if mouse enter cell
            if (cell[0] > -1 && cell[1] > -1){
                if (lastEnteredCell[0] != cell[0] || lastEnteredCell[1] != cell[1]){
                    executeEnter = true;
                    enterCell = cell;

                    lastExitedCell = lastEnteredCell;
                    lastEnteredCell = new int[] { cell[0], cell[1] };
                }
            }

            //Check if mouse leaves cell
            if (lastExitedCell[0] > -1 && lastExitedCell[1] > -1){
                if (cell[0] != lastExitedCell[0] || cell[1] != lastExitedCell[1]){
                    executeExit = true;
                    exitCell = lastExitedCell;

                    lastExitedCell = new int[] { cell[0], cell[1] };
                }
            }

            if (executeExit)
            {
                mouseLeaveCell(exitCell);
            }

            if (executeEnter)
            {
                mouseEnterCell(enterCell);
            }
        }
        private static int[] getCell(int x, int y)
        {
            int x1 = -1;
            int y1 = -1;

            //get row
            for (int rowIndex = 0; rowIndex < Display.Table.Rows.Length; rowIndex++)
            {
                int spacerY = 0;
                for (int b = 0; b < rowIndex; b++)
                {
                    if (Display.Table.Rows[b].Cells.Length > 0)
                    {
                        spacerY += Display.Table.Rows[b].Cells[0].Spacer.Bottom;
                    }
                }

                int[] RowArea = {Display.Table.Location.Y + (Display.Table.CellSize.Height * rowIndex) + spacerY,
                                 Display.Table.Location.Y + (Display.Table.CellSize.Height * rowIndex) + spacerY + Display.Table.CellSize.Height};
                if (y >= RowArea[0] && y <= RowArea[1])
                {
                    y1 = rowIndex;

                    goto after_loop;
                }
            }
        after_loop:

            if (y1 > -1)
            {
                //get row
                for (int cellIndex = 0; cellIndex < Display.Table.Rows[y1].Cells.Length; cellIndex++)
                {
                    int spacerX = 0;
                    for (int b = 0; b < cellIndex; b++)
                    {
                        spacerX += Display.Table.Rows[y1].Cells[b].Spacer.Right;
                    }

                    int[] CellArea = {Display.Table.Location.X + (Display.Table.CellSize.Width * cellIndex) + spacerX,
                                      Display.Table.Location.X + (Display.Table.CellSize.Width * cellIndex) + spacerX + Display.Table.CellSize.Width};
                    if (x >= CellArea[0] && x <= CellArea[1])
                    {
                        x1 = cellIndex;

                        goto after_loop2;
                    }
                }
            }
        after_loop2:

            if (x1 < 0)
            {
                y1 = -1;
            }

            return new int[] { y1, x1 };
        }


        private static Color prevSelectedCellColor = Color.Transparent;
        private static void mouseEnterCell(int[] cell) {
            if (Display.Table.Rows[cell[0]].Cells[cell[1]].CellType == CellType.NormalCellType) {
                Color color = Display.Table.Rows[cell[0]].Cells[cell[1]].BackColor;

                prevSelectedCellColor = color;

                Display.Table.Rows[cell[0]].Cells[cell[1]].BackColor = changeColorBrightness(color, 0.92);

                Display.graphics.drawCell(cell[0], cell[1]);
            }
        }
        private static void mouseLeaveCell(int[] cell) {
            if (Display.Table.Rows[cell[0]].Cells[cell[1]].CellType == CellType.NormalCellType) {
                Display.Table.Rows[cell[0]].Cells[cell[1]].BackColor = prevSelectedCellColor;

                Display.graphics.drawCell(cell[0], cell[1]);
            }
        }

        private static Color changeColorBrightness(Color color, double value)
        {
            int r = (int)((double)color.R * value);
            int g = (int)((double)color.G * value);
            int b = (int)((double)color.B * value);

            r = r > 255 ? 255 : r;
            g = g > 255 ? 255 : g;
            b = b > 255 ? 255 : b;

            return Color.FromArgb(color.A, r, g, b);
        }


        //Mouse Wheel
        private static double scrollChange = 0.025; // Percentage by which the image changes location relative to the picture box
        private static int deltaForOneChange = 120;
        private static void mouseWheel(object sender, MouseEventArgs e) {
            int howMuchPixelsToMove = (int)(form.pictureBox.Height * scrollChange);
            howMuchPixelsToMove *= e.Delta / deltaForOneChange;

            //Display.Table.Location = new Point(Display.Table.Location.X, Display.Table.Location.Y + howMuchPixelsToMove);

            form.pictureBox.Padding = new System.Windows.Forms.Padding(400, 40, 10, 10);


            activateMouseMove(e.X, e.Y);
            Display.graphics.UpdateGraphics();

            Thread.Sleep(50);
        }
    }
}
