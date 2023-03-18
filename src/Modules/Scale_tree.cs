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
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace ad064aea66b434bd38ceae72b4feb1142
{
    /// <summary>
    /// This module cycles through all the branches in the tree and multiplies their length by the specified
    /// [scaling factor](#scaling-factor).
    /// 
    /// Note that this may yield unexpected results if age distributions have been loaded, because that data
	/// is not affected by this module (hence, it will retain the original scale).
	/// 
	/// Modules that set up age distribution data should have a setting to scale the age distribution data.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Scale tree";
        public const string HelpText = "Scales all branch lengths in the tree by the specified factor.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "d064aea6-6b43-4bd3-8cea-e72b4feb1142";

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJUExURXJyckp9sQAAAML7cfcAAAADdFJOU///ANfKDUEAAAAJcEhZcwAADsIAAA7CARUoSoAAAABDSURBVChThY4BCgAgCANz/390upkWQR2Ueo5oABiOF0GhVsT2FHkcs7gpFGqR/ISRPaH5+Ybgx8gSWatpkbE7UXAEJuKwAZ0xciB3AAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADsSURBVEhLxZXbCsMgDIZ1j7fB9i7t9Q5dr/VdNthezxlrwEpq0oayD4q1YA6/SWNNg67rHnG5T7s53vvmWeSQ1yVI42uYRRFCyG8Tfd+nD9JoKSwaRWMUGgecRHowg1oeIF5ygCdvN8FVkVo2TqIhr5tpRkHJVmItn4QoTXR0ur3Ty+d5TuckDkRVBIZKY/W+xe5lKgrjeH2Rl/EdL+x5WZ6R2onE+GrAyVI2f8MqfgVD7GaYF000VSSaFemioJGgrrnORdbMiU2VwMnqnEt2IehdGq3sdDKDUirql4AZiCTSDBSJA41EgllhzA9Qa1fCA2UemAAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADxSURBVFhH7ZbBDsMgCEDrPm+X/Yuel67ZWf9ll/2ek5Y2nZ1UmZZLX2IohyoCAqpL4L3HrwljzCOIftK2OOeSe1FcUG5QSn2tQPLwQ9Bae1ioViPpgaNY4pZ7O26sU4h7IBvRHIifZE2y47l3++p14AcDypOF6/3lYaHKQvwZngawDIC6sK4NsV4Cy4BVix6J9RKK/9rL+vfzVrQny+yUEaWHAzy/BWIjOIf/TY1CJI5q0eORIXRImKRJWhaivuUcQTJPTzkGiJfiJk8nJ6+staF4KnkPNGHOAVRJ5HMg11IOEGeQVKds6YFxiqbbdNd9AI2IW7LSLuJkAAAAAElFTkSuQmCC";

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
                ( "StateData", "InstanceStateData:"),

                /// <param name="Scaling factor:">
                /// This parameter determines the value by which all branch lengths in the tree will be multiplied.
                /// </param>
                ( "Scaling factor:", "NumericUpDown:1[\"0\",\"Infinity\",\"1\",\"0.####\"]" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string message = "";

            double scalingFactor = (double)parameterValues["Scaling factor:"];

            if (scalingFactor == 0)
            {
                message = "Refusing to scale the tree with a scaling factor of 0!";
            }

            if (scalingFactor != 1 && scalingFactor > 0)
            {
                foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
                {
                    node.Length *= scalingFactor;
                }

                InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

                if (stateData.Tags.ContainsKey("a1ccf05a-cf3c-4ca4-83be-af56f501c2a6") && tree.Attributes.ContainsKey("a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"))
                {
                    message = "The tree contains age distribution data. Please note that this module will only scale the tree!";
                }

                if (stateData.Tags.ContainsKey("32858c9d-0247-497f-aeee-03f7bfe24158") && tree.Attributes.ContainsKey("32858c9d-0247-497f-aeee-03f7bfe24158"))
                {
                    // The tree contains stochastic mapping data, but these are stored in relative terms, thus they shouldn't be affected.
                }

                if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
                {
                    setWarning(message, Id);
                }
            }
        }
    }
}
