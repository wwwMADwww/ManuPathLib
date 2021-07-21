# ManuPathLib

![example](example.png?raw=true "example")

Converting vector paths to series of dots or line segments "manually", no graphic library is used.

Developed for my specific applications and not tested properly, use at your own risk.

# Features

- Primitives: Dot (for fills), line segment, cubic bezier curve.
- Fill rules: Even-odd, non-zero winding.
- Divide primitives to specific segments number.
- Divide primitives to segments of specific length with smooth transition from one primitive to the next within same Path.
- Grid pattern fill with randomization.
- Random fill.

# Example

No proper examples for now, but here is the test project [ManuPathTest](ManuPathTest). It loads one of the [SVG files](ManuPathTest/svg) as ManuPath primitives, converts these to the dots and lines and draws using <span>SFML.Net</span>.

# Thanks

Big thanks to [A Primer on Bézier Curves](https://pomax.github.io/bezierinfo)
