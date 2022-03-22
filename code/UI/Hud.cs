namespace DarkBinds.UI;

public partial class DarkBindsHud : HudEntity<RootPanel>
{
	public DarkBindsHud()
	{
		if ( !IsClient ) return;
		RootPanel.SetTemplate( "/UI/Hud.html" );
	}
}
