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

    }
    


    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false)
    {
        base.OnVariantsRegister(manager, noPerPlayer);

        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Blink"], BlinkArrowAtlas["BlinkArrowStartWith"], BlinkArrowAtlas["BlinkArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Gomboc"], BlinkArrowAtlas["GombocArrowStartWith"], BlinkArrowAtlas["GombocArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Nyoom"], BlinkArrowAtlas["NyoomArrowStartWith"], BlinkArrowAtlas["NyoomArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Perimeter"], BlinkArrowAtlas["PerimeterArrowStartWith"], BlinkArrowAtlas["PerimeterArrowExclude"]);
        manager.AddArrowVariant(RiseCore.ArrowsRegistry["Seeker"], BlinkArrowAtlas["SeekerArrowStartWith"], BlinkArrowAtlas["SeekerArrowExclude"]);


        manager.AddVariant(new CustomVariantInfo("MatterDisplacement", BlinkArrowAtlas["BlinkArrowMatterDisplacement"], "BLINK ARROWS HAVE A LITTLE EXTRA KICK", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Monostasis", BlinkArrowAtlas["GombocArrowMonostasis"], "GOMBOC ARROWS MAINTAIN THEIR NATURAL EQUILIBRIUM", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("KineticFusion", BlinkArrowAtlas["NyoomArrowKinesis"], "NYOOM ARROWS CONTAIN VOLATILE NUCLEAR MATERIAL", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("Photosynthesis", BlinkArrowAtlas["PerimeterArrowPhotosynthesis"], "BRAMBLES CREATED BY PERIMETER ARROWS NO LONGER DECAY", CustomVariantFlags.CanRandom), true);
        manager.AddVariant(new CustomVariantInfo("IntrusiveThoughts", BlinkArrowAtlas["SeekerArrowIntrusiveThoughts"], "SEEKER ARROWS GAIN A DASH", CustomVariantFlags.CanRandom), true);

    }

    public override void Unload()
    {

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
    public override void Update()
    {

        base.Update();
        if (canDie)
        {
            RemoveSelf();
        }
    }

    protected override void HitWall(TowerFall.Platform platform)
    {
        if (!used)
        {
            this.used = true;
            var pos = Owner.Position;
            Owner.Position = Position - (Speed * 2);
            Position = pos;
            if (Level.Session.MatchSettings.Variants.GetCustomVariant("MatterDisplacement"))
            {
                Explosion.Spawn(base.Level, pos, PlayerIndex, true, false, false);
            }
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(sCurrentDirectory, @"Mods/BlinkArrows/Content/Audio/tzz.wav");
            string sFilePath = Path.GetFullPath(sFile);
            SoundPlayer zzt = new SoundPlayer(sFilePath);
            zzt.Play();


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
            Explosion.Spawn(base.Level, pos, PlayerIndex, true, false, false);
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
            Explosion.Spawn(base.Level, Position, PlayerIndex, true, false, false);
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