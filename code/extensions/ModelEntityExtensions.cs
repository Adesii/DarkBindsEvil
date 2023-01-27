namespace DarkBinds;

public static class ModelEntityExtensions
{
	public static void SetGlow( this ModelEntity ent, bool state, Color col )
	{
		var glow = ent.Components.GetOrCreate<Glow>();
		glow.Enabled = state;
		glow.Color = col;
	}
}
