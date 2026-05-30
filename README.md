# Furkan Tokkan Service Locator

Scoped service lookup for Unity projects: project-wide, scene-wide, and GameObject-hierarchy services without pulling in a full dependency injection container.

[![Unity 6000.0+](https://img.shields.io/badge/Unity-6000.0%2B-black)](https://unity.com/)
[![UPM Git URL](https://img.shields.io/badge/UPM-Git%20URL-blue)](https://docs.unity3d.com/Manual/upm-git.html)

Use this package when a Unity project needs a small, explicit service registry that can answer a simple question:

> "Which service should this object use in this scope?"

It is intentionally smaller than Onity, Zenject, or VContainer. It does not do constructor injection, object graph building, lifecycle orchestration, analyzers, or reactive/event infrastructure. It gives legacy and production Unity code a predictable scoped lookup layer that is easy to inspect and migrate gradually.

---

## Why This Package

Many Unity projects eventually grow three kinds of services:

- global services that should survive scene loads,
- scene services that belong to the currently loaded gameplay/menu scene,
- local services owned by a specific prefab, character, UI root, or hierarchy.

Unity's built-in options usually push teams toward scene searches, static singletons, or duplicated references in many prefabs. This package keeps the model simple:

- register services in a context,
- resolve from the caller's GameObject when scope matters,
- fall back from local scope to scene scope to project scope.

The result is not a full DI framework. It is a small bridge for codebases that need better boundaries without a large architectural migration.

---

## Features

- `ProjectContext` for services shared across scenes.
- `SceneContext` for services owned by one scene.
- `GameObjectContext` for services scoped to a GameObject and its children.
- Inspector registration through `servicesToRegister`.
- Runtime registration for project, scene, and GameObject scopes.
- Concrete type and interface registration for `MonoBehaviour` services.
- Existing compatibility namespace: `Modules.ServiceLocator`.
- Existing compatibility assembly name: `Modules.ServiceLocator`.
- No third-party runtime dependencies.

---

## Install

Add the package to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.furkantokkan.service-locator": "https://github.com/furkantokkan/unity-service-locator.git#v0.1.1"
  }
}
```

For local package development, clone this repository into your Unity project's `Packages` folder:

```text
Packages/com.furkantokkan.service-locator
```

---

## Quick Start

Create a service contract and implementation:

```csharp
public interface IAudioService
{
    void Play(string key);
}

public sealed class AudioService : MonoBehaviour, IAudioService
{
    public void Play(string key)
    {
        // Play audio by key.
    }
}
```

Add these components to your bootstrap scene:

- `ServiceLocator`
- `ProjectContext`
- `AudioService`

Then drag `AudioService` into `ProjectContext.servicesToRegister`.

Resolve from gameplay code:

```csharp
using Modules.ServiceLocator;
using UnityEngine;

public sealed class ButtonAudio : MonoBehaviour
{
    public void PlayClick()
    {
        ServiceLocator.Instance
            ?.GetService<IAudioService>(gameObject)
            ?.Play("button_click");
    }
}
```

---

## Scopes

### ProjectContext

Use for app-level services that should survive scene changes:

- account/session state,
- save data,
- analytics,
- remote config,
- shared inventory/economy services.

`ProjectContext` is marked `DontDestroyOnLoad` by the locator.

### SceneContext

Use for services that belong to the active scene:

- battle state,
- scene input routing,
- level rules,
- scene-specific factories,
- UI presenters owned by one scene.

### GameObjectContext

Use for services owned by a prefab or hierarchy:

- character equipment cache,
- local combat view services,
- UI panel-specific coordinators,
- prefab-specific rules.

When resolving from a `GameObject`, the locator checks that object first, then walks up its parent hierarchy until it finds a `GameObjectContext`.

---

## Lookup Order

```text
GetService<T>(GameObject caller)

1. GameObjectContext on caller
2. GameObjectContext on caller parent hierarchy
3. SceneContext for caller.scene
4. ProjectContext
```

```text
GetService<T>()

1. SceneContext for SceneManager.GetActiveScene()
2. ProjectContext
```

Use the caller-aware overload when a prefab, battle object, UI panel, or scene-local object may need a scoped override:

```csharp
var config = ServiceLocator.Instance.GetService<IConfigService>(gameObject);
```

Use the parameterless overload only when scene/project scope is enough:

```csharp
var config = ServiceLocator.Instance.GetService<IConfigService>();
```

---

## Runtime Registration

You can register services manually when they are created dynamically:

```csharp
ServiceLocator.Instance.RegisterProjectService(typeof(IAudioService), audioService);
ServiceLocator.Instance.RegisterSceneService(typeof(IBattleRules), battleRules, gameObject.scene);
ServiceLocator.Instance.RegisterGameObjectService(typeof(IHeroServices), heroServices, heroRoot);
```

Generic registration is available when you already have the context:

```csharp
ServiceLocator.Instance.RegisterServiceToContext<IMyService>(service, context);
```

---

## Registration Rules

For every `MonoBehaviour` listed in `servicesToRegister`, the context registers:

- the concrete component type,
- every interface implemented by that component.

If two services in the same context expose the same type or interface, the first registration wins for inspector-based registration.

Runtime registration through `RegisterServiceToContext`, `RegisterProjectService`, `RegisterSceneService`, and `RegisterGameObjectService` writes directly to the context map for that type.

---

## Execution Order

The package uses negative execution order values so contexts register early:

- `ServiceLocator`: `-1505`
- `ProjectContext`: `-1504`
- `SceneContext`: `-1503`
- `GameObjectContext`: `-1505`

Keep your bootstrap locator in an early-loaded scene. Scene/prefab services should not assume that unrelated objects have already run `Start`.

---

## Design Goals

- Small enough to read in one sitting.
- No hidden reflection beyond interface discovery during context registration.
- No dependency on a project-specific namespace, Firebase, analytics, Addressables, or UI framework.
- Compatible with existing projects that already use `Modules.ServiceLocator`.
- Easy to replace later with a full DI framework when the project is ready.

---

## Non-Goals

This package deliberately does not provide:

- constructor injection,
- automatic object graph construction,
- named/keyed registrations,
- scoped lifetime disposal,
- validation tooling,
- async initialization orchestration,
- reactive state or message broker APIs.

For new large systems that need those features, use a real DI/reactive/event stack instead of stretching this package beyond its purpose.

---

## Migration Notes

The package preserves:

- namespace: `Modules.ServiceLocator`
- assembly name: `Modules.ServiceLocator`
- core component names: `ServiceLocator`, `ProjectContext`, `SceneContext`, `GameObjectContext`

That means projects can usually move the old source files into this package without changing `using` statements or assembly references.

When migrating a Unity project, keep the original `.meta` files for the runtime scripts if scenes or prefabs already reference these components.

---

## Package Layout

```text
Runtime/
  GameObjectContext.cs
  ProjectContext.cs
  SceneContext.cs
  ServiceContext.cs
  ServiceLocator.cs
  Modules.ServiceLocator.asmdef
package.json
README.md
CHANGELOG.md
```

---

## License

No license file is included yet. Add a license before accepting external contributions or treating this as an open-source package for third-party reuse.
