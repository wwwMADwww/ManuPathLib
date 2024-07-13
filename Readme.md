# ManuPathLib

![example](example.png?raw=true "example")

Converting vector figures to series of dots or line segments "manually", no graphic library is used.

Developed for my specific applications and not tested properly, use at your own risk.

# Features

- Figures: Ellipse, Rectangle, Path.
- Path Primitives: Dot, line segment, cubic bezier curve.
- Transformations: Scale, Translate, Rotate, Matrix.
- Stroke generator:
  - Divide Path Primitives to specific segments number.
  - Divide Path Primitives to segments of specific length with smooth transition from one primitive to the next within same Path.
- Fill generator:
  - Fill rules: Even-odd, Non-zero winding.
  - Grid pattern fill with randomization.

# Example

No proper examples for now, check the [ManuPathTest](ManuPathTest) project to see possible operations.

# Thanks

Big thanks to [A Primer on Bézier Curves](https://pomax.github.io/bezierinfo)
