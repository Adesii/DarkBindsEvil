using System.Collections.Concurrent;
using DarkBinds.Systems.Blocks;
using static FastNoiseLite;

namespace DarkBinds.Systems.Worlds;

public partial class World : Entity
{

	[ConCmd.Server]
	public static void RegenerateWorld()
	{
		Instance.Generate();
	}
	ConcurrentDictionary<Vector2Int, MapChunk> ConcurrentTiles = new();



	//Test Generation
	[SkipHotload]
	FastNoiseLite noise = new();
	private async void Generate()
	{
		noise = new( Time.Tick );

		noise.SetNoiseType( NoiseType.OpenSimplex2 );
		noise.SetFrequency( 0.08f );
		//noise.SetFractalType( FractalType.PingPong );
		noise.SetDomainWarpType( DomainWarpType.OpenSimplex2 );
		noise.SetDomainWarpAmp( 80 );
		//noise.SetFractalPingPongStrength( 0.5f );

		List<Task> tasks = new();

		ConcurrentTiles.Clear();


		ClearTiles();
		{
			for ( int x = 0; x < WorldSize; x++ )
			{
				//await GameTask.MainThread();
				for ( int y = 0; y < WorldSize; y++ )
				{
					int cx, cy;
					cx = x;
					cy = y;
					tasks.Add( GameTask.RunInThreadAsync( () => GenerateTile( cx, cy ) ) );
				}
			}
		}
		//await GameTask.WorkerThread();
		await GameTask.WhenAll( tasks );
		await GameTask.MainThread();
		Tiles = new();
		foreach ( var item in ConcurrentTiles )
		{
			Tiles.Add( item.Key, item.Value );
		}
		WorldNeedsUpdate = true;
		SendchunksChunked( 32 );
	}

	async Task GenerateTile( int x, int y )
	{
		await GameTask.WorkerThread();
		var chunk = new MapChunk
		{
			Position = new Vector2Int( x, y )
		};
		ConcurrentTiles.TryAdd( chunk.Position, chunk );

		for ( int i = 0; i < ChunkSize; i++ )
		{
			for ( int j = 0; j < World.ChunkSize; j++ )
			{
				Vector2Int WorldPosition = new Vector2Int( x * World.ChunkSize + i, y * World.ChunkSize + j );
				float nv = 0f;
				if ( WorldPosition.x <= 5 || WorldPosition.y <= 5 || WorldPosition.x >= WorldSize * ChunkSize - 5 || WorldPosition.y >= WorldSize * ChunkSize - 5 )
					nv = 1;
				else
				{
					float xy = WorldPosition.x;
					float yy = WorldPosition.y;
					noise.DomainWarp( ref xy, ref yy );
					nv = noise.GetNoise( xy, yy ) * 0.5f + 0.5f;
				}
				var tile = new MapTile
				{
					Position = new Vector2Int( WorldPosition.x, WorldPosition.y ),
					ParentChunk = chunk
				};
				tile.Block = nv > 0.4f ? BaseBlock.Create( "Dirt" ) : BaseBlock.Create( "Air" );
				tile.FloorBlock = nv > 0.2f ? BaseBlock.Create( "Dirt" ) : BaseBlock.Create( "Air" );
				if ( nv == 1 )
				{
					tile.Block = BaseBlock.Create( "Stone" );
					tile.FloorBlock = BaseBlock.Create( "Air" );
				}
				chunk.Tiles.Add( WorldPosition, tile );
			}
		}
	}
}
