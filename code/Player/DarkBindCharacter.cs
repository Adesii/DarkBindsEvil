using DarkBinds.Systems.Worlds;
using DarkBinds.UI;
using DarkBinds.UI.IngameHUD;
using DarkBinds.util;
using Sandbox;
using SpriteKit.Entities;
using SpriteKit.SceneObjects;
using SpriteKit.Player;

namespace DarkBinds.Player;

public partial class DarkBindCharacter : ModelSprite
{
	public float Speed => 125f;
	[Net, Predicted]
	public Vector2Int PlayerPosition { get; set; }

	[ClientInput]
	public Vector3 MousePosition { get; set; }

	[ClientInput]
	public Vector3 MouseDirection { get; set; }

	[Event.Client.BuildInput]
	public void BuildInput()
	{
		MousePosition = Camera.Position;
		MouseDirection = Screen.GetDirection( Mouse.Position );
	}

	public string CurrentBlock => DarkInventory.CurrentBlock;
	/// <summary>
	/// Provides an easy way to switch our current cameramode component
	/// </summary>
	[SkipHotload]
	public CameraMode CameraMode
	{
		get => Components.Get<CameraMode>( false );
		set => Components.Add( value );
	}
	[SkipHotload]
	public DarkInventory DarkInventory
	{
		get => Components.Get<DarkInventory>();
		set => Components.Add( value );
	}
	public override void Spawn()
	{
		//SpritePath = "/sprites/mainchar/darkbind.sprite";
		SpritePath = "sprites/mainchar/testing.sprite";
		CameraMode = new TopDownCamera();
		base.Spawn();
		Rotation = Rotation.LookAt( Vector3.Backward, Vector3.Up );
		Scale = 0.63f;

		//Components.Create<SoulPillar>();
		//Components.Create<SoulEater>();



		DarkInventory = new DarkInventory();


	}

	protected SceneLight FollowLight { get; set; }
	protected SceneLight FollowLight2 { get; set; }
	private SceneSpotLight FollowSun { get; set; }
	private SceneSpotLight FollowSun2 { get; set; }
	public override void ClientSpawn()
	{
		TargetSceneWorld = PixelRenderer.GetDefaultCharacters();
		base.ClientSpawn();
		if ( Game.IsClient && Game.LocalClient == this )
		{
			CreateLights();

		}
		Tracking = TrackingMode.Billboard;



	}
	[Event.Hotload]
	private void CreateLights()
	{
		if ( !Game.IsClient )
		{
			return;

		}
		if ( FollowSun.IsValid() )
		{
			FollowSun.Delete();
		}
		if ( FollowLight.IsValid() )
		{
			FollowLight.Delete();
		}
		if ( FollowSun2.IsValid() )
		{
			FollowSun2.Delete();
		}
		if ( FollowLight2.IsValid() )
		{
			FollowLight2.Delete();
		}
		var worldscene = PixelRenderer.GetDefaultWorld();
		var arr = CreateLights( worldscene );

		FollowSun = arr.Item1;
		FollowLight = arr.Item2;

		var arr2 = CreateLights( PixelRenderer.GetDefaultCharacters() );

		FollowSun2 = arr2.Item1;
		FollowLight2 = arr2.Item2;


	}

	private (SceneSpotLight, SceneLight) CreateLights( SceneWorld world )
	{
		var FollowSun = new SceneSpotLight( world, Position, Color.White )
		{
			Rotation = Rotation.LookAt( Vector3.Down ),
			Radius = 550,

			ConeInner = 18,
			ConeOuter = 23,
			LightColor = new Color( 1.0f, 0.95f, 0.8f ) * 3f,
		};

		var FollowLight = new SceneLight( world, Position, 150f, Color.White )
		{
			LightColor = Color.White * 1
		};

		return (FollowSun, FollowLight);

	}

	private TimeSince LastAttack;

	bool isEditingFloor = false;


	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		HandleCamera();
	}

	private void HandleCamera()
	{
		var camera = Components.Get<CameraMode>();
		//camera ??= Components.Create<PlayFieldCamera>();
		camera?.Build();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( FollowLight.IsValid() && Game.IsClient )
		{
			FollowLight.Position = Position + Vector3.Up * 32f;
			FollowSun.Position = Position + Vector3.Up * 500f;

			FollowLight2.Position = Position + Vector3.Up * 32f;
			FollowSun2.Position = Position + Vector3.Up * 500f;
			//Log.Info( "idk" );
		}
		else if ( Game.IsClient )
		{
			CreateLights();
			//Log.Info( "hello" );
		}

		Vector3 Vel = new();
		if ( Input.Down( InputButton.Forward ) )
		{
			Vel += Vector3.Forward;
		}
		if ( Input.Down( InputButton.Back ) )
		{
			Vel += Vector3.Backward;
		}
		if ( Input.Down( InputButton.Left ) )
		{
			Vel += Vector3.Left;
		}
		if ( Input.Down( InputButton.Right ) )
		{
			Vel += Vector3.Right;
		}
		if ( Input.Pressed( InputButton.Use ) && Game.IsClient )
		{
			Inventory.Toggle();
		}
		if ( Input.MouseWheel > 0 )
		{
			DarkInventory.NextBlock();
		}
		else if ( Input.MouseWheel < 0 )
		{
			DarkInventory.PreviousBlock();
		}
		/*
		if ( Input.Down( InputButton.Menu ) )
		{
			Rotation *= Rotation.FromAxis( Vector3.Up, -100f * Time.Delta );
		} */

		var direction = Vel.y < 0;
		Vel = Rotation * Vel;
		if ( Vel.Length.AlmostEqual( 0f ) )
		{
			SetAnimation( "idle" );
		}
		else
		{
			SetAnimation( "walk" );
			if ( Vel.y != 0 )
				SetFacingDirection( direction );
		}
		PlayerPosition = World.GetTilePosition( Position );
		var directionvector = Speed * Time.Delta * Vel.Normal;
		var TargetBlockPosition = LinePlaneIntersectionWithHeight( MousePosition, MouseDirection, 0 );
		var tileExtends = new Vector3( World.HalfTileSize + 1, World.HalfTileSize + 1, World.TileHeight + 1 );
		var blocks = World.GetBlocksInLine( Position, TargetBlockPosition, 3 );
		isEditingFloor = Input.Down( InputButton.Run );
		if ( Game.IsClient && Cursor.hovered == null )
		{
			for ( int i = 0; i < blocks.Count; i++ )
			{
				MapTile item = blocks[i];
				if ( item.IsSolid() )
				{
					DebugOverlay.Box( isEditingFloor ? item.WorldPosition - Vector3.Up * World.TileHeight : item.WorldPosition, -tileExtends.WithZ( 0 ), tileExtends, Color.White, Time.Delta * 2 );
					break;
				}
			}
			Vector3 pos = World.GetMapTile( TargetBlockPosition )?.WorldPosition ?? TargetBlockPosition.SnapToGrid( World.TileSize );
			Vector3 wpos = isEditingFloor ? pos - Vector3.Up * World.TileHeight : pos;
			DebugOverlay.Box( wpos, -tileExtends.WithZ( 0 ), tileExtends, Color.Green.WithAlpha( 0.3f ), Time.Delta * 2 );
		}

		if ( Input.Down( InputButton.PrimaryAttack ) && Game.IsServer && LastAttack > 0.1f )
		{
			LastAttack = 0;
			var currentPos = Position;
			for ( int i = 0; i < blocks.Count; i++ )
			{
				MapTile block = blocks[i];
				if ( (isEditingFloor || block.IsSolid()) && ((block.Position != PlayerPosition) || !isEditingFloor) && (!isEditingFloor || (i == blocks.Count - 1)) )
				{
					SetBlock( block, isEditingFloor, false );
					break;
				}
			}

		}
		if ( Input.Down( InputButton.SecondaryAttack ) && Game.IsServer && LastAttack > 0.1f )
		{
			LastAttack = 0;
			for ( int i = 0; i < blocks.Count; i++ )
			{
				MapTile block = blocks[i];
				if ( IsBlockSolid( block ) && i != blocks.Count - 1 )
				{
					continue;
				}
				else if ( i == blocks.Count - 1 && block.Position.Distance( World.GetTilePosition( TargetBlockPosition ) ) <= 1 && (isEditingFloor || block.Position != PlayerPosition) )
				{
					SetBlock( block, isEditingFloor, true );
				}
			}
		}
		var MoveHelper = new WorldMovementHelper( Position, 10 );
		if ( !Tags.Has( "noclip" ) )
		{
			//debug the time this method takes
			Position = MoveHelper.Move( directionvector );
		}
		else
			Position += directionvector;



	}

	public bool IsBlockSolid( MapTile Block )
	{
		if ( isEditingFloor )
			return Block.IsFloorSolid();
		return Block.IsSolid();

	}

	[ConCmd.Server]
	public static void NoclipDarkBind()
	{
		Entity.All.OfType<DarkBindCharacter>().ToList().ForEach( ( x ) =>
		{
			if ( x.Tags.Has( "noclip" ) )
				x.Tags.Remove( "noclip" );
			else
				x.Tags.Add( "noclip" );
		} );
	}


	public static Vector3 LinePlaneIntersectionWithHeight( Vector3 pos, Vector3 dir, float z )
	{
		float px, py, pz;

		//solve for temp, zpos = (zdir) * (temp) + (initialZpos)
		float temp = (z - pos.z) / dir.z;

		//plug in and solve for Px and Py
		px = dir.x * temp + pos.x;
		py = dir.y * temp + pos.y;
		pz = z;
		return new Vector3( px, py, pz );
	}


	public void SetBlock( MapTile block, bool floor, bool place = false )
	{
		if ( place && !IsBlockSolid( block ) )
		{
			if ( !floor )
				block.Block = Systems.Blocks.BaseBlock.Create( CurrentBlock );
			else
				block.FloorBlock = Systems.Blocks.BaseBlock.Create( CurrentBlock );
			World.SendTile( block );
		}
		else if ( !place && IsBlockSolid( block ) )
		{
			if ( !floor )
				block.Block = Systems.Blocks.BaseBlock.Create( "Air" );
			else
				block.FloorBlock = Systems.Blocks.BaseBlock.Create( "Air" );

			World.SendTile( block );
		}
	}

	[ConCmd.Client]
	public static void SpawnSpinningCube( float scale = 1 )
	{
		var player = Game.LocalPawn as DarkBindCharacter;

		new SpinningCube( PixelRenderer.GetDefaultWorld(), player.Transform.WithPosition( player.Position + Vector3.Up * 16 ).WithScale( scale ) );
	}

	[ConCmd.Client]
	public static void SpawnNonPixelatedSpinningCube( float scale = 1 )
	{
		var player = Game.LocalPawn as DarkBindCharacter;

		new SpinningCube( PixelRenderer.GetDefaultCharacters(), player.Transform.WithPosition( player.Position + Vector3.Up * 16 ).WithScale( scale ) );
	}

	[ConCmd.Client]
	public static void SpawnModel( string path, float scale )
	{
		var player = Game.LocalPawn as DarkBindCharacter;

		new SceneModel( PixelRenderer.GetDefaultWorld(), path, player.Transform.WithPosition( player.Position ).WithScale( scale ) );
	}


}
