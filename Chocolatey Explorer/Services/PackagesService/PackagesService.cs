using System.Collections.Generic;
using System.Linq;
using Chocolatey.Explorer.Model;
using Chocolatey.Explorer.Powershell;
using System;
using System.Threading.Tasks;
using Chocolatey.Explorer.Services.SourceService;

namespace Chocolatey.Explorer.Services.PackagesService
{
    public class PackagesService : IPackagesService
    {
        private readonly IRun _powershellAsync;
        private readonly IList<string> _lines;
        private readonly ISourceService _sourceService;
        private readonly IChocolateyLibDirHelper _libDirHelper;

        public delegate void FinishedDelegate(IList<Package> packages);
        public event FinishedDelegate RunFinshed;

		public delegate void FailedDelegate(Exception exc);
		public event FailedDelegate RunFailed;

        public PackagesService()
            : this(new RunAsync(), new SourceService.SourceService(), new ChocolateyLibDirHelper())
        {
        }

        public PackagesService(IRunAsync powershell, ISourceService sourceService, IChocolateyLibDirHelper libDirHelper)
        {
            _lines = new List<string>();
            _sourceService = sourceService;
            _libDirHelper = libDirHelper;
            _powershellAsync = powershell;
            _powershellAsync.OutputChanged += OutputChanged;
            _powershellAsync.RunFinished += RunFinished;
        }

        public void ListOfPackages()
        {
            this.Log().Info("Getting list of packages on source: " + _sourceService.Source);
            _powershellAsync.Run("clist -source " + _sourceService.Source);
        }

        public void ListOfInstalledPackages()
        {
            this.Log().Info("Getting list of installed packages");
			Task.Factory.StartNew(() => _libDirHelper.ReloadFromDir())
						.ContinueWith((task) => {
							if (!task.IsFaulted)
								OnRunFinshed(task.Result);
							else if (task.IsFaulted && RunFailed != null)
								RunFailed(task.Exception);
						});
        }

        private void OutputChanged(string line)
        {
            _lines.Add(line);
        }

        private void RunFinished()
        {
            OnRunFinshed((from result in _lines
                    let name = result.Split(" ".ToCharArray()[0])[0]
                    let version = result.Split(" ".ToCharArray()[0])[1]
                    select new Package { Name = name, InstalledVersion = version}).ToList());
        }

        private void OnRunFinshed(IList<Package> packages)
        {
            var handler = RunFinshed;
            if (handler != null) handler(packages);
        }

    }
}