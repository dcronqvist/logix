using ImGuiNET;
using LogiX.Architecture;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LogiX.Graphics.UI;

public enum FileDialogType
{
    SelectFolder,
    SelectFile,
    SaveFile
}

public class FileDialog : Modal
{
    private static readonly Vector2 DefaultFilePickerSize = new Vector2(600, 400);

    public static string LastDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    private string _currentFolder;
    public string CurrentFolder
    {
        get
        {
            return _currentFolder;
        }
        set
        {
            _currentFolder = value;
            LastDirectory = value;
        }
    }
    public string currentSelectedFile;
    public string SelectedFile { get; set; }
    public string[] FilteredExtensions { get; set; }

    public FileDialogType Type { get; set; }
    private Action<string> OnSelect { get; set; }

    public FileDialog(string startDirectory, FileDialogType fdt, Action<string> onSelect, params string[] filteredExtensions) : base(fdt.ToString(), ImGuiWindowFlags.AlwaysAutoResize, ImGuiPopupFlags.None)
    {
        this.CurrentFolder = startDirectory;
        this.SelectedFile = null;
        this.FilteredExtensions = filteredExtensions;
        this.Type = fdt;
        this.currentSelectedFile = "";
        this.OnSelect = onSelect;
    }

    public string[] GetMax5Parents(string directory)
    {
        List<string> parents = new List<string>();

        string dir = directory;

        for (int i = 0; i < 5; i++)
        {
            parents.Add(dir);
            if (Directory.GetParent(dir) == null)
            {
                break;
            }
            dir = Directory.GetParent(dir).FullName;
        }

        return parents.ToArray();
    }

    public bool ValidateFile(string filePath, IEnumerable<string> validFileExtensions)
    {
        bool fileExists = File.Exists(filePath);
        bool validExtension = validFileExtensions.Count() > 0 ? validFileExtensions.Contains(Path.GetExtension(filePath)) : true;

        return fileExists && validExtension;
    }

#pragma warning disable CA1416 // Validate platform compatibility
    private string GetDownloadFolderPath()
    {
#if _WINDOWS
        return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
#elif _OSX
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif
    }
#pragma warning restore CA1416 // Validate platform compatibility

    public override void SubmitUI(Editor editor)
    {
        if (this.Type == FileDialogType.SelectFile)
        {
            string[] parents = GetMax5Parents(this.CurrentFolder);

            foreach (string parent in parents.Reverse())
            {
                string dirName = parent.Split(Path.DirectorySeparatorChar).Last();
                if (dirName == "")
                {
                    dirName = "/";
                }

                if (ImGui.Button(dirName))
                {
                    this.CurrentFolder = parent;
                }
                ImGui.SameLine();
            }

            ImGui.NewLine();

            ImGui.BeginChild("File Area", new Vector2(400, 250), true);

            if (ImGui.MenuItem(".."))
            {
                this.CurrentFolder = Directory.GetParent(this.CurrentFolder).FullName;
            }

            string[] subDirs = Directory.GetDirectories(this.CurrentFolder);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorF.DarkGoldenRod.ToVector4());
            foreach (string subDir in subDirs)
            {
                DirectoryInfo di = new DirectoryInfo(subDir);
                if (ImGui.MenuItem($"[dir] {di.Name}"))
                {
                    this.CurrentFolder = subDir;
                }
            }
            ImGui.PopStyleColor();

            string[] files = Directory.GetFiles(this.CurrentFolder);
            if (this.FilteredExtensions.Length > 0)
                files = files.Where(file => this.FilteredExtensions.Contains(Path.GetExtension(file))).ToArray();
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                string fileText = $"[file] {fi.Name}";

                if (ImGui.Selectable(fileText, this.currentSelectedFile == file))
                {
                    this.currentSelectedFile = file;
                }

                float fileTextLength = ImGui.CalcTextSize(fileText).X;
                float startDistNext = 200f;
                float offset = startDistNext - fileTextLength;

                ImGui.SameLine();
                ImGui.Dummy(new Vector2(offset, 1));
                ImGui.SameLine();
                ImGui.Text(Utilities.GetAsByteString(fi.Length));
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("Common Directories", new Vector2(130, 250), true);

            ImGui.TextDisabled("Common Directories");

            if (ImGui.Button("LogiX", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                this.CurrentFolder = Directory.GetCurrentDirectory();
            }
            // if (editor.loadedProject != null && editor.loadedProject.HasFile() && ImGui.Button("Project Dir", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            // {
            //     this.CurrentFolder = Path.GetDirectoryName(editor.loadedProject.LoadedFromFile);
            // }
            ImGui.Separator();

            (string, string)[] specialFolders = new (string, string)[] {
                (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Desktop"),
                (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents"),
            };

            foreach ((string, string) specialFolder in specialFolders)
            {
                if (ImGui.Button(specialFolder.Item2, new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                {
                    this.CurrentFolder = specialFolder.Item1;
                }
            }

            ImGui.EndChild();

            ImGui.InputText("Selected file", ref this.currentSelectedFile, 100);

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            if (ImGui.Button("Select"))
            {
                if (!ValidateFile(this.currentSelectedFile, this.FilteredExtensions))
                {
                    // Make some kind of error stuff
                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    // File is valid, return to editor
                    this.OnSelect(this.currentSelectedFile);
                    ImGui.CloseCurrentPopup();
                }
            }
        }
        else if (this.Type == FileDialogType.SelectFolder)
        {
            string[] parents = GetMax5Parents(this.CurrentFolder);

            foreach (string parent in parents.Reverse())
            {
                string dirName = parent.Split(Path.DirectorySeparatorChar).Last();
                if (dirName == "")
                {
                    dirName = "/";
                }

                if (ImGui.Button(dirName))
                {
                    this.CurrentFolder = parent;
                }
                ImGui.SameLine();
            }

            ImGui.NewLine();

            ImGui.BeginChild("hej", new Vector2((ImGui.GetWindowContentRegionMax().X / 4) * 3 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            if (ImGui.MenuItem(".."))
            {
                this.CurrentFolder = Directory.GetParent(this.CurrentFolder).FullName;
            }

            string[] subDirs = Directory.GetDirectories(this.CurrentFolder);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorF.DarkGoldenRod.ToVector4());
            foreach (string subDir in subDirs)
            {
                DirectoryInfo di = new DirectoryInfo(subDir);
                if (ImGui.Selectable($"[dir] {di.Name}"))
                {
                    this.CurrentFolder = subDir;
                }
            }
            ImGui.PopStyleColor();

            string[] files = Directory.GetFiles(this.CurrentFolder);
            foreach (string file in files.Where(file => this.FilteredExtensions.Contains(Path.GetExtension(file))))
            {
                FileInfo fi = new FileInfo(file);

                string fileText = $"[file] {fi.Name}";

                if (ImGui.Selectable(fileText, this.currentSelectedFile == file))
                {
                    this.currentSelectedFile = file;
                }

                float fileTextLength = ImGui.CalcTextSize(fileText).X;
                float startDistNext = 200f;
                float offset = startDistNext - fileTextLength;

                ImGui.SameLine();
                ImGui.Dummy(new Vector2(offset, 1));
                ImGui.SameLine();
                ImGui.Text(Utilities.GetAsByteString(fi.Length));
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("hej2", new Vector2(ImGui.GetWindowContentRegionMax().X / 4 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            ImGui.Text("Group 2");

            ImGui.EndChild();

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            if (ImGui.Button("Select"))
            {
                if (!Directory.Exists(this.CurrentFolder))
                {
                    // Make some kind of error stuff
                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    // File is valid, return to editor
                    this.OnSelect(this.CurrentFolder);
                    ImGui.CloseCurrentPopup();
                }
            }
        }
        else if (this.Type == FileDialogType.SaveFile)
        {
            string[] parents = GetMax5Parents(this.CurrentFolder);

            foreach (string parent in parents.Reverse())
            {
                string dirName = parent.Split(Path.DirectorySeparatorChar).Last();
                if (dirName == "")
                {
                    dirName = "/";
                }

                if (ImGui.Button(dirName))
                {
                    this.CurrentFolder = parent;
                }
                ImGui.SameLine();
            }

            ImGui.NewLine();

            ImGui.BeginChild("hej", new Vector2((ImGui.GetWindowContentRegionMax().X / 4) * 3 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            if (ImGui.MenuItem(".."))
            {
                this.CurrentFolder = Directory.GetParent(this.CurrentFolder).FullName;
            }

            string[] subDirs = Directory.GetDirectories(this.CurrentFolder);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorF.DarkGoldenRod.ToVector4());
            foreach (string subDir in subDirs)
            {
                DirectoryInfo di = new DirectoryInfo(subDir);
                if (ImGui.Selectable($"[dir] {di.Name}"))
                {
                    this.CurrentFolder = subDir;
                }
            }
            ImGui.PopStyleColor();

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("hej2", new Vector2(ImGui.GetWindowContentRegionMax().X / 4 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            ImGui.Text("Group 2");

            ImGui.EndChild();

            ImGui.InputText("Filename", ref this.currentSelectedFile, 100);

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            string filePath = Path.Combine(this.CurrentFolder, this.currentSelectedFile);

            if (ImGui.Button("Save"))
            {
                if (File.Exists(filePath))
                {
                    // Make some kind of error stuff
                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    // File is valid, return to editor
                    this.OnSelect(filePath);
                    ImGui.CloseCurrentPopup();
                }
            }
        }
    }
}