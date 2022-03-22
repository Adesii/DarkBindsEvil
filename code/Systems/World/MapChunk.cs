using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Sandbox;
using Sandbox.Internal;

namespace DarkBinds.Systems.World;

public partial class MapChunk
{
	public static int ChunkSize = 16;
	private static int _viewsize = ChunkSize * MapTile.TileSize;
	public static int ViewSize
	{
		get
		{
			return _viewsize;
		}
	}
	private Vector2Int _pos;
	public Vector2Int Position
	{
		get => _pos; set
		{
			_pos = value;
			WorldPosition = (new Vector3( Position.x * ViewSize, Position.y * ViewSize, 0 ) - (World.WorldSize * MapChunk.ViewSize / 2)).WithZ( 0 );
		}
	}

	public Vector3 WorldPosition { get; set; }

	public Dictionary<Vector2Int, MapTile> Tiles { get; set; } = new();
	public bool IsGenerated { get; set; } = false;

	public MapChunkSceneObject SceneObject { get; set; }

	public List<VertexBuffer> VertexBuffers { get; set; } = new();

	public void GenerateMesh()
	{
		if ( SceneObject == null || !SceneObject.IsValid() )
		{
			SceneObject = new MapChunkSceneObject( Map.Scene, this );
		}
		if ( !IsGenerated )
		{

			//addGroundPlane
			ResolveMesh();

			IsGenerated = true;
		}


		SceneObject.Position = WorldPosition;
		SceneObject.Init();
	}

	private void ResolveMesh()
	{
		VertexBuffers = new();
		VertexBuffers.Clear();

		VertexBuffers.Add( MakeTesselatedPlane( new Vector3( MapTile.TileSize, MapTile.TileSize ) / 2, 0, new Vector3( ViewSize, ViewSize, 0 ), 4, 4 ) );
		var currVertexBuffer = new VertexBuffer();
		currVertexBuffer.Init( true );

		var HalfTileSize = MapTile.TileSize / 2;
		var TileSize = MapTile.TileSize;
		int VertCount = 0;
		foreach ( var Tile in Tiles.Values )
		{
			if ( Tile.BlockIndex == 0 ) continue;

			var TilePosition = Tile.Position;
			var TileWorldPosition = GetTilePositionRelativeToChunk( TilePosition );
			var NorthBlock = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y ) );
			var SouthBlock = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y ) );
			var EastBlock = World.GetMapTile( new Vector2Int( TilePosition.x, TilePosition.y + 1 ) );
			var WestBlock = World.GetMapTile( new Vector2Int( TilePosition.x, TilePosition.y - 1 ) );

			//var NorthEastBlock = World.GetMapTile( new Vector2Int( Position.x + 1, Position.y + 1 ) );
			//var NorthWestBlock = World.GetMapTile( new Vector2Int( Position.x - 1, Position.y + 1 ) );
			//var SouthEastBlock = World.GetMapTile( new Vector2Int( Position.x + 1, Position.y - 1 ) );
			//var SouthWestBlock = World.GetMapTile( new Vector2Int( Position.x - 1, Position.y - 1 ) );
			//NorthWall
			if ( NorthBlock != null && NorthBlock.BlockIndex == 0 )
			{
				var right = TileWorldPosition + (Vector3.Forward + Vector3.Right) * HalfTileSize;
				var left = right + Vector3.Left * TileSize;
				AddWall( currVertexBuffer, TileSize, right, left );
				VertCount += 4;
			}
			//SouthWall
			if ( SouthBlock != null && SouthBlock.BlockIndex == 0 )
			{
				var right = TileWorldPosition + (Vector3.Backward + Vector3.Left) * HalfTileSize;
				var left = right + Vector3.Right * TileSize;
				AddWall( currVertexBuffer, TileSize, right, left );
				VertCount += 4;
			}
			//EastWall
			if ( EastBlock != null && EastBlock.BlockIndex == 0 )
			{
				var right = TileWorldPosition + (Vector3.Left + Vector3.Forward) * HalfTileSize;
				var left = right + Vector3.Backward * TileSize;
				AddWall( currVertexBuffer, TileSize, right, left );
				VertCount += 4;
			}
			//WestWall
			if ( WestBlock != null && WestBlock.BlockIndex == 0 )
			{
				var right = TileWorldPosition + (Vector3.Right + Vector3.Backward) * HalfTileSize;
				var left = right + Vector3.Forward * TileSize;
				AddWall( currVertexBuffer, TileSize, right, left );
				VertCount += 4;
			}
			//Ceilng
			if ( Tile.BlockIndex != 0 )
			{
				var v1 = TileWorldPosition + (Vector3.Forward + Vector3.Right) * HalfTileSize;
				var v2 = TileWorldPosition + (Vector3.Left + Vector3.Forward) * HalfTileSize;
				var v3 = TileWorldPosition + (Vector3.Backward + Vector3.Left) * HalfTileSize;
				var v4 = TileWorldPosition + (Vector3.Right + Vector3.Backward) * HalfTileSize;
				Vertex[] arr = new Vertex[]{
					new Vertex(v1   + Vector3.Up * TileSize, new Vector2( 0, 1 ) ,Color.White),
					new Vertex(v2   + Vector3.Up * TileSize, new Vector2( 1, 1 ) ,Color.White),
					new Vertex(v3   + Vector3.Up * TileSize, new Vector2( 1, 0 ) ,Color.White),
					new Vertex(v4   + Vector3.Up * TileSize, new Vector2( 0, 0 ) ,Color.White),
				};
				currVertexBuffer.AddQuad( arr[0], arr[1], arr[2], arr[3] );
				VertCount += 4;
			}






			var newVB = new VertexBuffer();
			newVB.Init( true );
			VertexBuffers.Add( currVertexBuffer );
			currVertexBuffer = newVB;

		}


	}

	private static void AddWall( VertexBuffer currVertexBuffer, int TileSize, Vector3 right, Vector3 left )
	{
		Vertex[] arr = new Vertex[]{
					new Vertex(right, new Vector2( 0, 1 ) ,Color.White),
					new Vertex(left, new Vector2( 1, 1 ) ,Color.White),
					new Vertex(left + Vector3.Up * TileSize, new Vector2( 1, 0 ) ,Color.White),
					new Vertex(right + Vector3.Up * TileSize, new Vector2( 0, 0 ) ,Color.White),
				};
		currVertexBuffer.AddQuad( arr[0], arr[1], arr[2], arr[3] );
	}

	~MapChunk()
	{
		if ( SceneObject != null )
		{
			SceneObject.Delete();
		}
	}



	public void DeserializeFromCompressed( ref BinaryReader reader )
	{
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		Position = new Vector2Int( x, y );
		int count = reader.ReadInt32();
		Log.Debug( $"Deserializing {count} tiles" );

		Tiles = new Dictionary<Vector2Int, MapTile>();

		for ( int i = 0; i < count; i++ )
		{
			var tile = new MapTile();
			tile.Deserialize( ref reader );
			Tiles.Add( tile.Position, tile );
		}
	}
	public void SerializeToCompressed( ref BinaryWriter writer )
	{
		Log.Debug( $"Serializing {Tiles.Count} tiles" );
		writer.Write( Position.x );
		writer.Write( Position.y );
		writer.Write( Tiles.Count );

		foreach ( var tile in Tiles )
		{
			tile.Value.Serialize( ref writer );
		}
		writer.Flush();
	}

	public MapTile GetTile( Vector2Int worldPosition )
	{
		if ( Tiles.TryGetValue( worldPosition, out var tile ) )
			return tile;
		return null;
	}
	public MapTile GetTile( Vector3 WorldPosition )
	{
		var pos = new Vector2Int(
			(int)Math.Floor( (WorldPosition.x) / MapTile.TileSize ),
			(int)Math.Floor( (WorldPosition.y) / MapTile.TileSize )
		);
		pos += (World.WorldSize * ChunkSize) / 2;
		return GetTile( pos );
	}

	internal void DebugView()
	{
		foreach ( var tile in Tiles.Values )
		{
			tile.DebugView();
		}
	}

	public static Vector2Int GetChunkPosition( Vector3 WorldPosition )
	{
		return new Vector2Int(
			(int)Math.Floor( (WorldPosition.x + (World.WorldSize * ViewSize / 2)) / ViewSize ),
			(int)Math.Floor( (WorldPosition.y + (World.WorldSize * ViewSize / 2)) / ViewSize )
		);
	}
	public static Vector2Int GetChunkPosition( Vector2Int WorldPosition )
	{
		return (WorldPosition / ChunkSize) % ChunkSize;

	}

	public Vector3 GetTilePositionRelativeToChunk( Vector2Int TilePositionWorldSpace )
	{
		var TilePosition = TilePositionWorldSpace - (Position * ChunkSize);
		return new Vector3( TilePosition.x * MapTile.TileSize, TilePosition.y * MapTile.TileSize, 0 );

	}

	internal void Cleanup()
	{
		SceneObject?.Delete();
		IsGenerated = false;
	}

	internal void Render()
	{
		DebugView();
		GenerateMesh();
	}

	public VertexBuffer MakeTesselatedPlane( Vector3 pos, Vector3 from, Vector3 to, int xRes, int yRes )
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		var WaterHeight = Math.Max( from.z, to.z );

		for ( int x = 0; x <= xRes; x++ )
		{
			for ( int y = 0; y <= yRes; y++ )
			{
				float fX = MathX.LerpTo( from.x, to.x, x / (float)xRes );
				float fY = MathX.LerpTo( from.y, to.y, y / (float)yRes );

				Vector3 vPos = new Vector3(
					fX,
					fY,
					WaterHeight
				);

				vPos -= pos;

				var uv = new Vector2( x / (float)xRes, y / (float)yRes );

				vb.Add( new Vertex( vPos, Vector3.Down, Vector3.Right, uv ) );

			}
		}

		for ( int y = 0; y < yRes; y++ )
		{
			for ( int x = 0; x < xRes; x++ )
			{
				var i = y + (x * yRes) + x;

				vb.AddRawIndex( i + yRes + 1 );
				vb.AddRawIndex( i + 1 );
				vb.AddRawIndex( i );

				vb.AddRawIndex( i + 1 );
				vb.AddRawIndex( i + yRes + 1 );
				vb.AddRawIndex( i + yRes + 2 );

			}
		}
		return vb;
	}
}


public class MapChunkSceneObject : SceneCustomObject
{
	public Material mat = Material.Load( "materials/maptest.vmat" );
	public MapChunk Chunk { get; set; }
	public MapChunkSceneObject( SceneWorld sceneWorld, MapChunk chunk ) : base( sceneWorld )
	{
		Chunk = chunk;
	}



	public void Init()
	{
		var box = new BBox( new Vector3( 0, 0, -MapTile.TileSize ), new Vector3( MapChunk.ViewSize, MapChunk.ViewSize, MapTile.TileSize ) );

		box.Mins -= new Vector3( MapTile.TileSize, MapTile.TileSize, 0 ) / 2;
		box.Maxs -= new Vector3( MapTile.TileSize, MapTile.TileSize, 0 ) / 2;
		Bounds = box + (Position);

		Debug.Box( box.Mins + Position, box.Maxs + Position, Color.Green, 0f );
	}


	public override void RenderSceneObject()
	{
		if ( Chunk.IsGenerated )
		{
			foreach ( var item in Chunk.VertexBuffers )
			{
				item.Draw( mat );
			}
		}
		base.RenderSceneObject();
	}
}
