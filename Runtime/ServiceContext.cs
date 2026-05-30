using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.ServiceLocator
{
    public abstract class ServiceContext : MonoBehaviour
    {
        public Dictionary<Type, object> services = new Dictionary<Type, object>();
        public List<MonoBehaviour> servicesToRegister = new List<MonoBehaviour>();

        protected void RegisterServices(ServiceContext context)
        {
            var locator = ServiceLocator.Instance;

            foreach (var comp in servicesToRegister)
            {
                if (comp == null) continue;

                var type = comp.GetType();

                var types = new HashSet<Type>();
                foreach (var itf in type.GetInterfaces())
                    types.Add(itf);

                types.Add(type);

                foreach (var t in types)
                {
                    if (!context.services.ContainsKey(t))
                        locator.RegisterServiceToContext(t, comp, context);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            var locator = ServiceLocator.Instance;
            if (locator == null) return;

            if (this is ProjectContext project)
            {
                locator.ClearProjectContext(project);
                return;
            }

            if (this is SceneContext scene)
            {
                locator.RemoveSceneContext(scene.gameObject.scene, scene);
                return;
            }

            if (this is GameObjectContext gameObjectCtx)
                locator.RemoveGameObjectContext(gameObjectCtx.gameObject, gameObjectCtx);
        }
    }
}
