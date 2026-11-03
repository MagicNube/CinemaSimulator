<div align="center">

# Cinema Simulator

**Run the cinema, serve the queue, keep the show going.**

A university team project built with Unity and C#: sell popcorn and other products, handle orders and prepare the cinema's film listings.

![Unity](https://img.shields.io/badge/Unity-6000.0.52f1-292929?style=flat-square&logo=unity)
![CSharp](https://img.shields.io/badge/C%23-512bd4?style=flat-square)
![Project](https://img.shields.io/badge/Project-University-64748b?style=flat-square)

[My portfolio](https://jesuscaravaca.web.app/) · [Technical documentation](Cinema%20Simulator/DOCUMENTACION_COMPLETA.md)

</div>

## The project

Cinema Simulator is a cinema-service simulation developed during our degree at Universitat Politècnica de València. It received **2nd prize at the UPV Project Fair**.

The project brings together a cinema environment, customer orders, food and drink machines, and object interactions. Source documentation is mainly in Spanish.

## My contribution

I worked on the cinema's environment design, asset selection and integration, and the paths followed by characters in the ordering queue. I also contributed to the **game design document (GDD)**.

## Open the project

Use **Unity 6000.0.52f1**, the editor version recorded in the project settings. Open the nested Unity project, not the repository root:

```sh
git clone https://github.com/MagicNube/CinemaSimulator.git
```

1. In Unity Hub, choose **Add project from disk**.
2. Select the `Cinema Simulator` folder inside the clone. It contains `Assets`, `Packages` and `ProjectSettings`.
3. Let Unity restore packages and import assets.
4. Open `Assets/Scenes/01Splash.unity` and enter Play mode.

The enabled build scenes are `01Splash`, `02MainMenu` and `Javi`. Use Unity's Build Profiles with that scene list to create a standalone build.

## Find your way around

```text
Cinema Simulator/
  Assets/Scenes/       Splash, menu and gameplay scenes
  Assets/Scripts/      Orders, machines, player and UI interactions
  Assets/Prefabs/      Reusable scene objects
  Packages/           Unity dependencies
  ProjectSettings/    Editor version and build scene list
```

## Documentation

- [Complete technical documentation](Cinema%20Simulator/DOCUMENTACION_COMPLETA.md)
- [Interaction system](Cinema%20Simulator/SISTEMA_INTERACCIONES.md)
- [Quick interaction setup guide](Cinema%20Simulator/GUIA_RAPIDA_INTERACCIONES.md)
- [Canvas configuration](Cinema%20Simulator/CONFIGURACION_CANVAS.md)

## Assets and licensing

The project includes third-party Unity assets. A repository-wide `LICENSE` and a consolidated asset attribution list have not yet been added; no blanket license is claimed here.
