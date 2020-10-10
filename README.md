TypeSafe
=====


About this repository
----

The "development" Unity project is in the `StompyRobot.TypeSafe/` directory. Open this folder in Unity to edit the package source directly in a project with some test resource files and configuration.
The actual package source is located at `StompyRobot.TypeSafe/Packages/StompyRobot.TypeSafe/`

To include the package in your own project you can add this git repository directly to your manifest.json file:

`<UnityProjectPath>/Packages/manifest.json`
```
{
  "dependencies": {
    /* existing entries */
    "com.stompyrobot.typesafe": "git://github.com/stompyrobot/StompyRobot.TypeSafe.git"
  }
}
```
