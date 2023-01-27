using DarkBinds.Player;

namespace DarkBinds.UI;

public partial class DarkBindsHud : Sandbox.HudEntity<GameRootPanel>
{
	public static DarkBindsHud Instance;

	public DarkBindsHud() : base()
	{
		Instance = this;
	}

}
