using DarkBinds.Player;

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
	public override void Tick()
	{
		cursorImage.Style.Left = Length.Fraction( (Mouse.x / Screen.Width).Clamp( 0, 0.999f ) );
		cursorImage.Style.Top = Length.Fraction( (Mouse.y / Screen.Height).Clamp( 0, 0.999f ) );
	}

}
