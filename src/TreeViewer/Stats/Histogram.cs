using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp;

namespace TreeViewer.Stats
{
    internal class Histogram
    {
        public static (double underflow, int underflowCount, double overflow, int overflowCount) GetOverUnderflow(List<double> branchLengths)
        {
            double[] actualRange = new double[] { branchLengths.Min(), branchLengths.Max() };

            double[] hdi = BayesStats.HighestDensityInterval(branchLengths, 0.89);

            double overflow = double.PositiveInfinity;
            double underflow = double.NegativeInfinity;

            int overflowCount = 0;
            int underflowCount = 0;

            if ((hdi[1] - hdi[0]) / (actualRange[1] - actualRange[0]) < 0.75)
            {
                overflow = hdi[1];
                underflow = hdi[0];

                if ((underflow - actualRange[0]) / (hdi[1] - hdi[0]) < 0.1)
                {
                    underflow = actualRange[0];
                }

                if ((actualRange[1] - overflow) / (hdi[1] - hdi[0]) < 0.1)
                {
                    overflow = actualRange[1];
                }

                foreach (double d in branchLengths)
                {
                    if (d > overflow)
                    {
                        overflowCount++;
                    }
                    else if (d < underflow)
                    {
                        underflowCount++;
                    }
                }
            }

            return (underflow, underflowCount, overflow, overflowCount);
        }

        public static Page GetPlot(List<double> values, string xAxisName, string title, string tag)
        {
            return GetPlot(values, xAxisName, title, tag, out _, false);
        }

        public static Page GetPlot(List<double> values, string xAxisName, string title, string tag, out Dictionary<string, (Colour, Colour, string)> interactiveDescriptions, bool interactive = false)
        {
            interactiveDescriptions = null;

            if (interactive)
            {
                interactiveDescriptions = new Dictionary<string, (Colour, Colour, string)>();
            }

            List<double> originalBranchLenghts = values;

            double[] actualRange = new double[] { values.Min(), values.Max() };

            double[] hdi = BayesStats.HighestDensityInterval(values, 0.89);

            double overflow = double.PositiveInfinity;
            double underflow = double.NegativeInfinity;

            int overflowCount = 0;
            int underflowCount = 0;

            List<double> actualBranchLengths = new List<double>(values.Count / 2);

            if ((hdi[1] - hdi[0]) / (actualRange[1] - actualRange[0]) < 0.75)
            {
                overflow = hdi[1];
                underflow = hdi[0];

                if ((underflow - actualRange[0]) / (hdi[1] - hdi[0]) < 0.1)
                {
                    underflow = actualRange[0];
                }

                if ((actualRange[1] - overflow) / (hdi[1] - hdi[0]) < 0.1)
                {
                    overflow = actualRange[1];
                }

                foreach (double d in values)
                {
                    if (d > overflow)
                    {
                        overflowCount++;
                    }
                    else if (d < underflow)
                    {
                        underflowCount++;
                    }
                    else
                    {
                        actualBranchLengths.Add(d);
                    }
                }

                values = actualBranchLengths;
            }

            double minValue = values.Min();
            double maxValue = values.Max();


            (double _, double _, double iqr) = values.IQR();
            double h2 = 2 * iqr / Math.Pow(values.Count, 1.0 / 3.0);
            int binCount = Math.Max(1, (int)Math.Ceiling((maxValue - minValue) / h2));

            if (h2 == 0)
            {
                binCount = 1;
            }

            int[] bins = new int[binCount];

            if (binCount > 1)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    bins[(int)Math.Min(binCount - 1, (values[i] - minValue) / (maxValue - minValue) * binCount)]++;
                }
            }
            else
            {
                bins[0] = values.Count;
            }


            int binMax = Math.Max(Math.Max(overflowCount, underflowCount), bins.Max());

            binMax = (int)Math.Ceiling(binMax / 5.0) * 5;

            double plotWidth = 400;
            double plotHeight = 225;
            double axisMargin = 10;
            double axisLength = 10;
            Colour plotColour = Colour.FromRgb(0, 114, 178);
            Colour plotHighlightColour = Colour.FromRgb(75, 152, 220);

            double binWidth = plotWidth / binCount;

            Font axisFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 10);
            Font axisLegend = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
            Font titleFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14);

            Page pag = new Page(1, 1);
            Graphics gpr = pag.Graphics;

            gpr.Save();

            GraphicsPath yAxis = new GraphicsPath().MoveTo(-axisMargin - axisLength, 0).LineTo(-axisMargin, 0).LineTo(-axisMargin, plotHeight).LineTo(-axisMargin - axisLength, plotHeight);

            for (int i = 1; i < 5; i++)
            {
                yAxis.MoveTo(-axisMargin - axisLength, i * plotHeight / 5).LineTo(-axisMargin, i * plotHeight / 5);
            }

            double yWidth = 0;

            gpr.StrokePath(yAxis, Colours.Black);

            for (int i = 0; i <= 5; i++)
            {
                string text = (binMax / 5 * i).ToString();

                double width = axisFont.MeasureText(text).Width;

                yWidth = Math.Max(width, yWidth);

                gpr.FillText(-axisMargin - axisLength - 3 - width, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
            }

            gpr.Save();
            gpr.Translate(-axisMargin - axisLength - 3 - yWidth - axisLegend.FontSize * 1.4, plotHeight * 0.5);
            gpr.Rotate(-Math.PI * 0.5);

            gpr.FillText(-axisLegend.MeasureText("Frequency").Width * 0.5, 0, "Frequency", axisLegend, Colours.Black, TextBaselines.Baseline);

            gpr.Restore();

            if (underflowCount > 0)
            {
                gpr.Translate(binWidth + axisMargin, 0);
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

            for (int i = 0; i < binCount; i++)
            {
                if (!interactive)
                {
                    gpr.FillRectangle(i * binWidth, plotHeight - (double)bins[i] / binMax * plotHeight, binWidth, (double)bins[i] / binMax * plotHeight, plotColour);
                    gpr.StrokeRectangle(i * binWidth, plotHeight - (double)bins[i] / binMax * plotHeight, binWidth, (double)bins[i] / binMax * plotHeight, Colours.Black);
                }
                else
                {
                    double minRange = minValue + (maxValue - minValue) / binCount * i;
                    double maxRange = minValue + (maxValue - minValue) / binCount * (i + 1);

                    string barTag = Guid.NewGuid().ToString();
                    string description = "Range: " + minRange.ToString(minRange.GetDigits()) + " - " + maxRange.ToString(maxRange.GetDigits()) + "; " + bins[i].ToString() + " values";

                    interactiveDescriptions.Add(barTag, (plotColour, plotHighlightColour, description));

                    gpr.FillRectangle(i * binWidth, plotHeight - (double)bins[i] / binMax * plotHeight, binWidth, (double)bins[i] / binMax * plotHeight, plotColour, tag: barTag);
                    gpr.StrokeRectangle(i * binWidth, plotHeight - (double)bins[i] / binMax * plotHeight, binWidth, (double)bins[i] / binMax * plotHeight, Colours.Black);
                }
            }

            if (overflowCount > 0)
            {
                if (!interactive)
                {
                    gpr.FillRectangle(axisMargin + binCount * binWidth, plotHeight - (double)overflowCount / binMax * plotHeight, binWidth, (double)overflowCount / binMax * plotHeight, plotColour);
                    gpr.StrokeRectangle(axisMargin + binCount * binWidth, plotHeight - (double)overflowCount / binMax * plotHeight, binWidth, (double)overflowCount / binMax * plotHeight, Colours.Black);
                }
                else
                {
                    string barTag = Guid.NewGuid().ToString();
                    string description = "Overflow bin: " + overflowCount.ToString() + " values greater than " + overflow.ToString(overflow.GetDigits());

                    interactiveDescriptions.Add(barTag, (plotColour, plotHighlightColour, description));

                    gpr.FillRectangle(axisMargin + binCount * binWidth, plotHeight - (double)overflowCount / binMax * plotHeight, binWidth, (double)overflowCount / binMax * plotHeight, plotColour, tag: barTag);
                    gpr.StrokeRectangle(axisMargin + binCount * binWidth, plotHeight - (double)overflowCount / binMax * plotHeight, binWidth, (double)overflowCount / binMax * plotHeight, Colours.Black);
                }
            }

            if (underflowCount > 0)
            {
                if (!interactive)
                {
                    gpr.FillRectangle(-axisMargin - binWidth, plotHeight - (double)underflowCount / binMax * plotHeight, binWidth, (double)underflowCount / binMax * plotHeight, plotColour);
                    gpr.StrokeRectangle(-axisMargin - binWidth, plotHeight - (double)underflowCount / binMax * plotHeight, binWidth, (double)underflowCount / binMax * plotHeight, Colours.Black);
                }
                else
                {
                    string barTag = Guid.NewGuid().ToString();
                    string description = "Underflow bin: " + underflowCount.ToString() + " values smaller than " + underflow.ToString(overflow.GetDigits());

                    interactiveDescriptions.Add(barTag, (plotColour, plotHighlightColour, description));

                    gpr.FillRectangle(-axisMargin - binWidth, plotHeight - (double)underflowCount / binMax * plotHeight, binWidth, (double)underflowCount / binMax * plotHeight, plotColour, tag: barTag);
                    gpr.StrokeRectangle(-axisMargin - binWidth, plotHeight - (double)underflowCount / binMax * plotHeight, binWidth, (double)underflowCount / binMax * plotHeight, Colours.Black);
                }

            }

            (double q3, double q1, double _) = originalBranchLenghts.IQR();
            double median = originalBranchLenghts.Median();

            GraphicsPath whiskers = new GraphicsPath().MoveTo((hdi[0] - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 15).LineTo((hdi[0] - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 5)
                .MoveTo((hdi[0] - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 10).LineTo((hdi[1] - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 10)
                .MoveTo((hdi[1] - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 15).LineTo((hdi[1] - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 5);

            gpr.StrokePath(whiskers, Colours.Black);

            if (!interactive)
            {
                gpr.FillRectangle((q1 - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 20, (q3 - q1) / (maxValue - minValue) * plotWidth, 20, plotColour);
            }
            else
            {
                string barTag = Guid.NewGuid().ToString();
                string description = "Median: " + median.ToString(median.GetDigits()) + "\nInterquartile range: " + q1.ToString(q1.GetDigits()) + " - " + q3.ToString(q3.GetDigits()) + "\n89% HDI: " + hdi[0].ToString(hdi[0].GetDigits()) + " - " + hdi[1].ToString(hdi[1].GetDigits());

                interactiveDescriptions.Add(barTag, (plotColour, plotHighlightColour, description));

                gpr.FillRectangle((q1 - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 20, (q3 - q1) / (maxValue - minValue) * plotWidth, 20, plotColour, tag: barTag);
            }

            gpr.StrokeRectangle((q1 - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 20, (q3 - q1) / (maxValue - minValue) * plotWidth, 20, Colours.Black);
            gpr.StrokePath(new GraphicsPath().MoveTo((median - minValue) / (maxValue - minValue) * plotWidth, -axisMargin - 20).LineTo((median - minValue) / (maxValue - minValue) * plotWidth, -axisMargin), Colours.Black);

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
