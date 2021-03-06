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
	float backgroundscroll;

	public override void Tick()
	{
		base.Tick();

		ProgressBar.Style.Width = Length.Fraction( World.WorldLoadingProgress );
		backgroundscroll += RealTime.Delta * 64;
		backgroundscroll %= 127;

		ProgressBar.Style.BackgroundPositionX = Length.Pixels( (-128) + backgroundscroll );

		if ( World.WorldLoadingProgress >= 1f )
		{
			SetClass( "hidden", true );
			IngameRoot.Instance.SetClass( "hidden", false );
			if ( ComputedStyle.Opacity == 0 )
			{
				Delete();
			}
		}
	}
}
