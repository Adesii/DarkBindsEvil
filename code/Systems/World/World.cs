using System.IO;
using System.IO.Compression;
using static FastNoiseLite;

namespace DarkBinds.Systems.World;

public partial class World : Entity
{
	public static int WorldSize = 16;

	public static World Instance;

	public World()
	{

	}
	public Dictionary<Vector2Int, MapChunk> Tiles { get; set; } = new();

	public static Texture WorldMapTexture { get; private set; }

	private bool WorldNeedsUpdate = false;
	public bool ChunksFullyReceived = false;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		if ( Debug.Enabled )
			WorldMapTexture = Texture.Create( WorldSize * MapChunk.ChunkSize, WorldSize * MapChunk.ChunkSize ).WithDynamicUsage().Finish();
		Instance = this;
	}
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Instance = this;
	}


	[Event.Tick.Server]
	public void Tick()
	{
		if ( Tiles.Count == 0 )
		{
			//Generate();
			//WorldNeedsUpdate = true;
		}
		else if ( WorldNeedsUpdate )
		{
			//DebugMap();
		}
	}
	[ServerCmd]
	public static void RegenerateWorld()
	{
		Instance.Generate();
	}

	[Event.Hotload]
	public void onhotload()
	{
		ClearTiles();
		Log.Debug( "Hotloaded" );
		//RegenerateWorld();
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if ( Tiles != null && Tiles.Count > 0 )
		{

			RenderWorld();
		}
	}

	public List<MapChunk> ActiveChunks { get; set; } = new();

	public TimeSince LastCleanup = 0;

	public void RenderWorld()
	{
		var player = Local.Pawn;
		if ( player == null ) return;
		var pos = player.Position;
		var playerchunks = MapChunk.GetChunkPosition( pos );
		if ( ActiveChunks == null )
		{
			ActiveChunks = new();
		}
		for ( int x = -1; x < 2; x++ )
		{
			for ( int y = -1; y < 2; y++ )
			{
				if ( Tiles.TryGetValue( playerchunks + new Vector2Int( x, y ), out var chunk ) )
				{
					if ( !ActiveChunks.Contains( chunk ) )
						ActiveChunks.Add( chunk );

				}
			}
		}

		if ( LastCleanup > 3f )
		{
			for ( int i = 0; i < ActiveChunks.Count; i++ )
			{
				MapChunk item = ActiveChunks[i];
				if ( item.Position.Distance( playerchunks ) >= 3 && item.IsGenerated )
				{
					ActiveChunks.RemoveAt( i );
					item.Hide();
				}
			}
			LastCleanup = 0;
		}

		foreach ( var item in ActiveChunks )
		{
			item.Render();
		}

	}


	//Test Generation
	[SkipHotload]
	FastNoiseLite noise = new();
	private void Generate()
	{

		noise = new( Time.Tick );

		noise.SetNoiseType( NoiseType.OpenSimplex2 );
		noise.SetFrequency( 0.08f );
		//noise.SetFractalType( FractalType.PingPong );
		noise.SetDomainWarpType( DomainWarpType.OpenSimplex2 );
		noise.SetDomainWarpAmp( 80 );
		//noise.SetFractalPingPongStrength( 0.5f );
		ClearTiles();

		for ( int x = 0; x < WorldSize; x++ )
		{
			for ( int y = 0; y < WorldSize; y++ )
			{
				var chunk = new MapChunk
				{
					Position = new Vector2Int( x, y )
				};
				Tiles.Add( chunk.Position, chunk );

				for ( int i = 0; i < MapChunk.ChunkSize; i++ )
				{
					for ( int j = 0; j < MapChunk.ChunkSize; j++ )
					{
						Vector2Int WorldPosition = new Vector2Int( x * MapChunk.ChunkSize + i, y * MapChunk.ChunkSize + j );
						float nv = 0f;
						if ( WorldPosition.x <= 5 || WorldPosition.y <= 5 || WorldPosition.x >= (WorldSize * MapChunk.ChunkSize) - 5 || WorldPosition.y >= (WorldSize * MapChunk.ChunkSize) - 5 )
							nv = 1;
						else
						{
							float xy = WorldPosition.x;
							float yy = WorldPosition.y;
							noise.DomainWarp( ref xy, ref yy );
							nv = noise.GetNoise( xy, yy ) * 0.5f + 0.5f;
						}
						var tile = new MapTile();
						tile.Position = new Vector2Int( WorldPosition.x, WorldPosition.y );
						tile.BlockIndex = (int)(nv > 0.4f ? 255 : 0);
						if ( nv == 1 )
							tile.BlockIndex = 150;
						chunk.Tiles.Add( WorldPosition, tile );

					}
				}
			}
		}
		//SmoothChunks();
		WorldNeedsUpdate = true;


		SendchunksChunked( 8 );


	}

	private void ClearTiles()
	{
		foreach ( var item in Tiles.Values )
		{
			item.Cleanup();
			Event.Unregister( item );
		}
		Tiles = new();
		ActiveChunks = new();
	}

	public async void SendchunksChunked( int amountofChunks )
	{
		int amountofChunksToSend = Tiles.Count;
		MemoryStream output = new MemoryStream();
		using ( var dstream = new DeflateStream( output, CompressionMode.Compress ) )
		{
			var writer = new BinaryWriter( dstream );

			Log.Debug( $"Sending {amountofChunksToSend} chunks" );
			writer.Write( amountofChunksToSend );
			foreach ( var item in Tiles.Values )
			{
				item.SerializeToCompressed( ref writer );
			}
			writer.Flush();
		}

		var compressed = output.ToArray();

		Log.Debug( $"Compressed: {compressed.Length}" );
		int chunkSize = compressed.Length / amountofChunks;
		for ( int i = 0; i < amountofChunks; i++ )
		{
			byte[] chunkdata;
			if ( i == amountofChunks - 1 )
			{
				chunkdata = new byte[compressed.Length - i * chunkSize];
				Array.Copy( compressed, i * chunkSize, chunkdata, 0, chunkdata.Length );
			}
			else
			{
				chunkdata = new byte[chunkSize];
				Array.Copy( compressed, i * chunkSize, chunkdata, 0, chunkdata.Length );
			}
			SendChunks( To.Everyone, chunkdata, i, amountofChunks );
			await GameTask.Delay( 10 );
		}
	}

	[SkipHotload]
	private List<byte> CompressedChunks = new();
	[ClientRpc]
	public void SendChunks( byte[] chunk, int chunkNumber, int amountofChunks )
	{
		if ( chunkNumber == 0 )
		{
			CompressedChunks = new();
			CompressedChunks.Clear();
			ChunksFullyReceived = false;
		}
		CompressedChunks.AddRange( chunk );
		Log.Debug( "Chunk " + chunkNumber + "/" + (amountofChunks) + " received" );
		if ( chunkNumber == amountofChunks - 1 )
		{
			Log.Debug( $"Decompressing {CompressedChunks.Count} bytes" );
			using var stream = new MemoryStream( CompressedChunks.ToArray() );
			using var dstream = new DeflateStream( stream, CompressionMode.Decompress );
			var reader = new BinaryReader( dstream );
			int amount = reader.ReadInt32();
			Log.Debug( $"Amount of chunks: {amount}" );
			foreach ( var item in Tiles.Values )
			{
				item.Cleanup();
			}
			ClearTiles();
			for ( int i = 0; i < amount; i++ )
			{
				var mapchunk = new MapChunk();
				mapchunk.DeserializeFromCompressed( ref reader );
				Tiles.Add( mapchunk.Position, mapchunk );
				Log.Info( $"Chunk {mapchunk.Position} received" );
				Event.Register( mapchunk );
			}
			ChunksFullyReceived = true;
		}
	}





	private int GetSurroundCount( Vector2Int position )
	{
		int count = 0;
		for ( int x = -1; x <= 1; x++ )
		{
			for ( int y = -1; y <= 1; y++ )
			{
				if ( x == 0 && y == 0 )
					continue;
				var WorldPosition = new Vector2Int( position.x + x, position.y + y );

				MapTile tile = GetMapTile( WorldPosition );
				if ( tile != null && tile.BlockIndex == 255 )
					count++;
			}
		}

		return count;
	}

	public static MapTile GetMapTile( Vector2Int WorldPosition, bool debug = false )
	{
		var chunkpos = MapChunk.GetChunkPosition( WorldPosition );
		if ( debug )
		{
			Log.Info( $"Chunkpos: {chunkpos}" );
			Log.Info( $" WorldPosition: {WorldPosition}" );


		}
		if ( Instance.Tiles.TryGetValue( chunkpos, out var chunk ) )
		{
			var tile = chunk.GetTile( WorldPosition );
			if ( debug )
			{
				Log.Info( $"Chunk found: {chunkpos}" );
				Log.Info( $"Tile found: {tile?.Position}" );
			}
			return tile;
		}
		return null;
	}
	public static MapTile GetMapTile( Vector3 WorldPosition )
	{
		var chunkpos = MapChunk.GetChunkPosition( WorldPosition );

		if ( Instance.Tiles.TryGetValue( chunkpos, out var chunk ) )
		{
			return chunk.GetTile( WorldPosition );
		}
		return null;
	}

	private void SmoothChunks()
	{
		foreach ( var chunk in Tiles.Values )
		{
			foreach ( var tile in chunk.Tiles.Values )
			{
				int count = GetSurroundCount( tile.Position );
				if ( count > 4 )
				{
					tile.BlockIndex = 255;
				}
				else if ( count < 4 )
				{
					tile.BlockIndex = 0;
				}
			}
		}
	}

}
