using DarkBinds.Player;

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
			return Game.LocalClient.Components.Get<DevCamera>( true ).Enabled;
		} );
	}

	[Event.Client.BuildInput]
	public void BuildInput()
	{
		Mouse = Sandbox.Mouse.Position;
	}
	public override void Tick()
	{
		cursorImage.Style.Left = Length.Fraction( (Mouse.x / Screen.Width).Clamp( 0, 0.999f ) );
		cursorImage.Style.Top = Length.Fraction( (Mouse.y / Screen.Height).Clamp( 0, 0.999f ) );
	}

}
