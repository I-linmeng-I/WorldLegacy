namespace YgomGame.Duel
{
	public class EffectTaskCutinCoinDice : EffectTask
	{
		private bool finished;

		public static EffectTask Create(RunEffectWorker worker, int param1, int param2, int param3)
		{
			return null;
		}

		public EffectTaskCutinCoinDice(RunEffectWorker worker, int player, int cardId, int result)
			: base(null)
		{
		}

		public override bool Update()
		{
			return false;
		}
	}
}
