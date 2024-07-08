using System.Drawing;
using System.Numerics;

namespace ManuPath.Figures.Primitives
{
    public class Dot : IPathPrimitive
    {
        public Dot(Vector2 pos)
        {
            Pos = pos;
        }

        public Vector2 Pos { get; set; }

        public Vector2 FirstPoint => Pos;
        public Vector2 LastPoint => Pos;

        public RectangleF Bounds => new RectangleF(Pos.X, Pos.Y, 0, 0);


        public void Reverse() { }


        public override bool Equals(object obj)
        {
            return obj is Dot dot &&
                   Pos.Equals(dot.Pos);
        }

        public bool Equals(IPathPrimitive other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            return 1731973265 + Pos.GetHashCode();
        }

    }

}
