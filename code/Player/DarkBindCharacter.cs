using SpriteKit.Entities;
using SpriteKit.SceneObjects;
using DarkBinds.Systems.Worlds;
using DarkBinds.util;
using Sandbox;
using DarkBinds.UI.IngameHUD;
using DarkBinds.UI;

namespace DarkBinds.Player;

public partial class DarkBindCharacter : ModelSprite
{
	public float Speed => 125f;
	[Net, Predicted]
	public Vector2Int PlayerPosition { get; set; }

	public string CurrentBlock => DarkInventory.CurrentBlock;
	/// <summary>
	/// Provides an easy way to switch our current cameramode component
	/// </summary>
	[SkipHotload]
	public CameraMode CameraMode
	{
		get => Components.Get<CameraMode>();
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
		SpritePath = "/sprites/mainchar/testing.sprite";
		CameraMode = new TopDownCamera();
		base.Spawn();
		Rotation = Rotation.LookAt( Vector3.Backward, Vector3.Up );
		Scale = 0.63f;

		//Components.Create<SoulPillar>();
		//Components.Create<SoulEater>();

		FollowLight = new PointLightEntity()
		{
			Falloff = 1,
			Range = 150f,
			Color = Color.White,
			Brightness = 0.3f,
			DynamicShadows = true,
		};

		DarkInventory = new DarkInventory();


	}

	[Net, Predicted] protected PointLightEntity FollowLight { get; set; }
	private SpotLightEntity FollowSun { get; set; }
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		if ( Host.IsClient && Local.Pawn == this )
			FollowSun = new SpotLightEntity()
			{
				Rotation = Rotation.LookAt( Vector3.Down ),
				Falloff = 1,
				InnerConeAngle = 18,
				OuterConeAngle = 23,
				Range = 550,
				Color = new Color( 1.0f, 0.95f, 0.8f ),
				Brightness = 1.2f,
				DynamicShadows = true,
			};

		Tracking = TrackingMode.Billboard;



	}

	private TimeSince LastAttack;

	bool isEditingFloor = false;


	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
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
		if ( Input.Pressed( InputButton.Use ) && IsClient )
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
		var TargetBlockPosition = LinePlaneIntersectionWithHeight( Input.Cursor.Origin, Input.Cursor.Direction, 0 );
		var tileExtends = new Vector3( World.HalfTileSize + 1, World.HalfTileSize + 1, World.TileHeight + 1 );
		var blocks = World.GetBlocksInLine( Position, TargetBlockPosition, 3 );
		isEditingFloor = Input.Down( InputButton.Run );
		if ( IsClient && Cursor.hovered == null )
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

		if ( Input.Down( InputButton.Attack1 ) && IsServer && LastAttack > 0.1f )
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
		if ( Input.Down( InputButton.Attack2 ) && IsServer && LastAttack > 0.1f )
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


		if ( FollowLight.IsValid() )
		{
			if ( IsServer )
			{
				FollowLight.Position = Position + Vector3.Up * 64f;
				FollowLight.QuadraticAttenuation = 0f;
				FollowLight.LinearAttenuation = 1f;

			}
		}
		if ( FollowSun.IsValid() )
			FollowSun.Position = Position + Vector3.Up * 500f;
	}

	public bool IsBlockSolid( MapTile Block )
	{
		if ( isEditingFloor )
			return Block.IsFloorSolid();
		return Block.IsSolid();

	}

	[ServerCmd]
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
	[ServerCmd]
	public static void CreateLightSource()
	{
		if ( ConsoleSystem.Caller?.Pawn is DarkBindCharacter charr )
		{
			var light = new SceneLight( World.Scene, charr.Position + (Vector3.Up * 32), 400, Color.White );
		}
	}

}
