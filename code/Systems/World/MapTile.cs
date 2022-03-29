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

	public SceneObject TileSO { get; set; }

	public MapChunk ParentChunk { get; set; }

	public bool GeneratingMesh { get; set; } = false;

	[Event.Hotload]
	public void Delete()
	{
		if ( TileSO.IsValid() )
			TileSO.Delete();
	}

	internal void Deserialize( ref BinaryReader reader )
	{
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		Position = new Vector2Int( x, y );
		Block = BaseBlock.Create( reader.ReadString() );
	}


	internal void Serialize( ref BinaryWriter writer )
	{
		writer.Write( Position.x );
		writer.Write( Position.y );
		writer.Write( Block.Name );
	}

	public void BuildMesh()
	{
		//return Block.BuildMesh( ref mb, Position );
		if ( Block.IsSolid() )
			CreateSceneObject();


	}

	private void CreateSceneObject()
	{
		if ( TileSO.IsValid() )
		{
			TileSO.Delete();
		}

		GeneratingMesh = true;
		var model = Block.BuildMesh( Position ).Create();
		GeneratingMesh = false;
		if ( model == null )
			return;
		TileSO = new MapSceneObject( Map.Scene, model, new Transform( WorldPosition ) );

		SetAttributes();

	}

	public bool IsSolid()
	{
		return Block.IsSolid();
	}







	internal void DebugView( bool alwaysshow = false, Color color = default )
	{
		if ( !Block.IsSolid() && !alwaysshow )
			return;
		Debug.Box( WorldPosition + World.HalfTileSize, WorldPosition - World.HalfTileSize, color == new Color() ? Color.Red : color, 0, false );
	}

	internal void SetAttributes()
	{
		if ( !TileSO.IsValid() || Block == null || Block.MapSheet == null )
			return;
		var uv = Block.GetUVCoords();
		TileSO.Attributes.Set( "SpriteSheet", Block.MapSheet.SpriteSheetTexture );
	}
}
