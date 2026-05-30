# Furkan Tokkan Service Locator

Small scoped service locator package for Unity projects.

## Scopes

- `ProjectContext`: global services that should survive scene loads.
- `SceneContext`: services owned by the active scene.
- `GameObjectContext`: services scoped to a GameObject and its children.

Lookup order for `GetService<T>(GameObject)` is GameObject hierarchy, scene, then project.
Lookup order for `GetService<T>()` is active scene, then project.

## Setup

1. Add one `ServiceLocator` component to the bootstrap scene.
2. Add a `ProjectContext`, `SceneContext`, or `GameObjectContext` where services should be registered.
3. Add service `MonoBehaviour` instances to the context's `servicesToRegister` list.

Each registered component is exposed by its concrete type and implemented interfaces.

## Install With Unity Package Manager

Add the package to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.furkantokkan.service-locator": "https://github.com/furkantokkan/unity-service-locator.git#v0.1.0"
  }
}
```

For local development inside another Unity project, place this folder under `Packages/com.furkantokkan.service-locator`.

## API

```csharp
using Modules.ServiceLocator;

var service = ServiceLocator.Instance.GetService<IMyService>(gameObject);
ServiceLocator.Instance.RegisterProjectService(typeof(IMyService), serviceInstance);
```

## Compatibility

The package keeps the `Modules.ServiceLocator` namespace and `Modules.ServiceLocator` assembly name so existing Unity projects can migrate without code changes.
