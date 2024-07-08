using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ManuPath.Figures
{
    public class Stroke
    {
        public Color Color { get; set; }

        public Stroke Clone() 
        {
            return new Stroke()
            {
                Color = Color
            };
        } 
    }
}
