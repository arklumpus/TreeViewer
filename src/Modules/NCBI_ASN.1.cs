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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PhyloTree;
using PhyloTree.Extensions;
using PhyloTree.Formats;
using TreeViewer;

namespace OpenNewick
{
    /// <summary>
    /// This module adds support for the NCBI ASN.1 format (see [this link](https://www.ncbi.nlm.nih.gov/IEB/ToolBox/CPP_DOC/asn_spec/biotree.asn.html)
    /// for the formal definition of a tree object in this format, [this link](https://www.ncbi.nlm.nih.gov/tools/treeviewer/biotreecontainer/) for more
    /// information about the format and the feature names with special meaning, and [this link](https://www.ncbi.nlm.nih.gov/IEB/ToolBox/SDKDOCS/ASNLIB.HTML)
    /// for more information about the ASN.1 standard in NCBI).
    /// 
    /// Trees in this format can be downloaded e.g. from the output of a BLAST search, and will include more information about the sequences other than
    /// their name (with respect to downloading the tree in Newick format).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "NCBI ASN.1";
        public const string HelpText = "Opens a file containing a tree in the NCBI ASN.1 text and binary format.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "27d3b548-0bfe-4c8d-99f6-e8111a20f019";
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        public static string[] Extensions { get; } = { "NCBI ASN.1 files", "asn", "asnb" };
        public static double IsSupported(string fileName)
        {
            using FileStream fileStream = File.OpenRead(fileName);

            using BinaryReader reader = new BinaryReader(fileStream);

            byte[] header = reader.ReadBytes(4);

            if (header[0] == 0x30 && header[1] == 0x80 && header[3] == 0x80)
            {
                return 0.8;
            }

            fileStream.Seek(0, SeekOrigin.Begin);

            using (StreamReader sr = new StreamReader(fileStream))
            {
                StringBuilder sb = new StringBuilder();

                int c = sr.Read();

                while (c >= 0 && char.IsWhiteSpace((char)c))
                {
                    c = sr.Read();
                }

                for (int i = 0; i < 16; i++)
                {
                    sb.Append((char)c);
                    c = sr.Read();
                }

                if (sb.ToString() == "BioTreeContainer")
                {
                    return 0.8;
                }
            }

            return 0;
        }

        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            byte[] header;

            using (FileStream fileStream = File.OpenRead(fileName))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    header = reader.ReadBytes(4);
                }
            }

            if (header[0] == 0x30 && header[1] == 0x80 && header[3] == 0x80)
            {
                yield return NcbiAsnBer.ParseAllTrees(fileName)[0];
            }
            else
            {
                yield return NcbiAsnText.ParseAllTrees(fileName)[0];
            }
        }
    }
}