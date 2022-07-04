using System.Collections.Generic;
using System.Collections.Specialized;
using DarkBinds.Player;

namespace DarkBinds.Systems.Renderer;

public class PixelWorldRenderer : SceneCustomObject
{
	public static SortedDictionary<int, PixelLayer> Layers = new();

	public CameraMode PlayerCam => Local.Client.Components.Get<CameraMode>() ?? (Local.Pawn?.Components.Get<CameraMode>());

	public static Material ScreenMaterial { get; set; } = Material.Load( "materials/screen_renderer.vmat" );

	public PixelWorldRenderer( SceneWorld sceneWorld ) : base( sceneWorld )
	{
		Bounds = new( -10, 10 );
		Event.Register( this );

		ScreenMaterial = Material.Load( "materials/screen_renderer.vmat" );

	}
	[Event.Frame]
	public void UpdatePosition()
	{
		if ( PlayerCam != null )
			Position = PlayerCam.Position + PlayerCam.Rotation.Forward * 3;
	}

	public static PixelLayer GetDefaultWorld()
	{
		var layer = GetOrCreateLayer( -1, false );
		layer.Settings = new()
		{
			IsQuantized = true,
			IsFullScreen = false,
			IsPixelPerfectWithOverscan = true,
			RenderSize = new( Screen.Size / 8 ),
		};
		layer.Init();
		return layer;
	}
	public static PixelLayer GetDefaultCharacters()
	{
		var layer = GetOrCreateLayer( 0, false );
		layer.Settings = new()
		{
			IsQuantized = false,
			IsFullScreen = true,
			IsPixelPerfectWithOverscan = false
		};
		layer.Init();
		return layer;
	}
	public static PixelLayer CreateLayer( int layer, PixelLayer.LayerSettings settings )
	{
		var layerObj = GetOrCreateLayer( layer, false );
		layerObj.Settings = settings;

		layerObj.Init();
		return layerObj;
	}

	public static PixelLayer GetOrCreateLayer( int v, bool init = true )
	{
		if ( !Layers.ContainsKey( v ) )
		{
			var l = new PixelLayer();
			if ( init )
				l.Init();
			Layers.Add( v, l );
		}
		return Layers[v];
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();
		RenderAttributes attr = new();
		//Render.SetupLighting( this, attr );
		bool first = true;
		foreach ( var item in Layers )
		{
			var layer = item.Value;
			if ( first )
				layer.Scene.ClearColor = Color.Red;
			layer.RenderPosition = PlayerCam.Position;
			layer.RenderRotation = PlayerCam.Rotation;
			layer.FOV = PlayerCam.FieldOfView;
			layer.Attributes = attr;
			layer.RenderLayer();
			first = false;
		}
	}
}
