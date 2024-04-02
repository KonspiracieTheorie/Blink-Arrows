using FortRise;
using Monocle;
using MonoMod.ModInterop;
using BlinkMod;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using TowerFall;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Media;
using System.IO;
using System.Collections;
using static System.Windows.Forms.AxHost;
using static TowerFall.Arrow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Policy;
using MonoMod.Utils;
using MonoMod;
using MonoMod.RuntimeDetour;
using Platform = TowerFall.Platform;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

namespace BlinkMod;


[Fort("com.Konspiracie.BlinkArrows", "Blink Arrows")]
public class BlinkModModule : FortModule
{

    
    public static Atlas BlinkArrowAtlas;
    public static BlinkModModule Instance;
    public List<Variant> BlinkArrowVariantList = new List<Variant>();
    public static List<BlinkCustomArrowFormat> BlinkCustomArrowList = new List<BlinkCustomArrowFormat>();
    public static List<BlinkCustomPickupFormat> BlinkCustomPickupList = new List<BlinkCustomPickupFormat>();
    public BlinkModModule() 
    {
        Instance = this;
    }
    

    public override void LoadContent()
    {
        BlinkArrowAtlas = Content.LoadAtlas("Atlas/ArrowAtlas.xml", "Atlas/ArrowAtlas.png");
    }
    public override void Load()
    {
        LatencyArrow.Load();
        PlayerTweaks.Load();
    }
    


    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false)
    {
        base.OnVariantsRegister(manager, noPerPlayer);

        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Blink"], BlinkArrowAtlas["BlinkArrowStartWith"], BlinkArrowAtlas["BlinkArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Gomboc"], BlinkArrowAtlas["GombocArrowStartWith"], BlinkArrowAtlas["GombocArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Nyoom"], BlinkArrowAtlas["NyoomArrowStartWith"], BlinkArrowAtlas["NyoomArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Perimeter"], BlinkArrowAtlas["PerimeterArrowStartWith"], BlinkArrowAtlas["PerimeterArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Seeker"], BlinkArrowAtlas["SeekerArrowStartWith"], BlinkArrowAtlas["SeekerArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Sokoban"], BlinkArrowAtlas["SokobanArrowStartWith"], BlinkArrowAtlas["SokobanArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Latency"], BlinkArrowAtlas["LatencyArrowStartWith"], BlinkArrowAtlas["LatencyArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Decoy"], BlinkArrowAtlas["DecoyArrowStartWith"], BlinkArrowAtlas["DecoyArrowExclude"]);


        manager.AddVariant(new CustomVariantInfo("MatterDisplacement", BlinkArrowAtlas["BlinkArrowMatterDisplacement"], "BLINK ARROWS HAVE A LITTLE EXTRA KICK", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Monostasis", BlinkArrowAtlas["GombocArrowMonostasis"], "GOMBOC ARROWS MAINTAIN THEIR NATURAL EQUILIBRIUM", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("KineticFusion", BlinkArrowAtlas["NyoomArrowKinesis"], "NYOOM ARROWS CONTAIN VOLATILE NUCLEAR MATERIAL", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Photosynthesis", BlinkArrowAtlas["PerimeterArrowPhotosynthesis"], "BRAMBLES CREATED BY PERIMETER ARROWS NO LONGER DECAY", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("IntrusiveThoughts", BlinkArrowAtlas["SeekerArrowIntrusiveThoughts"], "SEEKER ARROWS GAIN A DASH", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Grudge", BlinkArrowAtlas["SokobanArrowGrudge"], "SOKOBAN ARROW BLOCKS KEEP CRUSHING", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Resynchronization", BlinkArrowAtlas["LatencyArrowResynchronization"], "LATENCY ARROWS READJUST THEIR AIM", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Cerberus", BlinkArrowAtlas["DecoyArrowCerberus"], "DECOY ARROWS TRIFURCATE MIDFLIGHT", CustomVariantFlags.CanRandom), true);

    }

    public override void Unload()
    {
        LatencyArrow.Unload();
        PlayerTweaks.Unload();
    }
}

public class BlinkCustomPickupFormat
{
    public Pickups BlinkCustomPickup;
    public string BlinkExclude;
    public string BlinkSpawnVariant;
    public BlinkCustomPickupFormat(Pickups pickup, string exclude, string spawnVariant = "SpawnInTowers")
    {
        BlinkCustomPickup = pickup;
        BlinkExclude = exclude;
        BlinkSpawnVariant = spawnVariant;
    }
}

public class BlinkCustomArrowFormat
{
    public ArrowTypes BlinkCustomArrow;
    public string BlinkStartWith;
    public string BlinkExclude;
    public string BlinkLevel;
    public int BlinkPickupID;
    public string BlinkSpawnVariant;
    public BlinkCustomArrowFormat(ArrowTypes arrow, string start, string exclude, string spawnLevel, int Pickup, string spawnVariant = "SpawnInTowers")
    {
        BlinkCustomArrow = arrow;
        BlinkStartWith = start;
        BlinkExclude = exclude;
        BlinkLevel = spawnLevel;
        BlinkPickupID = Pickup;
        BlinkSpawnVariant = spawnVariant;
    }
}


[CustomArrows("Blink", "CreateGraphicPickup")]
public class BlinkArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;
    bool blinked = false;

    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["BlinkArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["BlinkArrowHud"]);
        arrowInfo.Name = "Blink";
        return arrowInfo;
    }

    public BlinkArrow() : base()
    {
    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
        used = (canDie = false);
        StopFlashing();
    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["BlinkArrow"]);
        normalImage.Origin = new Vector2(13f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["BlinkArrowBuried"]);
        buriedImage.Origin = new Vector2(13f, 3f);
        Graphics = new Image[2] { normalImage, buriedImage };
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    protected override bool CheckForTargetCollisions()
    {
        if (Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
        {
            foreach (Entity entity in base.Level[GameTags.Target])
            {
                var levelEntity = (LevelEntity)entity;
                if (levelEntity.ArrowCheck(this) && levelEntity != this.CannotHit)
                {
                    if (!used)
                    {
                        this.used = true;
                        var pos = Owner.Position;
                        Owner.Position = Position + new Vector2(0, -4);
                        Position = pos;
                        if (Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
                        {
                            //Explosion.Spawn(base.Level, pos, PlayerIndex, true, false, false);
                        }
                        string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string sFile = System.IO.Path.Combine(sCurrentDirectory, @"Mods/BlinkArrows/Content/Audio/tzz.wav");
                        string sFilePath = Path.GetFullPath(sFile);
                        SoundPlayer tzz = new SoundPlayer(sFilePath);
                        tzz.Play();


                        canDie = true;
                    }
                }
            }
            return false;
        }
        else
        {
            return base.CheckForTargetCollisions();
        }
          
    }
    public override void Update()
    {

        if (State == ArrowStates.Shooting && Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
        {
            for (int i = 0; i < 8; i++)
            {
                base.Update();

            }
        }
        else
        {
            base.Update();
        }
        if (canDie)
        {
            RemoveSelf();
        }
    }
    public override void ShootUpdate()
    {
        if (!Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
        {
            base.ShootUpdate();
        }
    }

    protected override void HitWall(TowerFall.Platform platform)
    {
        if (!used)
        {
            this.used = true;
            var pos = Owner.Position;
            Owner.Position = Position;
            if (CollidedH)
            {
                if (Speed.X < 0)
                {
                    Owner.Position.X += 5;
                }
                else
                {
                    Owner.Position.X -= 5;
                }
            }
            else
            {
                if (Speed.Y > 0)
                {
                    Owner.Position.Y -= 7;
                }
                else
                {
                    Owner.Position.Y += 5;
                }
            }
            Position = pos;
            if (Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
            {
                //Explosion.Spawn(base.Level, pos, PlayerIndex, true, false, false);
            }
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(sCurrentDirectory, @"Mods/BlinkArrows/Content/Audio/tzz.wav");
            string sFilePath = Path.GetFullPath(sFile);
            SoundPlayer tzz = new SoundPlayer(sFilePath);
            tzz.Play();


            canDie = true;
        }
    }

    public override void HitLava()
    {
        this.used = true;
        var pos = Owner.Position;
        Owner.Position = Position + (Speed * 2);
        Position = pos;
        if (Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
        {
            //Explosion.Spawn(base.Level, pos, PlayerIndex, true, false, false);
        }
        string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string sFile = System.IO.Path.Combine(sCurrentDirectory, @"Mods/BlinkArrows/Content/Audio/tzz.wav");
        string sFilePath = Path.GetFullPath(sFile);
        SoundPlayer zzt = new SoundPlayer(sFilePath);
        zzt.Play();
        canDie = true;
    }

    public override void EnterFallMode(bool bounce = true, bool zeroX = false, bool sound = true)
    {
        if (bounce && Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
        {
            this.used = true;
            //Explosion.Spawn(base.Level, Position, PlayerIndex, true, false, false);
            Sounds.pu_superBombExplode.Play(base.X);
            canDie = true;

        }
        else
        {
            base.EnterFallMode(bounce, zeroX, sound);
        }
    }


}
public class Mirage : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Blink"].ID);
    }
}



[CustomArrows("Gomboc", "CreateGraphicPickup")]
public class GombocArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;

    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["GombocArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["GombocArrowHud"]);
        arrowInfo.Name = "Gomboc";
        return arrowInfo;
    }

    public GombocArrow() : base()
    {
    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
        used = (canDie = false);
        StopFlashing();
    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["GombocArrow"]);
        normalImage.Origin = new Vector2(13f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["GombocArrowBuried"]);
        buriedImage.Origin = new Vector2(13f, 3f);
        Graphics = new Image[2] { normalImage, buriedImage };
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    public override void Update()
    {

        base.Update();
        if (canDie)
        {
            RemoveSelf();
        }
    }


    private Vector2 lastpos = new Vector2(-9999, -9999);
    private int slideenabler = 20;
    protected override void HitWall(Platform platform)
    {
        if (!Level.Session.MatchSettings.Variants.GetCustomVariant("Monostasis"))
        {
            if (lastpos == Position && Speed.X == 0 && Speed.Y >= 0)
            {
                if (slideenabler <= 0)
                {
                    base.EnterFallModeBounceFrom(Position, false);
                }
                else
                {
                    slideenabler--;
                }
            }
            else
            {
                slideenabler = 20;
                lastpos = Position;
            }
        }
    }
}
public class Backfire : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Gomboc"].ID);
    }
}



[CustomArrows("Nyoom", "CreateGraphicPickup")]
public class NyoomArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;
    private Image unfiredImage;

    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["NyoomArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["NyoomArrowHud"]);
        arrowInfo.Name = "Nyoom";
        return arrowInfo;
    }

    public NyoomArrow() : base()
    {
    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
        Sounds.pu_brambleGrow.Play();
        used = (canDie = false);
        StopFlashing();
    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["NyoomArrow"]);
        normalImage.Origin = new Vector2(13f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["NyoomArrowBuried"]);
        buriedImage.Origin = new Vector2(13f, 3f);
        unfiredImage = new Image(BlinkModModule.BlinkArrowAtlas["NyoomArrowUnfired"]);
        unfiredImage.Origin = new Vector2(13f, 3f);
        Graphics = new Image[3] { normalImage, buriedImage, unfiredImage };
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
        unfiredImage.Visible = false;
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
        unfiredImage.Visible = false;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
        unfiredImage.Visible = false;
    }

    private void SwapToFallingGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = false;
        unfiredImage.Visible = true;
    }

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    
    public override void Update()
    {

        if (State == ArrowStates.Shooting)
        {
            for (int i = 0; i < 8; i++)
            {
                base.Update();
                
            }
            Add(new Coroutine(NyoomSpeedLoop.CreateNyoomSpeedLoop(Level, Position, normalImage.Rotation)));
        }
        else
        {
            base.Update();
        }
        if (canDie)
        {
            RemoveSelf();
        }
        if (Level.Session.MatchSettings.Variants.GetCustomVariant("KineticFusion") && State != ArrowStates.Shooting)
        {
            this.used = true;
            Explosion.Spawn(base.Level, Position, PlayerIndex, true, false, false);
            Sounds.pu_superBombExplode.Play(base.X);
            canDie = true;
        }
        else if (State == ArrowStates.Falling)
        {
            SwapToFallingGraphics();
        }

    }

    protected override void HitWall(Platform platform)
    {
    SwapToBuriedGraphics();
    base.HitWall(platform);
    }

    public override void ShootUpdate()
    {
        UpdateSeeking();
    }
}
public class NyoomSpeedLoop : LevelEntity
{
    private Image image;
    private int timer = 8;
    public NyoomSpeedLoop(Vector2 position, float rotation) : base(position)
    {
        Position = position;
        image = new Image(BlinkModModule.BlinkArrowAtlas["NyoomArrowSpeedLoop"]);
        base.Collider = new Hitbox(1f, 1f, -4f, -4f);
        base.Collidable = false;
        image.CenterOrigin();
        image.Rotation = (float)(rotation + (Math.PI / 2));
        image.Scale = new Vector2(0.01f, 0.01f);
        Add(image);
    }

    public static IEnumerator CreateNyoomSpeedLoop(Level level, Vector2 at, float rotation)
    {
        NyoomSpeedLoop MyNyoomLoop = new NyoomSpeedLoop(at, rotation);
        level.Add(MyNyoomLoop);
        yield return 0.000000001f;
    }

    public override void Update()
    {
        base.Update();
        if (base.Level.OnInterval(1))
        {
            
            timer -= 1;
            if (timer <= -24)
            {
                RemoveSelf();
            }
            else if (timer <= -16)
            {
                image.Scale -= new Vector2(0.1f, 0.1f);
            }
            else if (timer <= -8)
            {
                image.Scale -= new Vector2(0.04f, 0.04f);
            }
            else if (timer <= -0)
            {
                image.Scale += new Vector2(0.04f, 0.04f);
            }
            else if (timer <= 8)
            {
                image.Scale += new Vector2(0.1f, 0.1f);
            }
        }
    }
}
public class KingsCourt : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Nyoom"].ID);
    }
}



[CustomArrows("Perimeter", "CreateGraphicPickup")]
public class PerimeterArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;
    private int timeAlive = 0;

    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["PerimeterArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["PerimeterArrowHud"]);
        arrowInfo.Name = "Perimeter";
        return arrowInfo;
    }

    public PerimeterArrow() : base()
    {
    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
        used = (canDie = false);
        StopFlashing();
        timeAlive = 0;
    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["PerimeterArrow"]);
        normalImage.Origin = new Vector2(18f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["PerimeterArrowBuried"]);
        buriedImage.Origin = new Vector2(18f, 3f);
        Graphics = new Image[2] { normalImage, buriedImage };
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    public override void Update()
    {

        base.Update();
        if (canDie)
        {
            RemoveSelf();
        }
        if (State == ArrowStates.Shooting || State == ArrowStates.Gravity)
        {
            if (timeAlive > 2 && base.Level.OnInterval(1))
            {
                Add(new Coroutine(PerimeterBramble.CreatePerimeterBramble(Level, Position, normalImage.Rotation, PlayerIndex)));
            }
            else
            {
                timeAlive += 1;
            }
        }
    }
    protected override void HitWall(Platform platform)
    {
        base.HitWall(platform);
        SwapToBuriedGraphics();
    }
}
public class PerimeterBramble : Actor
{
    private FlashingImage image;
    private int lifetime = 600;
    private Vector2 movementhelper = new Vector2(0, 0);
    public int OwnerIndex { get; private set; }
    public PerimeterBramble(Vector2 position, float rotation) : base(position)
    {
        Position = position;
        movementhelper = Position;
        Tag(GameTags.PlayerCollider, GameTags.LavaCollider, GameTags.ExplosionCollider, GameTags.ShockCollider);
        
        ScreenWrap = true;
        base.Collider = new WrapHitbox(6f, 6f, -6f, -6f);
        base.Collidable = true;
        base.Pushable = false;
        base.IgnoreJumpThrus = true;
        image = new FlashingImage(BlinkModModule.BlinkArrowAtlas["PerimeterArrowBramble"]);
        image.CenterOrigin();
        image.Rotation = (float)((new Random().NextDouble()) * Math.PI);
        
        Add(image);
    }

    public static IEnumerator CreatePerimeterBramble(Level level, Vector2 at, float rotation, int ownerIndex)
    {
        PerimeterBramble MyPerimBramble = new PerimeterBramble(at, rotation);
        MyPerimBramble.OwnerIndex = ownerIndex;
        int depthmod = new Random().Next(-1, 2);
        MyPerimBramble.Depth += depthmod;
        level.Add(MyPerimBramble);
        yield return 0.000000001f;


    }
    public static void GetBrambleColors(int ownerIndex, bool teamsMode, Allegiance teamColor, out Color colorA, out Color colorB)
    {
        if (teamsMode)
        {
            colorA = ArcherData.GetColorA(ownerIndex, teamColor);
            colorB = ArcherData.GetColorB(ownerIndex, teamColor);
        }
        else
        {
            colorA = ArcherData.GetColorA(ownerIndex);
            colorB = ArcherData.GetColorB(ownerIndex);
        }
    }
    public override void Added()
    {
        base.Added();
        GetBrambleColors(OwnerIndex, base.Level.Session.MatchSettings.TeamMode, base.Level.Session.MatchSettings.Teams[OwnerIndex], out var colorA, out var colorB);
        image.StartFlashing(4, colorA, colorB);
        if (base.Level.OnInterval(8))
        {
            Sounds.pu_brambleGrow.Play();
        }
        
            
    }
    public override void Update()
    {
        base.Update();
        if (base.Level.OnInterval(1))
        {
            if (!Level.Session.MatchSettings.Variants.GetCustomVariant("Photosynthesis"))
            {
                lifetime -= 1;
            }
            
            if (lifetime <= 0)
            {
                if (base.Level.OnInterval(8))
                {
                    Sounds.pu_brambleDisappear.Play();
                }
                RemoveSelf();
            }
        }
    }

    public override void OnPlayerCollide(Player player)
    {
        player.Hurt(DeathCause.Brambles, Position, OwnerIndex, Sounds.pu_brambleDisappear);
    }

    public override void OnExplode(Explosion explosion, Vector2 normal)
    {
        RemoveSelf();
    }

    public override void OnShock(ShockCircle shock)
    {
        RemoveSelf();
    }

    public override void DoWrapRender()
    {
        image.DrawOutline();
        base.DoWrapRender();
    }
}
public class Thornwood : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Perimeter"].ID);
    }
}



[CustomArrows("Seeker", "CreateGraphicPickup")]
public class SeekerArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;
    private Image angyImage;
    private Sprite<int> tentacles;
    private bool dash = false;
    private int dashTimer = 35;
    private Vector2 dashDirec = new Vector2(0, 0);

    protected override float SeekMinDistSq => 0f;
    protected override float SeekRadiusSq => 100000f;
    protected override float SeekMaxAngle => 2.7925268f;
    protected override float SeekTurnRate => (float)Math.PI / 25f;

    private Vector2 speedHelper = new Vector2(2f, 2f);
    protected override float StartSpeed => 2f;

    private bool firstDash = true;



    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["SeekerArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["SeekerArrowHud"]);
        arrowInfo.Name = "Seeker";
        return arrowInfo;
    }

    public SeekerArrow() : base()
    {

    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
        used = (canDie = false);
        StopFlashing();
        dash = false;
        dashTimer = 35;
        firstDash = true;

    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["SeekerArrow"]);
        normalImage.Origin = new Vector2(14f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["SeekerArrowBuried"]);
        buriedImage.Origin = new Vector2(14f, 3f);
        angyImage = new Image(BlinkModModule.BlinkArrowAtlas["SeekerArrowAngy"]);
        angyImage.Origin = new Vector2(14f, 3f);
        tentacles = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["SeekerArrowTentacles"], 5, 5);
        tentacles.Origin = new Vector2(14f, 3f);
        tentacles.Add(0, 0.1f, new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 });
        tentacles.Play(0, false);

        Graphics = new Image[4] { normalImage, buriedImage, angyImage, tentacles };
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
        angyImage.Visible = false;
        tentacles.Visible = true;
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
        angyImage.Visible = false;
        tentacles.Origin = new Vector2(11f, 3f);
        tentacles.Visible = true;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
        angyImage.Visible = false;
        tentacles.Origin = new Vector2(14f, 3f);
        tentacles.Visible = true;
    }

    private void SwapToAngyGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = false;
        angyImage.Visible = true;
        tentacles.Visible = false;
    }

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    public override void ShootUpdate()
    {
        if (!dash)
        {
            UpdateSeeking();
        }
    }
    public override void Update()
    {
        if (dash)
        {
            Speed = dashDirec * 3f;
            if (base.Level.OnInterval(4))
            {
                Add(new Coroutine(SeekerTrail.CreateSeekerTrail(Level, Position, normalImage.Rotation)));
            }
            if (State != ArrowStates.Shooting && State != ArrowStates.Buried)
            {
                dash = false;
                dashTimer = 300;
            }
        }

        base.Update();
        if (canDie)
        {
            RemoveSelf();
        }
        if (dashTimer > 0 && Level.Session.MatchSettings.Variants.GetCustomVariant("IntrusiveThoughts"))
        {
            dashTimer--;
        }
        if (FindSeekTarget() != null && State == ArrowStates.Shooting && !firstDash)
        {
            
            SwapToAngyGraphics();

            if (Level.Session.MatchSettings.Variants.GetCustomVariant("IntrusiveThoughts") && dash == false && dashTimer <= 0)
            {
                float num = (float)Math.PI;
                LevelEntity levelEntity = FindSeekTarget();
                Direction += MathHelper.Clamp(Calc.AngleDiff(Direction, WrapMath.WrapAngle(Position, levelEntity.Position + levelEntity.SeekOffset)), 0f - num, num);
                Speed = Calc.AngleToVector(Direction, StartSpeed);

                string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string sFile = System.IO.Path.Combine(sCurrentDirectory, @"Mods/BlinkArrows/Content/Audio/seekerdash.wav");
                string sFilePath = Path.GetFullPath(sFile);
                SoundPlayer seekerdash = new SoundPlayer(sFilePath);
                seekerdash.Play();

                dash = true;
                dashDirec = Speed;
            }
        }
        else
        {
            Speed.X /= 1.01f;
            Speed.Y /= 1.01f;
            if (State != ArrowStates.Buried)
            {
                SwapToUnburiedGraphics();
            }
            if (firstDash)
            {
                firstDash = false;
            }
        }
        if (State == ArrowStates.Buried)
        {
            SwapToBuriedGraphics();
        }
        if (State == ArrowStates.Gravity)
        {
            State = ArrowStates.Shooting;
        }
        

        
    }
    protected override void HitWall(Platform platform)
    {
        base.HitWall(platform);
        SwapToBuriedGraphics();
    }

    protected override void OnCollideH(Platform platform)
    {
        if (base.State != 0)
        {
            base.OnCollideH(platform);
            return;
        }

        if (Level.Session.MatchSettings.Variants.GetCustomVariant("IntrusiveThoughts") && dash)
        {
            dash = false;
            dashTimer = 300;
        }

        Speed.X *= -1f;
        base.Direction = Calc.Angle(Speed);
    }

    protected override void OnCollideV(Platform platform)
    {
        if (base.State != 0)
        {
            base.OnCollideV(platform);
            return;
        }

        if (Level.Session.MatchSettings.Variants.GetCustomVariant("IntrusiveThoughts") && dash)
        {
            dash = false;
            dashTimer = 300;
        }

        Speed.Y *= -1f;
        base.Direction = Calc.Angle(Speed);

    }
}
public class SeekerTrail : LevelEntity
{
    private Sprite<int> image;
    private int timer = 32;
    public SeekerTrail(Vector2 position, float rotation) : base(position)
    {
        Position = position;
        image = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["SeekerArrowTrail"], 14, 5);
        image.Add(0, 0.1f, new int[4] { 0, 1, 2, 3});
        base.Collider = new Hitbox(1f, 1f, -4f, -4f);
        base.Collidable = false;
        image.CenterOrigin();
        image.Rotation = rotation;
        image.CurrentFrame = 0;
        Add(image);
    }

    public static IEnumerator CreateSeekerTrail(Level level, Vector2 at, float rotation)
    {
        SeekerTrail MySeekerTrail = new SeekerTrail(at, rotation);
        level.Add(MySeekerTrail);
        yield return 0.000000001f;
    }

    public override void Update()
    {
        base.Update();
        if (base.Level.OnInterval(1))
        {

            timer -= 1;
            if (timer <= 0)
            {
                RemoveSelf();
            }
            else if (timer <= 8)
            {
                image.CurrentFrame = 3;
            }
            else if (timer <= 16)
            {
                image.CurrentFrame = 2;
            }
            else if (timer <= 24)
            {
                image.CurrentFrame = 1;
                
            }
        }
    }
}
public class TwilightSpire : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Seeker"].ID);
    }
}



[CustomArrows("Sokoban", "CreateGraphicPickup")]
public class SokobanArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;
    private bool firstStep = true;



    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["SokobanArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["SokobanArrowHud"]);
        arrowInfo.Name = "Sokoban";
        return arrowInfo;
    }

    public SokobanArrow() : base()
    {

    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
        used = (canDie = false);
        StopFlashing();
        firstStep = true;

    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["SokobanArrow"]);
        normalImage.Origin = new Vector2(14f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["SokobanArrowBuried"]);
        buriedImage.Origin = new Vector2(14f, 3f);

        Graphics = new Image[2] { normalImage, buriedImage};
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    public override void Update()
    {
        base.Update();
        if (!firstStep && State != ArrowStates.Buried)
        {
            canDie = true;
        }
        if (firstStep)
        {
            Vector2 pos = Position;
            Position = new Vector2(0, 0);
            Add(new Coroutine(SokobanSlidingBlock.CreateSokoBlock(Level, pos + (Speed * 6), 0f, Speed)));
            firstStep = false;
        }
        
        if (canDie)
        {
            RemoveSelf();
        }
        



    }
    protected override void HitWall(Platform platform)
    {
        base.HitWall(platform);
    }

    public override void ShootUpdate()
    {

    }


}
public class SokobanSlidingBlock : Solid
{

    private Image image;
    private Image eyesimage;
    private Vector2 Speed;
    private bool crushing;
    private int postCrushLifetime;
    private int bouncesNoise = 3;
    private Vector2 lastpos = new Vector2(0, 0);
    private Vector2 pointing = new Vector2(0, 0);
    public SokobanSlidingBlock(Vector2 position, int width, int height, float rotation) : base(position, width, height)
    {
        Position = position;

        Tag(GameTags.Solid);


        ScreenWrap = true;

        base.Collider = new WrapHitbox(20f, 20f, -10f, -10f);
        base.Collidable = true;
        image = new Image(BlinkModModule.BlinkArrowAtlas["SokobanArrowBlock"]);
        image.CenterOrigin();
        eyesimage = new Image(BlinkModModule.BlinkArrowAtlas["SokobanArrowBlockEyes"]);
        eyesimage.CenterOrigin();


        Add(image);
        Add(eyesimage);
    }

    public static IEnumerator CreateSokoBlock(Level level, Vector2 at, float rotation, Vector2 speed)
    {
        SokobanSlidingBlock MySokoBlock = new SokobanSlidingBlock(at, 10, 10, rotation);
        MySokoBlock.Speed = speed / 1f;
        MySokoBlock.crushing = true;
        level.Add(MySokoBlock);
        yield return 0.000000001f;

        
    }

    public override void Added()
    {
        postCrushLifetime = 300;
        base.Added();

        if (Speed.X <= 1 && Speed.X >= -1)
        {
            if (Speed.Y < 1)
            {
                eyesimage.Position.Y += -4;
                Position.Y += 3;
                pointing = new Vector2(0, -1);
            }
            else
            {
                eyesimage.Position.Y += 4;
                Position.Y -= 10;
                pointing = new Vector2(0, 1);
            }
        }
        else if (Speed.Y <= 1 && Speed.Y >= -1)
        {
            if (Speed.X < 1)
            {
                eyesimage.Position.X += -4;
                Position.Y -= 1;
                Position.X += 10;
                pointing = new Vector2(1, 0);
            }
            else
            {
                eyesimage.Position.X += 4;
                Position.Y -= 1;
                Position.X -= 10;
                pointing = new Vector2(-1, 0);
            }
        }
        else
        {
            if (Speed.X > 1)
            {
                if (Speed.Y > 1)
                {
                    eyesimage.Position.X += 4;
                    eyesimage.Position.Y += 4;
                    pointing = new Vector2(1, -1);
                }
                else
                {
                    eyesimage.Position.X += 4;
                    eyesimage.Position.Y += -4;
                    pointing = new Vector2(1, 1);
                }
            }
            else
            {
                if (Speed.Y > 1)
                {
                    eyesimage.Position.X += -4;
                    eyesimage.Position.Y += 4;
                    pointing = new Vector2(-1, -1);
                }
                else
                {
                    eyesimage.Position.X += -4;
                    eyesimage.Position.Y += -4;
                    pointing = new Vector2(-1, 1);
                }
            }
        }

        


    }
    public override void Update()
    {
        base.Update();

        if (!crushing)
        {
            postCrushLifetime -= 1;
            if (postCrushLifetime < 0)
            {
                image.Scale /= 1.2f;
                base.Collider = new WrapHitbox(image.Scale.X * 20f, image.Scale.X * 20f, image.Scale.X * -10f, image.Scale.X * -10f);
                if (image.Scale.X <= 0.05)
                {
                    RemoveSelf();
                }
            }
        }
        if (crushing)
        {
            MoveTo(Position + Speed);
        }
        

        if (CollideCheck(GameTags.Solid) && crushing)
        {
            bool leftCollide = false;
            bool rightCollide = false;
            bool upCollide = false;
            bool downCollide = false;
            MoveTo(Position - Speed);

            Position.X += Math.Abs(Speed.X);
            if (CollideCheck(GameTags.Solid))
            {
                leftCollide = true;
            }
            Position.X -= Math.Abs(Speed.X);

            Position.Y -= Math.Abs(Speed.Y);
            if (CollideCheck(GameTags.Solid))
            {
                upCollide = true;
            }
            Position.Y += Math.Abs(Speed.Y);

            Position.X -= Math.Abs(Speed.X);
            if (CollideCheck(GameTags.Solid))
            {
                rightCollide = true;
            }
            Position.X += Math.Abs(Speed.X);

            Position.Y += Math.Abs(Speed.Y);
            if (CollideCheck(GameTags.Solid))
            {
                downCollide = true;
            }
            Position.Y -= Math.Abs(Speed.Y);
            
            //Entity leftCollide = new WrapHitbox(-10, -9, 1, 18);
            //Collider upCollide = new WrapHitbox(-9, -10, 18, 1);
            //Collider rightCollide = new WrapHitbox(9, -9, 1, 18);
            //Collider downCollide = new WrapHitbox(-9, 9, 18, 1);

            if (Level.Session.MatchSettings.Variants.GetCustomVariant("Grudge"))
            {
                if (leftCollide)
                {
                    Speed.X *= -1;
                    eyesimage.Position.X *= -1;
                }
                if (rightCollide)
                {
                    Speed.X *= -1;
                    eyesimage.Position.X *= -1;
                }
                if (upCollide)
                {
                    Speed.Y *= -1;
                    eyesimage.Position.Y *= -1;
                }
                if (downCollide)
                {
                    Speed.Y *= -1;
                    eyesimage.Position.Y *= -1;
                }
                if (leftCollide && rightCollide && upCollide && downCollide)
                {
                    crushing = false;
                }
                if (leftCollide || rightCollide || upCollide || downCollide)
                {
                    bouncesNoise -= 1;
                    if (bouncesNoise > 0)
                    {
                        base.Level.ScreenShake(12);
                    }
                       
                    Sounds.env_movingBlockEnd.Play(base.X);
                    MoveTo(Position - Speed);
                }
                if (crushing == false)
                {
                    eyesimage.Position = new Vector2(0, 0);
                }
            }
            else
            {
                if ((leftCollide || rightCollide) && (upCollide || downCollide))
                {
                    crushing = false;
                }
                if (leftCollide || rightCollide || upCollide || downCollide)
                {
                    if (leftCollide)
                    {
                        if (pointing.X == 0)
                        {
                            MoveTo(Position + new Vector2(3, 0));
                        }
                        if (pointing.Y == 0)
                        {
                            crushing = false;
                            MoveTo(Position + new Vector2(4, 0));
                        }
                        else
                        {
                            {
                                pointing.X = 0;
                                Speed.X = 0;
                                eyesimage.Position.X = 0;
                            }
                        }
                    }
                    if (rightCollide)
                    {
                        if (pointing.X == 0)
                        {
                            MoveTo(Position + new Vector2(-3, 0));
                        }
                        if (pointing.Y == 0)
                        {
                            crushing = false;
                            MoveTo(Position + new Vector2(-4, 0));
                        }
                        else
                        {
                            {
                                pointing.X = 0;
                                Speed.X = 0;
                                eyesimage.Position.X = 0;
                            }
                        }
                    }
                    if (upCollide)
                    {
                        if (pointing.Y == 0)
                        {
                            MoveTo(Position + new Vector2(0, 3));
                        }
                        if (pointing.X == 0)
                        {
                            crushing = false;
                            MoveTo(Position + new Vector2(0, -4));
                        }
                        else
                        {
                            {
                                pointing.Y = 0;
                                Speed.Y = 0;
                                eyesimage.Position.Y = 0;
                            }
                        }
                    }
                    if (downCollide)
                    {
                        if (pointing.Y == 0)
                        {
                            MoveTo(Position + new Vector2(0, -3));
                        }
                        if (pointing.X == 0)
                        {
                            crushing = false;
                            MoveTo(Position + new Vector2(0, 4));
                        }
                        else
                        {
                            {
                                pointing.Y = 0;
                                Speed.Y = 0;
                                eyesimage.Position.Y = 0;
                            }
                        }
                    }
                    base.Level.ScreenShake(12);
                    
                    Sounds.env_movingBlockEnd.Play(base.X);
                    MoveTo(Position - Speed);

                    
                }
                if (crushing == false)
                {
                    eyesimage.Position = new Vector2(0, 0);
                }
            }
           

        }



    }


}
public class Towerforge : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Sokoban"].ID);
    }
}



[CustomArrows("Latency", "CreateGraphicPickup")]
public class LatencyArrow : TriggerArrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private List<Vector2> posHist = new List<Vector2>();
    private List<float> directionHist = new List<float>();
    private List<Vector2> speedHist = new List<Vector2>();
    private bool seekUse = false;

    private Color fletchingCol = Color.White;

    private Image fletching;
    public int OwnerIndex { get; private set; }

    private static Action<TriggerArrow, LevelEntity, Vector2, float> BaseInit;
    private float turnMod = 90f;
    protected override float SeekTurnRate => (float)Math.PI / turnMod;

    private float seekMod = 4900f;
    protected override float SeekRadiusSq => seekMod;

    private float seekMaxMod = 0.6981317f;

    protected override float SeekMaxAngle => seekMaxMod;


    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["LatencyArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["LatencyArrowHud"]);
        arrowInfo.Name = "Latency";
        return arrowInfo;
    }

//note: the incessant beeping won't cease. override the fucking arrow base itself with bootleg trig arrow code.
//note: the incessant beeping can be ceased. override the get alarm (primed) in init.
    public LatencyArrow() : base()
    {
    }

    public static void GetOwnerColors(int ownerIndex, bool teamsMode, Allegiance teamColor, out Color colorA, out Color colorB)
    {
        if (teamsMode)
        {
            colorA = ArcherData.GetColorA(ownerIndex, teamColor);
            colorB = ArcherData.GetColorB(ownerIndex, teamColor);
        }
        else
        {
            colorA = ArcherData.GetColorA(ownerIndex);
            colorB = ArcherData.GetColorB(ownerIndex);
        }
    }
    protected override void CreateGraphics()
    {
        var self = DynamicData.For(this);
        var normalSprite = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["LatencyArrow"], 13, 4);
        normalSprite.Origin = new Vector2(12, 3);
        normalSprite.OnAnimationComplete = (s) => { };

        fletching = new FlashingImage(BlinkModModule.BlinkArrowAtlas["LatencyArrowFletching"]);
        fletching.Origin = new Vector2(12f, 3f);
        Color colorA = ArcherData.GetColorA(PlayerIndex);
        Color colorB = ArcherData.GetColorB(PlayerIndex);


        var buriedSprite = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["LatencyArrowBuried"], 13, 4);
        buriedSprite.Origin = new Vector2(12, 3);
        var eyeballSprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowEyeball");
        var pupilSprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowPupil");
        eyeballSprite.Visible = false;
        pupilSprite.Visible = false;

       

        this.Graphics = new Image[]
        {
            normalSprite,
            buriedSprite,
            eyeballSprite,
            pupilSprite,
            fletching
        };

        self.Set("normalSprite", normalSprite);
        self.Set("buriedSprite", buriedSprite);
        self.Set("eyeballSprite", eyeballSprite);
        self.Set("pupilSprite", pupilSprite);
        base.Add(this.Graphics);
    }

    protected override void InitGraphics()
    {
        base.InitGraphics();
        fletching.Color = ArcherData.GetColorA(base.CharacterIndex, ArcherData.ArcherTypes.Normal, base.TeamColor);
    }

    protected override void SwapToBuriedGraphics()
    {
        base.SwapToBuriedGraphics();
        fletching.Origin = new Vector2(8f, 3f);
    }

    protected override void SwapToUnburiedGraphics()
    {
        base.SwapToUnburiedGraphics();
        fletching.Origin = new Vector2(12f, 3f);
    }

    public static void Load()
    {
        BaseInit = CallHelper.CallBaseGen<Arrow, TriggerArrow, LevelEntity, Vector2, float>("Init", BindingFlags.NonPublic | BindingFlags.Instance);
        On.TowerFall.TriggerArrow.SetDetonator_Player += SetDetonatorPlayerPatch;
        On.TowerFall.TriggerArrow.SetDetonator_Enemy += SetDetonatorEnemyPatch;
        On.TowerFall.TriggerArrow.Detonate += DetonatePatch;
        On.TowerFall.TriggerArrow.Init += InitPatch;
        On.TowerFall.TriggerArrow.RemoveDetonator += RemoveDetonatorPatch;
    }

    public static void Unload()
    {
        On.TowerFall.TriggerArrow.SetDetonator_Player -= SetDetonatorPlayerPatch;
        On.TowerFall.TriggerArrow.SetDetonator_Enemy -= SetDetonatorEnemyPatch;
        On.TowerFall.TriggerArrow.Detonate -= DetonatePatch;
        On.TowerFall.TriggerArrow.Init -= InitPatch;
        On.TowerFall.TriggerArrow.RemoveDetonator -= RemoveDetonatorPatch;
    }

    private static void RemoveDetonatorPatch(On.TowerFall.TriggerArrow.orig_RemoveDetonator orig, TriggerArrow self)
    {
        if (self is LatencyArrow)
        {
            self.LightVisible = false;
            var dynData = DynamicData.For(self);
            Player player = dynData.Get<Player>("playerDetonator");
            dynData.Set("playerDetonator", null);
            if (player != null)
            {
                player.RemoveTriggerArrow(self);
            }
            dynData.Set("enemyDetonator", null);
            return;
        }
        orig(self);
    }

    [MonoModLinkTo("TowerFall.Arrow", "System.Void Init(TowerFall.LevelEntity,Microsoft.Xna.Framework.Vector2,System.Single)")]
    protected void base_Init(LevelEntity owner, Vector2 position, float direction)
    {
        base.Init(owner, position, direction);
    }

    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base_Init(owner, position, direction);
        posHist = new List<Vector2>();
        directionHist = new List<float>();
        speedHist = new List<Vector2>();
        seekUse = false;
        turnMod = 90f;
        seekMod = 4900f;
        LightVisible = true;
        Player playerDetonator = null;
        Enemy enemyDetonator = null;
        var dynData = DynamicData.For(this);
        //dynData.Get<Alarm>("primed").Start();
        dynData.Get<Alarm>("enemyDetonateCheck").Stop();
        dynData.Set("playerDetonator", playerDetonator);
        dynData.Set("enemyDetonator", enemyDetonator);
        if (owner is Enemy)
        {
            SetDetonator(owner as Enemy);
        }

        used = canDie = false;
        StopFlashing();
    }

    private static void InitPatch(On.TowerFall.TriggerArrow.orig_Init orig, TriggerArrow self, LevelEntity owner, Vector2 position, float direction)
    {
        if (self is LatencyArrow bramble)
        {

            BaseInit(self, owner, position, direction);
            Logger.Log("Working");
            self.LightVisible = true;
            Player playerDetonator = null;
            Enemy enemyDetonator = null;
            var dynData = DynamicData.For(self);
            dynData.Get<Alarm>("primed").Start();
            dynData.Get<Alarm>("enemyDetonateCheck").Stop();
            dynData.Set("playerDetonator", playerDetonator);
            dynData.Set("enemyDetonator", enemyDetonator);
            if (owner is Enemy)
            {
                bramble.SetDetonator(owner as Enemy);
            }

            bramble.used = bramble.canDie = false;
            bramble.StopFlashing();
            return;
        }
        orig(self, owner, position, direction);
    }



    private static void DetonatePatch(On.TowerFall.TriggerArrow.orig_Detonate orig, TriggerArrow self)
    {
        if (self is LatencyArrow brambleSelf)
        {
            DynamicData.For(self).Set("enemyDetonator", null);
            DynamicData.For(self).Set("playerDetonator", null);
            if (self.Scene != null && !self.MarkedForRemoval)
            {
                brambleSelf.UseBramblePower();
            }
            return;
        }
        orig(self);
    }

    private static void SetDetonatorEnemyPatch(On.TowerFall.TriggerArrow.orig_SetDetonator_Enemy orig, TriggerArrow self, Enemy enemy)
    {
        if (self is LatencyArrow)
        {
            DynamicData.For(self).Set("enemyDetonator", enemy);
            DynamicData.For(self).Get<Alarm>("enemyDetonateCheck").Start();
            return;
        }
        orig(self, enemy);
    }

    private static void SetDetonatorPlayerPatch(On.TowerFall.TriggerArrow.orig_SetDetonator_Player orig, TriggerArrow self, Player player)
    {
        if (self is LatencyArrow)
        {
            DynamicData.For(self).Set("playerDetonator", player);
            return;
        }
        orig(self, player);
    }


    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }

    public void UseBramblePower()
    {
        if (posHist.Count > 0)
        {
            Sounds.char_dodgeStallGrab.Play();
            Position = posHist[0];
            Direction = directionHist[0];
            Speed = speedHist[0];
            State = ArrowStates.Shooting;

            if (Level.Session.MatchSettings.Variants.GetCustomVariant("Resynchronization"))
            {
                seekUse = true;
               
            }
        }
    }


    public override void Update()
    {
        base.Update();
        if (canDie)
        {
            RemoveSelf();
        }

        if (State == ArrowStates.Shooting || State == ArrowStates.Gravity || State == ArrowStates.Falling)
        {
            

            posHist.Add(Position);
            directionHist.Add(Direction);
            speedHist.Add(Speed);

            if (posHist.Count > 30)
            {
                posHist.RemoveAt(0);
                directionHist.RemoveAt(0);
                speedHist.RemoveAt(0);
            }
        }
    }
    public override void ShootUpdate()
    {
        if (seekUse == true)
        {
            turnMod = 1f;
            seekMod = 50000f;
            seekMaxMod = (float)Math.PI;
        }
        this.UpdateSeeking(); 
        if (seekUse == true)
        {
            turnMod = 90f;
            seekMod = 4900f;
            seekMaxMod = 0.6981317f;
            seekUse = false;
        }
    }
}
public class SunkenCity : TowerPatch
{
    public override void VersusPatch(VersusTowerPatchContext Patcher)
    {
        Patcher.IncreaseTreasureRates(RiseCore.PickupRegistry["Latency"].ID);
    }
}



[CustomArrows("Decoy", "CreateGraphicPickup")]
public class DecoyArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private Image normalImage;
    private Image buriedImage;
    public Sprite<string> playerSprite;
    public Sprite<string> playerHeadSprite;
    public Sprite<string> playerBowSprite;
    public bool ignoreBow = false;


    private string logText;
    public bool yesItIsWorking = false;

    public static ArrowInfo CreateGraphicPickup()
    {
        var graphic = new Sprite<int>(BlinkModModule.BlinkArrowAtlas["DecoyArrowPickup"], 12, 12, 0);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, BlinkModModule.BlinkArrowAtlas["DecoyArrowHud"]);
        arrowInfo.Name = "Decoy";
        return arrowInfo;
    }

    public DecoyArrow() : base()
    {
    }
    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        
        base.Init(owner, position, direction);
        used = (canDie = false);
        StopFlashing();
    }
    protected override void CreateGraphics()
    {
        normalImage = new Image(BlinkModModule.BlinkArrowAtlas["DecoyArrow"]);
        normalImage.Origin = new Vector2(13f, 3f);
        buriedImage = new Image(BlinkModModule.BlinkArrowAtlas["DecoyArrowBuried"]);
        buriedImage.Origin = new Vector2(13f, 3f);
        Graphics = new Image[2] { normalImage, buriedImage };
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        
        normalImage.Color = Color.Lerp(ArcherData.GetColorA(base.CharacterIndex, ArcherData.ArcherTypes.Normal, base.TeamColor), Color.White, 0.4f);
        buriedImage.Color = Color.Lerp(ArcherData.GetColorA(base.CharacterIndex, ArcherData.ArcherTypes.Normal, base.TeamColor), Color.White, 0.4f);
        normalImage.Visible = true;
        buriedImage.Visible = false;

    }
    public static void GetBrambleColors(int ownerIndex, bool teamsMode, Allegiance teamColor, out Color colorA, out Color colorB)
    {
        if (teamsMode)
        {
            colorA = ArcherData.GetColorA(ownerIndex, teamColor);
            colorB = ArcherData.GetColorB(ownerIndex, teamColor);
        }
        else
        {
            colorA = ArcherData.GetColorA(ownerIndex);
            colorB = ArcherData.GetColorB(ownerIndex);
        }
    }

    protected override void SwapToBuriedGraphics()
    {
        normalImage.Visible = false;
        buriedImage.Visible = true;
    }

    protected override void SwapToUnburiedGraphics()
    {
        normalImage.Visible = true;
        buriedImage.Visible = false;
    }

    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }
    public override void Update()
    {

        base.Update();
        if (canDie)
        {
            RemoveSelf();
        }
    }

    protected override void HitWall(Platform platform)
    {
        if (CollidedH)
        {
            if (Speed.X < 0)
            {
                Add(new Coroutine(UnrealArcher.CreateUnrealArcher(Level, Position + new Vector2(7, 0), playerSprite, playerHeadSprite, playerBowSprite, PlayerIndex, ignoreBow)));
            }
            else
            {
                Add(new Coroutine(UnrealArcher.CreateUnrealArcher(Level, Position - new Vector2(7, 0), playerSprite, playerHeadSprite, playerBowSprite, PlayerIndex, ignoreBow)));
            }
        }
        else
        {
            if (Speed.Y > 0)
            {
                Add(new Coroutine(UnrealArcher.CreateUnrealArcher(Level, Position - new Vector2(0, 8), playerSprite, playerHeadSprite, playerBowSprite, PlayerIndex, ignoreBow)));
            }
            else
            {
                Add(new Coroutine(UnrealArcher.CreateUnrealArcher(Level, Position + new Vector2(0, 8), playerSprite, playerHeadSprite, playerBowSprite, PlayerIndex, ignoreBow)));
            }
        }
        base.HitWall(platform);
        
        


    }
}
public class UnrealArcher : Actor
{
    public Sprite<string> body;
    public Sprite<string> head;
    public Sprite<string> bow;
    public bool ignoreBow = false;
    private float ySpeed = 0;
    public int OwnerIndex { get; private set; }
    public UnrealArcher(Vector2 position) : base(position)
    {
        Position = position;
        Tag(GameTags.PlayerCollider, GameTags.LavaCollider, GameTags.ExplosionCollider, GameTags.ShockCollider);

        ScreenWrap = true;
        base.Collider = new WrapHitbox(8f, 16f, -4f, -8f);
        base.Collidable = true;
        base.Pushable = true;
        base.IgnoreJumpThrus = false;
    }

    public static IEnumerator CreateUnrealArcher(Level level, Vector2 at, Sprite<string> bodySprite, Sprite<string> headSprite, Sprite<string> bowSprite, int ownerIndex, bool ignoretehBow)
    {
        UnrealArcher MyUnrealArcher = new UnrealArcher(at);
        MyUnrealArcher.OwnerIndex = ownerIndex;
        MyUnrealArcher.body = bodySprite;
        MyUnrealArcher.head = headSprite;
        MyUnrealArcher.bow = bowSprite;
        if (ignoretehBow)
        {
            MyUnrealArcher.bow.Visible = false;
            MyUnrealArcher.ignoreBow = true;
        }

        //never happens. tweak code later, make sprites repos themselves.
        if (new Random().Next(0, 1) == 1)
        {
            MyUnrealArcher.body.FlipX = true;
            MyUnrealArcher.head.FlipX = true;
            MyUnrealArcher.bow.FlipX = true;
            MyUnrealArcher.body.Position.X *= -1;
            MyUnrealArcher.head.Position.X *= -1;
            MyUnrealArcher.bow.Position.X *= -1;
        }

        MyUnrealArcher.Add(bodySprite);
        MyUnrealArcher.Add(headSprite);
        MyUnrealArcher.Add(bowSprite);

        
        level.Add(MyUnrealArcher);
        yield return 0.000000001f;


    }
    public override void Added()
    {
        base.Added();


    }
    public override void Update()
    {
        base.Update();
        if (ySpeed < 5)
        {
            ySpeed += 0.3f;
        }
        MoveV(ySpeed * Engine.TimeMult, onCollideV);
    }

    private void onCollideV(Platform platform)
    {
        if (ySpeed > 0f)
        {

        }
    }
    public override void OnPlayerCollide(Player player)
    {
        player.Hurt(DeathCause.Brambles, Position, OwnerIndex, Sounds.pu_brambleDisappear);
    }

    public override void OnExplode(Explosion explosion, Vector2 normal)
    {
        RemoveSelf();
    }

    public override void OnShock(ShockCircle shock)
    {
        RemoveSelf();
    }

    public override void DoWrapRender()
    {
        body.DrawOutline();
        head.DrawOutline();
        if (!ignoreBow)
        {
            bow.DrawOutline();
        }
        

        base.DoWrapRender();
    }
}
class PlayerTweaks : Player
{
    public static IDetour Hook_MyPlayerRun;
    [EditorBrowsable(EditorBrowsableState.Never)]

    public PlayerTweaks(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, HatStates hatState, bool frozen, bool flash, bool indicator) : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator)
    {
    }
    public static void Load()
    {
        On.TowerFall.Player.ShootArrow += ShootArrowPatchUwU;
    }
    public static void Unload()
    {
        On.TowerFall.Player.ShootArrow -= ShootArrowPatchUwU;
    }
    public static void ShootArrowPatchUwU(On.TowerFall.Player.orig_ShootArrow orig, global::TowerFall.Player self)
    {
        
        if ((self.Arrows.HasArrows) && self.Arrows.Arrows[0] == RiseCore.ArrowsRegistry["Decoy"].Types)
        {
            self.Aiming = false;
            self.ArrowHUD.OnShoot();
            self.ArcherData.SFX.FireArrow.Play(self.X);
            DecoyArrow arrow = (DecoyArrow)DecoyArrow.Create(self.Arrows.UseArrow(), self, self.Position + ArrowOffset, FindLockAngle(self));
            Sprite<string> BodySpriteForArrow = TFGame.SpriteData.GetSpriteString(self.ArcherData.Sprites.Body);
            Sprite<string> HeadSpriteForArrow = TFGame.SpriteData.GetSpriteString(self.ArcherData.Sprites.HeadNormal);
            Sprite<string> bowSpriteForArrow = TFGame.SpriteData.GetSpriteString(self.ArcherData.Sprites.Bow);


            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(sCurrentDirectory, @"Mods/BlinkArrows/Content/log.txt");
            
            string XMLText = TFGame.SpriteData.GetXML(self.ArcherData.Sprites.Body).OuterXml;
            string XMLHead = TFGame.SpriteData.GetXML(self.ArcherData.Sprites.HeadNormal).OuterXml;
            string bodyPos = XMLText.Substring(XMLText.IndexOf("<OriginY>"), XMLText.IndexOf("</OriginY>") - XMLText.IndexOf("<OriginY>"));
            string headYPoss = XMLText.Substring(XMLText.IndexOf("<HeadYOrigins>"), XMLText.IndexOf("</HeadYOrigins>") - XMLText.IndexOf("<HeadYOrigins>"));
            string headHeight = XMLHead.Substring(XMLHead.IndexOf("<OriginY>"), XMLHead.IndexOf("</OriginY>") - XMLHead.IndexOf("<OriginY>"));
            string noBow = "";
            if (XMLText.Contains("<HideBowIdle>"))
            {
                noBow = XMLText.Substring(XMLText.IndexOf("<HideBowIdle>"), XMLText.IndexOf("</HideBowIdle>") - XMLText.IndexOf("<HideBowIdle>"));
            }
            

            headYPoss = headYPoss.Remove(0, 14);
            headHeight = headHeight.Remove(0, 9);
            bodyPos = bodyPos.Remove(0, 9);

            headYPoss = headYPoss.Substring(0, headYPoss.IndexOf(","));

            //BodySpriteForArrow.Position = new Vector2(HeadSpriteForArrow.Position.X, Int32.Parse(bodyPos));
            //HeadSpriteForArrow.Position = new Vector2(HeadSpriteForArrow.Position.X, Int32.Parse(headHeight));
            HeadSpriteForArrow.Position.Y -= Int32.Parse(headYPoss);

            if (XMLText.Contains("PlayerBody8"))
            {
                HeadSpriteForArrow.Position.Y += 7;
            }

            File.WriteAllText(sFile, "XMLTfffext");



            BodySpriteForArrow.Play("stand");
            HeadSpriteForArrow.Play("idle");
            bowSpriteForArrow.Play("idle");
            arrow.playerSprite = BodySpriteForArrow;
            arrow.playerHeadSprite = HeadSpriteForArrow;
            if (!noBow.Contains("True"))
            {
                arrow.playerBowSprite = bowSpriteForArrow;

            }
            else
            {
                arrow.playerBowSprite = bowSpriteForArrow;
                arrow.ignoreBow = true;
            }

            self.Level.Add(arrow);
           


            self.Level.Session.MatchStats[self.PlayerIndex].ArrowsShot++;
            SaveData.Instance.Stats.ArrowsShot++;
        }
        else
        {
            orig(self);
        }
        
    }
    private static float FindLockAngle(TowerFall.Player self)
    {
        LevelEntity levelEntity = null;
        float num = 0f;
        float result = self.AimDirection;
        foreach (LevelEntity item in self.Level[GameTags.Target])
        {
            if (item == self || item.SeekPriority <= 0 || !self.IsEnemy(item))
            {
                continue;
            }

            Vector2 vector = self.Position + ArrowOffset;
            float num2 = Vector2.DistanceSquared(vector, item.Position);
            if (levelEntity == null || item.SeekPriority > levelEntity.SeekPriority)
            {
                if (num2 > 1296f)
                {
                    continue;
                }
            }
            else if (num2 >= num)
            {
                continue;
            }

            float num3 = Calc.Angle(item.Position - vector);
            if (Math.Abs(Calc.AngleDiff(self.AimDirection, num3)) <= (float)Math.PI * 13f / 36f && !self.Level.CollideCheck(vector, item.Position, GameTags.Solid))
            {
                levelEntity = item;
                num = num2;
                result = num3;
            }
        }

        return result;
    }
}