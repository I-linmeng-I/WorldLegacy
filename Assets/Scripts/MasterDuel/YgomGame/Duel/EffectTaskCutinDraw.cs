namespace YgomGame.Duel
{
	public class EffectTaskCutinDraw : EffectTask
	{
		private bool finished;

		public static EffectTask Create(RunEffectWorker worker, int param1, int param2, int param3)
		{
			return null;
		}

		public EffectTaskCutinDraw(RunEffectWorker worker, int param1, int param2, int param3)
			: base(null)
		{
		}

		public override bool Update()
		{
			return false;
		}
	}
}
