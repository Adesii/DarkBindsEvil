using DarkBinds.Player;

namespace DarkBinds.UI.IngameHUD;

[UseTemplate]
public class MiniMap : Panel
{
	public Label coords { get; set; }
	public override void Tick()
	{
		if ( coords != null )
		{
			var p = Game.LocalPawn as DarkBindCharacter;
			coords.Text = $"Coordinates: {p.PlayerPosition.x}, {p.PlayerPosition.y}";
		}
	}

}
