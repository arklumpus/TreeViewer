using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp;

namespace TreeViewer.Stats
{
    internal static class Distribution
    {

        public static Page GetPlot(double[] values1, double[] values2, double value1, double value2, double p1, double p2, string yAxis1Name, string yAxis2Name, string xAxisName, string title, string variableName, string tag, out Dictionary<string, (Colour, Colour, string)> interactiveDescriptions, bool interactive = false)
        {
            interactiveDescriptions = null;

            if (interactive)
            {
                interactiveDescriptions = new Dictionary<string, (Colour, Colour, string)>();
            }

            double minValue = Math.Min(Math.Min(values1.Min(), values2.Min()), Math.Min(value1, value2));
            double maxValue = Math.Max(Math.Max(values1.Max(), values2.Max()), Math.Max(value1, value2));

            int resolutionSteps = 100;

            double d = (maxValue - minValue) / (resolutionSteps - 1);

            double[] cdf1 = new double[resolutionSteps];
            double[] cdf2 = new double[resolutionSteps];

            for (int i = 0; i < resolutionSteps; i++)
            {
                double val = minValue + d * i;

                cdf1[i] = values1.Count(x => x <= val) / (double)values1.Length;
                cdf2[i] = values2.Count(x => x <= val) / (double)values2.Length;
            }

            double[] pdf1 = new double[resolutionSteps];
            double[] pdf2 = new double[resolutionSteps];

            pdf1[0] = 0.5 * cdf1[1] / d;
            pdf1[resolutionSteps - 1] = 0.5 * (cdf1[resolutionSteps - 1] - cdf1[resolutionSteps - 2]) / d;

            pdf2[0] = 0.5 * cdf2[1] / d;
            pdf2[resolutionSteps - 1] = 0.5 * (cdf2[resolutionSteps - 1] - cdf2[resolutionSteps - 2]) / d;

            for (int i = 1; i < resolutionSteps - 1; i++)
            {
                pdf1[i] = 0.5 * (cdf1[i + 1] - cdf1[i - 1]) / d;
                pdf2[i] = 0.5 * (cdf2[i + 1] - cdf2[i - 1]) / d;
            }

            double maxY1 = pdf1.Max();
            double maxY2 = pdf2.Max();

            double plotWidth = 400;
            double plotHeight = 225;
            double axisMargin = 10;
            double axisLength = 10;
            Colour plotColour = Colour.FromRgb(0, 114, 178);

            Font axisFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 10);
            Font axisLegend = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
            Font titleFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14);

            Page pag = new Page(1, 1);
            Graphics gpr = pag.Graphics;

            gpr.Save();

            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(-axisMargin - axisLength, 0).LineTo(-axisMargin, 0).LineTo(-axisMargin, plotHeight).LineTo(-axisMargin - axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(-axisMargin - axisLength, i * plotHeight / 5).LineTo(-axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY1 / 5 * i).ToString((maxY1 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(-axisMargin - axisLength - 3 - width, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(-axisMargin - axisLength - 3 - yWidth - axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 15, 0, yAxis1Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis1Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178));

                gpr.Restore();
            }


            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(plotWidth + axisMargin + axisLength, 0).LineTo(plotWidth + axisMargin, 0).LineTo(plotWidth + axisMargin, plotHeight).LineTo(plotWidth + axisMargin + axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(plotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(plotWidth + axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY2 / 5 * i).ToString((maxY2 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(plotWidth + axisMargin + axisLength + 3, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(plotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 15, 0, yAxis2Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis2Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(213, 94, 0));

                gpr.Restore();
            }

            GraphicsPath xAxis = new GraphicsPath().MoveTo(0, plotHeight + axisMargin + axisLength).LineTo(0, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin + axisLength);

            for (int i = 1; i < 7; i++)
            {
                xAxis.MoveTo(i * plotWidth / 7, plotHeight + axisMargin + axisLength).LineTo(i * plotWidth / 7, plotHeight + axisMargin);
            }

            gpr.StrokePath(xAxis, Colours.Black);

            for (int i = 0; i <= 7; i++)
            {
                double val = minValue + (maxValue - minValue) / 7 * i;

                string text = val.ToString(val.GetDigits());

                gpr.FillText(i * plotWidth / 7 - axisFont.MeasureText(text).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent, text, axisFont, Colours.Black, TextBaselines.Baseline);
            }

            GraphicsPath distributionPath1 = new GraphicsPath();
            GraphicsPath distributionPath2 = new GraphicsPath();

            GraphicsPath distributionHighlight1 = new GraphicsPath();
            GraphicsPath distributionHighlight2 = new GraphicsPath();

            GraphicsPath distributionHighlight1Other = new GraphicsPath();
            GraphicsPath distributionHighlight2Other = new GraphicsPath();

            if (p1 <= 0.5)
            {
                distributionHighlight1.MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);


                distributionHighlight1Other.MoveTo(0, plotHeight);
            }
            else
            {
                distributionHighlight1.MoveTo(0, plotHeight);


                distributionHighlight1Other.MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
            }

            if (p2 <= 0.5)
            {
                distributionHighlight2.MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);


                distributionHighlight2Other.MoveTo(0, plotHeight);
            }
            else
            {
                distributionHighlight2.MoveTo(0, plotHeight);


                distributionHighlight2Other.MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
            }

            for (int i = 0; i < resolutionSteps; i++)
            {
                distributionPath1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                distributionPath2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);

                if (p1 > 0.5 && minValue + i * d < value1)
                {
                    distributionHighlight1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }
                else if (p1 <= 0.5 && minValue + i * d > value1)
                {
                    distributionHighlight1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }

                if (p1 <= 0.5 && minValue + i * d < value1)
                {
                    distributionHighlight1Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }
                else if (p1 > 0.5 && minValue + i * d > value1)
                {
                    distributionHighlight1Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }

                if (p2 > 0.5 && minValue + i * d < value2)
                {
                    distributionHighlight2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
                else if (p2 <= 0.5 && minValue + i * d > value2)
                {
                    distributionHighlight2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }

                if (p2 <= 0.5 && minValue + i * d < value2)
                {
                    distributionHighlight2Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
                else if (p2 > 0.5 && minValue + i * d > value2)
                {
                    distributionHighlight2Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
            }


            if (p1 <= 0.5)
            {
                distributionHighlight1.LineTo(plotWidth, plotHeight);
                distributionHighlight1.Close();


                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                distributionHighlight1Other.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight1Other.Close();
            }
            else
            {
                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                distributionHighlight1.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight1.Close();


                distributionHighlight1Other.LineTo(plotWidth, plotHeight);
                distributionHighlight1Other.Close();
            }

            if (p2 <= 0.5)
            {
                distributionHighlight2.LineTo(plotWidth, plotHeight);
                distributionHighlight2.Close();


                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                distributionHighlight2Other.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight2Other.Close();
            }
            else
            {
                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                distributionHighlight2.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight2.Close();


                distributionHighlight2Other.LineTo(plotWidth, plotHeight);
                distributionHighlight2Other.Close();
            }

            if (!interactive)
            {
                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3));
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3));
            }
            else
            {
                string sign1;
                string sign1Other;

                double v1Left;
                double v1Right;

                if (p1 <= 0.5)
                {
                    sign1 = ">";
                    sign1Other = "<";

                    v1Left = p1;
                    v1Right = 1 - p1;
                }
                else
                {
                    sign1 = "<";
                    sign1Other = ">";

                    v1Left = 1 - p1;
                    v1Right = p1;
                }

                string sign2;
                string sign2Other;

                double v2Left;
                double v2Right;

                if (p2 <= 0.5)
                {
                    sign2 = ">";
                    sign2Other = "<";

                    v2Left = p2;
                    v2Right = 1 - p2;
                }
                else
                {
                    sign2 = "<";
                    sign2Other = ">";

                    v2Left = 1 - p2;
                    v2Right = p2;
                }


                string tag1Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1Other + " " + variableName + "*) ≈ " + v1Right.ToString(v1Right.GetDigits())));

                string tag2Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2Other + " " + variableName + "*) ≈ " + v2Right.ToString(v2Right.GetDigits())));

                gpr.FillPath(distributionHighlight1Other, Colour.FromRgba(0, 0, 0, 0), tag: tag1Other);
                gpr.FillPath(distributionHighlight2Other, Colour.FromRgba(0, 0, 0, 0), tag: tag2Other);

                string tag1 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1, (Colour.FromRgba(0, 114, 178, 0.3), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1 + " "+ variableName + "*) ≈ " + v1Left.ToString(v1Left.GetDigits())));

                string tag2 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2, (Colour.FromRgba(213, 94, 0, 0.3), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2 + " "+ variableName + "*) ≈ " + v2Left.ToString(v2Left.GetDigits())));

                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3), tag: tag1);
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3), tag: tag2);
            }

            gpr.StrokePath(distributionPath1, Colour.FromRgb(0, 114, 178), 2, lineJoin: LineJoins.Round);
            gpr.StrokePath(distributionPath2, Colour.FromRgb(213, 94, 0), 2, lineJoin: LineJoins.Round);

            gpr.StrokePath(new GraphicsPath().MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(0, 78, 138), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(166, 55, 0), 2, lineDash: new LineDash(5, 5, 0));

            gpr.FillText(plotWidth * 0.5 - axisLegend.MeasureText(xAxisName).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent + axisLegend.FontSize * 1.4, xAxisName, axisLegend, Colours.Black, TextBaselines.Baseline);

            gpr.Restore();

            Rectangle bounds = gpr.GetBounds();

            gpr.FillText(bounds.Location.X + bounds.Size.Width * 0.5 - titleFont.MeasureText(title).Width * 0.5, bounds.Location.Y - 5, title, titleFont, Colours.Black, TextBaselines.Bottom);

            if (!string.IsNullOrEmpty(tag))
            {
                bounds = gpr.GetBounds();

                gpr.FillRectangle(bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, Colour.FromRgba(0, 0, 0, 0), tag: "plotBounds/" + tag);

                Font buttonFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), 7.5);

                for (int i = 0; i < 4; i++)
                {
                    double x = bounds.Location.X + bounds.Size.Width - 24 - 29 * i;
                    gpr.FillRectangle(x, bounds.Location.Y, 24, 24, Colour.FromRgba(0, 0, 0, 0), tag: "buttonBg[" + i.ToString() + "]/" + tag);

                    switch (i)
                    {
                        case 0:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("PDF").Width * 0.5, 16, "PDF", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 1:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("SVG").Width * 0.5, 16, "SVG", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 2:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("CSV").Width * 0.5, 16, "CSV", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 3:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(0, 0).LineTo(5, 0).LineTo(3.5, 1.5).LineTo(6, 4).LineTo(4, 6).LineTo(1.5, 3.5).LineTo(0, 5).Close();

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);

                                gpr.Restore();
                            }
                            break;

                    }
                }
            }

            pag.Crop();

            return pag;
        }


        public static Page GetPlotWithStandardNormal(double[] values1, double[] values2, double value1, double value2, double p1, double p2, string yAxis1Name, string yAxis2Name, string yAxis3Name, string xAxisName, string title, string variableName, string tag, out Dictionary<string, (Colour, Colour, string)> interactiveDescriptions, bool interactive = false)
        {
            interactiveDescriptions = null;

            if (interactive)
            {
                interactiveDescriptions = new Dictionary<string, (Colour, Colour, string)>();
            }

            double minValue = Math.Min(Math.Min(values1.Min(), values2.Min()), Math.Min(value1, value2));
            double maxValue = Math.Max(Math.Max(values1.Max(), values2.Max()), Math.Max(value1, value2));

            int resolutionSteps = 100;

            double d = (maxValue - minValue) / (resolutionSteps - 1);

            double[] cdf1 = new double[resolutionSteps];
            double[] cdf2 = new double[resolutionSteps];
            double[] cdf3 = new double[resolutionSteps * 2];

            for (int i = 0; i < resolutionSteps; i++)
            {
                double val = minValue + d * i;

                cdf1[i] = values1.Count(x => x <= val) / (double)values1.Length;
                cdf2[i] = values2.Count(x => x <= val) / (double)values2.Length;

                cdf3[i * 2] = Normal.CDF(0, 1, minValue + d * i);
                cdf3[i * 2 + 1] = Normal.CDF(0, 1, minValue + d * (i + 0.5));
            }

            double[] pdf1 = new double[resolutionSteps];
            double[] pdf2 = new double[resolutionSteps];
            double[] pdf3 = new double[resolutionSteps * 2];

            pdf1[0] = 0.5 * cdf1[1] / d;
            pdf1[resolutionSteps - 1] = 0.5 * (cdf1[resolutionSteps - 1] - cdf1[resolutionSteps - 2]) / d;

            pdf2[0] = 0.5 * cdf2[1] / d;
            pdf2[resolutionSteps - 1] = 0.5 * (cdf2[resolutionSteps - 1] - cdf2[resolutionSteps - 2]) / d;

            pdf3[0] = (Normal.CDF(0, 1, minValue + d * 0.5) - Normal.CDF(0, 1, minValue - d * 0.5)) / d;
            pdf3[resolutionSteps - 1] = (Normal.CDF(0, 1, maxValue + d * 0.5) - Normal.CDF(0, 1, maxValue - d * 0.5)) / d;

            for (int i = 1; i < resolutionSteps - 1; i++)
            {
                pdf1[i] = 0.5 * (cdf1[i + 1] - cdf1[i - 1]) / d;
                pdf2[i] = 0.5 * (cdf2[i + 1] - cdf2[i - 1]) / d;

                pdf3[i * 2] = (cdf3[i * 2 + 1] - cdf3[i * 2 - 1]) / d;
                pdf3[i * 2 + 1] = (cdf3[i * 2 + 2] - cdf3[i * 2]) / d;
            }

            double maxY1 = pdf1.Max();
            double maxY2 = pdf2.Max();
            double maxY3 = pdf3.Max();

            double plotWidth = 400;
            double plotHeight = 225;
            double axisMargin = 10;
            double axisLength = 10;
            Colour plotColour = Colour.FromRgb(0, 114, 178);

            Font axisFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 10);
            Font axisLegend = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
            Font titleFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14);

            Page pag = new Page(1, 1);
            Graphics gpr = pag.Graphics;

            gpr.Save();

            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(-axisMargin - axisLength, 0).LineTo(-axisMargin, 0).LineTo(-axisMargin, plotHeight).LineTo(-axisMargin - axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(-axisMargin - axisLength, i * plotHeight / 5).LineTo(-axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY3 / 5 * i).ToString((maxY3 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(-axisMargin - axisLength - 3 - width, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(-axisMargin - axisLength - 3 - yWidth - axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis3Name).Width + 15) * 0.5 + 15, 0, yAxis3Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis3Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis3Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115));

                gpr.Restore();
            }

            double rightAxisYWidth;

            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(plotWidth + axisMargin + axisLength, 0).LineTo(plotWidth + axisMargin, 0).LineTo(plotWidth + axisMargin, plotHeight).LineTo(plotWidth + axisMargin + axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(plotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(plotWidth + axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY1 / 5 * i).ToString((maxY1 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(plotWidth + axisMargin + axisLength + 3, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(plotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 15, 0, yAxis1Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis1Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178));

                gpr.Restore();

                rightAxisYWidth = yWidth;
            }

            {
                gpr.Save();

                gpr.Translate(axisMargin + axisLength + 3 + rightAxisYWidth + axisLegend.FontSize * 2.8 + axisMargin, 0);

                GraphicsPath yAxis = new GraphicsPath().MoveTo(plotWidth + axisMargin + axisLength, 0).LineTo(plotWidth + axisMargin, 0).LineTo(plotWidth + axisMargin, plotHeight).LineTo(plotWidth + axisMargin + axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(plotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(plotWidth + axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY2 / 5 * i).ToString((maxY2 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(plotWidth + axisMargin + axisLength + 3, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Translate(plotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 15, 0, yAxis2Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis2Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(213, 94, 0));

                gpr.Restore();
            }

            GraphicsPath xAxis = new GraphicsPath().MoveTo(0, plotHeight + axisMargin + axisLength).LineTo(0, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin + axisLength);

            for (int i = 1; i < 7; i++)
            {
                xAxis.MoveTo(i * plotWidth / 7, plotHeight + axisMargin + axisLength).LineTo(i * plotWidth / 7, plotHeight + axisMargin);
            }

            gpr.StrokePath(xAxis, Colours.Black);

            for (int i = 0; i <= 7; i++)
            {
                double val = minValue + (maxValue - minValue) / 7 * i;

                string text = val.ToString(val.GetDigits());

                gpr.FillText(i * plotWidth / 7 - axisFont.MeasureText(text).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent, text, axisFont, Colours.Black, TextBaselines.Baseline);
            }

            GraphicsPath distributionPath1 = new GraphicsPath();
            GraphicsPath distributionPath2 = new GraphicsPath();

            GraphicsPath distributionHighlight1 = new GraphicsPath();
            GraphicsPath distributionHighlight2 = new GraphicsPath();

            GraphicsPath distributionHighlight1Other = new GraphicsPath();
            GraphicsPath distributionHighlight2Other = new GraphicsPath();

            if (p1 <= 0.5)
            {
                distributionHighlight1.MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);


                distributionHighlight1Other.MoveTo(0, plotHeight);
            }
            else
            {
                distributionHighlight1.MoveTo(0, plotHeight);


                distributionHighlight1Other.MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
            }

            if (p2 <= 0.5)
            {
                distributionHighlight2.MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);


                distributionHighlight2Other.MoveTo(0, plotHeight);
            }
            else
            {
                distributionHighlight2.MoveTo(0, plotHeight);


                distributionHighlight2Other.MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
            }

            for (int i = 0; i < resolutionSteps; i++)
            {
                distributionPath1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                distributionPath2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);

                if (p1 > 0.5 && minValue + i * d < value1)
                {
                    distributionHighlight1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }
                else if (p1 <= 0.5 && minValue + i * d > value1)
                {
                    distributionHighlight1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }

                if (p1 <= 0.5 && minValue + i * d < value1)
                {
                    distributionHighlight1Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }
                else if (p1 > 0.5 && minValue + i * d > value1)
                {
                    distributionHighlight1Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }

                if (p2 > 0.5 && minValue + i * d < value2)
                {
                    distributionHighlight2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
                else if (p2 <= 0.5 && minValue + i * d > value2)
                {
                    distributionHighlight2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }

                if (p2 <= 0.5 && minValue + i * d < value2)
                {
                    distributionHighlight2Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
                else if (p2 > 0.5 && minValue + i * d > value2)
                {
                    distributionHighlight2Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
            }


            if (p1 <= 0.5)
            {
                distributionHighlight1.LineTo(plotWidth, plotHeight);
                distributionHighlight1.Close();


                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                distributionHighlight1Other.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight1Other.Close();
            }
            else
            {
                double i = (value1 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                distributionHighlight1.LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight1.Close();


                distributionHighlight1Other.LineTo(plotWidth, plotHeight);
                distributionHighlight1Other.Close();
            }

            if (p2 <= 0.5)
            {
                distributionHighlight2.LineTo(plotWidth, plotHeight);
                distributionHighlight2.Close();


                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                distributionHighlight2Other.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight2Other.Close();
            }
            else
            {
                double i = (value2 - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                distributionHighlight2.LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight2.Close();


                distributionHighlight2Other.LineTo(plotWidth, plotHeight);
                distributionHighlight2Other.Close();
            }

            if (!interactive)
            {
                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3));
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3));
            }
            else
            {
                string sign1;
                string sign1Other;

                double v1Left;
                double v1Right;

                if (p1 <= 0.5)
                {
                    sign1 = ">";
                    sign1Other = "<";

                    v1Left = p1;
                    v1Right = 1 - p1;
                }
                else
                {
                    sign1 = "<";
                    sign1Other = ">";

                    v1Left = 1 - p1;
                    v1Right = p1;
                }

                string sign2;
                string sign2Other;

                double v2Left;
                double v2Right;

                if (p2 <= 0.5)
                {
                    sign2 = ">";
                    sign2Other = "<";

                    v2Left = p2;
                    v2Right = 1 - p2;
                }
                else
                {
                    sign2 = "<";
                    sign2Other = ">";

                    v2Left = 1 - p2;
                    v2Right = p2;
                }


                string tag1Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1Other + " " + variableName + "*) ≈ " + v1Right.ToString(v1Right.GetDigits())));

                string tag2Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2Other + " " + variableName + "*) ≈ " + v2Right.ToString(v2Right.GetDigits())));

                gpr.FillPath(distributionHighlight1Other, Colour.FromRgba(0, 0, 0, 0), tag: tag1Other);
                gpr.FillPath(distributionHighlight2Other, Colour.FromRgba(0, 0, 0, 0), tag: tag2Other);

                string tag1 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1, (Colour.FromRgba(0, 114, 178, 0.3), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1 + " " + variableName + "*) ≈ " + v1Left.ToString(v1Left.GetDigits())));

                string tag2 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2, (Colour.FromRgba(213, 94, 0, 0.3), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2 + " " + variableName + "*) ≈ " + v2Left.ToString(v2Left.GetDigits())));

                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3), tag: tag1);
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3), tag: tag2);
            }

            gpr.StrokePath(distributionPath1, Colour.FromRgb(0, 114, 178), 2, lineJoin: LineJoins.Round);
            gpr.StrokePath(distributionPath2, Colour.FromRgb(213, 94, 0), 2, lineJoin: LineJoins.Round);



            GraphicsPath standardNormalPath = new GraphicsPath();

            for (int i = 0; i < resolutionSteps * 2; i++)
            {
                standardNormalPath.LineTo(i * plotWidth / (resolutionSteps * 2 - 1), plotHeight - (pdf3[i] / maxY3) * plotHeight);
            }

            gpr.StrokePath(standardNormalPath, Colour.FromRgb(0, 158, 115), 1.5, lineJoin: LineJoins.Round);



            gpr.StrokePath(new GraphicsPath().MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(0, 78, 138), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(166, 55, 0), 2, lineDash: new LineDash(5, 5, 0));

            gpr.FillText(plotWidth * 0.5 - axisLegend.MeasureText(xAxisName).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent + axisLegend.FontSize * 1.4, xAxisName, axisLegend, Colours.Black, TextBaselines.Baseline);

            gpr.Restore();

            Rectangle bounds = gpr.GetBounds();

            gpr.FillText(bounds.Location.X + bounds.Size.Width * 0.5 - titleFont.MeasureText(title).Width * 0.5, bounds.Location.Y - 5, title, titleFont, Colours.Black, TextBaselines.Bottom);

            if (!string.IsNullOrEmpty(tag))
            {
                bounds = gpr.GetBounds();

                gpr.FillRectangle(bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, Colour.FromRgba(0, 0, 0, 0), tag: "plotBounds/" + tag);

                Font buttonFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), 7.5);

                for (int i = 0; i < 4; i++)
                {
                    double x = bounds.Location.X + bounds.Size.Width - 24 - 29 * i;
                    gpr.FillRectangle(x, bounds.Location.Y, 24, 24, Colour.FromRgba(0, 0, 0, 0), tag: "buttonBg[" + i.ToString() + "]/" + tag);

                    switch (i)
                    {
                        case 0:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("PDF").Width * 0.5, 16, "PDF", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 1:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("SVG").Width * 0.5, 16, "SVG", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 2:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("CSV").Width * 0.5, 16, "CSV", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 3:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(0, 0).LineTo(5, 0).LineTo(3.5, 1.5).LineTo(6, 4).LineTo(4, 6).LineTo(1.5, 3.5).LineTo(0, 5).Close();

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);

                                gpr.Restore();
                            }
                            break;

                    }
                }
            }

            pag.Crop();

            return pag;
        }

        public static Page GetPlotTwoTailed(double[] pdf1, double[] pdf2, double minValue, double maxValue, double value1, double value2, double p1, double p2, string yAxis1Name, string yAxis2Name, string xAxisName, string title, string variableName, string tag, out Dictionary<string, (Colour, Colour, string)> interactiveDescriptions, bool interactive = false)
        {
            interactiveDescriptions = null;

            if (interactive)
            {
                interactiveDescriptions = new Dictionary<string, (Colour, Colour, string)>();
            }

            int resolutionSteps = 100;

            double d = (maxValue - minValue) / (resolutionSteps - 1);

            double maxY1 = pdf1.Max();
            double maxY2 = pdf2.Max();

            double plotWidth = 400;
            double plotHeight = 225;
            double axisMargin = 10;
            double axisLength = 10;
            Colour plotColour = Colour.FromRgb(0, 114, 178);

            Font axisFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 10);
            Font axisLegend = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
            Font titleFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14);

            Page pag = new Page(1, 1);
            Graphics gpr = pag.Graphics;

            gpr.Save();

            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(-axisMargin - axisLength, 0).LineTo(-axisMargin, 0).LineTo(-axisMargin, plotHeight).LineTo(-axisMargin - axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(-axisMargin - axisLength, i * plotHeight / 5).LineTo(-axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY1 / 5 * i).ToString((maxY1 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(-axisMargin - axisLength - 3 - width, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(-axisMargin - axisLength - 3 - yWidth - axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 15, 0, yAxis1Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis1Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178));

                gpr.Restore();
            }


            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(plotWidth + axisMargin + axisLength, 0).LineTo(plotWidth + axisMargin, 0).LineTo(plotWidth + axisMargin, plotHeight).LineTo(plotWidth + axisMargin + axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(plotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(plotWidth + axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY2 / 5 * i).ToString((maxY2 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(plotWidth + axisMargin + axisLength + 3, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(plotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 15, 0, yAxis2Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis2Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(213, 94, 0));

                gpr.Restore();
            }

            GraphicsPath xAxis = new GraphicsPath().MoveTo(0, plotHeight + axisMargin + axisLength).LineTo(0, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin + axisLength);

            for (int i = 1; i < 7; i++)
            {
                xAxis.MoveTo(i * plotWidth / 7, plotHeight + axisMargin + axisLength).LineTo(i * plotWidth / 7, plotHeight + axisMargin);
            }

            gpr.StrokePath(xAxis, Colours.Black);

            for (int i = 0; i <= 7; i++)
            {
                double val = minValue + (maxValue - minValue) / 7 * i;

                string text = val.ToString(val.GetDigits());

                gpr.FillText(i * plotWidth / 7 - axisFont.MeasureText(text).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent, text, axisFont, Colours.Black, TextBaselines.Baseline);
            }

            GraphicsPath distributionPath1 = new GraphicsPath();
            GraphicsPath distributionPath2 = new GraphicsPath();

            GraphicsPath distributionHighlight1 = new GraphicsPath();
            GraphicsPath distributionHighlight2 = new GraphicsPath();

            GraphicsPath distributionHighlight1Other = new GraphicsPath();
            GraphicsPath distributionHighlight2Other = new GraphicsPath();

            {
                distributionHighlight1.MoveTo(0, plotHeight);


                distributionHighlight1Other.MoveTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (-Math.Abs(value1) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
            }

            {
                distributionHighlight2.MoveTo(0, plotHeight);


                distributionHighlight2Other.MoveTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (-Math.Abs(value2) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
            }

            bool triggered1 = false;
            bool triggered2 = false;

            for (int i = 0; i < resolutionSteps; i++)
            {
                distributionPath1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                distributionPath2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);

                if (minValue + i * d < -Math.Abs(value1) || minValue + i * d > Math.Abs(value1))
                {
                    distributionHighlight1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }
                else if (!triggered1)
                {
                    triggered1 = true;

                    {
                        double i2 = (-Math.Abs(value1) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf1.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf1[iBottom] * (iTop - i2) + pdf1[iTop] * (i2 - iBottom);

                        distributionHighlight1.LineTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                        distributionHighlight1.LineTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                        distributionHighlight1.Close();
                    }

                    {
                        distributionHighlight1.MoveTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                        double i2 = (Math.Abs(value1) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf1.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf1[iBottom] * (iTop - i2) + pdf1[iTop] * (i2 - iBottom);

                        distributionHighlight1.LineTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                    }
                }

                if (minValue + i * d >= -Math.Abs(value1) && minValue + i * d <= Math.Abs(value1))
                {
                    distributionHighlight1Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }

                if (minValue + i * d < -Math.Abs(value2) || minValue + i * d > Math.Abs(value2)) 
                {
                    distributionHighlight2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
                else if (!triggered2)
                {
                    triggered2 = true;

                    {
                        double i2 = (-Math.Abs(value2) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf2.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf2[iBottom] * (iTop - i2) + pdf2[iTop] * (i2 - iBottom);

                        distributionHighlight2.LineTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                        distributionHighlight2.LineTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                        distributionHighlight2.Close();
                    }

                    {
                        distributionHighlight2.MoveTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                        double i2 = (Math.Abs(value2) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf2.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf2[iBottom] * (iTop - i2) + pdf2[iTop] * (i2 - iBottom);

                        distributionHighlight2.LineTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                    }
                }

                if (minValue + i * d >= -Math.Abs(value2) && minValue + i * d <= Math.Abs(value2))
                {
                    distributionHighlight2Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
            }

            {
                distributionHighlight1.LineTo(plotWidth, plotHeight);
                distributionHighlight1.Close();


                double i = (Math.Abs(value1) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                distributionHighlight1Other.LineTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight1Other.Close();
            }

            {
                distributionHighlight2.LineTo(plotWidth, plotHeight);
                distributionHighlight2.Close();


                double i = (Math.Abs(value2) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                distributionHighlight2Other.LineTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight2Other.Close();
            }

            if (p1 > 0.5)
            {
                GraphicsPath temp = distributionHighlight1;
                distributionHighlight1 = distributionHighlight1Other;
                distributionHighlight1Other = temp;
            }

            if (p2 > 0.5)
            {
                GraphicsPath temp = distributionHighlight2;
                distributionHighlight2 = distributionHighlight2Other;
                distributionHighlight2Other = temp;
            }

            if (!interactive)
            {
                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3));
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3));
            }
            else
            {
                string sign1;
                string sign1Other;

                double v1Left;
                double v1Right;

                if (p1 <= 0.5)
                {
                    sign1 = ">";
                    sign1Other = "<";

                    v1Left = p1;
                    v1Right = 1 - p1;
                }
                else
                {
                    sign1 = "<";
                    sign1Other = ">";

                    v1Left = 1 - p1;
                    v1Right = p1;
                }

                string sign2;
                string sign2Other;

                double v2Left;
                double v2Right;

                if (p2 <= 0.5)
                {
                    sign2 = ">";
                    sign2Other = "<";

                    v2Left = p2;
                    v2Right = 1 - p2;
                }
                else
                {
                    sign2 = "<";
                    sign2Other = ">";

                    v2Left = 1 - p2;
                    v2Right = p2;
                }


                string tag1Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1Other + " " + variableName + "*) ≈ " + v1Right.ToString(v1Right.GetDigits())));

                string tag2Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2Other + " " + variableName + "*) ≈ " + v2Right.ToString(v2Right.GetDigits())));

                gpr.FillPath(distributionHighlight1Other, Colour.FromRgba(0, 0, 0, 0), tag: tag1Other);
                gpr.FillPath(distributionHighlight2Other, Colour.FromRgba(0, 0, 0, 0), tag: tag2Other);

                string tag1 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1, (Colour.FromRgba(0, 114, 178, 0.3), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1 + " " + variableName + "*) ≈ " + v1Left.ToString(v1Left.GetDigits())));

                string tag2 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2, (Colour.FromRgba(213, 94, 0, 0.3), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2 + " " + variableName + "*) ≈ " + v2Left.ToString(v2Left.GetDigits())));

                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3), tag: tag1);
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3), tag: tag2);
            }

            gpr.StrokePath(distributionPath1, Colour.FromRgb(0, 114, 178), 2, lineJoin: LineJoins.Round);
            gpr.StrokePath(distributionPath2, Colour.FromRgb(213, 94, 0), 2, lineJoin: LineJoins.Round);

            gpr.StrokePath(new GraphicsPath().MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(0, 78, 138), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(166, 55, 0), 2, lineDash: new LineDash(5, 5, 0));

            gpr.StrokePath(new GraphicsPath().MoveTo((-value1 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((-value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgba(0, 78, 138, 0.5), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((-value2 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((-value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgba(166, 55, 0, 0.5), 2, lineDash: new LineDash(5, 5, 0));

            gpr.FillText(plotWidth * 0.5 - axisLegend.MeasureText(xAxisName).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent + axisLegend.FontSize * 1.4, xAxisName, axisLegend, Colours.Black, TextBaselines.Baseline);

            gpr.Restore();

            Rectangle bounds = gpr.GetBounds();

            gpr.FillText(bounds.Location.X + bounds.Size.Width * 0.5 - titleFont.MeasureText(title).Width * 0.5, bounds.Location.Y - 5, title, titleFont, Colours.Black, TextBaselines.Bottom);

            if (!string.IsNullOrEmpty(tag))
            {
                bounds = gpr.GetBounds();

                gpr.FillRectangle(bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, Colour.FromRgba(0, 0, 0, 0), tag: "plotBounds/" + tag);

                Font buttonFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), 7.5);

                for (int i = 0; i < 4; i++)
                {
                    double x = bounds.Location.X + bounds.Size.Width - 24 - 29 * i;
                    gpr.FillRectangle(x, bounds.Location.Y, 24, 24, Colour.FromRgba(0, 0, 0, 0), tag: "buttonBg[" + i.ToString() + "]/" + tag);

                    switch (i)
                    {
                        case 0:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("PDF").Width * 0.5, 16, "PDF", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 1:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("SVG").Width * 0.5, 16, "SVG", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 2:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("CSV").Width * 0.5, 16, "CSV", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 3:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(0, 0).LineTo(5, 0).LineTo(3.5, 1.5).LineTo(6, 4).LineTo(4, 6).LineTo(1.5, 3.5).LineTo(0, 5).Close();

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);

                                gpr.Restore();
                            }
                            break;

                    }
                }
            }

            pag.Crop();

            return pag;
        }


        public static Page GetPlotWithStandardNormalTwoTailed(double[] pdf1, double[] pdf2, double minValue, double maxValue, double value1, double value2, double p1, double p2, string yAxis1Name, string yAxis2Name, string yAxis3Name, string xAxisName, string title, string variableName, string tag, out Dictionary<string, (Colour, Colour, string)> interactiveDescriptions, bool interactive = false)
        {
            interactiveDescriptions = null;

            if (interactive)
            {
                interactiveDescriptions = new Dictionary<string, (Colour, Colour, string)>();
            }

            
            int resolutionSteps = 100;

            double d = (maxValue - minValue) / (resolutionSteps - 1);

            double[] cdf3 = new double[resolutionSteps * 2];

            double sqrt2 = Math.Sqrt(2);

            for (int i = 0; i < resolutionSteps; i++)
            {
                double val = minValue + d * i;

                cdf3[i * 2] = Normal.CDF(0, sqrt2, minValue + d * i);
                cdf3[i * 2 + 1] = Normal.CDF(0, sqrt2, minValue + d * (i + 0.5));
            }

            double[] pdf3 = new double[resolutionSteps * 2];

            pdf3[0] = (Normal.CDF(0, sqrt2, minValue + d * 0.5) - Normal.CDF(0, sqrt2, minValue - d * 0.5)) / d;
            pdf3[resolutionSteps - 1] = (Normal.CDF(0, sqrt2, maxValue + d * 0.5) - Normal.CDF(0, sqrt2, maxValue - d * 0.5)) / d;

            for (int i = 1; i < resolutionSteps - 1; i++)
            {
                pdf3[i * 2] = (cdf3[i * 2 + 1] - cdf3[i * 2 - 1]) / d;
                pdf3[i * 2 + 1] = (cdf3[i * 2 + 2] - cdf3[i * 2]) / d;
            }

            double maxY1 = pdf1.Max();
            double maxY2 = pdf2.Max();
            double maxY3 = pdf3.Max();

            double plotWidth = 400;
            double plotHeight = 225;
            double axisMargin = 10;
            double axisLength = 10;
            Colour plotColour = Colour.FromRgb(0, 114, 178);

            Font axisFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 10);
            Font axisLegend = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
            Font titleFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14);

            Page pag = new Page(1, 1);
            Graphics gpr = pag.Graphics;

            gpr.Save();

            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(-axisMargin - axisLength, 0).LineTo(-axisMargin, 0).LineTo(-axisMargin, plotHeight).LineTo(-axisMargin - axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(-axisMargin - axisLength, i * plotHeight / 5).LineTo(-axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY3 / 5 * i).ToString((maxY3 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(-axisMargin - axisLength - 3 - width, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(-axisMargin - axisLength - 3 - yWidth - axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis3Name).Width + 15) * 0.5 + 15, 0, yAxis3Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis3Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis3Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115));

                gpr.Restore();
            }

            double rightAxisYWidth;

            {
                GraphicsPath yAxis = new GraphicsPath().MoveTo(plotWidth + axisMargin + axisLength, 0).LineTo(plotWidth + axisMargin, 0).LineTo(plotWidth + axisMargin, plotHeight).LineTo(plotWidth + axisMargin + axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(plotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(plotWidth + axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY1 / 5 * i).ToString((maxY1 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(plotWidth + axisMargin + axisLength + 3, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(plotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 15, 0, yAxis1Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis1Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis1Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178));

                gpr.Restore();

                rightAxisYWidth = yWidth;
            }

            {
                gpr.Save();

                gpr.Translate(axisMargin + axisLength + 3 + rightAxisYWidth + axisLegend.FontSize * 2.8 + axisMargin, 0);

                GraphicsPath yAxis = new GraphicsPath().MoveTo(plotWidth + axisMargin + axisLength, 0).LineTo(plotWidth + axisMargin, 0).LineTo(plotWidth + axisMargin, plotHeight).LineTo(plotWidth + axisMargin + axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(plotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(plotWidth + axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    string text = (maxY2 / 5 * i).ToString((maxY2 / 5 * i).GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(plotWidth + axisMargin + axisLength + 3, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Translate(plotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 15, 0, yAxis2Name, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.FillPath(new GraphicsPath().Arc(-(axisLegend.MeasureText(yAxis2Name).Width + 15) * 0.5 + 5, -axisLegend.MeasureTextAdvanced(yAxis2Name).Top * 0.5, 5, 0, 2 * Math.PI), Colour.FromRgb(213, 94, 0));

                gpr.Restore();
            }

            GraphicsPath xAxis = new GraphicsPath().MoveTo(0, plotHeight + axisMargin + axisLength).LineTo(0, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin + axisLength);

            for (int i = 1; i < 7; i++)
            {
                xAxis.MoveTo(i * plotWidth / 7, plotHeight + axisMargin + axisLength).LineTo(i * plotWidth / 7, plotHeight + axisMargin);
            }

            gpr.StrokePath(xAxis, Colours.Black);

            for (int i = 0; i <= 7; i++)
            {
                double val = minValue + (maxValue - minValue) / 7 * i;

                string text = val.ToString(val.GetDigits());

                gpr.FillText(i * plotWidth / 7 - axisFont.MeasureText(text).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent, text, axisFont, Colours.Black, TextBaselines.Baseline);
            }

            GraphicsPath distributionPath1 = new GraphicsPath();
            GraphicsPath distributionPath2 = new GraphicsPath();

            GraphicsPath distributionHighlight1 = new GraphicsPath();
            GraphicsPath distributionHighlight2 = new GraphicsPath();

            GraphicsPath distributionHighlight1Other = new GraphicsPath();
            GraphicsPath distributionHighlight2Other = new GraphicsPath();

            {
                distributionHighlight1.MoveTo(0, plotHeight);


                distributionHighlight1Other.MoveTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (-Math.Abs(value1) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
            }

            {
                distributionHighlight2.MoveTo(0, plotHeight);


                distributionHighlight2Other.MoveTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                double i = (-Math.Abs(value2) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
            }

            bool triggered1 = false;
            bool triggered2 = false;

            for (int i = 0; i < resolutionSteps; i++)
            {
                distributionPath1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                distributionPath2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);

                if (minValue + i * d < -Math.Abs(value1) || minValue + i * d > Math.Abs(value1))
                {
                    distributionHighlight1.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }
                else if (!triggered1)
                {
                    triggered1 = true;

                    {
                        double i2 = (-Math.Abs(value1) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf1.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf1[iBottom] * (iTop - i2) + pdf1[iTop] * (i2 - iBottom);

                        distributionHighlight1.LineTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                        distributionHighlight1.LineTo((-Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                        distributionHighlight1.Close();
                    }

                    {
                        distributionHighlight1.MoveTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                        double i2 = (Math.Abs(value1) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf1.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf1[iBottom] * (iTop - i2) + pdf1[iTop] * (i2 - iBottom);

                        distributionHighlight1.LineTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                    }
                }

                if (minValue + i * d >= -Math.Abs(value1) && minValue + i * d <= Math.Abs(value1))
                {
                    distributionHighlight1Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf1[i] / maxY1) * plotHeight);
                }

                if (minValue + i * d < -Math.Abs(value2) || minValue + i * d > Math.Abs(value2))
                {
                    distributionHighlight2.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
                else if (!triggered2)
                {
                    triggered2 = true;

                    {
                        double i2 = (-Math.Abs(value2) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf2.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf2[iBottom] * (iTop - i2) + pdf2[iTop] * (i2 - iBottom);

                        distributionHighlight2.LineTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                        distributionHighlight2.LineTo((-Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                        distributionHighlight2.Close();
                    }

                    {
                        distributionHighlight2.MoveTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);

                        double i2 = (Math.Abs(value2) - minValue) / d;

                        int iTop = (int)Math.Min(Math.Ceiling(i2), pdf2.Length - 1);
                        int iBottom = (int)Math.Floor(i2);

                        double val = pdf2[iBottom] * (iTop - i2) + pdf2[iTop] * (i2 - iBottom);

                        distributionHighlight2.LineTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                    }
                }

                if (minValue + i * d >= -Math.Abs(value2) && minValue + i * d <= Math.Abs(value2))
                {
                    distributionHighlight2Other.LineTo(i * plotWidth / (resolutionSteps - 1), plotHeight - (pdf2[i] / maxY2) * plotHeight);
                }
            }

            {
                distributionHighlight1.LineTo(plotWidth, plotHeight);
                distributionHighlight1.Close();


                double i = (Math.Abs(value1) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf1.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf1[iBottom] * (iTop - i) + pdf1[iTop] * (i - iBottom);

                distributionHighlight1Other.LineTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY1) * plotHeight);
                distributionHighlight1Other.LineTo((Math.Abs(value1) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight1Other.Close();
            }

            {
                distributionHighlight2.LineTo(plotWidth, plotHeight);
                distributionHighlight2.Close();


                double i = (Math.Abs(value2) - minValue) / d;

                int iTop = (int)Math.Min(Math.Ceiling(i), pdf2.Length - 1);
                int iBottom = (int)Math.Floor(i);

                double val = pdf2[iBottom] * (iTop - i) + pdf2[iTop] * (i - iBottom);

                distributionHighlight2Other.LineTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight - (val / maxY2) * plotHeight);
                distributionHighlight2Other.LineTo((Math.Abs(value2) - minValue) / (maxValue - minValue) * plotWidth, plotHeight);
                distributionHighlight2Other.Close();
            }

            if (p1 > 0.5)
            {
                GraphicsPath temp = distributionHighlight1;
                distributionHighlight1 = distributionHighlight1Other;
                distributionHighlight1Other = temp;
            }

            if (p2 > 0.5)
            {
                GraphicsPath temp = distributionHighlight2;
                distributionHighlight2 = distributionHighlight2Other;
                distributionHighlight2Other = temp;
            }

            if (!interactive)
            {
                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3));
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3));
            }
            else
            {
                string sign1;
                string sign1Other;

                double v1Left;
                double v1Right;

                if (p1 <= 0.5)
                {
                    sign1 = ">";
                    sign1Other = "<";

                    v1Left = p1;
                    v1Right = 1 - p1;
                }
                else
                {
                    sign1 = "<";
                    sign1Other = ">";

                    v1Left = 1 - p1;
                    v1Right = p1;
                }

                string sign2;
                string sign2Other;

                double v2Left;
                double v2Right;

                if (p2 <= 0.5)
                {
                    sign2 = ">";
                    sign2Other = "<";

                    v2Left = p2;
                    v2Right = 1 - p2;
                }
                else
                {
                    sign2 = "<";
                    sign2Other = ">";

                    v2Left = 1 - p2;
                    v2Right = p2;
                }


                string tag1Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1Other + " " + variableName + "*) ≈ " + v1Right.ToString(v1Right.GetDigits())));

                string tag2Other = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2Other, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2Other + " " + variableName + "*) ≈ " + v2Right.ToString(v2Right.GetDigits())));

                gpr.FillPath(distributionHighlight1Other, Colour.FromRgba(0, 0, 0, 0), tag: tag1Other);
                gpr.FillPath(distributionHighlight2Other, Colour.FromRgba(0, 0, 0, 0), tag: tag2Other);

                string tag1 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag1, (Colour.FromRgba(0, 114, 178, 0.3), Colour.FromRgba(75, 152, 220, 0.3), variableName + "* = " + value1.ToString(value1.GetDigits()) + "\np(" + variableName + " " + sign1 + " " + variableName + "*) ≈ " + v1Left.ToString(v1Left.GetDigits())));

                string tag2 = Guid.NewGuid().ToString();

                interactiveDescriptions.Add(tag2, (Colour.FromRgba(213, 94, 0, 0.3), Colour.FromRgba(255, 134, 51, 0.3), variableName + "* = " + value2.ToString(value2.GetDigits()) + "\np(" + variableName + " " + sign2 + " " + variableName + "*) ≈ " + v2Left.ToString(v2Left.GetDigits())));

                gpr.FillPath(distributionHighlight1, Colour.FromRgba(0, 114, 178, 0.3), tag: tag1);
                gpr.FillPath(distributionHighlight2, Colour.FromRgba(213, 94, 0, 0.3), tag: tag2);
            }

            gpr.StrokePath(distributionPath1, Colour.FromRgb(0, 114, 178), 2, lineJoin: LineJoins.Round);
            gpr.StrokePath(distributionPath2, Colour.FromRgb(213, 94, 0), 2, lineJoin: LineJoins.Round);



            GraphicsPath standardNormalPath = new GraphicsPath();

            for (int i = 0; i < resolutionSteps * 2; i++)
            {
                standardNormalPath.LineTo(i * plotWidth / (resolutionSteps * 2 - 1), plotHeight - (pdf3[i] / maxY3) * plotHeight);
            }

            gpr.StrokePath(standardNormalPath, Colour.FromRgb(0, 158, 115), 1.5, lineJoin: LineJoins.Round);



            gpr.StrokePath(new GraphicsPath().MoveTo((value1 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(0, 78, 138), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((value2 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgb(166, 55, 0), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((-value1 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((-value1 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgba(0, 78, 138, 0.5), 2, lineDash: new LineDash(5, 5, 0));
            gpr.StrokePath(new GraphicsPath().MoveTo((-value2 - minValue) / (maxValue - minValue) * plotWidth, 0).LineTo((-value2 - minValue) / (maxValue - minValue) * plotWidth, plotHeight), Colour.FromRgba(166, 55, 0, 0.5), 2, lineDash: new LineDash(5, 5, 0));

            gpr.FillText(plotWidth * 0.5 - axisLegend.MeasureText(xAxisName).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent + axisLegend.FontSize * 1.4, xAxisName, axisLegend, Colours.Black, TextBaselines.Baseline);

            gpr.Restore();

            Rectangle bounds = gpr.GetBounds();

            gpr.FillText(bounds.Location.X + bounds.Size.Width * 0.5 - titleFont.MeasureText(title).Width * 0.5, bounds.Location.Y - 5, title, titleFont, Colours.Black, TextBaselines.Bottom);

            if (!string.IsNullOrEmpty(tag))
            {
                bounds = gpr.GetBounds();

                gpr.FillRectangle(bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, Colour.FromRgba(0, 0, 0, 0), tag: "plotBounds/" + tag);

                Font buttonFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), 7.5);

                for (int i = 0; i < 4; i++)
                {
                    double x = bounds.Location.X + bounds.Size.Width - 24 - 29 * i;
                    gpr.FillRectangle(x, bounds.Location.Y, 24, 24, Colour.FromRgba(0, 0, 0, 0), tag: "buttonBg[" + i.ToString() + "]/" + tag);

                    switch (i)
                    {
                        case 0:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("PDF").Width * 0.5, 16, "PDF", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 1:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("SVG").Width * 0.5, 16, "SVG", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 2:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("CSV").Width * 0.5, 16, "CSV", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 3:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(0, 0).LineTo(5, 0).LineTo(3.5, 1.5).LineTo(6, 4).LineTo(4, 6).LineTo(1.5, 3.5).LineTo(0, 5).Close();

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);

                                gpr.Restore();
                            }
                            break;

                    }
                }
            }

            pag.Crop();

            return pag;
        }
    }
}
