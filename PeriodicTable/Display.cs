using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PeriodicTableConstructorLibrary;
using PeriodicTableGraphicsLibrary;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PeriodicTable
{
    public partial class Display : Form
    {
        public static Table Table;

        public static TableGraphics graphics;

        public Display()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadTable();
            loadGraphics();

            CellUpdate.setupCellUpdater(this);
            ImageUpdate.setupImageUpdater(this);
        }

        //Load
        private void loadTable() {
            Table = ConstructTable.CreateTable(UnifiedFormatConverter.ConvertToXml(File.ReadAllText(@"D:\User\Darbvisma\Word Documents\XmlDocuments\TableStorage - Test.xml")));
        }
        private void loadGraphics() {
            graphics = new TableGraphics(
                        Table,
                        new Size(pictureBox.Width, pictureBox.Height)
            );

            graphics.UpdateGraphics();

            //Test
            //TimeSpan timeSpan;
            //DateTime time1 = DateTime.Now;

            //for (int i = 0; i < 100; i++)
            //{
            //    graphics.UpdateGraphics();
            //}

            //timeSpan = DateTime.Now - time1;
            //Debug.WriteLine(timeSpan.TotalSeconds + " seconds");

            //Test
        }


        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            int howMuchPixelsToMove = (int)(pictureBox.Height * 0.05);

            if (e.KeyCode == Keys.Up) {
                Table.Location = new Point(Table.Location.X, Table.Location.Y + howMuchPixelsToMove);
            } else if (e.KeyCode == Keys.Down) {
                Table.Location = new Point(Table.Location.X, Table.Location.Y - howMuchPixelsToMove);
            } else if (e.KeyCode == Keys.Left) {
                Table.Location = new Point(Table.Location.X + howMuchPixelsToMove, Table.Location.Y);
            } else if (e.KeyCode == Keys.Right) {
                Table.Location = new Point(Table.Location.X - howMuchPixelsToMove, Table.Location.Y);
            }

        }
    }
}
