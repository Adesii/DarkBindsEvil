using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

[Block( "Air" )]
public class AirBlock : BaseBlock
{
	public AirBlock()
	{
		Name = "Air";
	}

	public override bool IsSolid()
	{
		return false;
	}
}
