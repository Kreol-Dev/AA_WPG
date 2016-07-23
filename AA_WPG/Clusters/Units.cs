using System;
using System.Collections.Generic;
using System.Text;
namespace AA_WPG
{
	public class Units : RemoteDataCluster
	{
		public float TimeAheadModifier = 1.5f;
		public List<Unit> units = new List<Unit>();
		public List<int> levels = new List<int>();
		public override void Init(GoogleDataUpdater updater)
		{
			//var count = int.Parse(updater.GetSheetRange("Units", 'A', 1, 'A', 1, "1YwwVn7NpUcGyNGOOHquT2_4FHLYxICE3C2YYLm8MeAw")[0][0] as string);
			var unitsData = updater.GetSheetRange("Units", 'A', 2, 'Z', 32, "1YwwVn7NpUcGyNGOOHquT2_4FHLYxICE3C2YYLm8MeAw");
			var years = updater.GetSheetRange("AdditionalData", 'A', 1, 'Z', 1, "1YwwVn7NpUcGyNGOOHquT2_4FHLYxICE3C2YYLm8MeAw")[0];
			for (int i = 0; i < years.Count; i+=2)
			{
				levels.Add(int.Parse(years[i] as string));
			//	Console.Write(levels[i / 2]);
			//	Console.Write(" ");
			}
			//Console.WriteLine();
			foreach (var row in unitsData)
				{
				if (row.Count > 0)
				{
					Unit unit = new Unit(row[1] as string,
										 row[0] as string,
										 int.Parse(row[3] as string),
										 float.Parse(row[4] as string),
										 (UnitType)Enum.Parse(typeof(UnitType), row[2] as string),
					                     float.Parse(row[5] as string),
					                     float.Parse(row[6] as string)
					                    );
						units.Add(unit);
						
					}
				}

			var effectivenessData = updater.GetSheetRange("Effectiveness", 'B', 2, 'Z', 26, "1YwwVn7NpUcGyNGOOHquT2_4FHLYxICE3C2YYLm8MeAw");
			for (int i = 0; i < effectivenessData.Count; i++)
			{
				var row = effectivenessData[i];
				for (int j = 0; j < row.Count; j++)
				{
					float eff = 0;
					float.TryParse(row[j] as string, out eff);
					units[i].Effectiveness.Add(units[j], eff);
				}
			}
			//foreach (var unit in units)
			//	Console.WriteLine(unit);
			Console.WriteLine(units.Count);
		}

		public override void Upload(GoogleDataUpdater updater)
		{
			
		}
	}
	public enum UnitType { Land, Air, Sea }
	public class Unit
	{
		public UnitType Type { get; internal set; }

		public Dictionary<Unit, float> Effectiveness = new Dictionary<Unit, float>();
		public string ID { get; internal set; }
		public string Name { get; internal set; }
		public int Manpower { get; internal set; }
		public float HP { get; internal set; }
		public float BuildCost { get; internal set; }
		public float ConsumptionCost { get; internal set; }
		public Unit(string id, string name, int manpower, float hp, UnitType type, float build, float consume)
		{
			BuildCost = build;
			ConsumptionCost = consume;
			ID = id;
			Name = name;
			if (Name.Length < 35)
			{
				int lessBy = 35 - Name.Length;
				builder.Clear();
				builder.Append(Name).Append('_', lessBy);
				Name = builder.ToString();
			}
			Manpower = manpower;
			HP = hp;
			Type = type;
		}

		static StringBuilder builder = new StringBuilder();
		public override string ToString()
		{
			builder.Clear();
			builder.Append(string.Format("[Unit: ID={4}, Name={0}, Manpower={1}, HP={2}, Type={3}, Build ={5}, Consumes={6}]", 
			                             Name, Manpower, HP, Type, ID, BuildCost, ConsumptionCost));
			foreach (var other in Effectiveness)
				builder.Append(Environment.NewLine).Append(other.Key.Name).Append(" = ").Append(other.Value);
			//foreach (var cost in Costs)
			//	builder.Append(Environment.NewLine).Append(cost.Key).Append(" = ").Append(cost.Value.Build).Append(" ").Append(cost.Value.Consumption);
			return builder.ToString();
		}
	}
}

