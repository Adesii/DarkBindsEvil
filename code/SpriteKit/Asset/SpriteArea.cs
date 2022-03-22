namespace SpriteKit.Asset;

public class SpriteArea
{
	public string Name { get; set; }
	public Rect Area { get; set; }

	public int XSubdivisions { get; set; } = 1;
	public int YSubdivisions { get; set; } = 1;

	public float FrameRate { get; set; } = 12f;

	public Color AreaColor { get; set; } = Color.White;

	public Vector2 SpriteOrigin { get; set; } = new( 0.5f, 0 );

	[ResourceType( "img" )]
	public string SpriteSheetPath { get; set; }
	[ResourceType( "img" )]
	public string SpriteSheetNormalPath { get; set; }
	[Skip]
	public Texture SpriteSheetTexture { get; set; }
	[Skip]
	public Texture SpriteSheetNormalTexture { get; set; }


	public void LoadTextures()
	{

		SpriteSheetTexture = Sandbox.TextureLoader.Image.Load( FileSystem.Mounted, SpriteSheetPath, true );
		if ( !string.IsNullOrEmpty( SpriteSheetNormalPath ) )
			SpriteSheetNormalTexture = Sandbox.TextureLoader.Image.Load( FileSystem.Mounted, SpriteSheetNormalPath, false );
	}
}
