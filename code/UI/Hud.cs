using DarkBinds.Player;

namespace DarkBinds.UI;

public partial class DarkBindsHud : Sandbox.HudEntity<RootPanel>
{
	public static DarkBindsHud Instance;

	public Panel MainPanel;
	public DarkBindsHud() : base()
	{
		Instance = this;
		if ( !IsClient ) return;
		RootPanel.SetTemplate( "/UI/Hud.html" );
		RootPanel.BindClass( "Dev", () =>
		{
			return Local.Client.Components.Get<DevCamera>( true )?.Enabled ?? false;
		} );
	}

	public static void SetNewMainPanel( Panel panel )
	{
		Instance.RootPanel.DeleteChildren( true );
		Instance.MainPanel = panel;
		Instance.MainPanel?.SetClass( "mainpanel", true );
		Instance.RootPanel.AddChild( panel );
	}

}
