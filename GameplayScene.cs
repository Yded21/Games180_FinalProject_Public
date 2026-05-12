using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Games180_FinalProject_EddyZhang;

public class GameplayScene : GameScene
{
    private readonly Random _random;
    private readonly UIManager _ui;
    private readonly List<RoomDefinition> _rooms;
    private readonly List<string> _selectedMapBases;
    private readonly List<Enemy> _enemies;
    private readonly List<Projectile> _projectiles;
    private readonly List<AttackEffect> _attackEffects;
    private readonly List<UpgradeDefinition> _currentChoices;
    private readonly List<PlayerForm> _formChoices;
    private readonly List<InteractableChoice> _worldChoices;

    private Player _player;
    private RoomMap _currentMap;
    private int _roomIndex;
    private GameplayMode _mode;
    private int _eventId;
    private int _seed;
    private bool _choicesSpawned;
    private float _trapCooldownTimer;
    private int _pauseSelection;
    private string _statusMessage;
    private float _statusMessageTimer;

    private string SaveFilePath
    {
        get { return Path.Combine(AppContext.BaseDirectory, "Save", "run_save.json"); }
    }

    public GameplayScene(Game1 game) : base(game)
    {
        _seed = Environment.TickCount;
        _random = new Random(_seed);
        _ui = new UIManager();
        _rooms = new List<RoomDefinition>();
        _selectedMapBases = new List<string>();
        _enemies = new List<Enemy>();
        _projectiles = new List<Projectile>();
        _attackEffects = new List<AttackEffect>();
        _currentChoices = new List<UpgradeDefinition>();
        _formChoices = new List<PlayerForm>();
        _worldChoices = new List<InteractableChoice>();
        BuildRoomSequence();
    }

    public override void Open()
    {
        _player = new Player(new Vector2(96, Game1.HudHeight + 240));
        _roomIndex = 0;
        SelectRoomMaps();
        LoadRoom(_roomIndex);
    }

    public override void Update(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (InputState.IsKeyPressed(Keys.Escape))
        {
            if (_mode == GameplayMode.Paused)
            {
                _mode = GameplayMode.Playing;
            }
            else
            {
                _mode = GameplayMode.Paused;
            }
        }

        if (InputState.IsKeyPressed(Keys.T))
        {
            SaveRun();
            ShowStatusMessage("Saved");
        }
        if (InputState.IsKeyPressed(Keys.Y))
        {
            if (LoadRun())
            {
                ShowStatusMessage("Loaded");
            }
            else
            {
                ShowStatusMessage("No save file");
            }
            return;
        }

        if (_statusMessageTimer > 0f)
        {
            _statusMessageTimer -= elapsed;
        }

        if (_mode == GameplayMode.Paused)
        {
            if (InputState.IsKeyPressed(Keys.A) || InputState.IsKeyPressed(Keys.Left))
            {
                _pauseSelection = 0;
            }
            if (InputState.IsKeyPressed(Keys.D) || InputState.IsKeyPressed(Keys.Right))
            {
                _pauseSelection = 1;
            }
            if (InputState.IsKeyPressed(Keys.E))
            {
                if (_pauseSelection == 0)
                {
                    SaveRun();
                    ShowStatusMessage("Saved");
                }
                else if (File.Exists(SaveFilePath))
                {
                    LoadRun();
                    ShowStatusMessage("Loaded");
                    return;
                }
            }
            return;
        }

        UpdatePlaying(gameTime);
    }

    public override void Draw()
    {
        _currentMap.Draw(SpriteBatch);

        for (int i = 0; i < _worldChoices.Count; i++)
        {
            _worldChoices[i].Draw(SpriteBatch, _worldChoices[i].IsPlayerNear(_player));
        }

        for (int i = 0; i < _projectiles.Count; i++)
        {
            _projectiles[i].Draw(SpriteBatch);
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            _enemies[i].Draw(SpriteBatch);
        }

        _player.Draw(SpriteBatch);

        for (int i = 0; i < _attackEffects.Count; i++)
        {
            _attackEffects[i].Draw(SpriteBatch);
        }

        _ui.DrawHud(SpriteBatch, _player, CurrentRoom, _roomIndex, _rooms.Count, GetAcquiredUpgradeDefinitions(), GetBoss());

        if (_mode == GameplayMode.Paused)
        {
            _ui.DrawPause(SpriteBatch, _player, CurrentRoom, GetAcquiredUpgradeDefinitions(), _pauseSelection, File.Exists(SaveFilePath));
        }

        DrawStatusMessage();
    }

    private RoomDefinition CurrentRoom
    {
        get { return _rooms[_roomIndex]; }
    }

    private void UpdatePlaying(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        RoomDefinition room = CurrentRoom;

        _player.Update(gameTime, InputState, _currentMap);

        if (_trapCooldownTimer > 0f)
        {
            _trapCooldownTimer -= elapsed;
        }
        if (!_player.IsDead && _enemies.Count > 0 && _trapCooldownTimer <= 0f && _currentMap.IsOnTrap(_player.CollisionBounds))
        {
            _player.TakeDamage(10);
            _trapCooldownTimer = 1f;
        }

        if (_player.AttackStartedThisFrame)
        {
            _attackEffects.Add(_player.CreateAttackEffect());
            if (_player.DistantStrike)
            {
                _projectiles.Add(_player.CreateDistantStrikeProjectile());
            }
        }

        if (_player.SkillStartedThisFrame)
        {
            if (_player.Form == PlayerForm.Divine)
            {
                _attackEffects.Add(_player.CreateDivineSkillSlash());
            }
            else if (_player.Form == PlayerForm.Demon && _player.StormOfBlood)
            {
                ClearEnemyProjectiles();
            }
        }

        UpdateEnemies(gameTime);
        UpdateProjectiles(elapsed);
        UpdateAttackEffects(elapsed);
        HandleCollisions();
        RemoveDeadObjects();
        if (UpdateWorldChoices())
        {
            return;
        }

        if (_player.IsDead)
        {
            ShowEndScene(false);
            return;
        }

        if (room.Type == RoomType.Start && !_choicesSpawned)
        {
            BeginStartChoice();
        }
        else if (room.Type == RoomType.Sanctuary)
        {
            if (InputState.IsKeyPressed(Keys.E))
            {
                _player.HealMissing(0.35f);
                AdvanceRoom();
            }
        }
        else if (room.Type == RoomType.FormChoice && !_choicesSpawned)
        {
            BeginFormChoice();
        }
        else if (room.Type == RoomType.Event && !_choicesSpawned)
        {
            BeginEvent();
        }
        else if (room.Type == RoomType.Encounter && AllEnemiesDead() && !_choicesSpawned)
        {
            BeginReward(room.EncounterNumber);
        }
        else if (room.Type == RoomType.Boss && AllEnemiesDead())
        {
            ShowEndScene(true);
        }
    }

    private void UpdateEnemies(GameTime gameTime)
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            enemy.Update(gameTime, _player, _currentMap);

            Projectile projectile = enemy.CreateProjectile(_player);
            if (projectile != null)
            {
                _projectiles.Add(projectile);
            }

            if (enemy.AttackStartedThisFrame)
            {
                Texture2D attackTexture = AssetManager.SlashEnemy;
                if (enemy is BossEnemy)
                {
                    attackTexture = ((BossEnemy)enemy).GetAttackTexture();
                }

                float effectDuration = 0.14f;
                if (enemy.AttackParryable)
                {
                    effectDuration = 0.21f;
                }

                AttackEffect effect = new AttackEffect(attackTexture, enemy.AttackBounds, enemy.FacingDirection, enemy.Damage, false, enemy.AttackParryable, effectDuration);
                _attackEffects.Add(effect);
            }
        }
    }

    private void UpdateProjectiles(float elapsed)
    {
        for (int i = 0; i < _projectiles.Count; i++)
        {
            _projectiles[i].Update(elapsed, _currentMap);
        }
    }

    private void UpdateAttackEffects(float elapsed)
    {
        for (int i = 0; i < _attackEffects.Count; i++)
        {
            _attackEffects[i].Update(elapsed, _currentMap);
        }
    }

    private void HandleCollisions()
    {
        for (int i = 0; i < _attackEffects.Count; i++)
        {
            AttackEffect effect = _attackEffects[i];
            if (effect.PlayerOwned)
            {
                HitEnemiesWithEffect(effect);
            }
            else
            {
                HitPlayerWithEffect(effect);
            }
        }

        for (int i = 0; i < _projectiles.Count; i++)
        {
            Projectile projectile = _projectiles[i];
            if (projectile.PlayerOwned)
            {
                HitEnemiesWithProjectile(projectile);
            }
            else
            {
                if (projectile.CollisionBounds.Intersects(_player.CollisionBounds))
                {
                    _player.TakeDamage(projectile.Damage);
                    Vector2 projKnockDir = projectile.Velocity;
                    if (projKnockDir != Vector2.Zero) projKnockDir.Normalize();
                    _player.ApplyKnockback(projKnockDir);
                    projectile.Removed = true;
                }
            }
        }
    }

    private void HitEnemiesWithEffect(AttackEffect effect)
    {
        if (effect.Removed || effect.DamageDealt)
        {
            return;
        }

        if (effect.HitAllOnce)
        {
            for (int j = 0; j < _enemies.Count; j++)
            {
                Enemy enemy = _enemies[j];
                if (!enemy.IsDead && effect.Bounds.Intersects(enemy.CollisionBounds))
                {
                    enemy.TakeDamage(effect.Damage);
                    if (enemy.IsDead)
                    {
                        _player.OnEnemyKilled();
                    }
                    DemonHealOnHit();
                    TryCreateDivineExplosion(effect);
                }
            }
            effect.DamageDealt = true;
            return;
        }

        for (int j = 0; j < _enemies.Count; j++)
        {
            Enemy enemy = _enemies[j];
            if (!enemy.IsDead && effect.Bounds.Intersects(enemy.CollisionBounds))
            {
                enemy.TakeDamage(effect.Damage);
                if (enemy.IsDead)
                {
                    _player.OnEnemyKilled();
                }
                DemonHealOnHit();
                TryCreateDivineExplosion(effect);
                if (effect.HitOnce)
                {
                    effect.DamageDealt = true;
                    return;
                }
            }
        }
    }

    private void HitPlayerWithEffect(AttackEffect effect)
    {
        if (effect.Removed || effect.DamageDealt)
        {
            return;
        }

        if (!effect.Bounds.Intersects(_player.CollisionBounds))
        {
            return;
        }

        if (_player.IsParryActive && effect.Parryable)
        {
            StunEnemyNear(effect.Bounds);
            _player.OnSuccessfulParry();
            if (_player.Form == PlayerForm.Hero)
            {
                _attackEffects.Add(_player.CreateMasterStrike());
            }
            effect.DamageDealt = true;
            return;
        }

        _player.TakeDamage(effect.Damage);
        Vector2 knockDir = _player.Center - new Vector2(effect.Bounds.Center.X, effect.Bounds.Center.Y);
        if (knockDir != Vector2.Zero) knockDir.Normalize();
        _player.ApplyKnockback(knockDir);
        effect.DamageDealt = true;
    }

    private void TryCreateDivineExplosion(AttackEffect effect)
    {
        if (!effect.ExplodesOnHit || !_player.StarExplosion)
        {
            return;
        }

        int explosionSize = 120;
        Rectangle explosionBounds = new Rectangle(
            effect.Bounds.Center.X - explosionSize / 2,
            effect.Bounds.Center.Y - explosionSize / 2,
            explosionSize, explosionSize);
        AttackEffect explosion = new AttackEffect(
            AssetManager.PlayerSlashes[PlayerForm.Divine],
            explosionBounds, Direction.Down,
            _player.GetAttackDamage() / 2, true, false, 0.12f);
        explosion.HitAllOnce = true;
        _attackEffects.Add(explosion);
        effect.ExplodesOnHit = false;
    }

    private void HitEnemiesWithProjectile(Projectile projectile)
    {
        if (projectile.Removed)
        {
            return;
        }

        for (int j = 0; j < _enemies.Count; j++)
        {
            Enemy enemy = _enemies[j];
            if (!enemy.IsDead && projectile.CollisionBounds.Intersects(enemy.CollisionBounds))
            {
                enemy.TakeDamage(projectile.Damage);
                if (enemy.IsDead)
                {
                    _player.OnEnemyKilled();
                }
                DemonHealOnHit();

                if (projectile.Piercing && _player.StarExplosion)
                {
                    int explosionSize = 120;
                    Rectangle explosionBounds = new Rectangle(
                        (int)projectile.Position.X - explosionSize / 2,
                        (int)projectile.Position.Y - explosionSize / 2,
                        explosionSize, explosionSize);
                    AttackEffect explosion = new AttackEffect(
                        AssetManager.PlayerSlashes[PlayerForm.Divine],
                        explosionBounds, Direction.Down,
                        _player.GetAttackDamage() / 2, true, false, 0.12f);
                    explosion.HitAllOnce = true;
                    _attackEffects.Add(explosion);
                    projectile.Removed = true;
                    return;
                }

                if (!projectile.Piercing)
                {
                    projectile.Removed = true;
                    return;
                }
            }
        }
    }

    private void DemonHealOnHit()
    {
        if (_player.Form == PlayerForm.Demon && _player.IsBloodDaggersActive)
        {
            _player.CurrentHp += 10;
            if (_player.CurrentHp > _player.MaxHp)
            {
                _player.CurrentHp = _player.MaxHp;
            }
        }
    }

    private void StunEnemyNear(Rectangle attackBounds)
    {
        Rectangle expanded = new Rectangle(attackBounds.X - 6, attackBounds.Y - 6, attackBounds.Width + 12, attackBounds.Height + 12);
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (!_enemies[i].IsDead && expanded.Intersects(_enemies[i].CollisionBounds))
            {
                if (_enemies[i] is BossEnemy)
                {
                    _enemies[i].Stun(1.5f);
                }
                else
                {
                    _enemies[i].Stun(2f);
                }
                return;
            }
        }
    }

    private void RemoveDeadObjects()
    {
        for (int i = _attackEffects.Count - 1; i >= 0; i--)
        {
            if (_attackEffects[i].Removed)
            {
                _attackEffects.RemoveAt(i);
            }
        }

        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            if (_projectiles[i].Removed)
            {
                _projectiles.RemoveAt(i);
            }
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            if (_enemies[i].IsDead)
            {
                _enemies.RemoveAt(i);
            }
        }
    }

    private bool AllEnemiesDead()
    {
        return _enemies.Count == 0;
    }

    private void BeginStartChoice()
    {
        _worldChoices.Clear();
        InteractableChoice choice = new InteractableChoice(ChoiceKind.StarterForm, GetCenterChoicePosition(), AssetManager.UpgradeIcons[UpgradeCategory.Knight], "Knight Form", "+20% max HP and gain Parry. Parry - block melee attack and stun enemy, 2.0s CD.");
        choice.Form = PlayerForm.Knight;
        _worldChoices.Add(choice);
        _choicesSpawned = true;
    }

    private void BeginReward(int encounterNumber)
    {
        _currentChoices.Clear();
        _worldChoices.Clear();
        _player.EncounterKillCount = 0;

        UpgradeDefinition first = UpgradeCatalog.GetRandomGeneral(_random, _player.AcquiredUpgrades);
        _currentChoices.Add(first);

        if (encounterNumber <= 2 || _player.Form == PlayerForm.Knight)
        {
            UpgradeDefinition second = UpgradeCatalog.GetRandomGeneral(_random, _player.AcquiredUpgrades);
            while (second.Id == first.Id)
            {
                second = UpgradeCatalog.GetRandomGeneral(_random, _player.AcquiredUpgrades);
            }
            _currentChoices.Add(second);
        }
        else
        {
            _currentChoices.Add(UpgradeCatalog.GetRandomForm(_random, _player.Form, _player.AcquiredUpgrades));
        }

        for (int i = 0; i < _currentChoices.Count; i++)
        {
            UpgradeDefinition upgrade = _currentChoices[i];
            InteractableChoice choice = new InteractableChoice(ChoiceKind.Upgrade, GetChoicePosition(i, _currentChoices.Count), AssetManager.UpgradeIcons[upgrade.Category], upgrade.Name, upgrade.Description);
            choice.Upgrade = upgrade;
            _worldChoices.Add(choice);
        }

        _choicesSpawned = true;
    }

    private void BeginFormChoice()
    {
        _formChoices.Clear();
        _worldChoices.Clear();
        _formChoices.Add(PlayerForm.Knight);

        List<PlayerForm> pool = new List<PlayerForm>();
        pool.Add(PlayerForm.Hero);
        pool.Add(PlayerForm.Divine);
        pool.Add(PlayerForm.Demon);

        for (int i = 0; i < 2; i++)
        {
            int index = _random.Next(pool.Count);
            _formChoices.Add(pool[index]);
            pool.RemoveAt(index);
        }

        for (int i = 0; i < _formChoices.Count; i++)
        {
            PlayerForm form = _formChoices[i];
            InteractableChoice choice = new InteractableChoice(ChoiceKind.Form, GetChoicePosition(i, _formChoices.Count), GetFormIcon(form), GetFormTitle(form), GetFormSummary(form));
            choice.Form = form;
            _worldChoices.Add(choice);
        }

        _choicesSpawned = true;
    }

    private void BeginEvent()
    {
        _worldChoices.Clear();
        string mapBase = _selectedMapBases[_roomIndex];
        if (mapBase.Contains("fountain"))
        {
            _eventId = 0;
        }
        else if (mapBase.Contains("shrine"))
        {
            _eventId = 1;
        }
        else
        {
            _eventId = 2;
        }

        if (_eventId == 0)
        {
            AddEventChoice(0, AssetManager.UpgradeIcons[UpgradeCategory.Attack], "Drink", "+25 attack damage and -20 max HP");
            AddEventChoice(1, AssetManager.UpgradeIcons[UpgradeCategory.Defense], "Refuse", "No effect");
        }
        else if (_eventId == 1)
        {
            AddEventChoice(0, AssetManager.UpgradeIcons[UpgradeCategory.Defense], "Shield Blessing", "-10% damage taken");
            AddEventChoice(1, AssetManager.UpgradeIcons[UpgradeCategory.Speed], "Fury Blessing", "+15% attack speed");
        }
        else
        {
            AddEventChoice(0, AssetManager.UpgradeIcons[UpgradeCategory.Skill], "Accept", "-20% skill cooldown and -15 current HP");
            AddEventChoice(1, AssetManager.UpgradeIcons[UpgradeCategory.Defense], "Walk Away", "No effect");
        }

        _choicesSpawned = true;
    }

    private void ResolveEvent(int choice)
    {
        if (_eventId == 0 && choice == 0)
        {
            _player.BaseDamage += 25;
            _player.MaxHp -= 20;
            if (_player.MaxHp < 40)
            {
                _player.MaxHp = 40;
            }
            if (_player.CurrentHp > _player.MaxHp)
            {
                _player.CurrentHp = _player.MaxHp;
            }
        }
        else if (_eventId == 1)
        {
            if (choice == 0)
            {
                _player.ApplyUpgrade(UpgradeCatalog.ShieldBlessing);
            }
            else
            {
                _player.ApplyUpgrade(UpgradeCatalog.FuryBlessing);
            }
        }
        else if (_eventId == 2 && choice == 0)
        {
            _player.ApplyUpgrade(UpgradeCatalog.CursedPower);
            _player.CurrentHp -= 15;
            if (_player.CurrentHp < 1)
            {
                _player.CurrentHp = 1;
            }
        }
    }

    private void AdvanceRoom()
    {
        _roomIndex++;
        if (_roomIndex >= _rooms.Count)
        {
            ShowEndScene(true);
            return;
        }

        LoadRoom(_roomIndex);
    }

    private void LoadRoom(int roomIndex)
    {
        _enemies.Clear();
        _projectiles.Clear();
        _attackEffects.Clear();
        _worldChoices.Clear();
        _mode = GameplayMode.Playing;
        _choicesSpawned = false;

        string mapBase = _selectedMapBases[roomIndex];
        string groundPath = Path.Combine(AppContext.BaseDirectory, "Data", "Rooms", mapBase + "_ground.csv");
        string entityPath = Path.Combine(AppContext.BaseDirectory, "Data", "Rooms", mapBase + "_entities.csv");
        _currentMap = new RoomMap(groundPath, entityPath);
        _player.Position = _currentMap.TileToWorld(_currentMap.PlayerSpawn);

        for (int i = 0; i < _currentMap.EnemySpawns.Count; i++)
        {
            SpawnToken spawn = _currentMap.EnemySpawns[i];
            Vector2 position = _currentMap.TileToWorld(spawn.Tile);
            if (spawn.Token == "normal")
            {
                _enemies.Add(new NormalEnemy(position));
            }
            else if (spawn.Token == "elite")
            {
                _enemies.Add(new EliteEnemy(position));
            }
            else if (spawn.Token == "magic")
            {
                _enemies.Add(new MagicEnemy(position));
            }
            else if (spawn.Token == "tank")
            {
                _enemies.Add(new TankEnemy(position));
            }
            else if (spawn.Token == "boss")
            {
                _enemies.Add(new BossEnemy(position));
            }
        }
    }

    private bool UpdateWorldChoices()
    {
        if (_worldChoices.Count == 0 || !InputState.IsKeyPressed(Keys.E))
        {
            return false;
        }

        for (int i = 0; i < _worldChoices.Count; i++)
        {
            if (_worldChoices[i].IsPlayerNear(_player))
            {
                ApplyWorldChoice(_worldChoices[i]);
                return true;
            }
        }

        return false;
    }

    private void ApplyWorldChoice(InteractableChoice choice)
    {
        if (choice.Kind == ChoiceKind.StarterForm)
        {
            _player.MaxHp += 20;
            _player.CurrentHp += 20;
            _player.SetForm(PlayerForm.Knight);
        }
        else if (choice.Kind == ChoiceKind.Upgrade)
        {
            _player.ApplyUpgrade(choice.Upgrade);
        }
        else if (choice.Kind == ChoiceKind.Form)
        {
            ApplyFormChoice(choice.Form);
        }
        else if (choice.Kind == ChoiceKind.Event)
        {
            ResolveEvent(choice.EventChoice);
        }

        AdvanceRoom();
    }

    private void ApplyFormChoice(PlayerForm chosen)
    {
        if (chosen == PlayerForm.Knight)
        {
            _player.ApplyUpgrade(UpgradeCatalog.RoyalResolve);
            _player.MaxHp = (int)(_player.MaxHp * 1.1f);
            _player.CurrentHp += 12;
            if (_player.CurrentHp > _player.MaxHp)
            {
                _player.CurrentHp = _player.MaxHp;
            }
        }

        _player.SetForm(chosen);
    }

    private void AddEventChoice(int eventChoice, Texture2D icon, string title, string description)
    {
        InteractableChoice choice = new InteractableChoice(ChoiceKind.Event, GetChoicePosition(_worldChoices.Count, 2), icon, title, description);
        choice.EventChoice = eventChoice;
        _worldChoices.Add(choice);
    }

    private Vector2 GetChoicePosition(int index, int count)
    {
        float y = Game1.HudHeight + 215f;
        if (count == 3)
        {
            return new Vector2(270f + index * 210f, y);
        }

        return new Vector2(370f + index * 220f, y);
    }

    private Vector2 GetCenterChoicePosition()
    {
        return new Vector2(RoomMap.MapOriginX + RoomMap.Columns * RoomMap.TileSize / 2f, Game1.HudHeight + RoomMap.Rows * RoomMap.TileSize / 2f);
    }

    private Texture2D GetFormIcon(PlayerForm form)
    {
        if (form == PlayerForm.Knight)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Knight];
        }
        if (form == PlayerForm.Hero)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Hero];
        }
        if (form == PlayerForm.Divine)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Divine];
        }

        return AssetManager.UpgradeIcons[UpgradeCategory.Demon];
    }

    private string GetFormTitle(PlayerForm form)
    {
        if (form == PlayerForm.Knight)
        {
            return "Stay Knight";
        }

        return form.ToString();
    }

    private string GetFormSummary(PlayerForm form)
    {
        if (form == PlayerForm.Knight)
        {
            return "+10% max HP and -20% parry cooldown. Parry - block melee attack and stun enemy, 1.6s CD.";
        }
        if (form == PlayerForm.Hero)
        {
            return "-30% atk speed, +50% atk dmg. Skill: MasterStrike - counter damage = 200%, 7s CD";
        }
        if (form == PlayerForm.Divine)
        {
            return "+100% atk range. Skill: Stellar Blade - launch a piercing slash straight forward, 2.5s CD";
        }

        return "+40% atk speed, -35% atk range. Skill: Blood Daggers - cost 20% current HP, each hit heals 10 HP, lasts 5s, 15s CD";
    }

    private void ClearEnemyProjectiles()
    {
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            if (!_projectiles[i].PlayerOwned)
            {
                _projectiles.RemoveAt(i);
            }
        }
    }

    private BossEnemy GetBoss()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i] is BossEnemy)
            {
                return (BossEnemy)_enemies[i];
            }
        }

        return null;
    }

    private List<UpgradeDefinition> GetAcquiredUpgradeDefinitions()
    {
        List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();
        for (int i = 0; i < _player.AcquiredUpgrades.Count; i++)
        {
            UpgradeDefinition upgrade = UpgradeCatalog.Find(_player.AcquiredUpgrades[i]);
            if (upgrade != null)
            {
                upgrades.Add(upgrade);
            }
        }

        return upgrades;
    }

    // save / load system
    private void SaveRun()
    {
        RunSaveData data = new RunSaveData();
        data.CurrentHp = _player.CurrentHp;
        data.MaxHp = _player.MaxHp;
        data.BaseDamage = _player.BaseDamage;
        data.CurrentForm = _player.Form;
        data.HasForm = _player.HasForm;
        data.RoomIndex = _roomIndex;
        data.Seed = _seed;
        data.AcquiredUpgrades = new List<string>(_player.AcquiredUpgrades);
        data.RoomMapBases = new List<string>(_selectedMapBases);

        Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath));
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SaveFilePath, json);
    }

    private bool LoadRun()
    {
        if (!File.Exists(SaveFilePath))
        {
            return false;
        }

        string json = File.ReadAllText(SaveFilePath);
        RunSaveData data = JsonSerializer.Deserialize<RunSaveData>(json);
        if (data == null)
        {
            return false;
        }

        _seed = data.Seed;
        _selectedMapBases.Clear();
        if (data.RoomMapBases != null && data.RoomMapBases.Count == _rooms.Count)
        {
            for (int i = 0; i < data.RoomMapBases.Count; i++)
            {
                _selectedMapBases.Add(data.RoomMapBases[i]);
            }
        }
        else
        {
            SelectRoomMaps();
        }

        _roomIndex = data.RoomIndex;
        if (_roomIndex < 0)
        {
            _roomIndex = 0;
        }
        if (_roomIndex >= _rooms.Count)
        {
            _roomIndex = _rooms.Count - 1;
        }

        LoadRoom(_roomIndex);
        _player.MaxHp = data.MaxHp;
        _player.CurrentHp = data.CurrentHp;
        _player.BaseDamage = data.BaseDamage;
        _player.AcquiredUpgrades = new List<string>(data.AcquiredUpgrades);
        bool hasForm = data.HasForm;
        if (!hasForm && data.RoomIndex > 0)
        {
            hasForm = true;
        }
        _player.RestoreForm(data.CurrentForm, hasForm);
        return true;
    }

    private void ShowStatusMessage(string message)
    {
        _statusMessage = message;
        _statusMessageTimer = 1.25f;
    }

    private void DrawStatusMessage()
    {
        if (_statusMessageTimer <= 0f || string.IsNullOrEmpty(_statusMessage))
        {
            return;
        }

        Rectangle panel = new Rectangle(Game1.ScreenWidth / 2 - 90, Game1.HudHeight + 14, 180, 32);
        UiHelper.DrawPanel(SpriteBatch, panel, new Color(20, 20, 24) * 0.85f, new Color(220, 200, 95));
        UiHelper.DrawCenteredText(SpriteBatch, _statusMessage, panel, Color.White, 0.72f);
    }

    private void BuildRoomSequence()
    {
        _rooms.Add(new RoomDefinition("start", "Knight's Oath", RoomType.Start, "start_fixed", 0));
        _rooms.Add(new RoomDefinition("encounter1", "Encounter 1", RoomType.Encounter, new[] { "encounterA_1_open", "encounterA_2_sidewood", "encounterA_3_bar" }, 1));
        _rooms.Add(new RoomDefinition("encounter2", "Encounter 2", RoomType.Encounter, new[] { "encounterB_1_elitecenter", "encounterB_2_trapcross", "encounterB_3_twinwood" }, 2));
        _rooms.Add(new RoomDefinition("form", "Form Choice", RoomType.FormChoice, "form_fixed", 0));
        _rooms.Add(new RoomDefinition("encounter3", "Encounter 3", RoomType.Encounter, new[] { "encounterC_1_lanetraps", "encounterC_2_tankcenter", "encounterC_3_sidepockets" }, 3));
        _rooms.Add(new RoomDefinition("event", "Event Room", RoomType.Event, new[] { "event_fountain", "event_idol", "event_shrine" }, 0));
        _rooms.Add(new RoomDefinition("encounter4", "Encounter 4", RoomType.Encounter, new[] { "encounterD_1_mixed", "encounterD_2_trapring", "encounterD_3_doubletank" }, 4));
        _rooms.Add(new RoomDefinition("sanctuary", "Sanctuary", RoomType.Sanctuary, "rest_variant_1", 0));
        _rooms.Add(new RoomDefinition("boss", "Boss Room", RoomType.Boss, new[] { "boss_variant_1", "boss_variant_2" }, 0));
    }

    private void SelectRoomMaps()
    {
        _selectedMapBases.Clear();
        for (int i = 0; i < _rooms.Count; i++)
        {
            RoomDefinition room = _rooms[i];
            if (room.MapPool != null && room.MapPool.Length > 0)
            {
                _selectedMapBases.Add(room.MapPool[_random.Next(room.MapPool.Length)]);
            }
            else
            {
                _selectedMapBases.Add(room.MapBase);
            }
        }
    }
}
