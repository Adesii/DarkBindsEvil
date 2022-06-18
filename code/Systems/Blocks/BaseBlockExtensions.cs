using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

public static class BaseBlockExtensions
{
	public static bool IsValid( this MapTile tile )
	{
		return tile != null && tile.Block != null && tile.FloorBlock != null;
	}
}
