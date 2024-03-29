using System.IO;
using System.IO.Compression;

namespace DarkBinds.Systems.Worlds;

public partial class World : Entity
{
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

		var compressedArr = output.ToArray();
		var compressed = compressedArr.ToList().Chunk( compressedArr.Length / amountofChunks );
		int amount = compressed.Count();
		int chunk = 0;
		foreach ( var item in compressed )
		{
			SendChunks( To.Everyone, item, chunk, amount );
			chunk++;
			await GameTask.Delay( 10 );
		}


		//var compressed = output.ToArray();

		/* Log.Debug( $"Compressed: {compressed.Length}" );
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
			await GameTask.RunInThreadAsync( () =>
			{
				ThreadedSending( chunkdata, i, amountofChunks );
			} );
			//SendChunks( To.Everyone, chunkdata, i, amountofChunks );
			await GameTask.Delay( 5 );
		} */
	}

	private void ThreadedSending( byte[] data, int chunk, int amountofChunks )
	{
		SendChunks( To.Everyone, data, chunk, amountofChunks );
	}

	[SkipHotload]
	private List<byte> CompressedChunks = new();


	public static float WorldLoadingProgress = 0;
	[ClientRpc]
	public void SendChunks( byte[] chunk, int chunkNumber, int amountofChunks )
	{
		if ( chunkNumber == 0 )
		{
			CompressedChunks = new();
			CompressedChunks.Clear();
			ChunksFullyReceived = false;
			WorldLoadingProgress = 0;
		}
		CompressedChunks.AddRange( chunk );
		WorldLoadingProgress = (float)chunkNumber / (amountofChunks - 1);
		Log.Debug( "Chunk " + chunkNumber + "/" + (amountofChunks) + " received" );
		Log.Info( $"Chunk " + chunkNumber + "/" + (amountofChunks) + " received" );
		if ( chunkNumber == amountofChunks - 1 )
		{

			Log.Debug( $"Decompressing {CompressedChunks.Count} bytes" );
			using var stream = new MemoryStream( CompressedChunks.ToArray() );
			using var dstream = new DeflateStream( stream, CompressionMode.Decompress );
			var reader = new BinaryReader( dstream );
			int amount = reader.ReadInt32();
			Log.Debug( $"Amount of chunks: {amount}" );
			ClearTiles();
			for ( int i = 0; i < amount; i++ )
			{
				var mapchunk = new MapChunk();
				mapchunk.DeserializeFromCompressed( ref reader );
				Tiles.Add( mapchunk.Position, mapchunk );
				//Log.Info( $"Chunk {mapchunk.Position} received" );
				//Event.Register( mapchunk );
			}
			ChunksFullyReceived = true;
		}
	}
	[Flags]
	public enum RebuildChunksAround
	{
		None = 0,
		Up = 1,
		Down = 2,
		Left = 4,
		Right = 8,
	}

	public static async void SendSingleChunk( MapChunk Chunk, int amountofChunks = 1, RebuildChunksAround rebuild = RebuildChunksAround.None )
	{
		MemoryStream output = new MemoryStream();
		using ( var dstream = new DeflateStream( output, CompressionMode.Compress ) )
		{
			var writer = new BinaryWriter( dstream );
			Chunk.SerializeToCompressed( ref writer );
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
			Instance.SendSingleChunk( To.Everyone, chunkdata, i, amountofChunks, rebuild );
			await GameTask.Delay( 5 );
		}
	}
	[SkipHotload]
	private List<byte> CompressedChunkArray = new();
	[ClientRpc]
	public void SendSingleChunk( byte[] chunk, int chunkNumber, int amountofChunks, RebuildChunksAround rebuild = RebuildChunksAround.None )
	{
		if ( chunkNumber == 0 )
		{
			CompressedChunkArray = new();
			CompressedChunkArray.Clear();
		}
		CompressedChunkArray.AddRange( chunk );
		Log.Debug( "Chunk " + chunkNumber + "/" + (amountofChunks) + " received" );
		if ( chunkNumber == amountofChunks - 1 )
		{
			Log.Debug( $"Decompressing {CompressedChunkArray.Count} bytes" );
			using var stream = new MemoryStream( CompressedChunkArray.ToArray() );
			using var dstream = new DeflateStream( stream, CompressionMode.Decompress );
			var reader = new BinaryReader( dstream );

			var mapchunk = new MapChunk();
			mapchunk.DeserializeFromCompressed( ref reader );
			Tiles[mapchunk.Position].Delete();
			Tiles[mapchunk.Position].Tiles.Clear();
			Event.Unregister( Tiles[mapchunk.Position] );

			Tiles[mapchunk.Position] = mapchunk;
			mapchunk.GenerateMesh();

			bool rebuildTop = rebuild.HasFlag( RebuildChunksAround.Up );
			bool rebuildBottom = rebuild.HasFlag( RebuildChunksAround.Down );
			bool rebuildLeft = rebuild.HasFlag( RebuildChunksAround.Left );
			bool rebuildRight = rebuild.HasFlag( RebuildChunksAround.Right );

			if ( rebuildTop && Tiles.ContainsKey( mapchunk.Position + Vector2Int.Up ) )
				Tiles[mapchunk.Position + Vector2Int.Up]?.RegenerateMesh();
			if ( rebuildBottom && Tiles.ContainsKey( mapchunk.Position + Vector2Int.Down ) )
				Tiles[mapchunk.Position + Vector2Int.Down]?.RegenerateMesh();
			if ( rebuildLeft && Tiles.ContainsKey( mapchunk.Position + Vector2Int.Left ) )
				Tiles[mapchunk.Position + Vector2Int.Left]?.RegenerateMesh();
			if ( rebuildRight && Tiles.ContainsKey( mapchunk.Position + Vector2Int.Right ) )
				Tiles[mapchunk.Position + Vector2Int.Right]?.RegenerateMesh();
			Log.Debug( $"Chunk {mapchunk.Position} received" );
			//Event.Register( mapchunk );
		}
	}


	public static void SendTile( MapTile tile )
	{
		MemoryStream output = new MemoryStream();
		using ( var dstream = new DeflateStream( output, CompressionMode.Compress ) )
		{
			var writer = new BinaryWriter( dstream );


			tile.Serialize( ref writer );

			writer.Flush();
		}

		var compressed = output.ToArray();

		Instance.SendTile( To.Everyone, compressed );

	}
	[ClientRpc]
	public void SendTile( byte[] chunk )
	{
		using var stream = new MemoryStream( chunk.ToArray() );
		using var dstream = new DeflateStream( stream, CompressionMode.Decompress );
		var reader = new BinaryReader( dstream );

		var tile = new MapTile();
		tile.Deserialize( ref reader );
		var oldtile = World.GetMapTile( tile.Position );
		tile.ParentChunk = oldtile.ParentChunk;
		tile.ParentChunk.Tiles[tile.Position] = tile;
		//tile.ParentChunk.RegenerateMesh();

		tile.BuildMesh();

		oldtile.Delete();

		var toptile = World.GetMapTile( tile.Position + Vector2Int.Up );
		var bottomtile = World.GetMapTile( tile.Position + Vector2Int.Down );
		var lefttile = World.GetMapTile( tile.Position + Vector2Int.Left );
		var righttile = World.GetMapTile( tile.Position + Vector2Int.Right );

		var toprighttile = World.GetMapTile( tile.Position + Vector2Int.Up + Vector2Int.Right );
		var toplefttile = World.GetMapTile( tile.Position + Vector2Int.Up + Vector2Int.Left );
		var bottomrighttile = World.GetMapTile( tile.Position + Vector2Int.Down + Vector2Int.Right );
		var bottomlefttile = World.GetMapTile( tile.Position + Vector2Int.Down + Vector2Int.Left );

		if ( toptile != null )
			toptile.BuildMesh();
		if ( bottomtile != null )
			bottomtile.BuildMesh();
		if ( lefttile != null )
			lefttile.BuildMesh();
		if ( righttile != null )
			righttile.BuildMesh();

		if ( toprighttile != null )
			toprighttile.BuildMesh();
		if ( toplefttile != null )
			toplefttile.BuildMesh();
		if ( bottomrighttile != null )
			bottomrighttile.BuildMesh();
		if ( bottomlefttile != null )
			bottomlefttile.BuildMesh();

		Event.Run( "tile.changed", tile );
		Log.Debug( $"Tile {tile.Position} received of type {tile.Block.Name}" );
	}
}
