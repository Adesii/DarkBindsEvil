using System.Collections.Generic;
using System.Collections.Specialized;
using DarkBinds.Player;

namespace DarkBinds.Systems.Renderer;

public class PixelWorldRenderer : SceneCustomObject
{
	public static Dictionary<int, PixelLayer> Layers = new();

	public CameraMode PlayerCam => Local.Client.Components.Get<CameraMode>() ?? (Local.Pawn?.Components.Get<CameraMode>());

	public static Material ScreenMaterial { get; set; } = Material.Load( "materials/screen_renderer.vmat" );

	public PixelWorldRenderer( SceneWorld sceneWorld ) : base( sceneWorld )
	{
		Bounds = new( -10, 10 );
		Event.Register( this );

		ScreenMaterial = Material.Load( "materials/screen_renderer.vmat" );
		Layers = new();
	}
	[Event.Frame]
	public void UpdatePosition()
	{
		if ( PlayerCam != null )
			Position = PlayerCam.Position + PlayerCam.Rotation.Forward * 3;
	}

	public static SceneWorld GetDefaultWorld()
	{
		var layer = GetOrCreateLayer( 1 );
		layer.Settings = new()
		{
			IsQuantized = true,
			IsFullScreen = false,
			IsPixelPerfectWithOverscan = true,
			ScaleFactor = 8
		};
		layer.Init();
		Layers[1] = layer;
		return layer.Scene;
	}
	public static SceneWorld GetDefaultCharacters()
	{
		var layer = GetOrCreateLayer( 2 );
		layer.Settings = new()
		{
			IsQuantized = false,
			IsFullScreen = true,
			IsPixelPerfectWithOverscan = false
		};
		layer.Init();
		Layers[2] = layer;
		return layer.Scene;
	}
	public static PixelLayer CreateLayer( int layer, PixelLayer.LayerSettings settings )
	{
		var layerObj = GetOrCreateLayer( layer );
		layerObj.Settings = settings;
		return layerObj;
	}

	public static PixelLayer GetOrCreateLayer( int v )
	{
		if ( !Layers.ContainsKey( v ) )
		{
			var l = new PixelLayer();
			Layers[v] = l;
		}
		return Layers[v];
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();
		//Render.SetupLighting( this, attr );
		foreach ( var item in Layers.OrderBy( x => x.Key ) )
		{
			var layer = item.Value;
			if ( !layer.IsInit ) continue;
			Render.Clear( Color.Transparent );
			layer.RenderPosition = PlayerCam.Position;
			layer.RenderRotation = PlayerCam.Rotation;
			layer.FOV = PlayerCam.FieldOfView;
			layer.RenderLayer();

		}
	}
}
