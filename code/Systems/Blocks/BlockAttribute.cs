namespace DarkBinds.Systems.Blocks;

[AttributeUsage( AttributeTargets.Class )]
public class BlockAttribute : LibraryAttribute
{
	public string BlockName { get; set; }

	public BlockAttribute( string blockName )
	{
		BlockName = blockName;
	}
}
