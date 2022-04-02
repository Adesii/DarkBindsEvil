namespace DarkBinds.UI.IngameHUD;

[UseTemplate, Library( "Inventory" )]
public class Inventory : Panel
{
	public static Inventory Instance;

	public Inventory()
	{
		Instance = this;
	}
	public static void Toggle()
	{
		Instance.SetClass( "hidden", !Instance.HasClass( "hidden" ) );
	}
	[Event.BuildInput]
	public static void BuildInput( InputBuilder input )
	{
		if ( Instance?.HasClass( "hidden" ) ?? true )
			return;


		input.ClearButton( InputButton.Attack1 );
		input.ClearButton( InputButton.Attack2 );
	}
}
