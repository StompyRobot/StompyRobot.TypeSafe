TypeSafe
=====

TypeSafe scans your project and generates strong-typed static classes from your resources, layers and tags. Using these classes instead of 'naked-strings' in your code will dramatically reduce the number of runtime errors caused by mistyping, moving or renaming resources, layers or tags.

Key Features
----

- Automatically scans your project and generates **strong-typed classes** than can be used to replace raw strings in your code.
- Using these safe classes ensures that any resource, layer or scene you try and use in code **always exists**.
- Moving, renaming or deleting a resource will cause compile errors at the place in code they were used.

For detailed documentation, visit the web page at https://www.stompyrobot.uk/tools/typesafe/documentation

Trigger a scan from the Assets menu (Assets/TypeSafe Refresh)

The settings and welcome windows can be opened from the Window menu (Window/TypeSafe)

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

Restrictions:
----

 - This asset is provided open source and free of charge under the MIT license.
 
Support:
----

If you encounter problems while using TypeSafe, feel free to post in the thread on the Unity forums or open an issue on this GitHub repository.

Please be considerate that this asset is provided free of charge, support is provided by me in my (very limited) free time.

About this repository
----

The "development" Unity project is in the `StompyRobot.TypeSafe/` directory. Open this folder in Unity to edit the package source directly in a project with some test resource files and configuration.
The actual package source is located at `StompyRobot.TypeSafe/Packages/StompyRobot.TypeSafe/`

Credits:
----

- Programming/Design by Simon Moles @ Stompy Robot (simon@stompyrobot.uk, www.stompyrobot.uk)
- Settings/Welcome window header background provided by Subtle Patterns (www.subtlepatterns.com)