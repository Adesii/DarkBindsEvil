using DarkBinds.Systems.Worlds;
using DarkBinds.UI.IngameHUD;

namespace DarkBinds.UI.Loading;

[UseTemplate]
public class LoadingScreen : Panel
{
	public static LoadingScreen Instance;

	public LoadingScreen()
	{
		Instance = this;
	}

	public Panel ProgressBar { get; set; }

	public override void Tick()
	{
		base.Tick();

		ProgressBar.Style.Width = Length.Fraction( World.WorldLoadingProgress );

		if ( World.WorldLoadingProgress >= 1f )
		{
			SetClass( "hidden", true );
			if ( ComputedStyle.Opacity == 0 )
			{
				Delete();
				IngameRoot.Instance.SetClass( "hidden", false );
			}
		}
	}
}
