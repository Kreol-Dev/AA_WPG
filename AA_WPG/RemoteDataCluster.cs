using System;
namespace AA_WPG
{
	public abstract class RemoteDataCluster
	{
		public abstract void Init(GoogleDataUpdater updater);
		public abstract void Upload(GoogleDataUpdater updater);

	}
}

