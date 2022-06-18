using DarkBinds.Player;
using Gamelib.UI;

namespace DarkBinds.UI;

[UseTemplate]
public class Cursor : Panel
{
	public Panel cursorImage { get; set; }
	public static Panel hovered;
	Vector2 Mouse;

	public Cursor()
	{
		BindClass( "inactive", () =>
		{
			return Local.Client.Components.Get<DevCamera>( true ).Enabled;
		} );
	}

	[Event.BuildInput]
	public void BuildInput( InputBuilder input )
	{
		Mouse = Sandbox.Mouse.Position;
	}
	[Event.BuildInput]
	public static void Build( InputBuilder input )
	{
		hovered = UIUtility.GetHoveredPanel();
		if ( hovered == null/*  || hovered is RootPanel  */)
			return;

		if ( input.Pressed( InputButton.SecondaryAttack ) )
		{
			hovered.CreateEvent( new MousePanelEvent( "onclick", hovered, "mouseleft" ) );
		}
		if ( input.Pressed( InputButton.PrimaryAttack ) )
		{
			hovered.CreateEvent( new MousePanelEvent( "onclick", hovered, "mouseright" ) );
		}
		input.ClearButton( InputButton.PrimaryAttack );
		input.ClearButton( InputButton.SecondaryAttack );
	}
	public override void Tick()
	{
		cursorImage.Style.Left = Length.Fraction( (Mouse.x / Screen.Width).Clamp( 0, 0.999f ) );
		cursorImage.Style.Top = Length.Fraction( (Mouse.y / Screen.Height).Clamp( 0, 0.999f ) );
	}

}
