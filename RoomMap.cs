using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public class RoomMap
{
    public const int TileSize = 48;
    public const int Columns = 17;
    public const int Rows = 11;
    public const int MapOriginX = (Game1.ScreenWidth - Columns * TileSize) / 2;

    private readonly string[,] _tiles;

    public Point PlayerSpawn;
    public List<SpawnToken> EnemySpawns;
    public List<MapObjectToken> MapObjects;

    public RoomMap(string groundPath, string entityPath)
    {
        _tiles = new string[Columns, Rows];
        EnemySpawns = new List<SpawnToken>();
        MapObjects = new List<MapObjectToken>();
        PlayerSpawn = new Point(1, 5);
        LoadGround(groundPath);
        LoadEntities(entityPath);
    }

    public Vector2 TileToWorld(Point tile)
    {
        return new Vector2(MapOriginX + tile.X * TileSize, Game1.HudHeight + tile.Y * TileSize);
    }

    public bool IsOnTrap(Rectangle bounds)
    {
        int left = (int)System.Math.Floor((bounds.Left - MapOriginX) / (float)TileSize);
        int right = (int)System.Math.Floor((bounds.Right - 1 - MapOriginX) / (float)TileSize);
        int top = (int)System.Math.Floor((bounds.Top - Game1.HudHeight) / (float)TileSize);
        int bottom = (int)System.Math.Floor((bounds.Bottom - 1 - Game1.HudHeight) / (float)TileSize);

        for (int y = top; y <= bottom; y++)
        {
            for (int x = left; x <= right; x++)
            {
                if (x >= 0 && x < Columns && y >= 0 && y < Rows && NormalizeToken(_tiles[x, y]) == "trap")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsRectangleWalkable(Rectangle bounds)
    {
        int left = (int)System.Math.Floor((bounds.Left - MapOriginX) / (float)TileSize);
        int right = (int)System.Math.Floor((bounds.Right - 1 - MapOriginX) / (float)TileSize);
        int top = (int)System.Math.Floor((bounds.Top - Game1.HudHeight) / (float)TileSize);
        int bottom = (int)System.Math.Floor((bounds.Bottom - 1 - Game1.HudHeight) / (float)TileSize);

        for (int y = top; y <= bottom; y++)
        {
            for (int x = left; x <= right; x++)
            {
                if (!IsTileWalkable(x, y))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                Rectangle rect = new Rectangle(MapOriginX + x * TileSize, Game1.HudHeight + y * TileSize, TileSize, TileSize);
                Texture2D texture = AssetManager.Floor;
                string token = NormalizeToken(_tiles[x, y]);
                spriteBatch.Draw(AssetManager.Pixel, rect, new Color(52, 58, 62));

                if (token == "wall" || token == "wall_stone")
                {
                    texture = AssetManager.WallStone;
                }
                else if (token == "wall_wood")
                {
                    texture = AssetManager.WallWood;
                }
                else if (token == "gate")
                {
                    texture = AssetManager.Gate;
                }
                else if (token == "trap")
                {
                    texture = AssetManager.Trap;
                }
                else if (token == "fountain")
                {
                    texture = AssetManager.Fountain;
                }
                else if (token == "shrine")
                {
                    texture = AssetManager.Shrine;
                }
                else if (token == "idol")
                {
                    texture = AssetManager.Idol;
                }

                spriteBatch.Draw(texture, rect, AssetManager.GetSourceBounds(texture), Color.White);
            }
        }

        DrawMapObjects(spriteBatch);
    }

    private bool IsTileWalkable(int column, int row)
    {
        if (column < 0 || column >= Columns || row < 0 || row >= Rows)
        {
            return false;
        }

        string token = NormalizeToken(_tiles[column, row]);
        if (token == "wall" || token == "wall_stone" || token == "wall_wood")
        {
            return false;
        }

        return true;
    }

    private void LoadGround(string path)
    {
        string[] lines = File.ReadAllLines(path);
        for (int y = 0; y < Rows; y++)
        {
            string[] tokens = lines[y].Split(',');
            for (int x = 0; x < Columns; x++)
            {
                _tiles[x, y] = NormalizeToken(tokens[x]);
            }
        }
    }

    private void LoadEntities(string path)
    {
        string[] lines = File.ReadAllLines(path);
        for (int y = 0; y < Rows; y++)
        {
            string[] tokens = lines[y].Split(',');
            for (int x = 0; x < Columns; x++)
            {
                RegisterEntity(NormalizeToken(tokens[x]), x, y);
            }
        }
    }

    private void RegisterEntity(string token, int column, int row)
    {
        Point tile = new Point(column, row);

        if (token == "player")
        {
            PlayerSpawn = tile;
        }
        else if (token == "enemy_normal")
        {
            EnemySpawns.Add(new SpawnToken("normal", tile));
        }
        else if (token == "enemy_elite")
        {
            EnemySpawns.Add(new SpawnToken("elite", tile));
        }
        else if (token == "enemy_magic")
        {
            EnemySpawns.Add(new SpawnToken("magic", tile));
        }
        else if (token == "enemy_tank")
        {
            EnemySpawns.Add(new SpawnToken("tank", tile));
        }
        else if (token == "boss")
        {
            EnemySpawns.Add(new SpawnToken(token, tile));
        }
        else if (IsMapObject(token))
        {
            MapObjects.Add(new MapObjectToken(token, tile));
        }
    }

    private bool IsMapObject(string token)
    {
        if (token == "start_oath")
        {
            return true;
        }
        if (token == "event_fountain" || token == "event_idol" || token == "event_shrine")
        {
            return true;
        }
        if (token == "rest_fountain" || token == "rest_shrine")
        {
            return true;
        }
        if (token == "form_left" || token == "form_middle" || token == "form_right")
        {
            return true;
        }

        return false;
    }

    private void DrawMapObjects(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < MapObjects.Count; i++)
        {
            DrawMapObject(spriteBatch, MapObjects[i]);
        }
    }

    private void DrawMapObject(SpriteBatch spriteBatch, MapObjectToken mapObject)
    {
        Texture2D texture = GetMapObjectTexture(mapObject.Token);
        if (texture == null)
        {
            return;
        }

        Rectangle destination = GetMapObjectDestination(mapObject.Tile, mapObject.Token);
        spriteBatch.Draw(texture, destination, AssetManager.GetSourceBounds(texture), Color.White);

        if (mapObject.Token == "start_oath")
        {
            Texture2D icon = AssetManager.UpgradeIcons[UpgradeCategory.Knight];
            Rectangle iconRect = new Rectangle(destination.Center.X - 18, destination.Center.Y - 22, 36, 36);
            spriteBatch.Draw(AssetManager.Pixel, new Rectangle(iconRect.X - 4, iconRect.Y - 4, iconRect.Width + 8, iconRect.Height + 8), Color.Black * 0.35f);
            spriteBatch.Draw(icon, iconRect, Color.White);
        }
    }

    private Texture2D GetMapObjectTexture(string token)
    {
        if (token == "start_oath")
        {
            return AssetManager.Shrine;
        }
        if (token == "event_fountain" || token == "rest_fountain")
        {
            return AssetManager.Fountain;
        }
        if (token == "event_idol")
        {
            return AssetManager.Idol;
        }
        if (token == "event_shrine" || token == "rest_shrine")
        {
            return AssetManager.Shrine;
        }
        if (token == "form_left")
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Hero];
        }
        if (token == "form_middle")
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Divine];
        }
        if (token == "form_right")
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Demon];
        }

        return null;
    }

    private Rectangle GetMapObjectDestination(Point tile, string token)
    {
        int centerX = MapOriginX + tile.X * TileSize + TileSize / 2;
        int centerY = Game1.HudHeight + tile.Y * TileSize + TileSize / 2;

        if (token == "form_left" || token == "form_middle" || token == "form_right")
        {
            return new Rectangle(centerX - 22, centerY - 22, 44, 44);
        }

        return new Rectangle(centerX - 36, centerY - 48, 72, 72);
    }

    private string NormalizeToken(string token)
    {
        return token.Trim().ToLower();
    }
}

public class SpawnToken
{
    public string Token;
    public Point Tile;

    public SpawnToken(string token, Point tile)
    {
        Token = token;
        Tile = tile;
    }
}

public class MapObjectToken
{
    public string Token;
    public Point Tile;

    public MapObjectToken(string token, Point tile)
    {
        Token = token;
        Tile = tile;
    }
}
