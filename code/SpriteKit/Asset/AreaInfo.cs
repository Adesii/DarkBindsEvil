namespace SpriteKit.Asset;

public class AreaInfo
{
	public string Name { get; set; }
	public Rect Area { get; set; }
	public float XSubdivisions { get; set; }
	public float YSubdivisions { get; set; }
	public Color AreaColor { get; set; } = Color.Random;
	public float FrameRate { get; set; } = 12f;

	[ResourceType( "img" )]
	public string SpriteSheetPath { get; set; }

	[ResourceType( "img" )]
	public string SpriteSheetNormalPath { get; set; }

	[Skip]
	public Texture SpriteSheetTexture { get; set; }
	[Skip]
	public Texture SpriteSheetNormalTexture { get; set; }

	public virtual void LoadTextures()
	{

		SpriteSheetTexture = Sandbox.TextureLoader.Image.Load( FileSystem.Mounted, SpriteSheetPath, true );
		if ( !string.IsNullOrEmpty( SpriteSheetNormalPath ) )
			SpriteSheetNormalTexture = Sandbox.TextureLoader.Image.Load( FileSystem.Mounted, SpriteSheetNormalPath, false );
	}
}
