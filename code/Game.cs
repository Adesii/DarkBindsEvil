global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Component;
global using Editor;

global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;
global using System.Threading.Tasks;
global using Pixel;
using DarkBinds.Player;
using DarkBinds.Systems.Blocks;
using SpriteKit.Asset;
using DarkBinds.Systems.Worlds;
using DarkBinds.UI;

namespace DarkBinds;

public partial class DarkBindsGame : GameManager
{

	public static DarkBindsGame Instance = Current as DarkBindsGame;
	[Net] public World World { get; set; }

	public PixelRenderer WorldRenderer { get; set; }
	public DarkBindsGame()
	{
		Log.Debug( "Game created" );
		if ( Game.IsServer )
		{
			_ = new DarkBindsHud();
			World = new World();
			var idk = new EnvironmentLightEntity()
			{
				Brightness = 0,
				Rotation = Vector3.Forward.EulerAngles.ToRotation()
			};

		}
		if ( Game.IsClient )
		{
			WorldRenderer = new PixelRenderer();

			foreach ( var item in AreaAsset.All )
			{
				if ( item.Value is AreaAsset a )
				{
					a.PostInGameLoad();
				}
			}
		}
	}

	public override void DoPlayerDevCam( IClient client )
	{
	}



	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var player = new DarkBindCharacter();
		client.Pawn = player;

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			player.Transform = tx;
		}
	}

}

[SceneCamera.AutomaticRenderHook]
public class camrenderhook : RenderHook
{

	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.AfterOpaque )
		{
			var targetcam = target;
			var cam = new SceneCamera()
			{
				FieldOfView = targetcam.FieldOfView,
				Rotation = targetcam.Rotation,
				Position = targetcam.Position,
				AntiAliasing = targetcam.AntiAliasing,
				AmbientLightColor = targetcam.AmbientLightColor,
				ZFar = targetcam.ZFar,
				ZNear = targetcam.ZNear,
				BackgroundColor = targetcam.BackgroundColor,
				EnablePostProcessing = targetcam.EnablePostProcessing,
				Ortho = targetcam.Ortho,
				OrthoHeight = targetcam.OrthoHeight,
				OrthoWidth = targetcam.OrthoWidth
			};
			PixelLayer.target = cam;
			PixelRenderer.Instance.RenderSceneObject();
		}
	}
}
