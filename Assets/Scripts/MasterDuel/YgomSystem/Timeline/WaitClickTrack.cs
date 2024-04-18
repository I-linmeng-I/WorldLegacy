using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace YgomSystem.Timeline
{
	[Serializable]
	[ExcludeFromPreset]
	public class WaitClickTrack : TrackAsset
	{
		protected override void OnCreateClip(TimelineClip clip)
		{
		}

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return default(Playable);
		}
	}
}
