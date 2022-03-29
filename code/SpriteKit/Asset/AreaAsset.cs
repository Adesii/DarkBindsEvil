namespace SpriteKit.Asset;

public class AreaAsset<T> : Sandbox.Asset where T : AreaInfo
{
	public List<T> SpriteAreas { get; set; } = new();
	[Skip]
	public Dictionary<string, T> SpriteAreasByName = new();

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
