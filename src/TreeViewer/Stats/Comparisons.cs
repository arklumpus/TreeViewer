using PhyloTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer.Stats
{
    internal static class Comparisons
    {
        public static (int, double, double) RobinsonFouldsDistance(List<(string[], string[], double)> splits1, List<(string[], string[], double)> splits2)
        {
            bool AreSameSplit((string[], string[], double) split1, (string[], string[], double) split2)
            {
                if (split1.Item1.Length == split1.Item2.Length || split2.Item1.Length == split2.Item2.Length)
                {
                    if (split1.Item1.Length == split1.Item2.Length && split2.Item1.Length == split2.Item2.Length)
                    {
                        return AreSameSplit2(split1.Item1, split1.Item2, split2.Item1, split2.Item2) || AreSameSplit2(split1.Item1, split1.Item2, split2.Item2, split2.Item1);
                    }
                    else
                    {
                        return false;
                    }
                }

                string[] split11, split12;

                if (split1.Item1.Length > split1.Item2.Length)
                {
                    split11 = split1.Item1;
                    split12 = split1.Item2;
                }
                else
                {
                    split11 = split1.Item2;
                    split12 = split1.Item1;
                }

                string[] split21, split22;

                if (split2.Item1.Length > split2.Item2.Length)
                {
                    split21 = split2.Item1;
                    split22 = split2.Item2;
                }
                else
                {
                    split21 = split2.Item2;
                    split22 = split2.Item1;
                }

                return AreSameSplit2(split11, split12, split21, split22);
            }

            HashSet<string> union1 = new HashSet<string>(splits1[0].Item1.Length + splits1[0].Item2.Length + 1);
            HashSet<string> union2 = new HashSet<string>(splits1[0].Item1.Length + splits1[0].Item2.Length + 1);

            bool AreSameSplit2(string[] split11, string[] split12, string[] split21, string[] split22)
            {
                if (split11.Length != split21.Length || split12.Length != split22.Length)
                {
                    return false;
                }

                union2.Clear();

                for (int i = 0; i < split12.Length; i++)
                {
                    union2.Add(split12[i]);
                    union2.Add(split22[i]);
                }

                if (union2.Count != split12.Length)
                {
                    return false;
                }

                union1.Clear();

                for (int i = 0; i < split11.Length; i++)
                {
                    union1.Add(split11[i]);
                    union1.Add(split21[i]);
                }

                return union1.Count == split11.Length;
            }


            bool?[] matched1 = new bool?[splits1.Count];
            bool?[] matched2 = new bool?[splits2.Count];

            double diffSq = 0;

            for (int i = 0; i < splits1.Count; i++)
            {
                matched1[i] = false;

                for (int j = 0; j < splits2.Count; j++)
                {
                    if (AreSameSplit(splits1[i], splits2[j]))
                    {
                        matched1[i] = true;
                        matched2[j] = true;
                        diffSq += (splits1[i].Item3 - splits2[j].Item3) * (splits1[i].Item3 - splits2[j].Item3);
                        break;
                    }
                }
            }

            for (int j = 0; j < splits2.Count; j++)
            {
                if (matched2[j] == null)
                {
                    matched2[j] = false;

                    for (int i = 0; i < splits1.Count; i++)
                    {
                        if (AreSameSplit(splits1[i], splits2[j]))
                        {
                            matched2[j] = true;
                            diffSq += (splits1[i].Item3 - splits2[j].Item3) * (splits1[i].Item3 - splits2[j].Item3);
                            break;
                        }
                    }
                }
            }

            int unweighted = 0;
            double weighted = 0;

            for (int i = 0; i < matched1.Length; i++)
            {
                if (matched1[i] == false)
                {
                    unweighted++;
                    weighted += splits1[i].Item3;
                }
            }

            for (int i = 0; i < matched2.Length; i++)
            {
                if (matched2[i] == false)
                {
                    unweighted++;
                    weighted += splits2[i].Item3;
                }
            }

            return (unweighted, weighted, Math.Sqrt(diffSq));
        }


        public static (int, double, double) RobinsonFouldsDistance(List<(HashSet<string>, HashSet<string>, double)> splits1, List<(HashSet<string>, HashSet<string>, double)> splits2)
        {
            bool AreSameSplit((HashSet<string>, HashSet<string>, double) split1, (HashSet<string>, HashSet<string>, double) split2)
            {
                if (split1.Item1.Count == split1.Item2.Count || split2.Item1.Count == split2.Item2.Count)
                {
                    if (split1.Item1.Count == split1.Item2.Count && split2.Item1.Count == split2.Item2.Count)
                    {
                        return AreSameSplit2(split1.Item1, split1.Item2, split2.Item1, split2.Item2) || AreSameSplit2(split1.Item1, split1.Item2, split2.Item2, split2.Item1);
                    }
                    else
                    {
                        return false;
                    }
                }

                HashSet<string> split11, split12;

                if (split1.Item1.Count > split1.Item2.Count)
                {
                    split11 = split1.Item1;
                    split12 = split1.Item2;
                }
                else
                {
                    split11 = split1.Item2;
                    split12 = split1.Item1;
                }

                HashSet<string> split21, split22;

                if (split2.Item1.Count > split2.Item2.Count)
                {
                    split21 = split2.Item1;
                    split22 = split2.Item2;
                }
                else
                {
                    split21 = split2.Item2;
                    split22 = split2.Item1;
                }

                return AreSameSplit2(split11, split12, split21, split22);
            }

            HashSet<string> union1 = new HashSet<string>(splits1[0].Item1.Count + splits1[0].Item2.Count + 1);
            HashSet<string> union2 = new HashSet<string>(splits1[0].Item1.Count + splits1[0].Item2.Count + 1);

            bool AreSameSplit2(HashSet<string> split11, HashSet<string> split12, HashSet<string> split21, HashSet<string> split22)
            {
                if (split11.Count != split21.Count || split12.Count != split22.Count)
                {
                    return false;
                }

                union2.Clear();

                union2.UnionWith(split12);
                union2.UnionWith(split22);

                if (union2.Count != split12.Count)
                {
                    return false;
                }

                union1.Clear();

                union1.UnionWith(split11);
                union1.UnionWith(split21);

                return union1.Count == split11.Count;
            }


            bool?[] matched1 = new bool?[splits1.Count];
            bool?[] matched2 = new bool?[splits2.Count];

            double diffSq = 0;

            for (int i = 0; i < splits1.Count; i++)
            {
                matched1[i] = false;

                for (int j = 0; j < splits2.Count; j++)
                {
                    if (AreSameSplit(splits1[i], splits2[j]))
                    {
                        matched1[i] = true;
                        matched2[j] = true;
                        diffSq += (splits1[i].Item3 - splits2[j].Item3) * (splits1[i].Item3 - splits2[j].Item3);
                        break;
                    }
                }
            }

            for (int j = 0; j < splits2.Count; j++)
            {
                if (matched2[j] == null)
                {
                    matched2[j] = false;

                    for (int i = 0; i < splits1.Count; i++)
                    {
                        if (AreSameSplit(splits1[i], splits2[j]))
                        {
                            matched2[j] = true;
                            diffSq += (splits1[i].Item3 - splits2[j].Item3) * (splits1[i].Item3 - splits2[j].Item3);
                            break;
                        }
                    }
                }
            }

            int unweighted = 0;
            double weighted = 0;

            for (int i = 0; i < matched1.Length; i++)
            {
                if (matched1[i] == false)
                {
                    unweighted++;
                    weighted += splits1[i].Item3;
                }
            }

            for (int i = 0; i < matched2.Length; i++)
            {
                if (matched2[i] == false)
                {
                    unweighted++;
                    weighted += splits2[i].Item3;
                }
            }

            return (unweighted, weighted, Math.Sqrt(diffSq));
        }

        public static (int, double, double, int, int, int, List<double>) RobinsonFouldsDistanceWithSplitData(List<(string[], string[], double)> splits1, List<(string[], string[], double)> splits2)
        {
            bool AreSameSplit((string[], string[], double) split1, (string[], string[], double) split2)
            {
                if (split1.Item1.Length == split1.Item2.Length || split2.Item1.Length == split2.Item2.Length)
                {
                    if (split1.Item1.Length == split1.Item2.Length && split2.Item1.Length == split2.Item2.Length)
                    {
                        return AreSameSplit2(split1.Item1, split1.Item2, split2.Item1, split2.Item2) || AreSameSplit2(split1.Item1, split1.Item2, split2.Item2, split2.Item1);
                    }
                    else
                    {
                        return false;
                    }
                }

                string[] split11, split12;

                if (split1.Item1.Length > split1.Item2.Length)
                {
                    split11 = split1.Item1;
                    split12 = split1.Item2;
                }
                else
                {
                    split11 = split1.Item2;
                    split12 = split1.Item1;
                }

                string[] split21, split22;

                if (split2.Item1.Length > split2.Item2.Length)
                {
                    split21 = split2.Item1;
                    split22 = split2.Item2;
                }
                else
                {
                    split21 = split2.Item2;
                    split22 = split2.Item1;
                }

                return AreSameSplit2(split11, split12, split21, split22);
            }

            HashSet<string> union1 = new HashSet<string>(splits1[0].Item1.Length + splits1[0].Item2.Length + 1);
            HashSet<string> union2 = new HashSet<string>(splits1[0].Item1.Length + splits1[0].Item2.Length + 1);

            bool AreSameSplit2(string[] split11, string[] split12, string[] split21, string[] split22)
            {
                if (split11.Length != split21.Length || split12.Length != split22.Length)
                {
                    return false;
                }

                union2.Clear();

                for (int i = 0; i < split12.Length; i++)
                {
                    union2.Add(split12[i]);
                    union2.Add(split22[i]);
                }

                if (union2.Count != split12.Length)
                {
                    return false;
                }

                union1.Clear();

                for (int i = 0; i < split11.Length; i++)
                {
                    union1.Add(split11[i]);
                    union1.Add(split21[i]);
                }

                return union1.Count == split11.Length;
            }


            bool?[] matched1 = new bool?[splits1.Count];
            bool?[] matched2 = new bool?[splits2.Count];

            double diffSq = 0;
            List<double> splitLengthDiffs = new List<double>();

            for (int i = 0; i < splits1.Count; i++)
            {
                matched1[i] = false;

                for (int j = 0; j < splits2.Count; j++)
                {
                    if (AreSameSplit(splits1[i], splits2[j]))
                    {
                        matched1[i] = true;
                        matched2[j] = true;
                        splitLengthDiffs.Add(splits1[i].Item3 - splits2[j].Item3);
                        diffSq += (splits1[i].Item3 - splits2[j].Item3) * (splits1[i].Item3 - splits2[j].Item3);
                        break;
                    }
                }
            }

            for (int j = 0; j < splits2.Count; j++)
            {
                if (matched2[j] == null)
                {
                    matched2[j] = false;

                    for (int i = 0; i < splits1.Count; i++)
                    {
                        if (AreSameSplit(splits1[i], splits2[j]))
                        {
                            matched2[j] = true;
                            splitLengthDiffs.Add(splits1[i].Item3 - splits2[j].Item3);
                            diffSq += (splits1[i].Item3 - splits2[j].Item3) * (splits1[i].Item3 - splits2[j].Item3);
                            break;
                        }
                    }
                }
            }

            int unweighted = 0;
            double weighted = 0;
            int in1Notin2 = 0;
            int in2Notin1 = 0;
            int common = 0;

            for (int i = 0; i < matched1.Length; i++)
            {
                if (matched1[i] == false)
                {
                    unweighted++;
                    weighted += splits1[i].Item3;
                    in1Notin2++;
                }
                else
                {
                    common++;
                }
            }

            for (int i = 0; i < matched2.Length; i++)
            {
                if (matched2[i] == false)
                {
                    unweighted++;
                    weighted += splits2[i].Item3;
                    in2Notin1++;
                }
            }

            return (unweighted, weighted, Math.Sqrt(diffSq), in1Notin2, in2Notin1, common, splitLengthDiffs);
        }

    }
}
