using DarkBinds.Systems.Worlds;

namespace DarkBinds.Systems.Blocks;

[Block( "Air", 0 )]
public class AirBlock : BaseBlock
{

	public override bool IsSolid()
	{
		return false;
	}
	public override bool IsTranslucent()
	{
		return true;
	}

	public override bool HasMesh()
	{
		return false;
	}
	public override bool BreakAble()
	{
		return false;
	}
	public override bool HasCollisions()
	{
		return false;
	}
}
