using Sandbox;

namespace DarkBinds.Systems.Renderer;

public class PixelLayer
{
	private LayerSettings _settings;
	public LayerSettings Settings
	{
		get => _settings;
		set
		{
			_settings = value;
			Init();
		}
	}

	public RenderAttributes Attributes { get; set; }
	public Vector3 RenderOffsetPosition { get; set; }

	public SceneWorld Scene { get; set; }
	public bool IsInit = false;

	//public PixelTextures PixelTextures { get; set; }

	public Texture ColRender;
	public Texture DepRender;



	public Vector3 RenderPosition { get; set; }
	public Rotation RenderRotation { get; set; }
	public float FOV { get; set; }

	public static Vector2 ReferenceSize = new( 1920, 1080 );

	public bool canRender = false;
	internal int RenderOrder = 0;

	public string LayerGUID { get; set; }

	~PixelLayer()
	{
		if ( Scene != null )
			Scene.Delete();
	}
	private void Init()
	{
		Scene = new();
		LayerGUID = Guid.NewGuid().ToString();
		ViewChanged();
		Attributes = new();
		Event.Register( this );

		IsInit = true;
	}

	[Event.Screen.SizeChanged]
	public void ViewChanged()
	{
		ColRender?.Dispose();
		DepRender?.Dispose();
		//await GameTask.DelayRealtime( 100 );
		if ( Settings.IsFullScreen )
		{
			Settings.RenderSize = ReferenceSize;
		}
		else
		{
			Settings.RenderSize = ReferenceSize / Settings.ScaleFactor;
		}
		if ( Settings.RenderSize.Length <= 2 ) return;
		//PixelTextures = new( Settings.RenderSize, false );

		ColRender = Texture.CreateRenderTarget().WithSize( Settings.RenderSize ).WithScreenFormat().Create();
		DepRender = Texture.CreateRenderTarget().WithSize( Settings.RenderSize ).WithDepthFormat().Create();
		canRender = true;

	}

	public void RenderLayer()
	{
		if ( PixelWorldRenderer.ScreenMaterial == null || ColRender == null || DepRender == null || !canRender || !Scene.IsValid() )
		{
			return;
		}
		Rect renderrect = new( 0, Settings.RenderSize );
		Render.Draw.DrawScene( ColRender, DepRender, Scene, new(), renderrect, RenderPosition - RenderRotation.Forward, RenderRotation, FOV );
		Render.Draw2D.Material = PixelWorldRenderer.ScreenMaterial;
		Render.Draw2D.Texture = ColRender;
		Rect rect = new( 0, Screen.Size );
		Render.Draw2D.Quad( rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft );
	}

	public class LayerSettings
	{

		public bool IsQuantized { get; set; }
		public bool IsFullScreen { get; set; }
		public Vector2 RenderSize { get; set; }
		public int ScaleFactor { get; set; }
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
