namespace DarkBinds.UI.IngameHUD;

[UseTemplate]
public class IngameRoot : GameRootPanel
{
	public static IngameRoot Instance;

	public IngameRoot()
	{
		Instance = this;
	}
}
