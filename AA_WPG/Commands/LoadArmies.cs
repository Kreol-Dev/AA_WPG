using System;
namespace AA_WPG
{
	public class LoadArmies: ConsoleCommand
	{
		public override void Execute(string[] args)
		{
			var armies = Program.GetCluster<Armies>();
			var armyTags = Program.upd.GetSheetRange("Armies", "A", 1, "Z", 1, Sheets.ArmiesSheetID)[0];
			foreach (var armyTag in armyTags)
			{
				var army = armies.GetArmy(armyTag as string);

				//Console.WriteLine(army.ToString());
			}
		}
	}
}

