# Celestial Engine - Web UI Extension
A screen component for [Celestial Engine](https://github.com/mrazza/CelestialEngine) to render UI elements using HTML and CSS by leveraging Chromium Embedded Framework. Currently only supported on Windows.

![In-game Hello World screenshot](http://i.imgur.com/N0w6zdk.png)

Has support for:

* Serving local resources from the Content/WebUI folder
* Bidirectional message communication between Game and Browser
* Transparent backgrounds
* Input handling (mouse and keyboard)

Currently requires *VC++ 2013 x86* as a runtime dependency as this relies on [CefSharp](https://github.com/cefsharp/cefsharp) to function.
