using UnityEngine;

namespace Modules.ServiceLocator
{
    [DefaultExecutionOrder(-1504)]
    public class ProjectContext : ServiceContext
    {
        private void Awake()
        {
            ServiceLocator.Instance.InitializeProjectContext(this);
            RegisterServices(this);
        }
    }
}
