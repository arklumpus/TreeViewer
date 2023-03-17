/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace NodeAges
{
    /// <summary>
    /// This module computes the ages of nodes, based on the branch lengths in the tree. The ages can be computed
    /// either as a distance from the root of the tree, or as the distance in time from the most recent tip of
    /// the tree.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Compute node ages";
        public const string HelpText = "Computes node ages.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "70ea5221-9faf-4792-b428-5fee9aa1a001";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACwSURBVDhPrZNBDoQgDEUZjzebuQt74xj33MXNXM/h1340hUaczEsIQn8/tYRHMMQY33ma9lXFnFJCvFAMTGIl9OJicApWiRarHWTXSX6O64ahS0E1cx5SzaCODHRBLXJRgZyOjZtIFfILd04nzGEPfqbLIDfSrfDKgL2ZPBMx4E1YPssL+00T5sCg3GkLa6IzOG4vu21eFS2gRQ6+2QOposdENeX0/zymM0ZoMcYhfAEHUmJW1gWyiQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJHSURBVEhLtZYxTxRRFIXH1YLCwsKCwsISOkvXWECFISQQwaAd/gPdSEGxFaVm9Q8IISGBBEkgogkVFpq1MaEiRBuLLSgsJKGgIMHvm31vsoRhdoLLSU7uzNv37pl5c+95ey0pQK1Wu0sYgyNwAPZDcQD34TbcaDQaLQfzkCtAYhPNwxl4w7ECnMBlOIeQwmdwToDkPvEKvAldvAXX4Vf4B4rb8CGchM73IY7gc0Q+EDOcESD5C8Lb9l26BS743r7NB2vuExahWyhesuZduE6uhxiffAn61A04zcTfxEI0m81WtVp9z2UffAAfcf+D8Z/+nr5B2PNf0G15Q+JZxzsxWt88NX6eH7+wMMjzmvAK/oWD5Dmo+APwg5rcbak7cEm41hy3oDmTCqqWotXi1rjnx/5wGYS1z6C5Zsh9xzeIVbDFhMIPWgbk2CWsQnNOKGATCUuxV/gU4ogCsbys814h7sSAArH9YxP1AjFXf6yi0qBch8JlKSgQ/cP2L4OdEiIxV9oH1q3QW4owHKLoJqJ9iH0FtFwxHmIu6OAvhLIisTK3FdAtbYwJGiMq5yJPJMQM5LhHeArNuVGhMTQ0/dzGWGSCpnUhckQyhLU6q3GZ3K1YRXqIfm5PpB5SBEU0PRmGIlzrG5hrzoFsAupThDXoq+nn9eAtXRGe3OQ6qXjC2vTgyc4D/HsPHz/kchTq61Pcf2P83DHYibDnH+HjdKB94CyE61JHpsalt9j+nUemBWG1+EF9g+5HZgQiV3fodwKh//zbkiT/ALTUyTQ92fR/AAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAALESURBVFhHzZchbxRRFIWHDQKBQCAqKhErEAgqCCRU0IRiaAINqa9v2YRgINBQy9IfgGyCKKIbEmrWkJQgQCArKioQiEoEAsH3DfdtZtvddman3XKSk5udnbnnzJ337tw5l5VEq9W6QLgFH8ErcBJOQLEP9+Au7MBuu93+TTwWxxpAWJGncBFe9FgJ/ILrcAUjP/MjQzDUAMLnCc/gErzkMbAD38NP8EfQ86yGvAkfwiYUGnkNVzHyJz9yAAMNxF1vQEsuNqF38/3fz6PB9dcIL+BcfiDLtuH8oGocMsDFVwlb0DvyuXrhN2JlkOs64R10zVitGXJZxR76DMSdf4WKD3Ut7j3vvCEsw7WPr+4/zg8OADkvE6zmNNTEVDFnI6In+iw9MYnPDhMPKF6MA0EOd8gM7EJzb4RWjp4B4ILzmaeyu4BOBORyAS5At6kaauXIDUTpXe1iaNnrICqhCbEUmr0KuM/dapucONKCK4PI7Y5SS82sgRM7nE1GrEQ8TSSNRdeCFfCZ2OF2cFhqn9dBaLgV1ZzWgL1d2OHGhaT1QAM2CWF7HRc+R2xqwL0pbBLjQtKa1EC+HcBZGJjQQG1EWx4JGrBBiPQoymItoliuaCJp7WvA1iuqGngCizuniomktacB+7O4HbEUeAOm/j6KCQcXsasBZzjhJFMJNUwkrY4GfE365mvSGp1kKqGqidBwZFOz26A1Or06QArHqMoYYOKoV3nSWFfbCghfEF40h0PHqMoomHB3vOX3oSkpcjsnqpW/lHojGX++JOjORXkDd2l7ngjI7yvYcc/W74CrXt9EtAodxTyhb2yqi8iVhlM11MrRM4AjSzgPbZMOkFtc6EBZC3HnH+BdaG4nLrVyFCugCUexWeiJd+AXEoy0JkRca9mTuGN537j3/32YJJDE53Y2n2ZFRDXG/3F6EBg5hc/zLPsLX+0UgkzTDswAAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                /// <param name="Age type:">
                /// This parameter determines the kind of age that is computed.
                /// 
                /// If the value is `Since root`, the age of each node corresponds to the distance $d$ (as in, the
                /// sum of branch lengths) from the node to the root of the tree; in this case, the root node would
                /// have an age of `0`.
                /// 
                /// If the value is `Until tips`, first the total length $l$ of the tree from the root node to the
                /// most distant tip is computed; then, the age of each node is $d - l$. In this case, if all the
                /// tips of the tree are contemporaneous, they will have an age of `0`.
                /// </param>
                ( "Age type:", "ComboBox:0[\"Until tips\", \"Since root\"]"),
                
                /// <param name="Attribute:">
                /// The name of the attribute in which the age of the nodes is stored. If an attribute with the same
                /// name already exists, its value will be replaced by this module. The type of the attribute will be
                /// `Number`.
                /// </param>
                ( "Attribute:", "TextBox:Age" ),

                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw
                /// of the tree.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();

            parametersToChange = new Dictionary<string, object>()
            {
                { "Apply", false }
            };


            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string attributeName = (string)parameterValues["Attribute:"];

            bool fromLeft = (int)parameterValues["Age type:"] == 1;

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            double treeHeight = tree.LongestDownstreamLength();

            for (int i = 0; i < nodes.Count; i++)
            {
                double age = nodes[i].UpstreamLength();

                if (!fromLeft)
                {
                    age = treeHeight - age;
                }

                nodes[i].Attributes[attributeName] = age;
            }
        }
    }
}
