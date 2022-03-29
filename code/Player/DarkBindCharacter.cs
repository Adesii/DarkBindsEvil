using SpriteKit.Entities;
using SpriteKit.SceneObjects;
using DarkBinds.Systems.Worlds;
using DarkBinds.util;
using Sandbox;

namespace DarkBinds.Player;

public partial class DarkBindCharacter : ModelSprite
{
	public float Speed => 125f;
	[Net, Predicted]
	public Vector2Int PlayerPosition { get; set; }
	/// <summary>
	/// Provides an easy way to switch our current cameramode component
	/// </summary>
	[SkipHotload]
	public CameraMode CameraMode
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}
	public override void Spawn()
	{
		//SpritePath = "/sprites/mainchar/darkbind.sprite";
		SpritePath = "/sprites/mainchar/testing.sprite";
		CameraMode = new TopDownCamera();
		base.Spawn();
		Rotation = Rotation.LookAt( Vector3.Backward, Vector3.Up );
		Scale = 0.8f;

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

	}

	private TimeSince LastAttack;


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
		if ( Input.Down( InputButton.Use ) )
		{
			Rotation *= Rotation.FromAxis( Vector3.Up, 100f * Time.Delta );
		}
		if ( Input.Down( InputButton.Menu ) )
		{
			Rotation *= Rotation.FromAxis( Vector3.Up, -100f * Time.Delta );
		}
		float boost = 1;
		if ( Input.Down( InputButton.Run ) )
		{
			boost *= 10;
		}
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
		var directionvector = Vel.Normal * Speed * boost * Time.Delta;
		var TargetBlockPosition = LinePlaneIntersectionWithHeight( Input.Cursor.Origin, Input.Cursor.Direction, 0 );
		var tileExtends = new Vector3( World.HalfTileSize + 1, World.HalfTileSize + 1, World.TileHeight + 1 );
		var blocks = World.GetBlocksInLine( Position, TargetBlockPosition, 3 );
		if ( IsClient )
			for ( int i = 0; i < blocks.Count; i++ )
			{
				MapTile item = blocks[i];
				if ( !item.IsSolid() && i == blocks.Count - 1 )
				{
					DebugOverlay.Box( item.WorldPosition, -tileExtends.WithZ( 0 ), tileExtends, Color.White, Time.Delta * 2 );
				}
				else if ( item.IsSolid() )
				{
					DebugOverlay.Box( item.WorldPosition, -tileExtends.WithZ( 0 ), tileExtends, Color.White, Time.Delta * 2 );
					break;
				}
			}

		if ( Input.Down( InputButton.Attack1 ) && IsServer && LastAttack > 0.1f )
		{
			LastAttack = 0;
			var currentPos = Position;
			foreach ( var block in blocks )
			{
				if ( block.IsSolid() )
				{
					SetBlock( block, false );
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
				if ( block.IsSolid() && i != blocks.Count - 1 )
				{
					continue;
				}
				else if ( i == blocks.Count - 1 && block.Position.Distance( World.GetTilePosition( TargetBlockPosition ) ) <= 1 && block.Position != PlayerPosition )
				{
					SetBlock( block, true );
				}
			}
		}
		var MoveHelper = new WorldMovementHelper( Position, 10f );
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
				FollowLight.Position = Position + Vector3.Up * 64f;
		}
		if ( FollowSun.IsValid() )
			FollowSun.Position = Position + Vector3.Up * 500f;



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


	public void SetBlock( MapTile block, bool place = false )
	{
		if ( place && !block.IsSolid() )
		{
			block.Block = Systems.Blocks.BaseBlock.Create( "Dirt" );
			World.SendTile( block );
		}
		else if ( !place && block.IsSolid() )
		{
			block.Block = new Systems.Blocks.AirBlock();
			World.SendTile( block );
		}
	}
	[ServerCmd]
	public static void CreateLightSource()
	{
		if ( ConsoleSystem.Caller?.Pawn is DarkBindCharacter charr )
		{
			var light = new SceneLight( Map.Scene, charr.Position + (Vector3.Up * 32), 400, Color.White );
		}
	}

}
