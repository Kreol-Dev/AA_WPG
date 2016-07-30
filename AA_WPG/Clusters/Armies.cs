using System;
using System.Collections.Generic;
using System.Text;

namespace AA_WPG
{
	public class Armies : RemoteDataCluster
	{
		Units units;
		Dictionary<string, Army> armies = new Dictionary<string, Army>();
		GoogleDataUpdater updater;
		public override void Init(GoogleDataUpdater updater)
		{
			this.updater = updater;
			units = Program.GetCluster<Units>();

			//throw new NotImplementedException();

		
		}

		public override void Upload(GoogleDataUpdater updater)
		{
			throw new NotImplementedException();
		}


		public Army GetArmy(string armyTag)
		{
			Army army = null;
			if (armies.TryGetValue(armyTag, out army))
				return army;
			else
			{
				var eff = updater.GetSheetRange(armyTag as string, "B", 4, "Z", 28, "1qpiElAA2ytKQ_9HJRP7emvvBzupxNi8L78W9Q7r-gLU");
				var addData = updater.GetSheetRange(armyTag as string, "AA", 4, "AC", 28, "1qpiElAA2ytKQ_9HJRP7emvvBzupxNi8L78W9Q7r-gLU");
				var fronts = updater.GetSheetRange(armyTag as string, "A", 30, "Z", 40, "1qpiElAA2ytKQ_9HJRP7emvvBzupxNi8L78W9Q7r-gLU");
				var modifiers = updater.GetSheetRange(armyTag as string, "A", 42, "Z", 50, "1qpiElAA2ytKQ_9HJRP7emvvBzupxNi8L78W9Q7r-gLU");
				var variables = updater.GetSheetRange(armyTag as string, "A", 1, "Z", 1, "1qpiElAA2ytKQ_9HJRP7emvvBzupxNi8L78W9Q7r-gLU")[0];
				army = new Army();
				int initiative = 0;
				if (int.TryParse(variables[1] as string, out initiative))
					army.Initiative = initiative;
				List<Unit> localUnits = new List<Unit>();
				army.Units = localUnits;
				foreach (var unit in units.units)
					localUnits.Add(unit.Copy());
				if (eff != null)
				{
					for (int i = 0; i < eff.Count; i++)
					{
						var row = eff[i];
						for (int j = 0; j < row.Count; j++)
						{
							var cell = row[j];
							float effModDelta = 0;
							if (float.TryParse(cell as string, out effModDelta))
							{
								var targetBaseUnit = units.units[j];
								var localUnit = localUnits[i];
								if (localUnit.Effectiveness.ContainsKey(targetBaseUnit))
									localUnit.Effectiveness[targetBaseUnit] = localUnit.Effectiveness[targetBaseUnit] + effModDelta;
								else
									localUnit.Effectiveness.Add(targetBaseUnit, effModDelta);
							}
						}
					}
				}

				if (addData != null)
				{
					for (int i = 0; i < addData.Count; i++)
					{

						var row = addData[i];
						var targetUnit = localUnits[i];
						int hpModDelta = 0;
						float orgModDelta = 0;
						int priorityModDelta = 0;
						if (row.Count > 0 && int.TryParse(row[0] as string, out hpModDelta))
							targetUnit.HP += hpModDelta;
						if (row.Count > 1 && float.TryParse(row[1] as string, out orgModDelta))
							targetUnit.Organization += orgModDelta;
						if (row.Count > 2 && int.TryParse(row[2] as string, out priorityModDelta))
							targetUnit.Priority += priorityModDelta;
					}
				}

				army.Fronts = new List<Frontline>();
				List<FrontUnit> frontUnits = new List<FrontUnit>();
				if (fronts != null)
				{
					foreach (var row in fronts)
					{
						frontUnits.Clear();
						string frontID = row[0] as string;
						Frontline frontline = new Frontline(frontID);
						for (int i = 1; i < row.Count; i++)
						{
							int unitsCount = 0;
							if (int.TryParse(row[i] as string, out unitsCount))
							{
								var targetUnit = localUnits[i - 1];
								frontUnits.Add(new FrontUnit(targetUnit, unitsCount));
							}
						}
						army.Fronts.Add(frontline);
						frontline.Units = frontUnits.ToArray();
						frontline.Army = army;
					}

				}
				if (modifiers != null)
				{
					foreach (var mod in modifiers)
					{

					}
				}

				armies.Add(armyTag, army);
			}
			return army;
		}
		
	}

				    public class Army
	{
		public State State;
		public List<Unit> Units;
		public List<Frontline> Fronts;
		public int Initiative;
		StringBuilder builder = new StringBuilder();
		public override string ToString()
		{
			builder.Clear();
			builder.Append("Army of ").Append(State).Append(Environment.NewLine);
			//foreach (var unit in Units)
			//builder.Append(unit).Append(Environment.NewLine);
			foreach (var front in Fronts)
				builder.Append(front).Append(Environment.NewLine);
			return builder.ToString();

		}
	}

	public abstract class ArmyModifier
	{
		public abstract void Attach(Army to);
		public abstract void Detach(Army from);
	}
	public struct FrontUnit
	{
		public Unit Unit;
		public int Count;
		public bool WasAttacked;
		public FrontUnit(Unit unit, int count)
		{
			WasAttacked = false;
			Unit = unit;
			Count = count;
		}
	}
	public class Frontline
	{
		public Army Army;
		public string ID { get; internal set; }

		public FrontUnit[] Units;

		public Frontline(string id)
		{
			ID = id;
		}

		StringBuilder builder = new StringBuilder();
		public override string ToString()
		{
			builder.Clear();
			builder.Append("Front " + ID).Append(Environment.NewLine);
			foreach (var unit in Units)
				builder.Append(String.Format("{0,-35}", unit.Unit.Name)).Append(" = ").Append(unit.Count).Append(Environment.NewLine);
			return builder.ToString();
		}
	}
}

