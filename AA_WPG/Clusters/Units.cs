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
			//var count = int.Parse(updater.GetSheetRange("Units", "A", 1, "A", 1, "1YwwVn7NpUcGyNGOOHquT2_4FHLYxICE3C2YYLm8MeAw")[0][0] as string);
			var unitsData = updater.GetSheetRange("Units", "A", 2, "AC", 32, Sheets.ArmySheetID);
			var years = updater.GetSheetRange("AdditionalData", "A", 1, "Z", 1, Sheets.ArmySheetID)[0];
			for (int i = 0; i < years.Count; i+=2)
			{
				levels.Add(int.Parse(years[i] as string));
			//	Console.Write(levels[i / 2]);
			//	Console.Write(" ");
			}
			//Console.WriteLine();
			int index = 0;
			foreach (var row in unitsData)
				{
				if (row.Count > 0)
				{
					Unit unit = new Unit(row[1] as string,
										 row[0] as string,
					                     index++,
										 int.Parse(row[3] as string),
										 float.Parse(row[4] as string),
										 (UnitType)Enum.Parse(typeof(UnitType), row[2] as string),
					                     float.Parse(row[5] as string),
					                     float.Parse(row[6] as string),
										 int.Parse(row[8] as string),
										 float.Parse(row[7] as string)
					                    );
						units.Add(unit);
						
					}
				}

			var effectivenessData = updater.GetSheetRange("Effectiveness", "B", 2, "Z", 26, Sheets.ArmySheetID);
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
			//Console.WriteLine("Units count: " + units.Count);
			//foreach (var unit in units)
				//Console.WriteLine(unit);
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
		public int Index { get; internal set; }
		public string Name { get; internal set; }
		public int Manpower { get; set; }
		public float HP { get; set; }
		public float BuildCost { get; set; }
		public float ConsumptionCost { get; set; }
		public float Organization { get; set; }
		public int Priority { get; set; }
		public Unit(string id, string name, int index, int manpower, float hp, UnitType type, float build, float consume, int priority, float organization)
		{
			BuildCost = build;
			ConsumptionCost = consume;
			ID = id;
			Name = name;
			Index = index;
			//if (Name.Length < 35)
			//{
			//	int lessBy = 35 - Name.Length;
			//	builder.Clear();
			//	builder.Append(Name).Append('_', lessBy);
			//	Name = builder.ToString();
			//}
			Manpower = manpower;
			HP = hp;
			Type = type;
			Priority = priority;
			Organization = organization;
		}

		public Unit Copy()
		{
			var unit =  this.MemberwiseClone() as Unit;
			unit.Effectiveness = new Dictionary<Unit, float>();
			foreach (var pair in Effectiveness)
				unit.Effectiveness.Add(pair.Key, pair.Value);
			return unit;
		}

		static StringBuilder builder = new StringBuilder();
		public override string ToString()
		{
			builder.Clear();

			var props = typeof(Unit).GetProperties();
			builder.Append("");
			foreach (var prop in props)
				builder.Append(prop.Name).Append("=").Append(prop.GetValue(this)).Append(", ");
			builder.Length -= 2;
			builder.Append("]");
			//builder.Append(string.Format("[Unit: ID={4}, Name={0}, Manpower={1}, HP={2}, Type={3}, Build ={5}, Consumes={6}]", 
			//                             Name, Manpower, HP, Type, ID, BuildCost, ConsumptionCost));
			foreach (var other in Effectiveness)
				builder.Append(Environment.NewLine).Append(other.Key.Name).Append(" = ").Append(other.Value);
			//foreach (var cost in Costs)
			//	builder.Append(Environment.NewLine).Append(cost.Key).Append(" = ").Append(cost.Value.Build).Append(" ").Append(cost.Value.Consumption);
			return builder.ToString();
		}
	}
}

