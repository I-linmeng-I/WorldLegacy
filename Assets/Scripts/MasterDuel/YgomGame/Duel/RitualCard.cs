using System;
using UnityEngine;

namespace YgomGame.Duel
{
	public class RitualCard : SummonCardBase
	{
		public static string TimelinePath;

		protected override string timelinePath => null;

		protected override string seLabel => null;

		protected override string trailOffsetLabel => null;

		public static RitualCard Create(int cardID, int uniqueID, int rareID, Vector3 position, Quaternion rotation, CardRoot.ModelType modelType, Action onLoadFinished, Action onPlayFinished)
		{
			return null;
		}

		public static void PreloadTimeline(Action onFinished)
		{
		}

		public static void UnloadPreloadedTimeline()
		{
		}
	}
}
