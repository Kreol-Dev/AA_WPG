using System;
using System.Collections.Generic;

namespace AA_WPG
{
	public class ShowArmy : ConsoleCommand
	{
		public override void Execute(string[] args)
		{
			string stateTag = args[0];
			var army = Program.GetCluster<Armies>().GetArmy(stateTag);
			float lines = 0f;
			float fleetLines = 0f;
			Dictionary<Unit, int> unitCounts = new Dictionary<Unit, int>();
			foreach (var front in army.Fronts)
			{
				foreach (var unit in front.Units)
				{
					if(unit.Unit.Type == UnitType.Air || unit.Unit.Type == UnitType.Land)
						lines += unit.Count * unit.Unit.ConsumptionCost;
					else
						fleetLines += unit.Count * unit.Unit.ConsumptionCost;
					if (unitCounts.ContainsKey(unit.Unit))
						unitCounts[unit.Unit] = unitCounts[unit.Unit] + unit.Count;
					else
						unitCounts.Add(unit.Unit, unit.Count);
				}
			}
			foreach (var pair in unitCounts)
				Console.WriteLine(String.Format("{0:30} {1}", pair.Key.Name, pair.Value));
			Console.WriteLine(lines);
			Console.WriteLine(fleetLines);

		}
	}
}

