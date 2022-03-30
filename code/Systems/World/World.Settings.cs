namespace DarkBinds.Systems.Worlds;

public partial class World
{
	public const int WorldSize = 64;
	public const int TileSize = 32;
	public const int TileHeight = 64;
	public const int ChunkSize = 8;

	public static SceneWorld Scene;


	private static int _viewsize = World.ChunkSize * World.TileSize;
	public static int ViewSize
	{
		get
		{
			return _viewsize;
		}
	}

	private static float _HalfTileSize = TileSize / 2;
	public static float HalfTileSize
	{
		get
		{
			return _HalfTileSize;
		}
	}
}
