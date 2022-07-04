namespace DarkBinds.Systems.Renderer;

public class PixelTextures : IDisposable
{
	public bool HasScratchTextures { get; private set; }

	public Vector2 Size { get; set; }

	public Texture Color { get; set; }

	public Texture Depth { get; set; }

	public Texture Glow { get; private set; }

	public Texture ScreenSpaceTranslucent { get; private set; }

	public Texture IntermediateOutputColorResolved { get; private set; }

	public PixelTextures( Vector2 size, bool isHDRBuffer = true, bool hasMultiSample = true, bool allocateScratchTextures = true )
	{
		Size = size;
		TextureBuilder textureBuilder = Texture.CreateRenderTarget().WithSize( Size ).WithFormat( isHDRBuffer ? ImageFormat.RGBA16161616F : ImageFormat.RGBA8888 );
		if ( hasMultiSample )
		{
			textureBuilder = textureBuilder.WithScreenMultiSample();
		}
		Color = textureBuilder.Create();
		textureBuilder = Texture.CreateRenderTarget().WithSize( Size ).WithDepthFormat();
		if ( hasMultiSample )
		{
			textureBuilder = textureBuilder.WithScreenMultiSample();
		}
		Depth = textureBuilder.Create();
		HasScratchTextures = allocateScratchTextures;
		if ( HasScratchTextures )
		{
			Glow = Texture.CreateRenderTarget().WithSize( Size ).WithFormat( ImageFormat.RGBA16161616F )
				.WithUAVBinding()
				.Create();
			ScreenSpaceTranslucent = Texture.CreateRenderTarget().WithSize( Size ).WithFormat( ImageFormat.RGBA16161616F )
				.Create();
			IntermediateOutputColorResolved = Texture.CreateRenderTarget().WithSize( Size ).WithFormat( ImageFormat.RGBA16161616F )
				.WithUAVBinding()
				.Create();
		}
	}

	public void WriteScratchRenderTargets( RenderAttributes attributes )
	{
		if ( HasScratchTextures )
		{
			string k = "glowColorTex";
			Texture value = Glow;
			int mip = -1;
			attributes.Set( in k, in value, in mip );
			k = "ScreenSpaceTranslucentTex";
			value = ScreenSpaceTranslucent;
			mip = -1;
			attributes.Set( in k, in value, in mip );
			k = "IntermediateOutputColorResolvedTex";
			value = IntermediateOutputColorResolved;
			mip = -1;
			attributes.Set( in k, in value, in mip );
		}
	}

	~PixelTextures()
	{
		Dispose();
	}

	public void Dispose()
	{
		Color?.Dispose();
		Depth?.Dispose();
		Glow?.Dispose();
		ScreenSpaceTranslucent?.Dispose();
		IntermediateOutputColorResolved?.Dispose();
		Color = null;
		Depth = null;
		Glow = null;
		ScreenSpaceTranslucent = null;
		IntermediateOutputColorResolved = null;
	}
}
