using UnityEngine;

namespace Modules.ServiceLocator
{
    [DefaultExecutionOrder(-1503)]
    public class SceneContext : ServiceContext
    {
        private void Awake()
        {
            ServiceLocator.Instance.InitializeSceneContext(this);
            RegisterServices(this);
        }
    }
}
