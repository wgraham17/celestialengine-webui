# Celestial Engine - Web UI Extension
A screen component for [Celestial Engine](https://github.com/mrazza/CelestialEngine) to render UI elements using HTML and CSS by leveraging Chromium Embedded Framework. Currently only supported on Windows.

![In-game demo project screenshot](http://i.imgur.com/Y6afEfP.png)

Has support for:

* Serving local resources from the Content/WebUI folder
* Bidirectional message communication between Game and Browser
* Transparent backgrounds
* Input handling (mouse and keyboard)

Currently requires *VC++ 2013 x86* as a runtime dependency as this relies on [CefSharp](https://github.com/cefsharp/cefsharp) to function.

** NuGet package is coming soon **

## Basic Usage

* Call `WebUISystem.Initialize(this.Content.RootDirectory)` within your game's `Initialize`.
* Call `WebUISystem.Shutdown();` in an override of `OnExiting` within your game.
* Instantiate a new `WebUIScreenDrawableComponent` with the desired size.
* Create an `index.html` in your project's `Content\WebUI` folder. Ensure WebUI content files are copied to output on build.

`index.html` is the default page loaded by WebUI.  You can load other assets (such as images, Javascript, and CSS) from this folder as well. Remote sources are available as well.

### Sending data to the browser
Call `PushEventToBrowser(string name, string data)` on the WebUI Screen Drawable Component.  Complex data can be serialized to JSON if needed.

### Receiving data in the browser
For your webpage to receive data from the game, set a callback on the `window.webUICallbacks` object. The game *should* create this object in the window for you, but best practice is to ensure it's created like so:

    window.webUICallbacks = window.webUICallbacks || {};
    
Then, you can create a handler for data by adding to the `webUICallbacks` object.

    window.webUICallbacks["game:fps"] = function (data) {
        document.getElementById("lastKnownFPS").innerText = data;
    }


### Sending data to the game
Call `webUIMessage.push(name, data)` from Javascript. Data is optional.

### Receiving data in the game
Call `RegisterEventCallback(string name, Action<string> handler)` on the WebUI Screen Drawable Component to handle events from the browser.

## Questions? Comments?

Open an issue!
