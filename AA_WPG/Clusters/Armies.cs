using System;
using System.Collections.Generic;

namespace AA_WPG
{
	public class Armies : RemoteDataCluster
	{
		public override void Init(GoogleDataUpdater updater)
		{
			throw new NotImplementedException();
		}

		public override void Upload(GoogleDataUpdater updater)
		{
			throw new NotImplementedException();
		}

		public class Army
		{
			public State State;
		}

		public abstract class ArmyModifier
		{
			public abstract void Attach(Army to);
			public abstract void Detach(Army from);
		}
		public class Frontline
		{
			public Army Army;
			public string Name;
			public struct FrontUnit
			{
				public Unit Unit;
				public int Count;
			}
			public List<FrontUnit> Units = new List<FrontUnit>();

		}
	}
}

