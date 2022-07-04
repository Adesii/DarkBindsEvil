using System.IO;
using System.IO.Compression;
using DarkBinds.Systems.Renderer;
using static FastNoiseLite;

namespace DarkBinds.Systems.Worlds;

public partial class World : Entity
{

	public static World Instance;

	[SkipHotload]
	public Dictionary<Vector2Int, MapChunk> Tiles = new();

	public static Texture WorldMapTexture { get; private set; }

	private bool WorldNeedsUpdate = false;
	public bool ChunksFullyReceived = false;



	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		if ( Debug.Enabled )
			WorldMapTexture = Texture.Create( WorldSize * World.ChunkSize, WorldSize * World.ChunkSize ).WithDynamicUsage().Finish();
		Instance = this;
	}
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Instance = this;

		World.RenderLayer = PixelWorldRenderer.GetDefaultWorld().Scene;
	}


	[Event.Hotload]
	public void onhotload()
	{
		var startTimer = DateTime.Now;
		if ( World.RenderLayer.IsValid() )
			foreach ( var item in World.RenderLayer.SceneObjects )
			{
				if ( item is MapSceneObject )
				{
					item.Delete();
				}
			}
		Tiles = new();
		ClearTiles();

		//World.RenderLayer = PixelWorldRenderer.GetDefaultWorld().Scene;


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
		var playerchunks = World.GetChunkIndex( pos );
		if ( ActiveChunks == null )
		{
			ActiveChunks = new();
		}
		for ( int x = -2; x < 2; x++ )
		{
			for ( int y = -2; y < 2; y++ )
			{
				if ( Tiles.TryGetValue( playerchunks + new Vector2Int( x, y ), out var chunk ) )
				{
					if ( !ActiveChunks.Contains( chunk ) )
						ActiveChunks.Add( chunk );

				}
			}
		}

		if ( LastCleanup > 1f )
		{
			for ( int i = 0; i < ActiveChunks.Count; i++ )
			{
				MapChunk item = ActiveChunks[i];
				if ( item.Position.Distance( playerchunks ).FloorToInt() >= 4 && item.IsGenerated )
				{
					ActiveChunks.RemoveAt( i );
					item.Hide();
				}
			}
			LastCleanup = 0;
		}

		foreach ( var item in Tiles.Values )
		{
			item.Render();
		}

	}
	[ConCmd.Server]
	public static void SaveWorld( string filename )
	{
		Host.AssertServer();
		var startTimer = DateTime.Now;
		var data = new MemoryStream();
		var writer = new BinaryWriter( data );
		writer.Write( Instance.Tiles.Count );
		foreach ( var item in Instance.Tiles )
		{
			item.Value.SerializeToCompressed( ref writer );
		}
		writer.Flush();
		var compressed = new MemoryStream();
		using ( var gzip = new GZipStream( compressed, CompressionMode.Compress, false ) )
		{
			data.Position = 0;
			data.CopyTo( gzip );
		}
		var compressedData = compressed.ToArray();
		var compressedDataString = Convert.ToBase64String( compressedData );
		var endTimer = DateTime.Now;
		var time = endTimer - startTimer;
		Log.Info( $"World saved in {time.TotalMilliseconds}ms" );
		FileSystem.Data.WriteAllText( "Saves/" + filename + ".m_save", compressedDataString );
	}
	[ConCmd.Server]
	public static void LoadWorld( string filename )
	{
		Host.AssertServer();
		var startTimer = DateTime.Now;
		var compressedData = Convert.FromBase64String( FileSystem.Data.ReadAllText( "Saves/" + filename ) );
		var compressed = new MemoryStream( compressedData );
		using ( var gzip = new GZipStream( compressed, CompressionMode.Decompress, false ) )
		{
			var reader = new BinaryReader( gzip );
			var count = reader.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				var chunk = new MapChunk();
				chunk.DeserializeFromCompressed( ref reader );
				Instance.Tiles.Add( chunk.Position, chunk );
			}
		}
		var endTimer = DateTime.Now;
		var time = endTimer - startTimer;
		Log.Info( $"World loaded in {time.TotalMilliseconds}ms" );
		World.Instance.WorldNeedsUpdate = true;
		World.Instance.SendchunksChunked( 32 );
	}




	private void ClearTiles()
	{
		if ( Tiles != null )
			foreach ( var item in Tiles.Values )
			{
				item.Delete();
				Event.Unregister( item );
			}
		Tiles = new();
		ActiveChunks = new();
	}

}
