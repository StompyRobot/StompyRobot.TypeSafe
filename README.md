TypeSafe
=====

One principle advantage of type safety is catching errors at compile time.
TypeSafe brings this same advantage to your Unity-powered game when loading resources, scenes or using layers and tags.

Key Features
----

- Automatically scans your project and generates **strong-typed classes** than can be used to replace raw strings in your code.
- Using these safe classes ensures that any resource, layer or scene you try and use in code **always exists**.
- Moving, renaming or deleting a resource will cause compile errors at the place in code they were used.
- Read more on [the website](https://stompyrobot.uk/tools/typesafe/).

Installing
----

To include the package in your own project you can add this git repository directly to your manifest.json file:

`<UnityProjectPath>/Packages/manifest.json`
```
{
  "dependencies": {
    /* existing entries */
    "com.stompyrobot.typesafe": "https://github.com/stompyrobot/StompyRobot.TypeSafe.git#upm"
  }
}
```


About this repository
----

The "development" Unity project is in the `StompyRobot.TypeSafe/` directory. Open this folder in Unity to edit the package source directly in a project with some test resource files and configuration.
The actual package source is located at `StompyRobot.TypeSafe/Packages/StompyRobot.TypeSafe/`