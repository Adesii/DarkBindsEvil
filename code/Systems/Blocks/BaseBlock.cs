using System.IO;
using SpriteKit.Asset;
using DarkBinds.Systems.Worlds;
using System;
using System.Collections.Generic;

namespace DarkBinds.Systems.Blocks;

[Library( "BaseBlock" )]
public class BaseBlock
{
	private static Dictionary<string, BlockAttribute> _blocks = new Dictionary<string, BlockAttribute>();
	public static Dictionary<string, BlockAttribute> BlockList
	{
		get
		{
			if ( _blocks.Count == 0 )
			{
				RegisterBlocks();
			}
			return _blocks;
		}
	}

	public string Name { get; set; }
	public MapSheetArea MapSheet { get; set; }
	public MapTile Tile { get; set; }

	public bool IsFloor { get; set; } = false;

	public BaseBlock()
	{
	}

	public virtual void OnPlaced()
	{
	}
	public virtual void OnBroken()
	{

	}
	public virtual void OnDestroyed()
	{
	}

	public virtual void OnCreated()
	{

	}
	public virtual void OnSerialize( ref BinaryWriter writer )
	{
	}
	public virtual void OnDeserialize( ref BinaryReader reader )
	{
	}

	[Event.Entity.PostSpawn]
	public static void RegisterBlocks()
	{
		_blocks = new();
		var listofBlocks = TypeLibrary.GetAttributes<BlockAttribute>();
		foreach ( var block in listofBlocks )
		{
			_blocks[block.BlockName.Trim().ToLower()] = block;
			Log.Info( $"Registered block {block.BlockName}" );
		}
	}


	static int b = 100;
	public static BaseBlock Create( string name )
	{
		name = name.Trim().ToLower();
		var testblock = BlockList.GetValueOrDefault( name );
		if ( testblock != null )
		{
			var block = TypeLibrary.Create<BaseBlock>( TypeLibrary.GetTypeByName( testblock.BlockName.Trim().ToLower() ) );
			//Log.Info( $"Created block {block.Name}" );
			block.Name = name;
			block.MapSheet = MapSheetAsset.GetBlockArea( name );
			return block;
		}
		MapSheetArea area = MapSheetAsset.GetBlockArea( name );
		if ( area != null )
		{
			return new BaseBlock()
			{
				MapSheet = area,
				Name = name,
			};
		}
		return new();
	}
	protected List<MapVertex> vertices = new();
	protected List<int> indices = new();
	public virtual Mesh BuildMesh( Vector2Int Position )
	{
		if ( !HasMesh() )
			return null;
		indices = new();
		vertices = new();
		var HalfTileSize = World.TileSize / 2;
		var TileSize = World.TileSize;

		int VertCount = 0;

		var TilePosition = Position;
		var TileWorldPosition = IsFloor ? new Vector3( 0, 0, -TileSize ) : 0;//new Vector3( Tile.LocalPosition.x, Tile.LocalPosition.y, 0 );
		var NorthBlock = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y ) );
		var SouthBlock = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y ) );
		var EastBlock = World.GetMapTile( new Vector2Int( TilePosition.x, TilePosition.y + 1 ) );
		var WestBlock = World.GetMapTile( new Vector2Int( TilePosition.x, TilePosition.y - 1 ) );

		int hasNorthEastWall = -1;
		int hasNorthWestWall = -1;
		int hasSouthEastWall = -1;
		int hasSouthWestWall = -1;

		//NorthWall
		if ( NorthBlock.IsValid() && NorthBlock.Block.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Forward + Vector3.Right) * HalfTileSize;
			var left = right + Vector3.Left * TileSize;
			hasNorthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y + 1 ) )?.IsSolid() ?? false ? 1 : 0;
			hasNorthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y - 1 ) )?.IsSolid() ?? false ? 1 : 0;
			Vector2 VertBlends = new Vector2( hasNorthEastWall, hasNorthWestWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}
		//SouthWall
		if ( SouthBlock.IsValid() && SouthBlock.Block.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Backward + Vector3.Left) * HalfTileSize;
			var left = right + Vector3.Right * TileSize;
			hasSouthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y + 1 ) )?.IsSolid() ?? false ? 1 : 0;
			hasSouthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y - 1 ) )?.IsSolid() ?? false ? 1 : 0;

			Vector2 VertBlends = new Vector2( hasSouthWestWall, hasSouthEastWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}
		//EastWall
		if ( EastBlock.IsValid() && EastBlock.Block.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Left + Vector3.Forward) * HalfTileSize;
			var left = right + Vector3.Backward * TileSize;
			if ( hasNorthEastWall == -1 )
			{
				hasNorthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y + 1 ) )?.IsSolid() ?? false ? 1 : 0;
			}
			if ( hasSouthEastWall == -1 )
			{
				hasSouthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y + 1 ) )?.IsSolid() ?? false ? 1 : 0;
			}
			Vector2 VertBlends = new Vector2( hasSouthEastWall, hasNorthEastWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}
		//WestWall
		if ( WestBlock.IsValid() && WestBlock.Block.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Right + Vector3.Backward) * HalfTileSize;
			var left = right + Vector3.Forward * TileSize;
			if ( hasNorthWestWall == -1 )
			{
				hasNorthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y - 1 ) )?.IsSolid() ?? false ? 1 : 0;
			}
			if ( hasSouthWestWall == -1 )
			{
				hasSouthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y - 1 ) )?.IsSolid() ?? false ? 1 : 0;
			}
			Vector2 VertBlends = new Vector2( hasNorthWestWall, hasSouthWestWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}


		//Ceilng

		{
			AddCeiiling( HalfTileSize, TileWorldPosition );
			VertCount += 4;
		}
		if ( VertCount > 0 )
		{
			if ( !MapChunk.MaterialList.ContainsKey( MapSheet.SpriteSheetPath ) )
			{
				var copy = MapChunk.DefaultMaterial.CreateCopy();
				copy.OverrideTexture( "SpriteSheet", MapSheet.SpriteSheetTexture );
				var alpha = MapSheet.SpriteSheetAlphaTexture;
				if ( alpha != null )
					copy.OverrideTexture( "SpriteSheetOpacityMask", MapSheet.SpriteSheetAlphaTexture );
				MapChunk.MaterialList.Add( MapSheet.SpriteSheetPath, copy );
			}
			Mesh TileMesh = new( MapChunk.MaterialList[MapSheet.SpriteSheetPath] );
			TileMesh.CreateVertexBuffer<MapVertex>( vertices.Count, MapVertex.Layout, vertices );

			TileMesh.SetVertexBufferSize( vertices.Count );
			TileMesh.SetVertexBufferData( vertices );

			TileMesh.CreateIndexBuffer( indices.Count, indices );
			TileMesh.SetIndexBufferSize( indices.Count );
			TileMesh.SetIndexBufferData( indices );

			return TileMesh;
		}





		return null;

	}

	public virtual Mesh BuildFloorMesh( Vector2Int Position )
	{
		if ( !HasMesh() )
			return null;
		indices = new();
		vertices = new();
		var HalfTileSize = World.TileSize / 2;
		var TileSize = World.TileSize;

		int VertCount = 0;

		var TilePosition = Position;
		var TileWorldPosition = new Vector3( 0, 0, -World.TileHeight );//new Vector3( Tile.LocalPosition.x, Tile.LocalPosition.y, 0 );
		var NorthBlock = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y ) );
		var SouthBlock = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y ) );
		var EastBlock = World.GetMapTile( new Vector2Int( TilePosition.x, TilePosition.y + 1 ) );
		var WestBlock = World.GetMapTile( new Vector2Int( TilePosition.x, TilePosition.y - 1 ) );

		int hasNorthEastWall = -1;
		int hasNorthWestWall = -1;
		int hasSouthEastWall = -1;
		int hasSouthWestWall = -1;

		//NorthWall
		if ( NorthBlock.IsValid() && NorthBlock.FloorBlock.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Forward + Vector3.Right) * HalfTileSize;
			var left = right + Vector3.Left * TileSize;
			hasNorthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y + 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			hasNorthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y - 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			Vector2 VertBlends = new Vector2( hasNorthEastWall, hasNorthWestWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}
		//SouthWall
		if ( SouthBlock.IsValid() && SouthBlock.FloorBlock.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Backward + Vector3.Left) * HalfTileSize;
			var left = right + Vector3.Right * TileSize;
			hasSouthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y + 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			hasSouthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y - 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;

			Vector2 VertBlends = new Vector2( hasSouthWestWall, hasSouthEastWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}
		//EastWall
		if ( EastBlock.IsValid() && EastBlock.FloorBlock.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Left + Vector3.Forward) * HalfTileSize;
			var left = right + Vector3.Backward * TileSize;
			if ( hasNorthEastWall == -1 )
			{
				hasNorthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y + 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			}
			if ( hasSouthEastWall == -1 )
			{
				hasSouthEastWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y + 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			}
			Vector2 VertBlends = new Vector2( hasSouthEastWall, hasNorthEastWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}
		//WestWall
		if ( WestBlock.IsValid() && WestBlock.FloorBlock.IsTranslucent() )
		{
			var right = TileWorldPosition + (Vector3.Right + Vector3.Backward) * HalfTileSize;
			var left = right + Vector3.Forward * TileSize;
			if ( hasNorthWestWall == -1 )
			{
				hasNorthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x + 1, TilePosition.y - 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			}
			if ( hasSouthWestWall == -1 )
			{
				hasSouthWestWall = World.GetMapTile( new Vector2Int( TilePosition.x - 1, TilePosition.y - 1 ) )?.IsFloorSolid() ?? false ? 1 : 0;
			}
			Vector2 VertBlends = new Vector2( hasNorthWestWall, hasSouthWestWall );
			AddWall( TileSize, right, left, VertBlends );
			VertCount += 4;
		}


		//Ceilng
		if ( !Tile.Block?.IsSolid() ?? false )
		{
			AddCeiiling( HalfTileSize, TileWorldPosition );
			VertCount += 4;
		}
		if ( VertCount > 0 )
		{
			if ( !MapChunk.MaterialList.ContainsKey( MapSheet.SpriteSheetPath ) )
			{
				var copy = MapChunk.DefaultMaterial.CreateCopy();
				copy.OverrideTexture( "SpriteSheet", MapSheet.SpriteSheetTexture );
				MapChunk.MaterialList.Add( MapSheet.SpriteSheetPath, copy );
			}
			Mesh TileMesh = new( MapChunk.MaterialList[MapSheet.SpriteSheetPath] );
			TileMesh.CreateVertexBuffer<MapVertex>( vertices.Count, MapVertex.Layout, vertices );

			TileMesh.SetVertexBufferSize( vertices.Count );
			TileMesh.SetVertexBufferData( vertices );

			TileMesh.CreateIndexBuffer( indices.Count, indices );
			TileMesh.SetIndexBufferSize( indices.Count );
			TileMesh.SetIndexBufferData( indices );

			return TileMesh;
		}





		return null;
	}

	protected virtual void AddCeiiling( int HalfTileSize, Vector3 TileWorldPosition )
	{
		var v0 = TileWorldPosition + (Vector3.Forward + Vector3.Left) * HalfTileSize;
		var v1 = TileWorldPosition + (Vector3.Forward + Vector3.Right) * HalfTileSize;
		var v2 = TileWorldPosition + (Vector3.Backward + Vector3.Left) * HalfTileSize;
		var v3 = TileWorldPosition + (Vector3.Right + Vector3.Backward) * HalfTileSize;

		var uv0 = new Vector2( 0, 0 );
		var uv1 = new Vector2( 1, 0 );
		var uv2 = new Vector2( 0, 0.5f );
		var uv3 = new Vector2( 1, 0.5f );

		Vector2 VertBlends = new Vector2( 0, 0 );
		Vector4 Texcord0 = GetUVCoords( Vector3.Up );

		MapVertex[] arr = new MapVertex[]{
					new MapVertex(v0   + Vector3.Up * World.TileHeight,Vector3.Up, Vector3.Right, uv0,VertBlends,Texcord0),
					new MapVertex(v1   + Vector3.Up * World.TileHeight,Vector3.Up, Vector3.Right, uv1,VertBlends,Texcord0),
					new MapVertex(v2   + Vector3.Up * World.TileHeight,Vector3.Up, Vector3.Right, uv2,VertBlends,Texcord0),
					new MapVertex(v3   + Vector3.Up * World.TileHeight,Vector3.Up, Vector3.Right, uv3,VertBlends,Texcord0)
				};
		vertices.AddRange( arr );
		int i = vertices.Count;
		indices.AddRange( new List<int>(){
					i - 1,i - 3,i - 4,
					i - 2,i - 1,i - 4
			} );
	}

	public Color32 GetMiniMapColor()
	{
		return Color32.White;
	}
	public virtual bool BreakAble()
	{
		return true;
	}

	public virtual bool IsSolid()
	{
		return true;
	}
	public virtual bool IsTranslucent()
	{
		return false;
	}

	public virtual bool HasMesh()
	{
		return true;
	}

	public virtual bool HasCollisions()
	{
		return true;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <returns> Start and End UV Coords</returns>	
	public Vector4 GetUVCoords( Vector3 Axis )
	{
		if ( MapSheet != null && Tile != null )
		{
			var SpriteViewport = MapSheet.Area;
			if ( MapSheet.XSubdivisions != 1 )
				SpriteViewport.width /= MapSheet.XSubdivisions;
			if ( MapSheet.YSubdivisions != 1 )
				SpriteViewport.height /= MapSheet.YSubdivisions;

			var view = SpriteViewport;
			float axisprogress = 0;
			/* if ( Axis == Vector3.Right || Axis == Vector3.Left )
				axisprogress = (Tile.Position.y) % (MapSheet.XSubdivisions);
			else if ( Axis == Vector3.Forward || Axis == Vector3.Backward )
				axisprogress = (Tile.Position.x) % (MapSheet.XSubdivisions);
			else if ( Axis == Vector3.Up )
			{ */
			axisprogress = (Tile.Position.x + Tile.Position.y) % (MapSheet.XSubdivisions);
			/* } */
			axisprogress += 1;

			int x = ((MapSheet.XSubdivisions.FloorToInt()) - axisprogress.FloorToInt()).Clamp( 0, MapSheet.XSubdivisions.FloorToInt() - 1 );
			view.Position += new Vector2( x * SpriteViewport.width,
										  0 * SpriteViewport.height );
			Vector2 StartUV = view.Position / MapSheet.SpriteSheetTexture.Size;
			Vector2 EndUV = (view.Position + view.Size) / MapSheet.SpriteSheetTexture.Size;
			return new Vector4( StartUV.x, StartUV.y, EndUV.x, EndUV.y );
		}
		return Vector4.One;
	}


















	protected virtual void AddWall( int TileSize, Vector3 right, Vector3 left, Vector2 Blends )
	{
		var topleft = left + Vector3.Up * World.TileHeight;
		var topright = right + Vector3.Up * World.TileHeight;

		var uv0 = new Vector2( 0, 0 );
		var uv1 = new Vector2( 1, 0 );
		var uv2 = new Vector2( 0, 1 );
		var uv3 = new Vector2( 1, 1 );

		Vector3 v0 = topleft;
		Vector3 v1 = topright;
		Vector3 v2 = left;
		Vector3 v3 = right;


		ComputeTriangleNormalAndTangent( out var outnormal, out var outtangent, v0, v1, v2, uv0, uv1, uv2 );
		Vector3 Axis;
		//Get Closest Axis x forward y right z up
		if ( Math.Abs( outnormal.x ) > Math.Abs( outnormal.y ) && Math.Abs( outnormal.x ) > Math.Abs( outnormal.z ) )
			Axis = Vector3.Right;
		else if ( Math.Abs( outnormal.y ) > Math.Abs( outnormal.z ) )
			Axis = Vector3.Forward;
		else
			Axis = Vector3.Up;

		Vector4 uvcoords = GetUVCoords( Axis );
		MapVertex[] arr = new MapVertex[]{
					new MapVertex(v0,outnormal, outtangent,uv0,Blends,uvcoords),
					new MapVertex(v1,outnormal, outtangent,uv1,Blends,uvcoords),
					new MapVertex(v2,outnormal, outtangent,uv2,Blends,uvcoords),
					new MapVertex(v3,outnormal, outtangent,uv3,Blends,uvcoords),
				};
		vertices.AddRange( arr );
		int i = vertices.Count;
		indices.AddRange( new List<int>(){
				i - 4,i - 3,i - 1,
				i - 4,i - 1,i - 2
		} );


	}

	protected static Vector4 ComputeTangentForFace( Vector3 faceS, Vector3 faceT, Vector3 normal )
	{
		var leftHanded = Vector3.Dot( Vector3.Cross( faceS, faceT ), normal ) < 0.0f;
		var tangent = Vector4.Zero;

		if ( !leftHanded )
		{
			faceT = Vector3.Cross( normal, faceS );
			faceS = Vector3.Cross( faceT, normal );
			faceS = faceS.Normal;

			tangent.x = faceS[0];
			tangent.y = faceS[1];
			tangent.z = faceS[2];
			tangent.w = 1.0f;
		}
		else
		{
			faceT = Vector3.Cross( faceS, normal );
			faceS = Vector3.Cross( normal, faceT );
			faceS = faceS.Normal;

			tangent.x = faceS[0];
			tangent.y = faceS[1];
			tangent.z = faceS[2];
			tangent.w = -1.0f;
		}

		return tangent;
	}

	protected static Vector3 ComputeTriangleNormal( Vector3 v1, Vector3 v2, Vector3 v3 )
	{
		var e1 = v2 - v1;
		var e2 = v3 - v1;

		return Vector3.Cross( e1, e2 ).Normal;
	}

	protected static void ComputeTriangleTangentSpace( Vector3 p0, Vector3 p1, Vector3 p2, Vector2 t0, Vector2 t1, Vector2 t2, out Vector3 s, out Vector3 t )
	{
		const float epsilon = 1e-12f;

		s = Vector3.Zero;
		t = Vector3.Zero;

		var edge0 = new Vector3( p1.x - p0.x, t1.x - t0.x, t1.y - t0.y );
		var edge1 = new Vector3( p2.x - p0.x, t2.x - t0.x, t2.y - t0.y );

		var cross = Vector3.Cross( edge0, edge1 );

		if ( MathF.Abs( cross.x ) > epsilon )
		{
			s.x += -cross.y / cross.x;
			t.x += -cross.z / cross.x;
		}

		edge0 = new Vector3( p1.y - p0.y, t1.x - t0.x, t1.y - t0.y );
		edge1 = new Vector3( p2.y - p0.y, t2.x - t0.x, t2.y - t0.y );

		cross = Vector3.Cross( edge0, edge1 );

		if ( MathF.Abs( cross.x ) > epsilon )
		{
			s.y += -cross.y / cross.x;
			t.y += -cross.z / cross.x;
		}

		edge0 = new Vector3( p1.z - p0.z, t1.x - t0.x, t1.y - t0.y );
		edge1 = new Vector3( p2.z - p0.z, t2.x - t0.x, t2.y - t0.y );

		cross = Vector3.Cross( edge0, edge1 );

		if ( MathF.Abs( cross.x ) > epsilon )
		{
			s.z += -cross.y / cross.x;
			t.z += -cross.z / cross.x;
		}

		s = s.Normal;
		t = t.Normal;
	}

	protected static void ComputeTriangleNormalAndTangent( out Vector3 outNormal, out Vector4 outTangent, Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2 )
	{
		outNormal = ComputeTriangleNormal( v0, v1, v2 );
		ComputeTriangleTangentSpace( v0, v1, v2, uv0, uv1, uv2, out var faceS, out var faceT );
		outTangent = ComputeTangentForFace( faceS, faceT, outNormal );
	}
}
