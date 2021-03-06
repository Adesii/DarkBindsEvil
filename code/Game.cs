global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Component;
global using SandboxEditor;

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.ComponentModel;
global using System.Threading.Tasks;

using DarkBinds.Player;
using DarkBinds.UI;
using DarkBinds.Systems.Worlds;
using DarkBinds.Systems.Blocks;

namespace DarkBinds;

public partial class DarkBindsGame : Game
{

	public static DarkBindsGame Instance = Current as DarkBindsGame;
	[Net] public World World { get; set; }
	public DarkBindsGame()
	{
		Log.Debug( "Game created" );
		if ( IsServer )
		{
			_ = new DarkBindsHud();
			World = new World();
			var idk = new EnvironmentLightEntity()
			{
				Brightness = 0,
				Rotation = Vector3.Forward.EulerAngles.ToRotation()
			};
		}
	}



	public override void ClientJoined( Client client )
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

	public override void DoPlayerSuicide( Client cl )
	{
		if ( cl.Pawn == null ) return;

		cl.Pawn.Kill();
	}


}
