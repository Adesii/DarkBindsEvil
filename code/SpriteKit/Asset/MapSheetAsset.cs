using System.Text.Json.Serialization;

namespace SpriteKit.Asset;

[Library( "m_sprite" ), AutoGenerate]
public class MapSheetAsset : AreaAsset<MapSheetArea>
{
	public static Dictionary<string, MapSheetArea> BlockList = new();
	protected override void PostLoad()
	{
		if ( BlockList == null )
		{
			BlockList = new();
		}

		Log.Info( $"Loading Map asset {Name}" );
		foreach ( var area in SpriteAreas )
		{
			area.LoadTextures();
			BlockList[area.Name.ToLower()] = area;
		}
	}
	protected override void PostReload()
	{
		if ( BlockList == null )
		{
			BlockList = new();
		}

		Log.Info( $"Reloading Map asset {Name}" );
		foreach ( var area in SpriteAreas )
		{
			area.LoadTextures();
			BlockList[area.Name.ToLower()] = area;
		}
	}
	public static MapSheetArea GetBlockArea( string name )
	{
		return BlockList.GetValueOrDefault( name.ToLower() );
	}
}
