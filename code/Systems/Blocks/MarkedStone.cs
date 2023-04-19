using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

[Block( "MarkedStone" )]
public class MarkedStone : BaseBlock
{
	private SceneLight Light;
	public override void OnCreated()
	{
		base.OnCreated();
		Light = new SceneLight( PixelLayerAsset.GetSceneWorld( "world" ), Tile.WorldPosition + Vector3.Up * World.HalfTileSize, World.HalfTileSize * 4, ((Color.Parse( "#24799e" ) ?? Color.Blue)) * 1 )
		{
			QuadraticAttenuation = 4,
			LinearAttenuation = 0
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
