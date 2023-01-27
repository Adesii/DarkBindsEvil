using DarkBinds.Player;

namespace DarkBinds.UI.IngameHUD;

[UseTemplate]
public class ItemBar : Panel
{

	public Panel ItemList { get; set; }

	public DarkBindCharacter Player { get; set; }

	public ItemBar()
	{

	}

	public override void Tick()
	{
		base.Tick();

		int i = 0;
		if ( ItemList != null )
		{
			Player = Game.LocalPawn as DarkBindCharacter;
			if ( ItemList.ChildrenCount <= 0 )
				foreach ( var item in Player.DarkInventory.BlockList )
				{
					var TempItem = new TempItem( item.Value );
					ItemList.AddChild( TempItem );
				}
			else
				foreach ( var item in ItemList.Children )
				{
					var TempItem = item as TempItem;

					if ( i == Player.DarkInventory.CurrentBlockID )
					{
						TempItem.SetClass( "Selected", true );
					}
					else
					{
						TempItem.SetClass( "Selected", false );
					}
					i++;
				}
		}

	}

}


class TempItem : Panel
{
	string blockname;
	public TempItem( string Item )
	{
		this.blockname = Item;
		Add.Label( blockname, "ItemName" );
	}
}
