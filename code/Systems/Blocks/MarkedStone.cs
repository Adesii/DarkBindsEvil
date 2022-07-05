using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

[Block( "MarkedStone" )]
public class MarkedStone : BaseBlock
{
	private SceneLight Light;
	public override void OnCreated()
	{
		base.OnCreated();
		Light = new SceneLight( PixelRenderer.GetDefaultWorld(), Tile.WorldPosition + Vector3.Up * World.HalfTileSize, 300f, ((Color.Parse( "#24799e" ) ?? Color.Blue)) * 100 )
		{
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
