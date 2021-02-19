using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Runtime.InteropServices;

static List<List<string>> GetDirectories(string root)
{
	string[] directories = Directory.GetDirectories(root, "*");
	
	List<List<string>> tbr = new List<List<string>>();
	
	for (int i = 0; i < directories.Length; i++)
	{
		tbr.Add(new List<string> { Path.GetFileName(directories[i]) }); 
		
		List<List<string>> subDirs = GetDirectories(directories[i]);
		
		for (int j = 0; j < subDirs.Count; j++)
		{
			subDirs[j].Insert(0, Path.GetFileName(directories[i]));
			tbr.Add(subDirs[j]);
		}
	}
	
	return tbr;
}

class DirectoryEntry
{
	public string Name { get; set; }
	public List<DirectoryEntry> SubDirectories { get; set; }
	public DirectoryEntry Parent { get; }
	
	public DirectoryEntry(DirectoryEntry parent)
	{
		this.SubDirectories = new List<DirectoryEntry>();
		this.Parent = parent;
	}
	
	public static List<DirectoryEntry> Build(List<List<string>> directories)
	{
		List<DirectoryEntry> tbr = new List<DirectoryEntry>();
		
		for (int i = 0; i < directories.Count; i++)
		{
			List<DirectoryEntry> currLevel = tbr;
			DirectoryEntry currParent = null;
			
			for (int j = 0; j < directories[i].Count; j++)
			{
				DirectoryEntry parent = GetEntry(directories[i][j], currLevel);
			
				if (parent == null)
				{
					parent = new DirectoryEntry(currParent) { Name = directories[i][j] };
					currLevel.Add(parent);
				}
				
				currLevel = parent.SubDirectories;
				currParent = parent;
			}
		}
		
		return tbr;
	}
	
	public static DirectoryEntry GetEntry(string name, List<DirectoryEntry> directories)
	{
		for (int i = 0; i < directories.Count; i++)
		{
			if (directories[i].Name == name)
			{
				return directories[i];
			}
		}
		
		return null;
	}
	
	public string GetId()
	{
		if (this.Parent != null)
		{
			return this.Parent.GetId() + "_" + this.Name.Replace("-", ".");
		}
		else
		{
			return this.Name.Replace("-", ".");
		}	
	}
	
	public string GetXML()
	{
		StringBuilder tbr = new StringBuilder();
		
		tbr.Append("\t\t\t\t<Directory Id=\"" + this.GetId() + "\" Name=\"" + this.Name + "\">\n");
		
		for (int i = 0; i < this.SubDirectories.Count; i++)
		{
			string[] xml = this.SubDirectories[i].GetXML().Split('\n');
			
			foreach (string line in xml)
			{
				tbr.Append("\t" + line + "\n");
			}
			
			if (i < this.SubDirectories.Count - 1)
			{
				tbr.Append("\n");
			}
		}
		
		tbr.Append("\t\t\t\t</Directory>");
		
		return tbr.ToString();
	}
}

Dictionary<string, string> Guids = new Dictionary<string, string>();
string[] savedGuids = File.ReadAllLines("savedGuids.txt");
for (int i = 0; i < savedGuids.Length; i++)
{
	Guids.Add(savedGuids[i].Substring(0, savedGuids[i].IndexOf("\t")), savedGuids[i].Substring(savedGuids[i].IndexOf("\t") + 1));
}

List<DirectoryEntry> directories = DirectoryEntry.Build(GetDirectories("SourceDir"));

string directoriesText = "";

for (int i = 0; i < directories.Count; i++)
{
	directoriesText += directories[i].GetXML();
	
	if (i < directories.Count - 1)
	{
		directoriesText += "\n";
	}
}

string[] files = (from el in Directory.GetFiles("SourceDir", "*", SearchOption.AllDirectories) select el.Substring("SourceDir\\".Length)).ToArray();

Dictionary<string, string> newGuids = new Dictionary<string, string>();

string tbr = "";

for (int i = 0; i < files.Length; i++)
{
	string guid;
	if (Guids.ContainsKey(files[i]))
	{
		guid = Guids[files[i]];
	}
	else
	{
		guid = Guid.NewGuid().ToString();
		newGuids.Add(files[i], guid);
	}
	
	string directoryId = Path.GetDirectoryName(files[i]);
	
	if (string.IsNullOrEmpty(directoryId))
	{
		directoryId = "INSTALLFOLDER";
	}
	else
	{
		directoryId = directoryId.Replace("-", ".").Replace("\\", "_");
	}
	
	tbr += "\t\t<Component Directory=\"" + directoryId + "\" Id=\"" + files[i].Replace("-", ".").Replace("\\", "_") + "\" Guid=\"" + guid + "\">\n\t\t\t<CreateFolder />\n\t\t\t<File Id=\"" + files[i].Replace("-", ".").Replace("\\", "_") + "\" KeyPath=\"yes\" Source=\"SourceDir\\" + files[i] + "\" />\n\t\t</Component>\n";
}

string version = System.Reflection.AssemblyName.GetAssemblyName(@"..\..\Release\Win-x64\TreeViewer.dll").Version.ToString(3);

string file = File.ReadAllText("TreeViewer.wxs.original");
file = file.Replace("@@VersionHere@@", version);
file = file.Replace("<!-- Files here -->", tbr);
file = file.Replace("<!-- Directories here -->", directoriesText);
File.WriteAllText("TreeViewer.wxs", file);

string newGuidsString = "";

foreach (KeyValuePair<string, string> kvp in newGuids)
{
	newGuidsString += kvp.Key + "\t" + kvp.Value + "\n";
}

File.AppendAllText("savedGuids.txt", newGuidsString);
