using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Runtime.Data;
using UnityEngine;

namespace Utils
{
    public static class FileLoader
    {
        private static readonly FieldInfo[] FieldInfos = typeof(Simfile).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static Simfile FileLoad(string src)
        {
            string filePath = $"{Application.dataPath}/TestFiles/{src}/{src}.ssc";

            if (!File.Exists(filePath))
                return null;

            string fileContent = File.ReadAllText(filePath);
            return CreateFile(fileContent);
        }

        private static Simfile CreateFile(string text)
        {
            Simfile simfile = new Simfile();
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            int index = 0;

            while (index < lines.Length)
            {
                string line = lines[index].Trim();
                
                if (line.StartsWith("//"))
                {
                    CreateNoteData(simfile, lines, index);
                    break;
                }

                if (line.Length > 1)
                {
                    var subString = line[1..^1].Split(':');
                    if (subString.Length >= 2)
                    {
                        string component = subString[0].Trim();
                        string value = subString[1].Trim();

                        FieldInfo fieldInfo = Array.Find(FieldInfos, fi => 
                        {
                            string fieldName = fi.Name;
                            if (fieldName.StartsWith("<") && fieldName.Contains(">"))
                            {
                                int startIndex = fieldName.IndexOf('<') + 1;
                                int endIndex = fieldName.IndexOf('>');
                                fieldName = fieldName.Substring(startIndex, endIndex - startIndex);
                            }
                            return string.Equals(fieldName, component, StringComparison.CurrentCultureIgnoreCase);
                        });

                        if (fieldInfo != null)
                        {
                            object convertedValue = Convert.ChangeType(value, fieldInfo.FieldType);
                            fieldInfo.SetValue(simfile, convertedValue);
                        }
                    }
                }

                index++;
            }

            return simfile;
        }

        private static void CreateNoteData(Simfile simfile, IReadOnlyList<string> lines, int noteDataStartIndex)
        {
            while (true)
            {
                if (noteDataStartIndex < 0 || noteDataStartIndex >= lines.Count) return;

                var noteData = simfile.NoteDatas;
                int index = noteDataStartIndex;

                Difficulty difficulty = Difficulty.None;

                while (index < lines.Count)
                {
                    var line = lines[index].Trim();
                    if (line.Equals("#NOTES:"))
                    {
                        index += 2;
                        break;
                    }

                    if (line.Length > 1)
                    {
                        var subString = line[1..^1].Split(':');
                        if (subString.Length >= 2)
                        {
                            string component = subString[0].Trim();
                            string value = subString[1].Trim();

                            switch (component)
                            {
                                case "DIFFICULTY":
                                    difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), value);
                                    break;
                                case "METER":
                                    simfile.Difficulty[difficulty] = int.Parse(value);
                                    break;
                            }
                        }
                    }

                    index++;
                }

                noteData[difficulty] = new NoteData();
                int measure = 0;

                while (index < lines.Count)
                {
                    var line = lines[index].Trim();
                    if (line == ";")
                    {
                        index++;
                        break;
                    }

                    if (line.StartsWith(","))
                    {
                        measure++;
                    }
                    else
                    {
                        if (!noteData[difficulty].NoteInMeasure.TryGetValue(measure, out var noteList))
                        {
                            noteList = new List<string>();
                            noteData[difficulty].NoteInMeasure[measure] = noteList;
                        }

                        noteList.Add(line);
                    }

                    index++;
                }

                if (index < lines.Count && lines[index].StartsWith("//"))
                {
                    noteDataStartIndex = index;
                    continue;
                }

                break;
            }
        }
    }
}
