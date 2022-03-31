using DarkBinds.UI;
using Sandbox;
using Sandbox.UI;

namespace Gamelib.UI
{
	public static class UIUtility
	{
		public static Panel GetHoveredPanel( Panel root = null )
		{
			root ??= Local.Hud;

			if ( root.HasHovered )
			{
				if ( !string.IsNullOrEmpty( root.ComputedStyle.PointerEvents ) && root is not RootPanel && root is not GameRootPanel )
				{
					if ( root.ComputedStyle.PointerEvents == "visible" && root.ComputedStyle.PointerEvents != "none" )
						return root;
				}
			}

			foreach ( var child in root.Children )
			{
				var panel = GetHoveredPanel( child );

				if ( panel != null )
					return panel;
			}

			return null;
		}
	}
}
