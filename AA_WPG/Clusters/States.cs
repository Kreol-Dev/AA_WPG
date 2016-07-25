using System;
namespace AA_WPG
{
	public class States : RemoteDataCluster
	{
		public override void Init(GoogleDataUpdater updater)
		{
			throw new NotImplementedException();
		}

		public override void Upload(GoogleDataUpdater updater)
		{
			throw new NotImplementedException();
		}
	}

	public class State
	{
		public string Name;
	}

	public abstract class StateModifier
	{
		public abstract void Attach(State to);
		public abstract void Detach(State from);
	}

}

