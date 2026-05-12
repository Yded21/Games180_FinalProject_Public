using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public static class AssetManager
{
    public static Texture2D Pixel;
    public static SpriteFont Font;

    public static Texture2D Floor;
    public static Texture2D WallStone;
    public static Texture2D WallWood;
    public static Texture2D Gate;
    public static Texture2D Trap;
    public static Texture2D Fountain;
    public static Texture2D Shrine;
    public static Texture2D Idol;
    public static Dictionary<Texture2D, Rectangle> SourceBounds;

    public static Texture2D BossIndicatorParryable;
    public static Texture2D BossIndicatorUnparryable;
    public static Texture2D BossStunned;
    public static Texture2D BossWalkSheet;
    public static Texture2D ChargeEnemy;
    public static Texture2D SmashEnemy;
    public static Texture2D SlashEnemy;
    public static Texture2D ProjectileEnemy;

    public static Dictionary<PlayerForm, Texture2D> PlayerWalkSheets;
    public static Dictionary<PlayerForm, Texture2D> PlayerSlashes;
    public static Dictionary<PlayerForm, Texture2D> PlayerProjectiles;
    public static Dictionary<string, Texture2D> EnemyWalkSheets;
    public static Dictionary<string, Texture2D> EnemyStunnedTextures;
    public static Dictionary<UpgradeCategory, Texture2D> UpgradeIcons;

    public static void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        Pixel = new Texture2D(graphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });

        Font = content.Load<SpriteFont>("UIFont");

        Floor = content.Load<Texture2D>("Floor");
        WallStone = content.Load<Texture2D>("Wall_Stone");
        WallWood = content.Load<Texture2D>("Wall_Wood");
        Gate = content.Load<Texture2D>("Gate");
        Trap = content.Load<Texture2D>("Trap");
        Fountain = content.Load<Texture2D>("Fountain");
        Shrine = content.Load<Texture2D>("Shrine");
        Idol = content.Load<Texture2D>("Idol");

        SourceBounds = new Dictionary<Texture2D, Rectangle>();
        RegisterSourceBounds(Floor);
        RegisterSourceBounds(WallStone);
        RegisterSourceBounds(WallWood);
        RegisterSourceBounds(Gate);
        RegisterSourceBounds(Trap);
        RegisterSourceBounds(Fountain);
        RegisterSourceBounds(Shrine);
        RegisterSourceBounds(Idol);

        BossIndicatorParryable = content.Load<Texture2D>("Boss_Indicator_Parryable");
        BossIndicatorUnparryable = content.Load<Texture2D>("Boss_Indicator_Unparryable");
        BossStunned = content.Load<Texture2D>("Boss_Stunned");
        BossWalkSheet = content.Load<Texture2D>("BossWalkSheet");
        ChargeEnemy = content.Load<Texture2D>("Charge_Enemy");
        SmashEnemy = content.Load<Texture2D>("Smash_Enemy");
        SlashEnemy = content.Load<Texture2D>("Slash_Enemy");
        ProjectileEnemy = content.Load<Texture2D>("Projectile_Enemy");

        PlayerWalkSheets = new Dictionary<PlayerForm, Texture2D>();
        PlayerWalkSheets[PlayerForm.Knight] = content.Load<Texture2D>("PlayerWalkSheet_Knight");
        PlayerWalkSheets[PlayerForm.Hero] = content.Load<Texture2D>("PlayerWalkSheet_Hero");
        PlayerWalkSheets[PlayerForm.Divine] = content.Load<Texture2D>("PlayerWalkSheet_Divine");
        PlayerWalkSheets[PlayerForm.Demon] = content.Load<Texture2D>("PlayerWalkSheet_Demon");

        PlayerSlashes = new Dictionary<PlayerForm, Texture2D>();
        PlayerSlashes[PlayerForm.Knight] = content.Load<Texture2D>("Slash_Player_Knight");
        PlayerSlashes[PlayerForm.Hero] = content.Load<Texture2D>("Slash_Player_Hero");
        PlayerSlashes[PlayerForm.Divine] = content.Load<Texture2D>("Slash_Player_Divine");
        PlayerSlashes[PlayerForm.Demon] = content.Load<Texture2D>("Slash_Player_Demon");

        PlayerProjectiles = new Dictionary<PlayerForm, Texture2D>();
        PlayerProjectiles[PlayerForm.Knight] = content.Load<Texture2D>("Projectile_Player_Knight");
        PlayerProjectiles[PlayerForm.Hero] = content.Load<Texture2D>("Projectile_Player_Hero");
        PlayerProjectiles[PlayerForm.Divine] = content.Load<Texture2D>("Projectile_Player_Divine");
        PlayerProjectiles[PlayerForm.Demon] = content.Load<Texture2D>("Projectile_Player_Demon");

        EnemyWalkSheets = new Dictionary<string, Texture2D>();
        EnemyWalkSheets["normal"] = content.Load<Texture2D>("EnemyWalkSheet_Normal");
        EnemyWalkSheets["elite"] = content.Load<Texture2D>("EnemyWalkSheet_Elite");
        EnemyWalkSheets["magic"] = content.Load<Texture2D>("EnemyWalkSheet_Magic");
        EnemyWalkSheets["tank"] = content.Load<Texture2D>("EnemyWalkSheet_Tank");

        EnemyStunnedTextures = new Dictionary<string, Texture2D>();
        EnemyStunnedTextures["normal"] = content.Load<Texture2D>("Enemy_Normal_Stunned");
        EnemyStunnedTextures["elite"] = content.Load<Texture2D>("Enemy_Elite_Stunned");
        EnemyStunnedTextures["magic"] = content.Load<Texture2D>("Enemy_Magic_Stunned");
        EnemyStunnedTextures["tank"] = content.Load<Texture2D>("Enemy_Tank_Stunned");

        UpgradeIcons = new Dictionary<UpgradeCategory, Texture2D>();
        UpgradeIcons[UpgradeCategory.Defense] = content.Load<Texture2D>("Upgrade_Defense");
        UpgradeIcons[UpgradeCategory.Attack] = content.Load<Texture2D>("Upgrade_Attack");
        UpgradeIcons[UpgradeCategory.Speed] = content.Load<Texture2D>("Upgrade_Speed");
        UpgradeIcons[UpgradeCategory.Skill] = content.Load<Texture2D>("Upgrade_Skill");
        UpgradeIcons[UpgradeCategory.Magic] = content.Load<Texture2D>("Upgrade_Magic");
        UpgradeIcons[UpgradeCategory.Knight] = content.Load<Texture2D>("Upgrade_KnightForm");
        UpgradeIcons[UpgradeCategory.Hero] = content.Load<Texture2D>("Upgrade_HeroForm");
        UpgradeIcons[UpgradeCategory.Divine] = content.Load<Texture2D>("Upgrade_DivineForm");
        UpgradeIcons[UpgradeCategory.Demon] = content.Load<Texture2D>("Upgrade_DemonForm");
    }

    public static Rectangle GetSourceBounds(Texture2D texture)
    {
        if (SourceBounds != null && SourceBounds.ContainsKey(texture))
        {
            return SourceBounds[texture];
        }

        return new Rectangle(0, 0, texture.Width, texture.Height);
    }

    private static void RegisterSourceBounds(Texture2D texture)
    {
        SourceBounds[texture] = FindOpaqueBounds(texture);
    }

    private static Rectangle FindOpaqueBounds(Texture2D texture)
    {
        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);

        int minX = texture.Width;
        int minY = texture.Height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < texture.Height; y++)
        {
            for (int x = 0; x < texture.Width; x++)
            {
                Color color = pixels[y * texture.Width + x];
                if (color.A > 10)
                {
                    if (x < minX)
                    {
                        minX = x;
                    }
                    if (y < minY)
                    {
                        minY = y;
                    }
                    if (x > maxX)
                    {
                        maxX = x;
                    }
                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }
            }
        }

        if (maxX < minX || maxY < minY)
        {
            return new Rectangle(0, 0, texture.Width, texture.Height);
        }

        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
