using DarkBinds.Player;

namespace DarkBinds.UI;

public partial class DarkBindsHud : Sandbox.HudEntity<RootPanel>
{
	public static DarkBindsHud Instance;

	public Panel MainPanel;
	public DarkBindsHud() : base()
	{
		Instance = this;
		if ( !Game.IsClient ) return;
		RootPanel.SetTemplate( "/UI/Hud.html" );
	}

	public static void SetNewMainPanel( Panel panel )
	{
		Instance.RootPanel.DeleteChildren( true );
		Instance.MainPanel = panel;
		Instance.MainPanel?.SetClass( "mainpanel", true );
		Instance.RootPanel.AddChild( panel );
	}

}
