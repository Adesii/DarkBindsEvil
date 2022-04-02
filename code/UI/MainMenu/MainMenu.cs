using DarkBinds.Systems.Worlds;
using DarkBinds.UI.Loading;

namespace DarkBinds.UI;

[UseTemplate]
public class MainMenu : Panel
{
	public static MainMenu Instance;

	public MainMenu()
	{
		Instance = this;
	}

	public Panel LoadList { get; set; }
	public Panel LoadMenu { get; set; }
	public void NewGame()
	{
		SetClass( "hidden", true );
		LoadingScreen.Instance.SetClass( "hidden", false );
		World.RegenerateWorld();
	}

	public void OpenLoadMenu()
	{
		LoadMenu.SetClass( "hidden", false );
		if ( !FileSystem.Data.DirectoryExists( "Saves" ) )
		{
			FileSystem.Data.CreateDirectory( "Saves" );
		}
		var files = FileSystem.Data.FindFile( "Saves", "*.m_save" );
		if ( LoadList.ChildrenCount == 0 )
			foreach ( var file in files )
			{
				var TempItem = new SaveState( file, this );
				LoadList.AddChild( TempItem );
			}

	}
	public void CloseLoadMenu()
	{
		LoadMenu.SetClass( "hidden", true );
	}
}

class SaveState : Panel
{
	public string SaveName { get; set; }
	public SaveState( string SaveName, Panel parent )
	{
		this.SaveName = SaveName;
		Add.Label( SaveName, "SaveName" );
		Add.Button( "Load", "LoadButton", () =>
		{
			parent.SetClass( "hidden", true );
			LoadingScreen.Instance.SetClass( "hidden", false );
			World.LoadWorld( SaveName );
		} );
	}
}
