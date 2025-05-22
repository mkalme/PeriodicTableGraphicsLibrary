using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PeriodicTableConstructorLibrary;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PeriodicTableGraphicsLibrary
{
    public class TableGraphics
    {
        public Bitmap Image { get; set; }

        private Graphics graphics;

        public Table Table { get; set; }

        public int BorderWidth = 2;
        public Color BorderColor = ColorTranslator.FromHtml("#3D3D3D");


        public TableGraphics(Table tableModel, Size size) {
            Image = new Bitmap(size.Width, size.Height);
            graphics = Graphics.FromImage(Image);

            this.Table = tableModel;
            this.Table.Location = getStartingPoint();
        }

        private Point getStartingPoint(){
            // Get the X
            int tableWidth = 0;

            if (Table.Rows.Length > 0)
            {
                int spacerX = 0;
                foreach (var cell in Table.Rows[0].Cells)
                {
                    spacerX += cell.Spacer.Right;
                }
                tableWidth = (Table.Rows[0].Cells.Length * Table.CellSize.Width) + BorderWidth + spacerX;
            }

            int x = (Image.Width - tableWidth) / 2;

            // Get the Y
            int tableHeight = 0;

            int spacerY = 0;
            foreach (var row in Table.Rows) {
                if (Table.Rows.Length > 0)
                {
                    spacerY += row.Cells[0].Spacer.Bottom;
                }
            }

            tableHeight = (Table.Rows.Length * Table.CellSize.Height) + BorderWidth + spacerY;
            int y = (Image.Height - tableHeight) / 2;


            return new Point(x, y);
        }

        public void UpdateGraphics() {
            graphics.Clear(ColorTranslator.FromHtml("#F2F2F2"));

            drawGrid();
        }

        private void drawGrid() {
            for (int i = 0; i < Table.Rows.Length; i++) {
                for (int b = 0; b < Table.Rows[i].Cells.Length; b++) {
                    if (Table.Rows[i].Cells[b].CellType == CellType.NormalCellType) {
                        drawCell(i, b);
                    }
                }
            }
        }
        public void drawCell(int rowIndex, int cellIndex) {
            Cell cell = Table.Rows[rowIndex].Cells[cellIndex];

            int spacerX = 0;
            int spacerY = 0;

            //X
            for (int i = 0; i < cellIndex; i++)
            {
                spacerX += Table.Rows[rowIndex].Cells[i].Spacer.Right;
            }
            //Y
            for (int i = 0; i < rowIndex; i++)
            {
                spacerY += Table.Rows[i].Cells[cellIndex].Spacer.Bottom;
            }

            Point currentPoint = new Point(Table.Location.X + (cellIndex * Table.CellSize.Width) + spacerX,
                                           Table.Location.Y + (rowIndex * Table.CellSize.Height) + spacerY
            );

            //Color
            graphics.FillRectangle(
                new SolidBrush(cell.BackColor),
                new Rectangle(new Point(currentPoint.X, currentPoint.Y), new Size(Table.CellSize.Width, Table.CellSize.Height))
            );

            //Border
            graphics.DrawRectangle(
                new Pen(BorderColor, BorderWidth),
                new Rectangle(new Point(currentPoint.X, currentPoint.Y), new Size(Table.CellSize.Width, Table.CellSize.Height))
            );

            //Text
            for (int i = 0; i < cell.Attributes.Count; i++) {
                PeriodicTableConstructorLibrary.Attribute attribute = cell.Attributes.ElementAt(i).Value;

                if (attribute.Visible) {
                    string[] allLines = attribute.Text.Split(new string[] { @"\n" }, StringSplitOptions.None);

                    for (int b = 0; b < allLines.Length; b++) {
                        PeriodicTableConstructorLibrary.Attribute newAttribute = attribute.Clone();
                        newAttribute.Text = allLines[b];

                        Padding paddingModel = new Padding(newAttribute.Padding.X, newAttribute.Padding.Y);

                        //Get Padding
                        double padding = (newAttribute.Font.Size + newAttribute.LineSpacing) / (double)Table.CellSize.Height;
                        padding *= b;

                        paddingModel.Y += padding;

                        drawAttribute(currentPoint, newAttribute, paddingModel);
                    }
                }
            }
        }

        private void drawAttribute(Point currentPoint, PeriodicTableConstructorLibrary.Attribute attribute, Padding padding)
        {
            StringAlignment stringAlignment = StringAlignment.Near;

            PointF locationF = new PointF();
            SizeF sizeF = new SizeF();

            if (attribute.Alignment == AttributeAlignmentType.TopRight)
            {
                locationF = new PointF(currentPoint.X, (float)((currentPoint.Y + Table.CellSize.Height) - (Table.CellSize.Height * (1 - padding.Y))));
                sizeF = new SizeF((float)(Table.CellSize.Width - (Table.CellSize.Width * padding.X)), (float)(Table.CellSize.Height - (Table.CellSize.Height * padding.Y)));
                stringAlignment = StringAlignment.Far;
            }
            else if (attribute.Alignment == AttributeAlignmentType.TopLeft)
            {
                locationF = new PointF((float)(currentPoint.X + (padding.X * Table.CellSize.Width)), (float)(currentPoint.Y + (padding.Y * Table.CellSize.Height)));
                sizeF = new SizeF((currentPoint.X + Table.CellSize.Width) - locationF.X, (currentPoint.Y + Table.CellSize.Height) - locationF.Y);
                stringAlignment = StringAlignment.Near;
            }
            else
            {
                locationF = new PointF(0, 0);
                sizeF = new SizeF((float)(0.5 * Table.CellSize.Width), (float)(0.5 * Table.CellSize.Height));
                stringAlignment = StringAlignment.Center;
            }

            RectangleF rectangleF = new RectangleF(locationF.X, locationF.Y, sizeF.Width, sizeF.Height);
            StringFormat stringFormat = new StringFormat() { Alignment = stringAlignment };

            //graphics.DrawRectangle(
            //    new Pen(BorderColor, 1),
            //    new Rectangle(new Point((int)rectangleF.Location.X, (int)rectangleF.Location.Y), new Size((int)rectangleF.Width, (int)rectangleF.Height))
            //);

            graphics.DrawString(
                Regex.Unescape(attribute.Text),
                attribute.Font,
                new SolidBrush(attribute.Color),
                rectangleF,
                stringFormat
            );
        }
    }
}
