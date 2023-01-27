using Sandbox.Diagnostics;

namespace DarkBinds;

public static class Debug
{
	[ConVar.Replicated( "DarkDebug" )]
	public static int Level { get; set; } = 0;

	public static bool Enabled => Level > 0;
}

public static class LoggerExtension
{
	public static void Debug( this Logger log, object obj )
	{
		if ( !DarkBinds.Debug.Enabled ) return;

		log.Info( $"[{(Game.IsClient ? "CL" : "SV")}] {obj}" );
	}

	public static void Debug( this Logger log, object obj, int level )
	{
		if ( !(DarkBinds.Debug.Level >= level) ) return;

		log.Info( $"[{(Game.IsClient ? "CL" : "SV")}] {obj}" );
	}
}
