using System.Collections.Generic;
using YgomSystem.Network;

namespace YgomGame.Menu
{
	public class PvpMenuMatchingTeam_Leader : PvpMenuMatchingTeam
	{
		protected override Handle CallAPIMatching(Dictionary<string, object> matchParam)
		{
			return null;
		}

		protected override void OnCompleteMatchingHandle(Handle e)
		{
		}
	}
}
