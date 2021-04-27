using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class BrushTransition : Transition<IBrush>
    {
        public override IObservable<IBrush> DoTransition(IObservable<double> progress, IBrush oldValue, IBrush newValue)
        {            
            if (oldValue is SolidColorBrush brush1 && newValue is SolidColorBrush brush2)
            {
                Color c1 = brush1.Color;
                Color c2 = brush2.Color;

                return progress.Select(p =>
                {
                    double f = Easing.Ease(p);

                    return new SolidColorBrush(Blend(c1, c2, f));
                });
            }
            else if ((oldValue is LinearGradientBrush || oldValue is SolidColorBrush) && (newValue is LinearGradientBrush || newValue is SolidColorBrush))
            {
                LinearGradientBrush lb1 = GetLinearGradientBrush(oldValue);
                LinearGradientBrush lb2 = GetLinearGradientBrush(newValue);

                List<(double, Color)> stops1 = new List<(double, Color)>(from el in lb1.GradientStops orderby el.Offset ascending select (el.Offset, el.Color));
                List<(double, Color)> stops2 = new List<(double, Color)>(from el in lb2.GradientStops orderby el.Offset ascending select (el.Offset, el.Color));

                if (stops1.Count > stops2.Count)
                {
                    List<(double, int)> available = (from el in Enumerable.Range(0, stops2.Count) select (stops2[el].Item1, el)).ToList();

                    List<(double, int)> unassigned = (from el in Enumerable.Range(0, stops1.Count) select (stops1[el].Item1, el)).ToList();

                    List<(int, int)> assignments = new List<(int, int)>();

                    while (available.Count > 0)
                    {
                        double minDist = double.MaxValue;
                        int minDistI = -1;
                        int minDistJ = -1;

                        for (int i = 0; i < unassigned.Count; i++)
                        {
                            for (int j = 0; j < available.Count; j++)
                            {
                                double dist = Math.Abs(unassigned[i].Item1 - available[j].Item1);

                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    minDistI = i;
                                    minDistJ = j;
                                }
                            }
                        }

                        assignments.Add((minDistI, minDistJ));

                        unassigned.RemoveAt(minDistI);
                        available.RemoveAt(minDistJ);
                    }

                    Color[] newColors = new Color[unassigned.Count];

                    for (int i = 0; i < unassigned.Count; i++)
                    {
                        newColors[i] = GetColorAt(stops2, unassigned[i].Item1);
                    }

                    for (int i = 0; i < unassigned.Count; i++)
                    {
                        stops2.Add((unassigned[i].Item1, newColors[i]));
                    }

                    stops2.Sort((a, b) => Math.Sign(a.Item1 - b.Item1));
                }
                else if (stops2.Count > stops1.Count)
                {
                    List<(double, int)> available = (from el in Enumerable.Range(0, stops1.Count) select (stops1[el].Item1, el)).ToList();

                    List<(double, int)> unassigned = (from el in Enumerable.Range(0, stops2.Count) select (stops2[el].Item1, el)).ToList();

                    List<(int, int)> assignments = new List<(int, int)>();

                    while (available.Count > 0)
                    {
                        double minDist = double.MaxValue;
                        int minDistI = -1;
                        int minDistJ = -1;

                        for (int i = 0; i < unassigned.Count; i++)
                        {
                            for (int j = 0; j < available.Count; j++)
                            {
                                double dist = Math.Abs(unassigned[i].Item1 - available[j].Item1);

                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    minDistI = i;
                                    minDistJ = j;
                                }
                            }
                        }

                        assignments.Add((minDistI, minDistJ));

                        unassigned.RemoveAt(minDistI);
                        available.RemoveAt(minDistJ);
                    }

                    Color[] newColors = new Color[unassigned.Count];

                    for (int i = 0; i < unassigned.Count; i++)
                    {
                        newColors[i] = GetColorAt(stops1, unassigned[i].Item1);
                    }

                    for (int i = 0; i < unassigned.Count; i++)
                    {
                        stops1.Add((unassigned[i].Item1, newColors[i]));
                    }

                    stops1.Sort((a, b) => Math.Sign(a.Item1 - b.Item1));
                }

                return progress.Select(p =>
                {
                    double f = Easing.Ease(p);

                    LinearGradientBrush transitionBrush = new LinearGradientBrush() { StartPoint = Blend(lb1.StartPoint, lb2.StartPoint, f), EndPoint = Blend(lb1.EndPoint, lb2.EndPoint, f) };

                    for (int i = 0; i < stops1.Count; i++)
                    {
                        transitionBrush.GradientStops.Add(new Avalonia.Media.GradientStop(Blend(stops1[i].Item2, stops2[i].Item2, f), stops1[i].Item1 * (1 - f) + stops2[i].Item1 * f));
                    }

                    return transitionBrush;
                });
            }
            else
            {
                return progress.Select(p =>
                {
                    double f = Easing.Ease(p);

                    return f > 0.5 ? newValue : oldValue;
                });
            }
        }

        static Color GetColorAt(List<(double, Color)> brush, double position)
        {
            position = Math.Max(Math.Min(position, 1), 0);

            double prevPos = 0;
            double pos = brush[1].Item1;
            int index = 1;

            while (pos < position && index < brush.Count - 1)
            {
                prevPos = pos;
                index++;
                pos = brush[index].Item1;
            }

            if (pos - prevPos > 0)
            {
                double factor = (position - prevPos) / (pos - prevPos);
                return Blend(brush[index].Item2, brush[index - 1].Item2, 1 - factor);
            }
            else
            {
                return brush[index].Item2;
            }

        }

        static LinearGradientBrush GetLinearGradientBrush(IBrush brush)
        {
            if (brush is LinearGradientBrush linear)
            {
                return linear;
            }
            else if (brush is SolidColorBrush solid)
            {
                LinearGradientBrush tbr = new LinearGradientBrush();
                tbr.GradientStops.Add(new Avalonia.Media.GradientStop(solid.Color, 0));
                tbr.GradientStops.Add(new Avalonia.Media.GradientStop(solid.Color, 1));
                return tbr;
            }
            else
            {
                return new LinearGradientBrush();
            }
        }

        static Color Blend(Color c1, Color c2, double position)
        {
            byte a = (byte)(c1.A * (1 - position) + c2.A * position);
            byte r = (byte)(c1.R * (1 - position) + c2.R * position);
            byte g = (byte)(c1.G * (1 - position) + c2.G * position);
            byte b = (byte)(c1.B * (1 - position) + c2.B * position);

            return Color.FromArgb(a, r, g, b);
        }

        static RelativePoint Blend(RelativePoint p1, RelativePoint p2, double position)
        {
            if (p1.Unit == p2.Unit)
            {
                return new RelativePoint((p1.Point.X + p2.Point.X) * 0.5, (p1.Point.Y + p2.Point.Y) * 0.5, p1.Unit);
            }
            else
            {
                return position > 0.5 ? p2 : p1;
            }
        }
    }
}
