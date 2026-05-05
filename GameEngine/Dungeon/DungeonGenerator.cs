using GameEngine.Dungeon;
using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator
{
    private Random rnd = new();

    public void Generate(TileMapData tileMapData)
    {
        FillWalls(tileMapData);

        var root = new Rectangle(0, 0, tileMapData.Width, tileMapData.Height);
        GenerateBSP(tileMapData, root, rnd.Next(2) == 0);

        if (tileMapData.Rooms.Count == 0)
            return;

        // ★ ここで「本当に孤立している部屋だけ」繋ぐ
        EnsureAllRoomsConnected(tileMapData);

        // ★ ランダムな部屋をスタートにする
        var startRoom = tileMapData.Rooms[rnd.Next(tileMapData.Rooms.Count)];
        tileMapData.StartPos = GetRandomFloorInsideRoom(tileMapData, startRoom);

        // ★ スタートから遠い部屋トップ3を選ぶ
        var farRooms = tileMapData.Rooms
            .OrderByDescending(r => Distance(r.Center, tileMapData.StartPos))
            .Take(3)
            .ToList();

        // ★ その中からランダムにゴール部屋を選ぶ
        var goalRoom = farRooms[rnd.Next(farRooms.Count)];
        tileMapData.GoalPos = GetRandomFloorInsideRoom(tileMapData, goalRoom);

        // ゴールタイルを設定
        tileMapData.Tiles[tileMapData.GoalPos.X, tileMapData.GoalPos.Y] = 51;

        PlaceEnemies(tileMapData);
    }

    private int Distance(Point a, Point b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return dx * dx + dy * dy; // √は不要、比較なら二乗距離でOK
    }

    private void FillWalls(TileMapData tileMapData)
    {
        for (int y = 0; y < tileMapData.Height; y++)
            for (int x = 0; x < tileMapData.Width; x++)
                tileMapData.Tiles[x, y] = 6;
    }

    private Rectangle? CreateRoom(TileMapData map, Rectangle rect)
    {
        if (rect.Width < map.MinRoomWidth || rect.Height < map.MinRoomHeight)
            return null;

        int maxRoomWidth = rect.Width;
        int maxRoomHeight = rect.Height;

        int roomWidth = rnd.Next(map.MinRoomWidth, maxRoomWidth + 1);
        int roomHeight = rnd.Next(map.MinRoomHeight, maxRoomHeight + 1);

        int roomX = rect.X + rnd.Next(0, rect.Width - roomWidth + 1);
        int roomY = rect.Y + rnd.Next(0, rect.Height - roomHeight + 1);

        for (int y = 0; y < roomHeight; y++)
            for (int x = 0; x < roomWidth; x++)
                map.Tiles[roomX + x, roomY + y] = 0;

        var room = new Rectangle(roomX, roomY, roomWidth, roomHeight);
        map.Rooms.Add(room);

        return room;
    }

    private void ConnectRoom(TileMapData map, Rectangle r1, Rectangle r2, int divline, bool horizontal)
    {
        if (horizontal)
        {
            // 通路の中心 x を部屋の中心から決定
            int x1 = r1.X + r1.Width / 2;
            int x2 = r2.X + r2.Width / 2;
            int cx = (x1 + x2) / 2;

            // 安全クランプ
            if (cx < 1) cx = 1;
            if (cx >= map.Width - 2) cx = map.Width - 2;

            // 分割線上の2マス
            map.Tiles[cx, divline] = 0;
            map.Tiles[cx + 1, divline] = 0;

            // ★ 入口を必ず2マスに広げる（上側）
            if (divline - 1 >= 0)
            {
                map.Tiles[cx, divline - 1] = 0;
                map.Tiles[cx + 1, divline - 1] = 0;
            }

            // 上方向へ掘る
            for (int y = divline - 2; y >= 1; y--)
            {
                map.Tiles[cx, y] = 0;
                map.Tiles[cx + 1, y] = 0;

                // ★ 次の2マスが両方部屋なら終了
                if (map.Tiles[cx, y - 1] == 0 && map.Tiles[cx + 1, y - 1] == 0)
                    break;
            }

            // ★ 入口を必ず2マスに広げる（下側）
            if (divline + 1 < map.Height)
            {
                map.Tiles[cx, divline + 1] = 0;
                map.Tiles[cx + 1, divline + 1] = 0;
            }

            // 下方向へ掘る
            for (int y = divline + 2; y < map.Height - 1; y++)
            {
                map.Tiles[cx, y] = 0;
                map.Tiles[cx + 1, y] = 0;

                if (map.Tiles[cx, y + 1] == 0 && map.Tiles[cx + 1, y + 1] == 0)
                    break;
            }
        }
        else
        {
            // 通路の中心 y を部屋の中心から決定
            int y1 = r1.Y + r1.Height / 2;
            int y2 = r2.Y + r2.Height / 2;
            int cy = (y1 + y2) / 2;

            if (cy < 1) cy = 1;
            if (cy >= map.Height - 2) cy = map.Height - 2;

            // 分割線上の2マス
            map.Tiles[divline, cy] = 0;
            map.Tiles[divline, cy + 1] = 0;

            // ★ 入口を必ず2マスに広げる（左側）
            if (divline - 1 >= 0)
            {
                map.Tiles[divline - 1, cy] = 0;
                map.Tiles[divline - 1, cy + 1] = 0;
            }

            // 左方向へ掘る
            for (int x = divline - 2; x >= 1; x--)
            {
                map.Tiles[x, cy] = 0;
                map.Tiles[x, cy + 1] = 0;

                if (map.Tiles[x - 1, cy] == 0 && map.Tiles[x - 1, cy + 1] == 0)
                    break;
            }

            // ★ 入口を必ず2マスに広げる（右側）
            if (divline + 1 < map.Width)
            {
                map.Tiles[divline + 1, cy] = 0;
                map.Tiles[divline + 1, cy + 1] = 0;
            }

            // 右方向へ掘る
            for (int x = divline + 2; x < map.Width - 1; x++)
            {
                map.Tiles[x, cy] = 0;
                map.Tiles[x, cy + 1] = 0;

                if (map.Tiles[x + 1, cy] == 0 && map.Tiles[x + 1, cy + 1] == 0)
                    break;
            }
        }
    }

    private Rectangle? GenerateBSP(TileMapData map, Rectangle rect, bool horizontal)
    {
        if (rect.Width < map.MinRoomWidth * 2 || rect.Height < map.MinRoomHeight * 2)
            return CreateRoom(map, rect);

        if (horizontal)
        {
            int minDiv = map.MinRoomHeight;
            int maxDiv = rect.Height - map.MinRoomHeight;

            if (minDiv >= maxDiv)
                return CreateRoom(map, rect);

            int div = rnd.Next(minDiv, maxDiv + 1);

            var top = new Rectangle(rect.X, rect.Y, rect.Width, div);
            var bottom = new Rectangle(rect.X, rect.Y + div, rect.Width, rect.Height - div);

            var room1 = GenerateBSP(map, top, false);
            var room2 = GenerateBSP(map, bottom, false);

            if (room1 != null && room2 != null)
            {
                var r1 = GetNearestRoomToDiv(room1.Value, top, bottom, true);
                var r2 = GetNearestRoomToDiv(room2.Value, top, bottom, true);

                ConnectRoom(map, r1, r2, rect.Y + div, true);
            }

            return room1 ?? room2;
        }
        else
        {
            int minDiv = map.MinRoomWidth;
            int maxDiv = rect.Width - map.MinRoomWidth;

            if (minDiv >= maxDiv)
                return CreateRoom(map, rect);

            int div = rnd.Next(minDiv, maxDiv + 1);

            var left = new Rectangle(rect.X, rect.Y, div, rect.Height);
            var right = new Rectangle(rect.X + div, rect.Y, rect.Width - div, rect.Height);

            var room1 = GenerateBSP(map, left, true);
            var room2 = GenerateBSP(map, right, true);

            if (room1 != null && room2 != null)
            {
                var r1 = GetNearestRoomToDiv(room1.Value, left, right, false);
                var r2 = GetNearestRoomToDiv(room2.Value, left, right, false);

                ConnectRoom(map, r1, r2, rect.X + div, false);
            }

            return room1 ?? room2;
        }
    }

    private Rectangle GetNearestRoomToDiv(Rectangle room, Rectangle a, Rectangle b, bool horizontal)
    {
        if (horizontal)
        {
            if (room.X + room.Width >= a.X - 2 && room.X <= a.X + a.Width + 2)
                return room;

            int cx = room.X + room.Width / 2;
            return new Rectangle(cx, room.Y, 1, room.Height);
        }
        else
        {
            if (room.Y + room.Height >= a.Y - 2 && room.Y <= a.Y + a.Height + 2)
                return room;

            int cy = room.Y + room.Height / 2;
            return new Rectangle(room.X, cy, room.Width, 1);
        }
    }

    private Point GetRoomCenter(Rectangle room)
    {
        return new Point(
            room.X + room.Width / 2,
            room.Y + room.Height / 2
        );
    }

    private Point GetRandomFloorInsideRoom(TileMapData map, Rectangle room)
    {
        List<Point> floors = new();

        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                if (map.Tiles[x, y] == 0)
                    floors.Add(new Point(x, y));
            }
        }

        if (floors.Count == 0)
            return GetRoomCenter(room);

        return floors[rnd.Next(floors.Count)];
    }

    // =========================
    // ここから「無駄な通路ゼロ」用の接続処理
    // =========================

    private void EnsureAllRoomsConnected(TileMapData map)
    {
        int n = map.Rooms.Count;
        if (n <= 1) return;

        while (true)
        {
            bool[] connected = new bool[n];
            MarkReachableRoomsFrom(map, 0, connected);

            bool all = true;
            for (int i = 0; i < n; i++)
            {
                if (!connected[i])
                {
                    all = false;
                    break;
                }
            }

            if (all) break;

            int from = -1, to = -1;
            int bestDist = int.MaxValue;

            for (int i = 0; i < n; i++)
            {
                if (!connected[i]) continue;

                for (int j = 0; j < n; j++)
                {
                    if (connected[j]) continue;

                    int dx = map.Rooms[i].Center.X - map.Rooms[j].Center.X;
                    int dy = map.Rooms[i].Center.Y - map.Rooms[j].Center.Y;
                    int dist = dx * dx + dy * dy;

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        from = i;
                        to = j;
                    }
                }
            }

            if (from == -1 || to == -1) break;

            var a = GetRoomEdgePoint(map, map.Rooms[from]);
            var b = GetRoomEdgePoint(map, map.Rooms[to]);
            CarveCorridor(map, a, b);
        }
    }

    private void MarkReachableRoomsFrom(TileMapData map, int startRoomIndex, bool[] connected)
    {
        int w = map.Width;
        int h = map.Height;

        bool[,] visited = new bool[w, h];
        Queue<Point> q = new();

        var startRoom = map.Rooms[startRoomIndex];
        Point start = GetRandomFloorInsideRoom(map, startRoom);

        q.Enqueue(start);
        visited[start.X, start.Y] = true;

        connected[startRoomIndex] = true;

        while (q.Count > 0)
        {
            var p = q.Dequeue();

            for (int i = 0; i < map.Rooms.Count; i++)
            {
                if (connected[i]) continue;
                if (map.Rooms[i].Contains(p))
                    connected[i] = true;
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int k = 0; k < 4; k++)
            {
                int nx = p.X + dx[k];
                int ny = p.Y + dy[k];

                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                if (visited[nx, ny]) continue;
                if (map.Tiles[nx, ny] != 0) continue;

                visited[nx, ny] = true;
                q.Enqueue(new Point(nx, ny));
            }
        }
    }

    private void CarveCorridor(TileMapData map, Point a, Point b)
    {
        int w = map.Width;
        int h = map.Height;

        int x = a.X;
        int y = a.Y;

        // 横方向
        int stepX = a.X < b.X ? 1 : -1;
        while (x != b.X)
        {
            if (x >= 0 && x < w && y >= 0 && y < h)
                map.Tiles[x, y] = 0;

            // ★ 2マス幅（範囲チェック付き）
            if (y + 1 < h)
                map.Tiles[x, y + 1] = 0;

            x += stepX;
        }

        // 縦方向
        int stepY = a.Y < b.Y ? 1 : -1;
        while (y != b.Y)
        {
            if (x >= 0 && x < w && y >= 0 && y < h)
                map.Tiles[x, y] = 0;

            // ★ 2マス幅（範囲チェック付き）
            if (x + 1 < w)
                map.Tiles[x + 1, y] = 0;

            y += stepY;
        }
    }


    private Point GetRoomEdgePoint(TileMapData map, Rectangle room)
    {
        List<Point> edges = new();

        for (int x = room.X; x < room.X + room.Width; x++)
        {
            if (map.Tiles[x, room.Y] == 0) edges.Add(new Point(x, room.Y));
            if (map.Tiles[x, room.Y + room.Height - 1] == 0) edges.Add(new Point(x, room.Y + room.Height - 1));
        }
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            if (map.Tiles[room.X, y] == 0) edges.Add(new Point(room.X, y));
            if (map.Tiles[room.X + room.Width - 1, y] == 0) edges.Add(new Point(room.X + room.Width - 1, y));
        }

        if (edges.Count == 0)
            return GetRoomCenter(room);

        return edges[rnd.Next(edges.Count)];
    }

    private void PlaceEnemies(TileMapData map)
    {
        map.Enemies.Clear();

        // スタートとゴールの部屋は避ける
        Rectangle startRoom = FindRoomContaining(map, map.StartPos);
        Rectangle goalRoom = FindRoomContaining(map, map.GoalPos);

        foreach (var room in map.Rooms)
        {
            // スタート部屋とゴール部屋はスキップ
            if (room == startRoom || room == goalRoom)
                continue;

            // 部屋の広さに応じて敵数を決める
            int area = room.Width * room.Height;
            int enemyCount = area / 20; // 例：20タイルにつき1体

            for (int i = 0; i < enemyCount; i++)
            {
                var pos = GetRandomFloorInsideRoom(map, room);
                map.Enemies.Add(pos);
            }
        }

        // ★ ゴール部屋に強敵を1体置く（演出）
        var bossPos = GetRandomFloorInsideRoom(map, goalRoom);
        map.Enemies.Add(bossPos);
    }

    private Rectangle FindRoomContaining(TileMapData map, Point p)
    {
        foreach (var r in map.Rooms)
        {
            if (r.Contains(p))
                return r;
        }
        return Rectangle.Empty;
    }


}
