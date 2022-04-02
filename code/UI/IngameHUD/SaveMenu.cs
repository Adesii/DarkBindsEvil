using System.IO;
using DarkBinds.Systems.Worlds;

namespace DarkBinds.UI.IngameHUD;

[UseTemplate]
public class SaveMenu : Panel
{

	public TextEntry SaveGameNameEntry { get; set; }
	public Panel SaveMenuPanel { get; set; }

	public Panel SaveList { get; set; }
	public Button OpenButton { get; set; }

	public void OpenSaveMenu()
	{
		SaveMenuPanel.SetClass( "hidden", false );
		SaveGameNameEntry.Focus();
		if ( !FileSystem.Data.DirectoryExists( "Saves" ) )
		{
			FileSystem.Data.CreateDirectory( "Saves" );
		}
		var files = FileSystem.Data.FindFile( "Saves", "*.m_save" );
		if ( SaveList.ChildrenCount == 0 )
			foreach ( var file in files )
			{
				var TempItem = new SaveState( file );
				SaveList.AddChild( TempItem );
			}

	}

	public override void Tick()
	{
		base.Tick();
		if ( Local.Client.IsListenServerHost )
		{
			OpenButton?.SetClass( "hidden", false );
			SaveGameNameEntry.AcceptsFocus = true;
		}
		else
		{
			OpenButton?.SetClass( "hidden", true );
		}

	}

	public void CloseSaveMenu()
	{
		SaveMenuPanel.SetClass( "hidden", true );
		SaveGameNameEntry.Blur();
	}
	public void SaveGame()
	{
		if ( SaveGameNameEntry.Text.Length > 0 )
		{
			World.SaveWorld( SaveGameNameEntry.Text );
			SaveList.DeleteChildren();
			OpenSaveMenu();
			SaveGameNameEntry.Text = "";
		}
	}
}

class SaveState : Panel
{
	public string SaveName { get; set; }
	public SaveState( string SaveName )
	{
		this.SaveName = SaveName;
		Add.Label( SaveName, "SaveName" );
		Add.Button( "save", "overwritesave", () =>
		{
			World.SaveWorld( Path.GetFileNameWithoutExtension( SaveName ) );
			Log.Error( $"Overwrote {SaveName}" );
		} );
	}
}
