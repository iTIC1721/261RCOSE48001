using System.Collections.Generic;
using System.IO;

public static class CSVLoader
{
    public static List<WordState> Load(string path)
    {
        var list = new List<WordState>();
        var lines = File.ReadAllLines(path);

        foreach (var line in lines)
        {
            var parts = line.Split(',');

            list.Add(new WordState
            {
                word = parts[0],
                meaning = parts[1]
            });
        }

        return list;
    }
}
