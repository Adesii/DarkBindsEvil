using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

[Block( "MarkedStone" )]
public class MarkedStone : BaseBlock
{
	private PointLightEntity Light;
	public override void OnCreated()
	{
		base.OnCreated();
		Light = new PointLightEntity
		{
			Falloff = 1,
			Range = 100f,
			Color = Color.Parse( "#24799e" ) ?? Color.Blue,
			Brightness = 0.3f,
			DynamicShadows = false,
			Position = Tile.WorldPosition + Vector3.Up * World.HalfTileSize
		};
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();
		if ( Light.IsValid() )
			Light.Delete();
	}

	public override Mesh BuildMesh( Vector2Int Position )
	{
		return base.BuildMesh( Position );
	}
}
