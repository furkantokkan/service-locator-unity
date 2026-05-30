using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.ServiceLocator
{
    [DefaultExecutionOrder(-1505)]
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;

        private ProjectContext projectContext;
        private readonly Dictionary<Scene, SceneContext> sceneContexts = new Dictionary<Scene, SceneContext>();
        private readonly Dictionary<GameObject, GameObjectContext> gameObjectContexts = new Dictionary<GameObject, GameObjectContext>();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    return null;
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeProjectContext(ProjectContext context)
        {
            if (projectContext == null)
            {
                projectContext = context;
                DontDestroyOnLoad(context.gameObject);
            }
            else
            {
                Destroy(context);
                Debug.LogWarning($"Multiple project contexts found! The project context on GameObject {context.gameObject} was destroyed");
            }
        }

        public void InitializeSceneContext(SceneContext context)
        {
            if (!sceneContexts.ContainsKey(context.gameObject.scene))
            {
                sceneContexts[context.gameObject.scene] = context;
            }
            else
            {
                Destroy(context);
                Debug.LogWarning($"Multiple scene contexts found! The scene context on GameObject {context.gameObject} was destroyed");
            }
        }

        public void InitializeGameObjectContext(GameObjectContext context)
        {
            if (!gameObjectContexts.ContainsKey(context.gameObject))
            {
                gameObjectContexts[context.gameObject] = context;
            }
            else
            {
                Destroy(context);
                Debug.LogWarning($"Multiple GameObject contexts on one object found! The context on GameObject {context.gameObject} was destroyed");
            }
        }

        internal void ClearProjectContext(ProjectContext context)
        {
            if (context == null) return;
            if (ReferenceEquals(projectContext, context))
                projectContext = null;
        }

        internal void RemoveSceneContext(Scene scene, SceneContext context)
        {
            if (!sceneContexts.TryGetValue(scene, out SceneContext existing))
                return;

            if (existing == null || ReferenceEquals(existing, context))
                sceneContexts.Remove(scene);
        }

        internal void RemoveGameObjectContext(GameObject obj, GameObjectContext context)
        {
            if (obj == null) return;
            if (!gameObjectContexts.TryGetValue(obj, out GameObjectContext existing))
                return;

            if (existing == null || ReferenceEquals(existing, context))
                gameObjectContexts.Remove(obj);
        }

        private bool TryGetSceneContext(Scene scene, out SceneContext sceneContext)
        {
            if (sceneContexts.TryGetValue(scene, out sceneContext))
            {
                if (sceneContext != null)
                    return true;

                sceneContexts.Remove(scene);
            }

            sceneContext = null;
            return false;
        }

        public void RegisterServiceToContext(Type type, object serviceInstance, ServiceContext serviceContext)
        {
            serviceContext.services[type] = serviceInstance;
        }

        public void RegisterServiceToContext<T>(T serviceInstance, ServiceContext serviceContext)
        {
            serviceContext.services[typeof(T)] = serviceInstance;
        }

        public void RegisterProjectService(Type type, object serviceInstance)
        {
            if (projectContext == null)
            {
                var newProjectContextObj = new GameObject("Project context", typeof(ProjectContext));
                projectContext = newProjectContextObj.GetComponent<ProjectContext>();
            }
            RegisterServiceToContext(type, serviceInstance, projectContext);
        }

        public void RegisterSceneService(Type type, object serviceInstance, Scene scene)
        {
            if (sceneContexts.TryGetValue(scene, out SceneContext matchingContext))
            {
                if (matchingContext != null)
                {
                    RegisterServiceToContext(type, serviceInstance, matchingContext);
                    return;
                }

                sceneContexts.Remove(scene);
            }

            if (SceneManager.GetActiveScene() == scene)
            {
                matchingContext = new GameObject("Scene context", typeof(SceneContext)).GetComponent<SceneContext>();
                RegisterServiceToContext(type, serviceInstance, matchingContext);
            }
            else
            {
                Debug.LogWarning($"Couldn't register the scene service, because there is no scene context in the {scene.name} scene");
            }
        }

        public void RegisterGameObjectService(Type type, object serviceInstance, GameObject obj)
        {
            if (gameObjectContexts.TryGetValue(obj, out GameObjectContext matchingContext))
            {
                if (matchingContext != null)
                {
                    RegisterServiceToContext(type, serviceInstance, matchingContext);
                    return;
                }

                gameObjectContexts.Remove(obj);
            }

            matchingContext = obj.AddComponent<GameObjectContext>();
            RegisterServiceToContext(type, serviceInstance, matchingContext);
        }

        public T GetServiceFromContext<T>(ServiceContext serviceContext)
        {
            if (serviceContext == null)
            {
                Debug.LogWarning("Service context wasn't assigned when getting a service!");
                return default;
            }

            serviceContext.services.TryGetValue(typeof(T), out object serviceObject);
            return (T)serviceObject;
        }

        public bool TryGetServiceFromContext<T>(ServiceContext serviceContext, out T service)
        {
            if (serviceContext == null)
            {
                service = default;
                return false;
            }

            service = GetServiceFromContext<T>(serviceContext);
            return service != null;
        }
        public T GetService<T>()
        {
            T resultObject = default;

            var active = SceneManager.GetActiveScene();
            if (active.IsValid() &&
                TryGetSceneContext(active, out SceneContext activeSceneCtx) &&
                TryGetServiceFromContext(activeSceneCtx, out resultObject))
                return resultObject;

            if (projectContext != null &&
                TryGetServiceFromContext(projectContext, out resultObject))
                return resultObject;

            return default;
        }

        public T GetService<T>(Component caller)
        {
            if (caller == null) return GetService<T>();
            return GetService<T>(caller.gameObject);
        }
        public T GetService<T>(GameObject callerObject)
        {
            if (callerObject == null) return GetService<T>();

            T resultObject = default;

            if (TryGetGameObjectContext(callerObject, out GameObjectContext gameObjectContext) &&
                TryGetServiceFromContext(gameObjectContext, out resultObject))
                return resultObject;

            if (TryGetSceneContext(callerObject.scene, out SceneContext sceneContext) &&
                TryGetServiceFromContext(sceneContext, out resultObject))
                return resultObject;

            if (projectContext != null &&
                TryGetServiceFromContext(projectContext, out resultObject))
                return resultObject;

            return default;
        }


        public bool TryGetService<T>(GameObject callerObject, out T service)
        {
            service = GetService<T>(callerObject);
            return service != null;
        }

        private bool TryGetGameObjectContext(GameObject callerObject, out GameObjectContext gameObjectContext)
        {
            if (gameObjectContexts.TryGetValue(callerObject, out gameObjectContext))
            {
                if (gameObjectContext != null)
                    return true;

                gameObjectContexts.Remove(callerObject);
            }

            Transform currentParent = callerObject.transform.parent;
            while (currentParent != null)
            {
                if (gameObjectContexts.TryGetValue(currentParent.gameObject, out gameObjectContext))
                {
                    if (gameObjectContext != null)
                        return true;

                    gameObjectContexts.Remove(currentParent.gameObject);
                }

                currentParent = currentParent.parent;
            }

            gameObjectContext = null;
            return false;
        }
    }
}
