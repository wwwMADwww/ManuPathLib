using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ManuPath.Figures
{
    public enum FillRule { EvenOdd, NonZeroWinding }

    public class Fill
    {
        public Color Color { get; set; }

        public FillRule Rule { get; set; }

        public Fill Clone()
        {
            return new Fill()
            {
                Color = Color,
                Rule = Rule
            };
        }
    }
}
