namespace DarkBinds.Systems.Worlds;

public partial class World
{
	public static int GetSurroundCount( Vector2Int position )
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
				if ( tile != null && tile.Block.IsSolid() )
					count++;
			}
		}

		return count;
	}

	public static MapTile GetMapTile( Vector2Int WorldPosition, bool debug = false )
	{
		var chunkpos = GetChunkIndex( WorldPosition );
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
		var chunkpos = GetChunkIndex( WorldPosition );

		if ( Instance.Tiles.TryGetValue( chunkpos, out var chunk ) )
		{
			return chunk.GetTile( WorldPosition );
		}
		return null;
	}

	public static Vector2Int GetChunkIndex( Vector3 WorldPosition )
	{

		return GetChunkIndex( GetTilePosition( WorldPosition ) );
	}
	public static Vector2Int GetChunkIndex( Vector2Int WorldPosition )
	{
		//get chunk from tile position
		return new Vector2Int(
			WorldPosition.x / ChunkSize,
			WorldPosition.y / ChunkSize
		);
	}

	public static Vector2Int GetTilePosition( Vector3 WorldPosition )
	{
		var pos = new Vector2Int(
			(int)Math.Round( (WorldPosition.x) / World.TileSize ),
			(int)Math.Round( (WorldPosition.y) / World.TileSize )
		);
		pos += (World.WorldSize * World.ChunkSize) / 2;
		return pos;
	}
	public static Vector3 GetTileWorldPosition( Vector2Int TilePosition )
	{
		return (new Vector3( TilePosition.x * World.TileSize, TilePosition.y * World.TileSize, 0 ) - (World.WorldSize * World.ViewSize / 2)).WithZ( 0 );
	}


	public static List<MapTile> GetBlocksInLine( Vector3 currentPos, Vector3 targetPos, int MaxDistance = 100 )
	{
		var blocks = new List<MapTile>();
		var direction = targetPos - currentPos;
		var distance = direction.Length;
		var steps = distance;
		var step = direction / steps;
		var currentvecPos = GetTilePosition( currentPos );
		for ( int i = 0; i < steps; i++ )
		{
			var pos = currentPos + step * i;
			var tile = GetMapTile( pos );
			if ( tile != null && !blocks.Contains( tile ) )
			{
				if ( Math.Abs( (tile.Position - currentvecPos).x ) > MaxDistance || Math.Abs( (tile.Position - currentvecPos).y ) > MaxDistance )
				{
					return blocks;
				}
				blocks.Add( tile );
			}
		}
		return blocks;
	}
}
