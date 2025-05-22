using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;

namespace PeriodicTable
{
    class UnifiedFormatConverter
    {
        private static string[][,] attributes = { };
        private static Dictionary<string, string> colorPact = new Dictionary<string, string>();

        private static bool relativeFontSize = false;
        private static Size CellSize;

        public static string ConvertToXml(string text) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(text);

            //Load Attributes
            XmlNodeList allAttributes = document.SelectNodes("/root/attributes/att");
            attributes = new string[allAttributes.Count][,];

            for (int i = 0; i < allAttributes.Count; i++) {

                attributes[i] = new string[allAttributes[i].Attributes.Count, 2];
                for (int b = 0; b < allAttributes[i].Attributes.Count; b++) {
                    attributes[i][b, 0] = allAttributes[i].Attributes[b].Name;
                    attributes[i][b, 1] = allAttributes[i].Attributes[b].Value;
                }
            }

            //Load Colors
            XmlNodeList allColors = document.SelectNodes("/root/colorpact/color");

            for (int i = 0; i < allColors.Count; i++) {
                colorPact.Add(allColors[i].Attributes["id"].Value, allColors[i].Attributes["name"].Value);
            }

            //Table
            XmlDocument newDocument = getTable(document);

            newDocument.PreserveWhitespace = true;

            return makeDocumentReadable(newDocument.InnerXml);
        }

        private static string makeDocumentReadable(string text) {
            text = text.Replace("><", ">\n<");

            text = text.Replace("<row", "\t<row");
            text = text.Replace("</row>", "\t</row>");
            text = text.Replace("<elem", "\t\t<elem");
            text = text.Replace("</elem>", "\t\t</elem>");
            text = text.Replace("<attribute", "\t\t\t<attribute");

            text = text.Replace("<bl", "\t\t<bl");

            text = text.Replace(">\n</attribute>", "></attribute>");

            return text;
        }

        private static XmlDocument getTable(XmlDocument document) {
            XmlDocument newDocument = new XmlDocument();

            XmlElement tableElement = newDocument.CreateElement("table");
            CellSize = new Size(Int32.Parse(document.SelectSingleNode("/root/cellsize").Attributes["width"].Value),
                                Int32.Parse(document.SelectSingleNode("/root/cellsize").Attributes["height"].Value
            ));

            tableElement.SetAttribute("cellWidth", CellSize.Width.ToString());
            tableElement.SetAttribute("cellHeight", CellSize.Height.ToString());

            if (nodeHasAttribute(document.SelectSingleNode("/root/relativefontsize"), "enable")) {
                bool.TryParse(document.SelectSingleNode("/root/relativefontsize").Attributes["enable"].Value, out relativeFontSize);
            }

            XmlNodeList allRows = document.SelectNodes("/root/table/row");

            //Rows
            for (int i = 0; i < allRows.Count; i++) {
                XmlElement rowElement = newDocument.CreateElement("row");

                if (nodeHasAttribute(allRows[i], "order")) {
                    rowElement.SetAttribute("order", allRows[i].Attributes["order"].Value);
                }
                if (nodeHasAttribute(allRows[i], "spacer")){
                    rowElement.SetAttribute("spacer", allRows[i].Attributes["spacer"].Value);
                }

                XmlNodeList allCells = allRows[i].ChildNodes;

                //Cells
                for (int b = 0; b < allCells.Count; b++) {
                    if (allCells[b].Name.Equals("elem"))
                    {
                        XmlElement regularCellElement = newDocument.CreateElement("elem");

                        if (nodeHasAttribute(allCells[b], "colorid"))
                        {
                            regularCellElement.SetAttribute("color", colorPact[allCells[b].Attributes["colorid"].Value]);
                        }
                        if (nodeHasAttribute(allCells[b], "spacer"))
                        {
                            regularCellElement.SetAttribute("spacer", allCells[b].Attributes["spacer"].Value);
                        }

                        //All Cell Attributes
                        XmlNodeList allCellAttributes = allCells[b].SelectNodes("att");
                        for (int c = 0; c < allCellAttributes.Count; c++) {
                            XmlElement attributeElement = newDocument.CreateElement("attribute");

                            for (int d = 0; d < attributes[c].GetLength(0); d++) {
                                attributeElement.SetAttribute(attributes[c][d, 0], attributes[c][d, 1]);
                            }

                            //Font
                            if (relativeFontSize) {
                                if (attributeElement.HasAttribute("relative"))
                                {
                                    //Get the value
                                    int size = (int)Math.Round((double)CellSize.Height / 10.0);
                                    int spacing = (int)Math.Round((double)CellSize.Height / 30.0);


                                    string[] splitFont = attributeElement.Attributes["relative"].Value.Replace(".", ",").Trim().Split(new string[] { ";" }, StringSplitOptions.None);

                                    double value = 0;
                                    double.TryParse(splitFont[0], out value);

                                    if (value > 0)
                                    {
                                        size =  (int)Math.Round(((double)CellSize.Height) * value);
                                    }

                                    double value1 = 0;
                                    if (splitFont.Length > 1) {
                                        double.TryParse(splitFont[1], out value1);

                                        if (value1 > 0)
                                        {
                                            spacing = (int)Math.Round(((double)CellSize.Height) * value1);
                                        }
                                    }

                                    //Implement the value
                                    string[] fontSplit = { };
                                    if (attributeElement.HasAttribute("font"))
                                    {
                                        fontSplit = attributeElement.Attributes["font"].Value.Split(new string[] { ";" }, StringSplitOptions.None);
                                    }

                                    if (fontSplit.Length >= 4) {
                                        fontSplit[1] = size.ToString();
                                        fontSplit[3] = spacing.ToString();

                                        attributeElement.Attributes["font"].Value = string.Join(";", fontSplit);
                                    }
                                    else if (fontSplit.Length == 3) {
                                        attributeElement.Attributes["font"].Value = fontSplit[0] + "; " + size.ToString() + ";" + fontSplit[2] + "; " + spacing.ToString();
                                    }
                                    else if (fontSplit.Length == 2)
                                    {
                                        attributeElement.Attributes["font"].Value = fontSplit[0] + "; " + size.ToString() + ";Regular; " + spacing.ToString();
                                    }
                                    else if (fontSplit.Length == 1)
                                    {
                                        attributeElement.Attributes["font"].Value += ";" + size.ToString() + ";Regular;" + spacing.ToString();
                                    }
                                    else
                                    {
                                        attributeElement.SetAttribute("font", "Microsoft Sans Serif; " + size.ToString() + ";Regular; " + spacing.ToString());
                                    }
                                }
                            }

                            attributeElement.InnerText = allCellAttributes[c].InnerText;

                            regularCellElement.AppendChild(attributeElement);
                        }

                        rowElement.AppendChild(regularCellElement);
                    }
                    else if (allCells[b].Name.Equals("bl")) {
                        XmlElement blankCellElement = newDocument.CreateElement("bl");

                        if (nodeHasAttribute(allCells[b], "spacer"))
                        {
                            blankCellElement.SetAttribute("spacer", allCells[b].Attributes["spacer"].Value);
                        }

                        rowElement.AppendChild(blankCellElement);
                    }

                    tableElement.AppendChild(rowElement);
                }
            }

            newDocument.AppendChild(tableElement);

            return newDocument;
        }

        private static bool nodeHasAttribute(XmlNode node, string attribute)
        {
            bool hasOne = false;

            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name.Equals(attribute))
                {
                    hasOne = true;
                    goto after_loop;
                }
            }
        after_loop:

            return hasOne;
        }
    }
}
