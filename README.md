# Unity Visual Debugging
Small package for drawing debug lines & shapes in play mode. \
Uses pooled [line renderers](https://docs.unity3d.com/ScriptReference/LineRenderer.html) for rendering.

## Usage
Just like with [`UnityEngine.Debug`](https://docs.unity3d.com/ScriptReference/Debug.html) methods, simply call the desired method(s) of `Zenvin.VisualDebugging.VisualDebugger` once a frame for every shape you want to be drawn.

## Draw methods list
* `DrawCircle` - Draws a circle at a given position, a given rotation and a given number of vertices.
* `DrawLine` - Draws a line between two points.
* `DrawPath` - Draws a path between any number of points.
* `DrawRay` - Draws a line starting at a given point, going in a given direction for a given length.
* `DrawRectangle` - Draws a rectangle of a given size at a given position.
* `DrawSphere` - Draws a wire sphere of a given size at a given position. The circles making up the sphere have 32 vertices each.
