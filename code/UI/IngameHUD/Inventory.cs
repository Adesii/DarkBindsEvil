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
	[Event.Client.BuildInput]
	public static void BuildInput()
	{
		if ( Instance?.HasClass( "hidden" ) ?? true )
			return;


		Input.ClearButton( InputButton.PrimaryAttack );
		Input.ClearButton( InputButton.SecondaryAttack );
	}
}
