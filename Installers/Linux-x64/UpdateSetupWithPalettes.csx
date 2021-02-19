using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

string tbrPalettes = "";
string tbrPaletteCondition = "[ \"$confirm\" != \"0\" ] && ";
string tbrPaletteInstall = "";

string[] palettes = (from el in Directory.GetFiles("..\\..\\Resources\\Palettes", "*.palette") select Path.GetFileName(el)).ToArray();

for (int i = 0; i < palettes.Length; i++)
{
	string[] lines = File.ReadAllLines("..\\..\\Resources\\Palettes\\" + palettes[i]);
	string title = lines[0].Substring(1).Replace("\"", "\\\"").Replace("'", "\\'");
	string desc = lines[1].Substring(1).Replace("\"", "\\\"").Replace("'", "\\'");

    tbrPalettes += "\tprintf \"\\t" + (i + 1).ToString() + ". " + title + "\\n\\t\\t" + desc + "\\n\"\n";

	tbrPaletteCondition += "[ \"$confirm\" != \"" + (i + 1).ToString() + "\" ]";
	if (i < palettes.Length - 1)
	{
		tbrPaletteCondition += " && ";
	}

	tbrPaletteInstall += "\t\tif [ \"$confirm\" = \"" + (i + 1).ToString() + "\" ]; then\n\t\t\tprintf \"\\nInstalling " + title + ".\\n\"\n\t\t\tcp Palettes/" + palettes[i] + " $prefix/sMap/\n\t\tfi\n";
}

string file = File.ReadAllText("sMap_setup.sh.original");

file = file.Replace("#Palettes here#", tbrPalettes);
file = file.Replace("#Palette condition here#", tbrPaletteCondition);
file = file.Replace("#Install palette command here#", tbrPaletteInstall);
file = file.Replace("#Palette max here#", palettes.Length.ToString());

File.WriteAllText("sMap_setup/sMap_setup.sh", file);

