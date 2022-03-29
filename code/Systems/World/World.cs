using System.IO;
using System.IO.Compression;
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


	[Event.Hotload]
	public void onhotload()
	{
		var startTimer = DateTime.Now;
		foreach ( var item in Map.Scene.SceneObjects )
		{
			if ( item is MapSceneObject )
			{
				item.Delete();
			}
		}
		Tiles = new();
		ClearTiles();
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

		foreach ( var item in ActiveChunks )
		{
			item.Render();
		}

	}



	private void ClearTiles()
	{
		foreach ( var item in Tiles.Values )
		{
			item.Delete();
			Event.Unregister( item );
		}
		Tiles = new();
		ActiveChunks = new();
	}

}
