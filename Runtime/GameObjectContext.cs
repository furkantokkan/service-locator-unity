using UnityEngine;

namespace Modules.ServiceLocator
{
    [DefaultExecutionOrder(-1505)]
    public class GameObjectContext : ServiceContext
    {
        private void Awake()
        {
            ServiceLocator.Instance.InitializeGameObjectContext(this);
            RegisterServices(this);
        }
    }
}
