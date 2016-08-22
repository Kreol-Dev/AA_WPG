using System;
using System.IO;
namespace AA_WPG
{
	public static class Sheets
	{
		public static string ArmySheetID;
		public static string ArmiesSheetID;
		public static string StatesSheetID;
		static Sheets()
		{
			var lines = File.ReadAllLines("Sheets.txt");
			ArmySheetID = lines[0].Split(' ')[1];
			ArmiesSheetID = lines[1].Split(' ')[1];
			StatesSheetID = lines[2].Split(' ')[1];
		}



	}
}

