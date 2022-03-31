using DarkBinds.Systems.Worlds;
using DarkBinds.UI.Loading;

namespace DarkBinds.UI;

[UseTemplate]
public class MainMenu : Panel
{

	public void NewGame()
	{
		SetClass( "hidden", true );
		LoadingScreen.Instance.SetClass( "hidden", false );
		World.RegenerateWorld();
	}
}
