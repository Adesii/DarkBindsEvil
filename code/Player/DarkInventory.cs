namespace DarkBinds.Player;

public partial class DarkInventory : EntityComponent
{
	[Net, Local] public string CurrentBlock { get; set; } = "Dirt";
	[Net, Local] public int CurrentBlockID { get; set; } = 0;

	[Net] public Dictionary<int, string> BlockList { get; set; }

	[Net] public int InventorySize { get; set; } = 10;

	protected override void OnActivate()
	{
		base.OnActivate();
		if ( Entity is DarkBindCharacter )
		{
			BlockList = new Dictionary<int, string>(){
				{0,"Dirt"},
				{1,"Stone"},
				{2,"Ice"},
				{3,"Concrete"},
				{4,"Sand"},
				{5,"MarkedStone"},
				{6,"Marble"},
				{7,"Moss"},
				{8,"Water"},
				{9,"Wood"}
			};
		}
	}
	public void NextBlock()
	{
		CurrentBlockID++;
		if ( CurrentBlockID >= BlockList.Count )
			CurrentBlockID = 0;
		CurrentBlock = BlockList[CurrentBlockID];
	}

	public void PreviousBlock()
	{
		CurrentBlockID--;
		if ( CurrentBlockID < 0 )
			CurrentBlockID = BlockList.Count - 1;
		CurrentBlock = BlockList[CurrentBlockID];
	}
}
