using System;
namespace AA_WPG
{
	public class States : RemoteDataCluster
	{
		public float BaseStateFactoryTaxPercent;
		public float BaseInvestements;

		public override void Init(GoogleDataUpdater updater)
		{
			var statesIDs = updater.GetSheetRange("States", "A", 2, "Z", 3,"1NSBFZxNsfgRmbgaQIj0lYHzUliNOS2bwPxOqs4bj3gM");

			var consumptionTable = updater.GetSheetRange("States", "A", 5, "Z", 8, "1NSBFZxNsfgRmbgaQIj0lYHzUliNOS2bwPxOqs4bj3gM");


			var variablesTable = updater.GetSheetRange("States", "A", 10, "A", 12, "1NSBFZxNsfgRmbgaQIj0lYHzUliNOS2bwPxOqs4bj3gM");



		}

		public override void Upload(GoogleDataUpdater updater)
		{
			throw new NotImplementedException();
		}
	}

	public class State
	{
		public string Name;
		public int Population;
		public float Tax;
		public float Corruption;
		public float EconomyEfficiency;
		public float StateEfficiency;
	}

	public abstract class StateModifier
	{
		public abstract void Attach(State to);
		public abstract void Detach(State from);
	}

}

