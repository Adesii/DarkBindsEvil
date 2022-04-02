namespace DarkBinds.Systems.Blocks;

[AttributeUsage( AttributeTargets.Class )]
public class BlockAttribute : LibraryAttribute
{
	public string BlockName { get; set; }
	public int BlockID { get; set; }

	public BlockAttribute( string blockName, int BlockID = -1 )
	{
		BlockName = blockName;
		this.BlockID = BlockID;
	}
}
