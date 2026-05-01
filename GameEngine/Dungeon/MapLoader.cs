using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

public static class MapLoader
{
    public static int[,] LoadCsv(string path)
    {
        var lines = File.ReadAllLines(path);

        int height = lines.Length;
        int width = lines[0].Split(',').Length;

        int[,] tiles = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            var cols = lines[y].Split(',');

            for (int x = 0; x < width; x++)
            {
                tiles[x, y] = int.Parse(cols[x]);
            }
        }

        return tiles;
    }
}
