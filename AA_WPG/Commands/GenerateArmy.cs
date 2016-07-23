using System;
using System.IO;
using System.Collections.Generic;
namespace AA_WPG
{
	public class GenerateArmy : ConsoleCommand
	{
		System.Random random = new Random(DateTime.Now.Millisecond);
		struct UnitData
		{
			public Unit Unit;
			public int LowPercent;
			public int HighPercent;
			public int Count;
			public float Lines;
			public float ResultLines;
			public override string ToString()
			{
				return string.Format("{0} {1:#0.00} {2}", Unit.Name, ResultLines, Count);
			}
		}
		public override void Execute(string[] args)
		{
			var units = Program.GetCluster<Units>().units;
			var levels = Program.GetCluster<Units>().levels;
			var lines = File.ReadAllLines(String.Format("Data/{0}.txt", args[0]));
			var file = new System.IO.StreamWriter(String.Format("Data/{0}_Army.txt", args[9]));
			UnitData[] composition = new UnitData[lines.Length];
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var idAndPercents = line.Split('=');
				var percents = idAndPercents[1].Split('-');

				var id = idAndPercents[0];
				int lowPercent = int.Parse(percents[0]);
				int hightPercent = int.Parse(percents[1]);
				var unit = units.Find(x => x.ID == id);
				composition[i].Unit = unit;
				composition[i].LowPercent = lowPercent;
				composition[i].HighPercent = hightPercent;
				composition[i].Count = 0;
			}

			List<YearPercent> armyData = new List<YearPercent>();
			float yearPercent = 100f;
			foreach (var level in levels)
			{

				float percent = (float)(random.NextDouble() * 0.5 + 0.3) * yearPercent;
				if (yearPercent > percent)
				{
					armyData.Add(new YearPercent() { Year = level, Percent = percent });
					yearPercent -= percent;
				}
				else
				{
					
					percent = yearPercent;
					yearPercent = 0;
					armyData.Add(new YearPercent() { Year = level, Percent = percent });
					break;
			
				}
			}

			float landEquipped = 100f;
			float seaEquipped = 100f;
			var linesCount = int.Parse(args[1]);
			var armyLinesCountLimit = int.Parse(args[2]);

			var airLinesCountLimit = int.Parse(args[3]);

			var seaCount = int.Parse(args[4]);

			var seaLinesCountLimit = int.Parse(args[5]);

			float landLinesUsed = 0f;
			float fleetLinesUsed = 0f;

			var landManpowerLimit = int.Parse(args[6]);
			var airManpowerLimit = int.Parse(args[7]);
			var seaManpowerLimit = int.Parse(args[8]);
			var countryName = args[9];
			file.WriteLine(countryName);
			foreach (var yearData in armyData)
				file.Write(String.Format("{0} = {1:##.###}% | ", yearData.Year, yearData.Percent));
			file.WriteLine();
			int landManpower = 0;
			int airManpower = 0;
			int seaManpower = 0;
			int landPercent = 100;
			int airPercent = 100;
			int seaPercent = 100;
			for (int i = 0; i < composition.Length; i++)
			{
				int percent = random.Next(composition[i].LowPercent, composition[i].HighPercent);

				switch (composition[i].Unit.Type)
				{
					case UnitType.Air:
						if (airPercent >= percent)
							airPercent -= percent;
						else
						{
							percent = airPercent;
							airPercent = 0;
						}
						composition[i].Lines = ((float)percent / 100f)* airLinesCountLimit;
						break;
					case UnitType.Land:
						if (landPercent >= percent)
							landPercent -= percent;
						else
						{
							percent = landPercent;
							landPercent = 0;
						}
						composition[i].Lines = ((float)percent / 100f) * armyLinesCountLimit;
						break;
					case UnitType.Sea:
						if (seaPercent >= percent)
							seaPercent -= percent;
						else
						{
							percent = seaPercent;
							seaPercent = 0;
						}

						composition[i].Lines = ((float)percent / 100f) * seaLinesCountLimit;
						break;
				}

				float unitCost = composition[i].Unit.ConsumptionCost;
				int unitManpower = composition[i].Unit.Manpower;
				int count = (int)Math.Floor((float)composition[i].Lines / unitCost);
				var totalmanpower = count * unitManpower;

				switch (composition[i].Unit.Type)
				{
					case UnitType.Air:
							airManpower += totalmanpower;
							landLinesUsed += count * unitCost;
						break;
					case UnitType.Land:
							landManpower += totalmanpower;
							landLinesUsed += count * unitCost;
						break;
					case UnitType.Sea:
							seaManpower += totalmanpower;
							fleetLinesUsed += count * unitCost;
						break;
				}
				composition[i].Count = count;
				composition[i].ResultLines = count * unitCost;
				if (count > 0)
				file.WriteLine(composition[i]);
			}

			landEquipped = (float) linesCount / (float)landLinesUsed * 100;
			if (seaCount != 0)

				seaEquipped = (float)seaCount / (float)fleetLinesUsed * 100;
			else
				seaEquipped = 100;
			var totalManpower = airManpower + landManpower + seaManpower;
			var totalLimit = airManpowerLimit + landManpowerLimit + seaManpowerLimit;
			float landManpowerPercent = (float)landManpowerLimit / (float)landManpower * 100f;
			float airManpowerPercent = (float)airManpowerLimit / (float)airManpower * 100f;
			float seaManpowerPercent = (float)seaManpowerLimit / (float)seaManpower * 100f;
			var factoriesData = String.Format("Занято линий: {0:##.##} из {1} \r\nЗанято верфей: {2:#0.##} из {3}", landLinesUsed, linesCount, fleetLinesUsed, seaCount);
			var globalEquipmentData = String.Format("Снаряжено армии: {0:##.#}% \r\nСнаряжено флота: {1:##.#}%", Math.Min(landEquipped, 100), Math.Min(seaEquipped, 100));
			var globalArmyManpowerData = String.Format("Нужно людей в армии: {0} \r\nВ наличии: {1} \r\nСостав: {2:##.#}%", landManpower, landManpowerLimit, Math.Min(landManpowerPercent, 100));
			var globalAirManpowerData = String.Format("Нужно людей в авиации: {0} \r\nВ наличии: {1} \r\nСостав: {2:##.#}%", airManpower, airManpowerLimit, Math.Min(airManpowerPercent, 100));
          	var globalSeaManpowerData = String.Format("Нужно людей во флоте: {0} \r\nВ наличии: {1} \r\nСостав: {2:##.#}%", seaManpower, seaManpowerLimit, Math.Min(seaManpowerPercent, 100));
			var globalManpowerData = String.Format("Нужно людей всего: {0} \r\nВ наличии: {1}", totalManpower, totalLimit);
			file.WriteLine(factoriesData);
			file.WriteLine(globalEquipmentData);
			file.WriteLine(globalArmyManpowerData);
			file.WriteLine(globalAirManpowerData);
			if(seaCount > 0)
			file.WriteLine(globalSeaManpowerData);
			file.WriteLine(globalManpowerData);



			file.Close();
		}



	}
}

