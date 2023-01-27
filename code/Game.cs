global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Component;
global using Editor;

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.ComponentModel;
global using System.Threading.Tasks;

using DarkBinds.Player;
using DarkBinds.UI;
using DarkBinds.Systems.Worlds;
using DarkBinds.Systems.Blocks;
using SpriteKit.Asset;

namespace DarkBinds;

public partial class DarkBindsGame : GameManager
{

	public static DarkBindsGame Instance = Current as DarkBindsGame;
	[Net] public World World { get; set; }
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
		else
		{

			foreach ( var item in AreaAsset.All )
			{
				if ( item.Value is AreaAsset a )
				{
					a.PostInGameLoad();
				}
			}
		}
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
