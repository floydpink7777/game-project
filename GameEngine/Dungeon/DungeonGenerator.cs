using GameEngine.Dungeon;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public class DungeonGenerator
{
    private Random rnd = new();

    public void Generate(TileMap map)
    {
        FillWalls(map);

        var root = new Rectangle(0, 0, map.TileMapData.Width, map.TileMapData.Height);
        GenerateBSP(map, root, rnd.Next(2) == 0);

        if (map.TileMapData.Rooms.Count == 0)
            return;

        map.TileMapData.StartPos = GetRoomCenter(map.TileMapData.Rooms[0]);
        map.TileMapData.GoalPos = FindFarthestPoint(map, map.TileMapData.StartPos);

        map.TileMapData.Tiles[map.TileMapData.GoalPos.X, map.TileMapData.GoalPos.Y] = 51;
    }

    private void FillWalls(TileMap map)
    {
        for (int y = 0; y < map.TileMapData.Height; y++)
            for (int x = 0; x < map.TileMapData.Width; x++)
                map.TileMapData.Tiles[x, y] = 6;
    }

    private Rectangle CreateRoom(TileMap map, Rectangle rect)
    {
        if (rect.Width < map.TileMapData.MinRoomWidth || rect.Height < map.TileMapData.MinRoomHeight)
            return rect;

        int x1 = rnd.Next(rect.Width - map.TileMapData.MinRoomWidth + 1);
        int x2 = rnd.Next(rect.Width - map.TileMapData.MinRoomWidth + 1);
        int y1 = rnd.Next(rect.Height - map.TileMapData.MinRoomHeight + 1);
        int y2 = rnd.Next(rect.Height - map.TileMapData.MinRoomHeight + 1);

        int x = rect.X + Math.Min(x1, x2);
        int w = map.TileMapData.MinRoomWidth + Math.Abs(x1 - x2);

        int y = rect.Y + Math.Min(y1, y2);
        int h = map.TileMapData.MinRoomHeight + Math.Abs(y1 - y2);

        for (int iy = 0; iy < h; iy++)
            for (int ix = 0; ix < w; ix++)
                map.TileMapData.Tiles[x + ix, y + iy] = 0;

        var room = new Rectangle(x, y, w, h);
        map.TileMapData.Rooms.Add(room);

        return room;
    }

    private void ConnectRoom(TileMap map, Rectangle parent, Rectangle child, int divline, bool horizontal)
    {
        if (horizontal)
        {
            int x1 = parent.X + rnd.Next(Math.Max(1, parent.Width));
            int x2 = child.X + rnd.Next(Math.Max(1, child.Width));

            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);

            for (int x = minX; x <= maxX; x++)
                map.TileMapData.Tiles[x, divline] = 0;

            for (int i = 1; divline - i >= 0 && map.TileMapData.Tiles[x1, divline - i] == 6; i++)
                map.TileMapData.Tiles[x1, divline - i] = 0;

            for (int i = 1; divline + i < map.TileMapData.Height && map.TileMapData.Tiles[x2, divline + i] == 6; i++)
                map.TileMapData.Tiles[x2, divline + i] = 0;
        }
        else
        {
            int y1 = parent.Y + rnd.Next(Math.Max(1, parent.Height));
            int y2 = child.Y + rnd.Next(Math.Max(1, child.Height));

            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);

            for (int y = minY; y <= maxY; y++)
                map.TileMapData.Tiles[divline, y] = 0;

            for (int i = 1; divline - i >= 0 && map.TileMapData.Tiles[divline - i, y1] == 6; i++)
                map.TileMapData.Tiles[divline - i, y1] = 0;

            for (int i = 1; divline + i < map.TileMapData.Width && map.TileMapData.Tiles[divline + i, y2] == 6; i++)
                map.TileMapData.Tiles[divline + i, y2] = 0;
        }
    }

    private Rectangle GenerateBSP(TileMap map, Rectangle rect, bool horizontal)
    {
        if (rect.Width < map.TileMapData.MinRoomWidth * 2 || rect.Height < map.TileMapData.MinRoomHeight * 2)
            return CreateRoom(map, rect);

        if (horizontal)
        {
            int div = rect.Height / 2 + rnd.Next(-2, 3);
            div = Math.Clamp(div, map.TileMapData.MinRoomHeight, rect.Height - map.TileMapData.MinRoomHeight);

            var top = new Rectangle(rect.X, rect.Y, rect.Width, div);
            var bottom = new Rectangle(rect.X, rect.Y + div, rect.Width, rect.Height - div);

            var r1 = GenerateBSP(map, top, false);
            var r2 = GenerateBSP(map, bottom, false);

            ConnectRoom(map, r1, r2, rect.Y + div, true);
            return r1;
        }
        else
        {
            int div = rect.Width / 2 + rnd.Next(-2, 3);
            div = Math.Clamp(div, map.TileMapData.MinRoomWidth, rect.Width - map.TileMapData.MinRoomWidth);

            var left = new Rectangle(rect.X, rect.Y, div, rect.Height);
            var right = new Rectangle(rect.X + div, rect.Y, rect.Width - div, rect.Height);

            var r1 = GenerateBSP(map, left, true);
            var r2 = GenerateBSP(map, right, true);

            ConnectRoom(map, r1, r2, rect.X + div, false);
            return r1;
        }
    }

    private Point GetRoomCenter(Rectangle room)
    {
        return new Point(
            room.X + room.Width / 2,
            room.Y + room.Height / 2
        );
    }

    private Point FindFarthestPoint(TileMap map, Point start)
    {
        int w = map.TileMapData.Width;
        int h = map.TileMapData.Height;

        bool[,] visited = new bool[w, h];
        Queue<(Point p, int dist)> q = new();

        q.Enqueue((start, 0));
        visited[start.X, start.Y] = true;

        Point farthest = start;
        int maxDist = 0;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var (p, dist) = q.Dequeue();

            if (dist > maxDist)
            {
                maxDist = dist;
                farthest = p;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = p.X + dx[i];
                int ny = p.Y + dy[i];

                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (map.TileMapData.Tiles[nx, ny] != 0)
                    continue;

                visited[nx, ny] = true;
                q.Enqueue((new Point(nx, ny), dist + 1));
            }
        }

        return farthest;
    }
}
