namespace YgomGame.Duel
{
	public class EffectTaskCardVanish : EffectTask
	{
		private enum Step
		{
			WaitCardMove = 0,
			Finish = 1
		}

		private bool finished;

		private Step step;

		private int team;

		private int position;

		private int index;

		public static EffectTask Create(RunEffectWorker worker, int param1, int param2, int param3)
		{
			return null;
		}

		public EffectTaskCardVanish(RunEffectWorker worker, int param1, int param2, int param3)
			: base(null)
		{
		}

		public override bool Update()
		{
			return false;
		}

		private void WaitCardMoveStep()
		{
		}

		private void FinishStep()
		{
		}
	}
}
