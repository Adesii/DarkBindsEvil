using System.Text.Json.Serialization;

namespace SpriteKit.Asset;

[Library( "sprite" ), AutoGenerate]
public class SpriteAsset : Sandbox.Asset
{

	public List<SpriteArea> SpriteAreas { get; set; } = new();
	[Skip]
	public Dictionary<string, SpriteArea> SpriteAreasByName = new();

	protected override void PostLoad()
	{
		base.PostLoad();
		Log.Info( $"Loading sprite asset {Name}" );


		foreach ( var area in SpriteAreas )
		{
			area.LoadTextures();
			SpriteAreasByName[area.Name.ToLower()] = area;
		}
	}
	protected override void PostReload()
	{
		base.PostReload();
		Log.Info( $"Reloading sprite asset {Name}" );
		foreach ( var area in SpriteAreas )
		{
			area.LoadTextures();
			SpriteAreasByName[area.Name.ToLower()] = area;
		}
		Event.Run( "spriteassets_changed", Id );
	}
}
