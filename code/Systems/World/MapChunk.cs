using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DarkBinds.Systems.Renderer;
using Sandbox;
using Sandbox.Internal;
using SpriteKit.Asset;

namespace DarkBinds.Systems.Worlds;
[SkipHotload]

public partial class MapChunk
{
	public Dictionary<MapSheetArea, (List<MapVertex>, List<int>)> MaterialList = new();
	public static Material DefaultMaterial => Material.Load( "materials/maptest.vmat" );

	private Vector2Int _pos;
	public Vector2Int Position
	{
		get => _pos; set
		{
			_pos = value;
			WorldPosition = (new Vector3( Position.x * World.ViewSize, Position.y * World.ViewSize, 0 ) - (World.WorldSize * World.ViewSize / 2)).WithZ( 0 );
		}
	}

	public Vector3 WorldPosition { get; set; }

	public Dictionary<Vector2Int, MapTile> Tiles { get; set; } = new();
	private SceneObject GroundPlaneSO { get; set; }
	private List<SceneObject> ChunkSO { get; set; }
	public bool IsGenerated { get; set; } = false;
	public bool IsGenerating { get; set; } = false;

	public bool RenderingEnabled { get; set; } = true;

	public void GenerateMesh()
	{
		if ( !IsGenerated && !IsGenerating )
		{

			//addGroundPlane
			ClearTileSO();
			IsGenerating = true;
			ResolveMesh();
		}
		else if ( !IsGenerating )
		{
			Show();
		}
	}
	public void RegenerateMesh()
	{
		IsGenerated = false;
		GenerateMesh();
	}
	[Event.Hotload]
	private void HotReload()
	{
		RegenerateMesh();
	}
	private void ClearTileSO()
	{

		if ( GroundPlaneSO.IsValid() )
		{
			GroundPlaneSO.Delete();
		}
		if ( ChunkSO != null )
		{
			foreach ( var ch in ChunkSO )
			{
				ch.Delete();
			}
		}
		else
		{
			ChunkSO = new();
		}
		foreach ( var item in Tiles.Values )
		{
			if ( item != null )
				item.Delete();
		}
	}
	private void ResolveMesh()
	{
		GenerateChunk();
		IsGenerated = true;
		IsGenerating = false;
	}

	private void GenerateChunk()
	{

		//Mesh idk = new( spriteMapFloor );
		//idk.CreateBuffers( MakeTesselatedPlane( new Vector3( World.TileSize, World.TileSize ) / 2, 0, new Vector3( World.ViewSize, World.ViewSize, 0 ), 4, 4 ) );
		//Model groundplane = Model.Builder.AddMesh( idk ).Create();
		//GroundPlaneSO = new MapSceneObject( Map.Scene, groundplane, new Transform( WorldPosition ) );
		//GroundPlaneSO.Attributes.Set( "SpriteSheet", MapSheetAsset.GetBlockArea( "Dirt" ).SpriteSheetTexture );
		int index = 0;
		MaterialList = new();
		foreach ( var Tile in Tiles.Values )
		{
			Tile.BuildMesh();
			//if ( index % 8 == 0 )
			//	await GameTask.NextPhysicsFrame();
			index++;
		}
		//ClearTileSO();
		foreach ( var item in MaterialList )
		{
			var modelbuilder = Model.Builder;

			var vertices = item.Value.Item1;
			var indices = item.Value.Item2;

			var matcopy = MapChunk.DefaultMaterial.CreateCopy();
			matcopy.OverrideTexture( "SpriteSheet", item.Key.SpriteSheetTexture );
			matcopy.OverrideTexture( "SpriteSheetOpacityMask", item.Key.SpriteSheetAlphaTexture );


			Mesh TileMesh = new( matcopy );
			TileMesh.CreateVertexBuffer<MapVertex>( vertices.Count, MapVertex.Layout, vertices );

			TileMesh.SetVertexBufferSize( vertices.Count );
			TileMesh.SetVertexBufferData( vertices );

			TileMesh.CreateIndexBuffer( indices.Count, indices );
			TileMesh.SetIndexBufferSize( indices.Count );
			TileMesh.SetIndexBufferData( indices );
			modelbuilder.AddMesh( TileMesh );

			Model model = modelbuilder.Create();

			var chunkio = new MapSceneObject( PixelWorldRenderer.GetDefaultWorld().Scene, model, new Transform( WorldPosition ) );
			//chunkio.Attributes.Set( "sheet", item.Key.SpriteSheetTexture );
			//chunkio.Attributes.Set( "sheetalpha", item.Key.SpriteSheetAlphaTexture );
			chunkio.Bounds = new BBox( new Vector3( 0, 0, 0 ), new Vector3( World.ViewSize, World.ViewSize, 1000 ) );
			ChunkSO.Add( chunkio );
		}
		MaterialList.Clear();
		//
		//ChunkSO.Attributes.Set( "SpriteSheet", MapSheetAsset.GetBlockArea( "Dirt" ).SpriteSheetTexture );

	}



	public void DeserializeFromCompressed( ref BinaryReader reader )
	{
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		Position = new Vector2Int( x, y );
		int count = reader.ReadInt32();
		Log.Debug( $"Deserializing {count} tiles", 100 );

		Tiles = new Dictionary<Vector2Int, MapTile>();

		for ( int i = 0; i < count; i++ )
		{
			var tile = new MapTile();
			tile.Deserialize( ref reader );
			tile.ParentChunk = this;
			Tiles.Add( tile.Position, tile );
		}
	}
	public void SerializeToCompressed( ref BinaryWriter writer )
	{
		Log.Debug( $"Serializing {Tiles.Count} tiles", 100 );
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

		return GetTile( World.GetTilePosition( WorldPosition ) );
	}

	internal void DebugView()
	{
		foreach ( var tile in Tiles.Values )
		{
			tile.DebugView();
		}
	}



	public Vector3 GetTilePositionRelativeToChunk( Vector2Int TilePositionWorldSpace )
	{
		var TilePosition = TilePositionWorldSpace - (Position * World.ChunkSize);
		return new Vector3( TilePosition.x * World.TileSize, TilePosition.y * World.TileSize, 0 );

	}

	internal void Delete()
	{
		ClearTileSO();
		IsGenerated = false;
	}

	public void Hide()
	{
		if ( !RenderingEnabled )
			return;
		foreach ( var item in Tiles.Values )
		{
			if ( item.TileSO.IsValid() )
				item.TileSO.RenderingEnabled = false;
		}
		if ( GroundPlaneSO.IsValid() )
			GroundPlaneSO.RenderingEnabled = false;
		RenderingEnabled = false;
	}
	public void Show()
	{
		if ( RenderingEnabled )
			return;
		foreach ( var item in Tiles.Values )
		{
			if ( item.TileSO.IsValid() )
				item.TileSO.RenderingEnabled = true;
		}
		if ( GroundPlaneSO.IsValid() )
			GroundPlaneSO.RenderingEnabled = true;
		RenderingEnabled = true;
	}

	internal void Render()
	{
		//DebugView();
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

				vb.Add( new Vertex( vPos, Vector3.Up, Vector3.Right, uv ) );

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
