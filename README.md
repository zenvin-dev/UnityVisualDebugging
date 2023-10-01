# Unity Visual Debugging
Small package for drawing debug lines & shapes, as well as debugging variable values during runtime.

---
## Visual Debugger
Uses pooled [line renderers](https://docs.unity3d.com/ScriptReference/LineRenderer.html) for rendering wireframe shapes in a scene.

### Usage
Just like with [`UnityEngine.Debug`](https://docs.unity3d.com/ScriptReference/Debug.html) methods, simply call the desired method(s) of `Zenvin.VisualDebugging.VisualDebugger` once a frame for every shape you want to be drawn.

### Draw methods list
* `DrawCircle` - Draws a circle at a given position, a given rotation and a given number of vertices.
* `DrawLine` - Draws a line between two points.
* `DrawPath` - Draws a path between any number of points.
* `DrawRay` - Draws a line starting at a given point, going in a given direction for a given length.
* `DrawRectangle` - Draws a rectangle of a given size at a given position.
* `DrawSphere` - Draws a wire sphere of a given size at a given position. The circles making up the sphere have 32 vertices each.


## Value Debugger
Allows the display of labelled values via [`OnGUI`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnGUI.html). \
[`Func<string>`](https://learn.microsoft.com/en-us/dotnet/api/system.func-1?view=net-7.0) delegate methods are used to update displayed values in a specific interval. This defaults to once every 0.25 seconds, but may be set manually using `SetUpdateInterval(float)`

### Usage
To display a value, simply call the `static` method `RegisterTarget(DebugTarget)` on `Zenvin.VisualDebugging.ValueDebugger`, passing it a valid target.
```csharp
Zenvin.VisualDebugging.ValueDebugger.RegisterTarget(new DebugTarget(Foo, "Some Value"));    // assuming Foo is a method with return type string.
```
The `RegisterTarget(DebugTarget)` method will return a numeric ID, which can be used to remove the value from display again using `RemoveTarget(int)`.


## Gizmo Utility
A collection of static methods to expand the functionalilty of Unity's `Gizmos` class.

### DrawGizmos methods list
* `DrawArrow` - Draws an arrow between two points, with a given normal, size, and color.
* `DrawLine` - Draws a line between two points with a given color.
* `DrawPointLines` - Draws 3 lines of variable length and color that intersect at a given point.
* `DrawRay` - Draws a ray of a given color.
* `DrawWireAngle` - Draws the outline of an angle at a given position, rotation and with a given radius and color.
* `DrawWireAngle` - Draws a circle at a given position, rotation and with a given radius and color.
* `DrawRect` - Draws a rect at a given position, with a given size, rotation and color.