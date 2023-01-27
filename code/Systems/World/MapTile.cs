using System.IO;
using System.Numerics;
using System.Runtime.Serialization;
using DarkBinds.Systems.Blocks;
using Sandbox;
using SpriteKit.Asset;

namespace DarkBinds.Systems.Worlds;

[SkipHotload]
public partial class MapTile
{

	private Vector2Int _pos;
	public Vector2Int Position
	{
		get => _pos;
		set
		{
			_pos = value;
			WorldPosition = World.GetTileWorldPosition( Position );

		}
	}
	public Vector3 WorldPosition { get; set; }
	public Vector2Int LocalPosition
	{
		get
		{
			return ParentChunk?.GetTilePositionRelativeToChunk( Position ) ?? Vector2Int.Zero;
		}
	}
	private BaseBlock _block;
	public BaseBlock Block
	{
		get => _block; set
		{
			if ( value != null )
			{
				value.Tile = this;
				_block = value;
			}
		}
	}


	private BaseBlock _Floorblock;
	public BaseBlock FloorBlock
	{
		get => _Floorblock; set
		{
			if ( value != null )
			{
				value.Tile = this;
				_Floorblock = value;
				_Floorblock.IsFloor = true;
			}
		}
	}

	public SceneObject TileSO { get; set; }

	public MapChunk ParentChunk { get; set; }

	public bool GeneratingMesh { get; set; } = false;

	[Event.Hotload]
	public void Delete()
	{
		if ( TileSO.IsValid() )
			TileSO.Delete();
		Block?.OnDestroyed();
		FloorBlock?.OnDestroyed();
	}

	internal void Deserialize( ref BinaryReader reader )
	{
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		Position = new Vector2Int( x, y );
		Block = BaseBlock.Create( reader.ReadString() );
		FloorBlock = BaseBlock.Create( reader.ReadString() );
	}


	internal void Serialize( ref BinaryWriter writer )
	{
		writer.Write( Position.x );
		writer.Write( Position.y );
		writer.Write( Block.Name );
		writer.Write( FloorBlock.Name );
	}

	public void BuildMesh()
	{
		//return Block.BuildMesh( ref mb, Position );
		CreateSceneObject();


	}

	private void CreateSceneObject()
	{
		if ( TileSO.IsValid() )
		{
			TileSO.Delete();
		}
		Block?.OnDestroyed();
		FloorBlock?.OnDestroyed();

		GeneratingMesh = true;
		var BlockModel = Block.BuildMesh( Position );
		var builder = Model.Builder;
		if ( BlockModel != null )
		{
			builder.AddMesh( BlockModel );
		}
		var FloorBlockModel = FloorBlock.BuildFloorMesh( Position );
		if ( FloorBlockModel != null )
		{
			builder.AddMesh( FloorBlockModel );
		}
		var model = builder.Create();
		GeneratingMesh = false;
		if ( model == null || (BlockModel == null && FloorBlockModel == null) )
			return;
		Block?.OnCreated();
		FloorBlock?.OnCreated();

		TileSO = new MapSceneObject( World.Scene, model, new Transform( WorldPosition ) )
		{
			//Bounds = new BBox( new Vector3( -World.HalfTileSize, -World.HalfTileSize, -World.TileHeight ), new Vector3( World.HalfTileSize, World.HalfTileSize, World.TileHeight ) ) + WorldPosition
		};

		//SetAttributes();

	}

	public bool IsSolid()
	{
		return Block.IsSolid();
	}
	public bool IsFloorSolid()
	{
		return FloorBlock.IsSolid();
	}







	internal void DebugView( bool alwaysshow = false, Color color = default )
	{
		if ( (!Block.IsSolid() && !alwaysshow) || !Debug.Enabled )
			return;
		DebugOverlay.Box( WorldPosition + World.HalfTileSize, WorldPosition - World.HalfTileSize, color == new Color() ? Color.Red : color, 0, false );
	}
	[Event.PreRender]
	internal void SetAttributes()
	{
		//if ( !TileSO.IsValid() || Block == null || Block.MapSheet == null || FloorBlock == null || FloorBlock.MapSheet == null )
		//	return;
		if ( TileSO.IsValid() )
			TileSO.Attributes.Set( "SpriteSheet", Block.MapSheet.SpriteSheetTexture );
		//TileSO.Attributes.Set( "FloorSpriteSheet", FloorBlock.MapSheet.SpriteSheetTexture );

	}
}
