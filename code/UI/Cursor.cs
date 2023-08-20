namespace DarkBinds.UI;
public partial class Cursor : Panel
{
	[GameEvent.Client.BuildInput]
	private void buildinput()
	{

		this.Mouse = Sandbox.Mouse.Position;
	}
}
