using System.Collections.Generic;
using System.Collections.Specialized;
using DarkBinds.Player;

namespace DarkBinds.Systems.Renderer;

public class PixelWorldRenderer : SceneCustomObject
{
	public static Dictionary<int, PixelLayer> Layers;

	public CameraMode PlayerCam => Local.Client.Components.Get<CameraMode>() ?? (Local.Pawn?.Components.Get<CameraMode>());

	public static Material ScreenMaterial { get; set; } = Material.Load( "materials/screen_renderer.vmat" );

	public PixelWorldRenderer( SceneWorld sceneWorld ) : base( sceneWorld )
	{
		Bounds = new( -10, 10 );
		Event.Register( this );

		ScreenMaterial = Material.Load( "materials/screen_renderer.vmat" );

		Flags.IsTranslucent = true;
	}
	[Event.Frame]
	public void UpdatePosition()
	{
		if ( PlayerCam != null )
			Position = PlayerCam.Position + PlayerCam.Rotation.Forward * 3;
	}

	public static SceneWorld GetDefaultWorld()
	{
		var layer = Get( -1 );
		if ( !layer.IsInit )
			layer.Settings = new()
			{
				IsQuantized = true,
				IsFullScreen = false,
				IsPixelPerfectWithOverscan = true,
				ScaleFactor = 8
			};
		return layer.Scene;
	}
	public static SceneWorld GetDefaultCharacters()
	{
		var layer = Get( 0 );
		if ( !layer.IsInit )
			layer.Settings = new()
			{
				IsQuantized = false,
				IsFullScreen = true,
				IsPixelPerfectWithOverscan = false
			};
		return layer.Scene;
	}

	public static PixelLayer Get( int v )
	{
		if ( Layers == null )
			Layers = new();
		if ( !Layers.ContainsKey( v ) )
			Layers[v] = new PixelLayer();
		return Layers[v];
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();
		//Render.SetupLighting( this, attr );
		if ( Layers != null && Layers.Count > 0 )
			foreach ( var item in Layers.OrderBy( x => x.Key ) )
			{
				var layer = item.Value;
				if ( !layer.IsInit ) continue;
				layer.RenderPosition = PlayerCam.Position;
				layer.RenderRotation = PlayerCam.Rotation;
				layer.FOV = PlayerCam.FieldOfView;
				layer.RenderLayer();

			}
	}
}
