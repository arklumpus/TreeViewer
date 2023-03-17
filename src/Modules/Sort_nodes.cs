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

using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using System;
using VectSharp;
using System.Runtime.InteropServices;

namespace SortNodes
{
    /// <summary>
    /// This module sort the nodes of the tree based on the number of levels of descendants. If two nodes have the same number
    /// of levels of descendants, they are sorted alphabetically based on the taxon names.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Sort nodes";
        public const string HelpText = "Sorts the nodes of the tree in ascending or descending order.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "8a3e4e83-6c4d-45a8-8737-bff99accd176";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;
        public static bool Repeatable { get; } = false;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC5SURBVDhPrZI9DoMwDIUD4mCwduEu2asidc9dunSFm7V+xo6cqKSW4JMix4HnvyRYYowf2brpxUK8WOslByAeKaUOdnd9cIA6a6uK2/1VtKkVIOu0b9m6q+hNtpX2iL7C8c4CFXB29K8Lvpz/hVsg0caeUPuXUg9xkL5/Iu00GeqfKOBIBoPUW2liH1IhtnNA2bTwLQMf5zn7kRiIWKsqLAdoiRUTRJnez3nrPGLFVgIxzhDgxC2E8AVK310M57Fi/wAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJBSURBVEhLxZQ9aBRBFMd39xaus7BQQppAIKVBm0ia5DqJoKCJ0d42d9xHaT5NwOa+mpOcYCWaJolBMWVSCXZKihAIwVKwPcJBuLv83tzMsh/5MHsBf/Duzbzd+795M2/Htnzk8/lbnU5nn6HbbDb7a7Vao/skPo72hknsJnYjmUxOq0iPBBK02+0p3F53Zl1LAm+LCoXCIAkObdt+yjaNEsozH6xUKkfdN+LhVaBXbxWLxY1GozEnY5L1XIWXgFVPYV9lXK/Xj+Ww/zXBxOxWR0xPA6gEuVzuLmL3EF1Lp9O3xRiv8miYzhqRd+JiKlDb4zjOB9d1/4gxrkiMRM/Fx8UkmEToPaJ9fiO+ifV0DnY2mx1je3ZbrVaqWq3u6rgik8k8SSQS6zx/xOF/0eEIZv+/vX4c+HAFhz/Lx/U7LC4Q28D9xWJX4ZRKpRlWeUfPI1DZEC2c1tP/w0Vt6soPbfoS907GYTj8lXK5/EpPr4zpos8ISScpY/4M/1YecEZyu14/dFcb29LTC7l0i8Ig/JGV21Sh7iQ/CC3g5rEUbRnpPIF3xnE72KJ3Fxm4GsbRfoH4G/b+lw77EXFhRwsF8IkL85EPg9Uf4JKID3QjQUICQgozc/9YSAUqQHyZ1Q9h5/a93hYRMvgFA+LyrlcB4sMI/2Rrtln9hA6fyxmV+PHOx6sA8RXxiD9UgUs4oxJD4PBVBaz+AQm2Ga5SwSeJGYifcJ1819MIoUoinaUS8CXP4pZkHIaEP6jqvp5eEcs6BRKL9NCqWRqKAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAMSSURBVFhHxZY9iBNREMc3iSfBkIBgc1cpKLYi2ol2FiKI4nlnoagHgSsk3xgQCfFOohz5MFhZqCh+IHJicwh2WhxYqFjINWJpIUYICaj58j+zkzW7eS+5eBv8wWTmTZKdefNmZ9f433hE9xGLxRY9Hs/mdrv9pFQqvRO363hF24hGo4cR/DLMFPQF0zselAkg6IyYRK/tOn0JhMPhCSgKWoGsIpltiUTiBH03DvoSCAQCMwga6HQ6WSwz5IM9S3ocqI6ASx4KhW4VCoVX7DGM6XQ6vVXsdXHkyou7kA5pcSmxJZBKpaaw+6PY8Vo2m22TD/Y90o1GY9ReOOfQSmwJtFotLrXX6y2yA8BeIj2uY3AeAe8yn8/f5hWA/QnBv8M8FIlEdpte97AGUTKZ3IOh816WSpBIplgsXpXlQOj8xTRWFo5pB55VAQTvlvg55IZCCNdngpVZPB7/AjWJzvebHjsYzW/RoPuR6EGM5jfi1jJSBWj0Qm1HiR/QWsMj+nBMyQ3DCeCix3llll+J3+9/KOa0aFfgBNBY87VaLQC9wl4FuVzuG2aBv16v7xCXK2jPZqOstwesL9AHkziKObKhrT/3EgwGlzAhf8tyICPfhj6fbxcCL5BguaiSarW6F9pVrAQw8V5jhxNOwSjeKT/5WqlUPojtGkN7APf/GqpCI3gOM+KO6f0LSk3PjSikhFLH2Al0R+D8vVUBFRhOcQn+UhVcoIuxlotr6QlOsNYmkMlktkDlyW42m5dIO8EFN0E9M1eMNglHcIL/p00ADfeUNCpwvVwuf2SnA5SwCXUaMjAJTXD6n7oHcEueQfPdx2j+jOHUbUItUonHkJPsGAwHl+TVFaBdi55nxxA0lVBhC070JYC+u4nAU9j9cs874VDWkURfcMJ2BAh+AIoftQg+9BZVoTkOZXDCVgHs+hppPPPPsuMf6KkEv8yKVgYnrF1i4Myi9JT5DwiVvo6EatC/IAwacxUTc1mWrmBVAMG7k4ve/09BzsN3EZLsChLax79wEds5Y/goX8e64En4U0yXMIw/G4EyYjoaOG8AAAAASUVORK5CYII=";

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
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Order:">
                /// This parameter determines the order according to which the nodes are sorted.
                /// </param>
                ( "Order:", "ComboBox:0[\"Ascending\",\"Descending\"]" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            return (int)currentParameterValues["Order:"] != (int)previousParameterValues["Order:"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            tree.SortNodes((int)parameterValues["Order:"] == 1);
        }

    }
}
