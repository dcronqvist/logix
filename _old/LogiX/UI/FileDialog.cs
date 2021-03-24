using ImGuiNET;
using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace LogiX.UI
{
    enum FileDialogType
    {
        SelectFile,
        SaveFile,
        SelectMultipleFiles,
    }

    class FileDialog
    {
        public string StartDirectory { get; set; }
        public string CurrentDirectory { get; set; }
        public string[] CurrentDirectoryEntries { get; set; }
        public bool IsOpen { get; set; }
        public bool IsDone { get; set; }
        public string Title { get; set; }
        public List<string> SelectedFiles { get; set; }

        public static string[] defaultFileTypes = { ".*", Utility.EXT_IC, Utility.EXT_ICCOLLECTION, Utility.EXT_PROJ };
        string[] fileTypeOptions;
        int selectedFileType = 0;

        FileDialogType DialogType { get; set; }

        // Convenient size variables
        private static Vector2 windowSize = new Vector2(440, 240);
        private static float quickLinksFactor = 0.25f;
        private string saveFileName = "";

        public FileDialog(string start, string title, FileDialogType type) : this(start, title, type, defaultFileTypes)
        {

        }

        public FileDialog(string start, string title, FileDialogType type, string[] fileTypeOptions)
        {
            this.StartDirectory = start.Replace(@"\", @"/");
            this.IsOpen = false;
            this.IsDone = false;
            this.Title = title;
            this.SelectedFiles = new List<string>();
            this.fileTypeOptions = fileTypeOptions;
            this.DialogType = type;
            ChangeDirectory(StartDirectory);
        }

        public void SubmitQuickLinks()
        {
            if (ImGui.BeginChild("quicklinks", new Vector2(windowSize.X * quickLinksFactor, windowSize.Y), true))
            {
                ImGui.Text("Quick Access");
                ImGui.Separator();

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 1, 1, 1f));
                foreach (KeyValuePair<string, string> kvp in Utility.QUICKLINK_DIRS)
                {
                    if (ImGui.Selectable(kvp.Key))
                    {
                        ChangeDirectory(kvp.Value);
                    }
                }
                ImGui.PopStyleColor();

                ImGui.EndChild();
            }
        }

        public void Submit()
        {
            Vector2 windowSize = new Vector2(440, 240);
            float quickLinks = 0.25f;
            float entries = 1f - quickLinks;     

            if (ImGui.Begin(Title, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(CurrentDirectory);

                SubmitQuickLinks();

                ImGui.SameLine();

                if (ImGui.BeginChild("fs-entries", new Vector2(windowSize.X * entries, windowSize.Y), true))
                {
                    if (ImGui.Selectable(".."))
                    {
                        // .. directory to move backwards.
                        ChangeDirectory(BackOneDirectory(CurrentDirectory));
                    }

                    foreach (string entry in CurrentDirectoryEntries)
                    {
                        FileAttributes attr = File.GetAttributes(entry);

                        if (attr.HasFlag(FileAttributes.Directory)) { ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 1, 1, 1f)); }
                        else { ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1f)); }

                        if (PassFileTypeFilter(entry, fileTypeOptions[selectedFileType]))
                        {
                            if (ImGui.Selectable(Path.GetFileName(entry), SelectedFiles.Contains(entry)))
                            {
                                if (attr.HasFlag(FileAttributes.Directory))
                                {
                                    ChangeDirectory(entry);
                                }
                                else
                                {
                                    switch(DialogType)
                                    {
                                        case FileDialogType.SaveFile:
                                            // Do nothing
                                            break;

                                        case FileDialogType.SelectFile:
                                            if (!SelectedFiles.Contains(entry))
                                            {
                                                if (SelectedFiles.Count > 0)
                                                    SelectedFiles[0] = entry;
                                                else
                                                    SelectedFiles.Add(entry);
                                            }
                                            else
                                            {
                                                SelectedFiles.Clear();
                                            }
                                            break;

                                        case FileDialogType.SelectMultipleFiles:
                                            ToggleSelection(entry);
                                            break;
                                    }
                                }
                            }
                        }

                        ImGui.PopStyleColor();
                    }

                    ImGui.EndChild();

                    if(DialogType == FileDialogType.SaveFile)
                    {
                        ImGui.SetNextItemWidth(windowSize.X - 100);
                        ImGui.InputText("", ref saveFileName, 50);
                    }
                    else
                    {
                        if (ImGui.BeginChildFrame(2, new Vector2(windowSize.X - 100, 20), ImGuiWindowFlags.NoScrollbar))
                        {
                            ImGui.Text(GetSelectedFilesAsString(new Vector2(windowSize.X - 100, 20)));
                            ImGui.EndChildFrame();
                        }
                    }
                    

                    ImGui.SameLine();

                    ImGui.SetNextItemWidth(100);
                    ImGui.Combo("", ref selectedFileType, fileTypeOptions, fileTypeOptions.Length, 5);

                    if (ImGui.Button("Close"))
                    {
                        SelectedFiles.Clear();
                        IsDone = true;
                    }

                    ImGui.SameLine();

                    string submitButton = DialogType != FileDialogType.SaveFile ? "Select" : "Save";

                    if (ImGui.Button(submitButton))
                    {
                        IsDone = true;
                        if(DialogType == FileDialogType.SaveFile)
                        {
                            SelectedFiles.Add(CurrentDirectory + @$"/{saveFileName}{fileTypeOptions[0]}");
                        }
                    }
                }
                ImGui.End();
            }
        }

        private bool PassFileTypeFilter(string entry, string filter)
        {
            FileAttributes attr = File.GetAttributes(entry);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                return true;
            }
            else
            {
                if (filter == ".*")
                    return true;

                string ext = Path.GetExtension(entry);

                return ext == filter;
            }

        }

        private void ToggleSelection(string entry)
        {
            if (SelectedFiles.Contains(entry))
                SelectedFiles.Remove(entry);
            else
                SelectedFiles.Add(entry);
        }

        public string GetSelectedFilesAsString()
        {
            return GetSelectedFilesAsString(new Vector2(10000, 0));
        }

        public string GetSelectedFilesAsString(Vector2 sizeOfFrame)
        {
            string s = "";

            if (ImGui.CalcTextSize(s).X > sizeOfFrame.X - 20)
                return SelectedFiles.Count.ToString() + " selected files";

            foreach(string file in SelectedFiles)
            {
                s += "'" + Path.GetFileName(file) + "'";

                if(file != SelectedFiles[SelectedFiles.Count - 1])
                {
                    s += ", ";
                }
            }


            return s;
        }

        public string BackOneDirectory(string path)
        {
            DirectoryInfo di = Directory.GetParent(path);
            if (di != null)
                return di.FullName.Replace(@"\", @"/");
            else
                return path;
        }

        public void ChangeDirectory(string newDir)
        {
            this.CurrentDirectory = newDir.Replace(@"\", @"/");

            this.CurrentDirectoryEntries = Directory.GetFileSystemEntries(newDir);
        }

        public bool Done()
        {
            if (IsDone)
            {
                for (int i = 0; i < SelectedFiles.Count; i++)
                {
                    SelectedFiles[i] = SelectedFiles[i].Replace(@"\", "/");
                }


                IsDone = false;
                return true;
            }
            else
            {
                Submit();
                return false;
            }
        }
    }
}
