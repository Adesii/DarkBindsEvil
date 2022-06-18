namespace DarkBinds;

public static class Debug
{
	public static int Level { get; set; } = 0;

	// @TODO: revert when ConVar.Replicated is fixed
	public static bool Enabled => false;
}

public static class LoggerExtension
{
	public static void Debug( this Logger log, object obj )
	{
		if ( !DarkBinds.Debug.Enabled )
			return;

		log.Info( $"[{(Host.IsClient ? "CL" : "SV")}] {obj}" );
	}

	public static void Debug( this Logger log, object obj, int level )
	{
		if ( DarkBinds.Debug.Level < level )
			return;

		log.Info( $"[{(Host.IsClient ? "CL" : "SV")}] {obj}" );
	}
}
