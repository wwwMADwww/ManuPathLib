using ManuPath.Transforms;
using System.Drawing;
using System.Numerics;

namespace ManuPath.Figures.PathPrimitives
{
    public class Dot : IPathPrimitive
    {
        public Dot() { }

        public Dot(Vector2 pos)
        {
            Pos = pos;
        }

        public Vector2 Pos { get; set; }

        public Vector2 FirstPoint => Pos;
        public Vector2 LastPoint => Pos;

        public RectangleF GetBounds()
        {
            return new RectangleF(Pos.X, Pos.Y, 0, 0);
        }

        public void Reverse() { }

        public IPathPrimitive Transform(ITransform[] transforms)
        {
            var dot = (Dot)Clone();

            foreach (var transform in transforms)
            {
                dot.Pos = transform.Transform(dot.Pos, dot.Pos);
            }

            return dot;
        }

        public object Clone()
        {
            return new Dot(Pos);
        }
    }

}
