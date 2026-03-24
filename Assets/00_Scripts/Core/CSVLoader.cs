using System.Collections.Generic;
using System.IO;

public static class CSVLoader
{
    public static List<Card> Load(string path)
    {
        var list = new List<Card>();
        var lines = File.ReadAllLines(path);

        int id = 0;
        foreach (var line in lines)
        {
            var parts = line.Split(',');

            list.Add(new Card
            {
                id = id,
                front = parts[0],
                back = parts[1]
            });
        }

        return list;
    }
}
