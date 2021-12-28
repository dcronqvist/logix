using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LogiX.SaveSystem;

public class FileDialog
{
    private static readonly Vector2 DefaultFilePickerSize = new Vector2(600, 400);

    public string CurrentFolder { get; set; }
    public string SelectedFile { get; set; }
    public string[] FilteredExtensions { get; set; }

    public FileDialog(string startDirectory, params string[] filteredExtensions)
    {
        this.CurrentFolder = startDirectory;
        this.SelectedFile = null;
        this.FilteredExtensions = filteredExtensions;
    }

    public void Open()
    {
        ImGui.OpenPopup("###FileDialog");
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

    public bool Pick(ref string selectedFile, out bool selected)
    {
        if (ImGui.BeginPopupModal("###FileDialog"))
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

            ImGui.BeginChild("hej", new Vector2((ImGui.GetWindowContentRegionWidth() / 4) * 3 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            if (ImGui.MenuItem(".."))
            {
                this.CurrentFolder = Directory.GetParent(this.CurrentFolder).FullName;
            }

            string[] subDirs = Directory.GetDirectories(this.CurrentFolder);
            ImGui.PushStyleColor(ImGuiCol.Text, Color.YELLOW.ToVector4());
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
            foreach (string file in files.Where(file => this.FilteredExtensions.Contains(Path.GetExtension(file))))
            {
                FileInfo fi = new FileInfo(file);

                string fileText = $"[file] {fi.Name}";

                if (ImGui.Selectable(fileText, selectedFile == file))
                {
                    selectedFile = file;
                }

                float fileTextLength = ImGui.CalcTextSize(fileText).X;
                float startDistNext = 200f;
                float offset = startDistNext - fileTextLength;

                ImGui.SameLine();
                ImGui.Dummy(new Vector2(offset, 1));
                ImGui.SameLine();
                ImGui.Text(Util.BytesToString(fi.Length));
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("hej2", new Vector2(ImGui.GetWindowContentRegionWidth() / 4 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            ImGui.Text("Group 2");

            ImGui.EndChild();

            ImGui.InputText("Selected file", ref selectedFile, 100);

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                selected = false;
                return true;
            }

            ImGui.SameLine();

            if (ImGui.Button("Select"))
            {
                if (!ValidateFile(selectedFile, this.FilteredExtensions))
                {
                    // Make some kind of error stuff
                    selected = false;
                    return false;
                }
                else
                {
                    // File is valid, return to editor
                    ImGui.CloseCurrentPopup();
                    selected = true;
                    return true;
                }
            }

            ImGui.EndPopup();
        }

        selected = false;
        return false;
    }

    public bool ValidateFile(string filePath, IEnumerable<string> validFileExtensions)
    {
        bool fileExists = File.Exists(filePath);
        bool validExtension = validFileExtensions.Count() > 0 ? validFileExtensions.Contains(Path.GetExtension(filePath)) : true;

        return fileExists && validExtension;
    }

    public bool SelectFolder(ref string selectedFolder, out bool selected)
    {
        if (selectedFolder == "")
        {
            selectedFolder = this.CurrentFolder;
        }

        if (ImGui.BeginPopupModal("###FileDialog"))
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

            ImGui.BeginChild("hej", new Vector2((ImGui.GetWindowContentRegionWidth() / 4) * 3 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            if (ImGui.MenuItem(".."))
            {
                this.CurrentFolder = Directory.GetParent(this.CurrentFolder).FullName;
            }

            string[] subDirs = Directory.GetDirectories(this.CurrentFolder);
            ImGui.PushStyleColor(ImGuiCol.Text, Color.YELLOW.ToVector4());
            foreach (string subDir in subDirs)
            {
                DirectoryInfo di = new DirectoryInfo(subDir);
                if (ImGui.Selectable($"[dir] {di.Name}", selectedFolder == subDir))
                {
                    this.CurrentFolder = subDir;
                    selectedFolder = subDir;
                }
            }
            ImGui.PopStyleColor();

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("hej2", new Vector2(ImGui.GetWindowContentRegionWidth() / 4 - 5, ImGui.GetWindowHeight() * 0.7f), true);

            ImGui.Text("Group 2");

            ImGui.EndChild();

            ImGui.InputText("Selected folder", ref selectedFolder, 100);

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                selected = false;
                return true;
            }

            ImGui.SameLine();

            if (ImGui.Button("Select"))
            {
                if (!Directory.Exists(selectedFolder))
                {
                    // Make some kind of error stuff
                    selected = false;
                    return false;
                }
                else
                {
                    // File is valid, return to editor
                    ImGui.CloseCurrentPopup();
                    selected = true;
                    return true;
                }
            }

            ImGui.EndPopup();
        }


        selected = false;
        return false;
    }
}