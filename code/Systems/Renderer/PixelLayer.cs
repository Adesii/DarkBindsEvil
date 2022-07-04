using Sandbox;

namespace DarkBinds.Systems.Renderer;

public class PixelLayer
{
	public PixelLayer( LayerSettings Settings = null )
	{
		if ( Settings == null )
			Settings = LayerSettings.Default();
		this.Settings = Settings;
	}

	public LayerSettings Settings { get; set; }

	public RenderAttributes Attributes { get; set; }
	public Vector3 RenderOffsetPosition { get; set; }

	public SceneWorld Scene { get; set; }

	public ScenePortal Renderer { get; set; }

	public Texture FrameBuffer => Renderer.ColorTarget;
	public Texture DepthBuffer => Renderer.DepthTarget;

	public Texture RenderTexture;
	public Texture RenderDepthTexture;



	public Vector3 RenderPosition { get; set; }
	public Rotation RenderRotation { get; set; }
	public float FOV { get; set; }

	public string LayerGuidName { get; set; }

	public void Init()
	{
		Scene = new()
		{
			ClearColor = Color.Transparent,
		};
		//Renderer = new( Scene, Model.Load( "models/room.vmdl" ), new(), true, Settings.RenderSize );
		Attributes = new();
		LayerGuidName = Guid.NewGuid().ToString();

		if ( Settings.IsFullScreen )
		{

			RenderTexture = Texture.CreateRenderTarget().WithSize( Screen.Size ).WithScreenFormat().Create( LayerGuidName + "_color" );
			RenderDepthTexture = Texture.CreateRenderTarget().WithSize( Screen.Size ).WithDepthFormat().Create( LayerGuidName + "_depth" );
			Settings.RenderSize = Screen.Size;
		}
		else
		{
			RenderTexture = Texture.CreateRenderTarget().WithSize( Settings.RenderSize ).WithScreenFormat().Create( LayerGuidName + "_color" );
			RenderDepthTexture = Texture.CreateRenderTarget().WithSize( Settings.RenderSize ).WithDepthFormat().Create( LayerGuidName + "_depth" );
		}
		Event.Register( this );
	}

	[Event.Screen.SizeChanged]
	public void ViewChanged()
	{
		if ( !Settings.IsFullScreen ) return;
		RenderTexture.Dispose();
		RenderDepthTexture.Dispose();
		RenderTexture = Texture.CreateRenderTarget().WithSize( Screen.Size ).WithScreenFormat().Create( LayerGuidName + "_color" );
		RenderDepthTexture = Texture.CreateRenderTarget().WithSize( Screen.Size ).WithDepthFormat().Create( LayerGuidName + "_depth" );
		LayerGuidName = Guid.NewGuid().ToString();

		Settings.RenderSize = Screen.Size;
	}

	public void RenderLayer()
	{
		if ( PixelWorldRenderer.ScreenMaterial == null ) return;
		Rect renderrect = new( 0, Settings.RenderSize );
		Render.Draw.DrawScene( RenderTexture, RenderDepthTexture, Scene, new(), renderrect, RenderPosition - RenderRotation.Forward * 200f, RenderRotation, FOV, 0.01f, 10000 );
		Render.Draw2D.Material = PixelWorldRenderer.ScreenMaterial;
		Render.Draw2D.Texture = RenderTexture;
		Render.Draw2D.Color = Color.White;
		Rect rect = new( 0, Screen.Size );
		Render.Draw2D.Quad( rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft );
		//Render.Draw2D.Circle( new Vector2( width / 2, height / 2 ), 5f );
	}

	public class LayerSettings
	{

		public bool IsQuantized { get; set; }
		public bool IsFullScreen { get; set; }
		public Vector2 RenderSize { get; set; }
		public bool IsPixelPerfectWithOverscan { get; set; }

		public static LayerSettings Default()
		{
			return new LayerSettings()
			{
				IsQuantized = true,
				RenderSize = Screen.Size / 4,
				IsPixelPerfectWithOverscan = true
			};
		}
	}
}
