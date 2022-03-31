using DarkBinds.Player;
using Gamelib.UI;

namespace DarkBinds.UI;

[UseTemplate]
public class Cursor : Panel
{
	public Panel cursorImage { get; set; }
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
		var hovered = UIUtility.GetHoveredPanel();
		if ( hovered == null/*  || hovered is RootPanel  */)
			return;

		if ( input.Pressed( InputButton.Attack2 ) )
		{
			hovered.CreateEvent( new MousePanelEvent( "onclick", hovered, "mouseleft" ) );
		}
		if ( input.Pressed( InputButton.Attack1 ) )
		{
			hovered.CreateEvent( new MousePanelEvent( "onclick", hovered, "mouseright" ) );
		}
		input.ClearButton( InputButton.Attack1 );
		input.ClearButton( InputButton.Attack2 );
	}
	public override void Tick()
	{
		cursorImage.Style.Left = Length.Fraction( (Mouse.x / Screen.Width).Clamp( 0, 0.999f ) );
		cursorImage.Style.Top = Length.Fraction( (Mouse.y / Screen.Height).Clamp( 0, 0.999f ) );
	}

}
