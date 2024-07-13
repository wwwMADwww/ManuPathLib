using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using ManuPath.Transforms;

namespace ManuPath.Figures
{
    public interface IFigure: IGraphic, ICloneable
    {
        string Id { get; set; }

        Fill Fill { get; set; }

        Stroke Stroke { get; set; }

        /// <summary>
        /// Get bounding box. Transforms are ignored.
        /// </summary>
        RectangleF GetBounds();

        /// <summary>
        /// Transforms to be applied. Should be applied implicitly via <see cref="Transform"/> or by settings special flags.
        /// </summary>
        ITransform[] Transforms { get; set; }

        /// <summary>
        /// Applying <see cref="Transforms"/> to figure. Allways returns new IFigure with empty Transforms.
        /// </summary>
        IFigure Transform();

        /// <summary>
        /// Convert figure to Path. 
        /// </summary>
        /// <param name="transformed">Apply transforms during conversion.</param>
        IFigure ToPath(bool transformed);
    }
}
