using System;
namespace AA_WPG
{
	public class States : RemoteDataCluster
	{
		public float BaseStateFactoryTaxPercent;
		public float BaseInvestements;
		public float BaseTax;
		public override void Init(GoogleDataUpdater updater)
		{
			var statesIDs = updater.GetSheetRange("States", "A", 2, "Z", 3,"1NSBFZxNsfgRmbgaQIj0lYHzUliNOS2bwPxOqs4bj3gM");

			var consumptionTable = updater.GetSheetRange("States", "B", 5, "Z", 8, "1NSBFZxNsfgRmbgaQIj0lYHzUliNOS2bwPxOqs4bj3gM");


			var variablesTable = updater.GetSheetRange("States", "B", 10, "B", 12, "1NSBFZxNsfgRmbgaQIj0lYHzUliNOS2bwPxOqs4bj3gM");
			BaseStateFactoryTaxPercent = float.Parse(variablesTable[0][0] as string);
			BaseInvestements = float.Parse(variablesTable[1][0] as string);
			BaseTax = float.Parse(variablesTable[2][0] as string);

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

