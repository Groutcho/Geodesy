using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTerra.DataModel.Features
{
    /// <summary>
    /// A landmark is a feature associated with a geometry.
    /// </summary>
    public class Landmark : Feature
    {
        /// <summary>
        /// Create a new instance of the <see cref="Landmark"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier assigned to this feature throughout the session.</param>
        public Landmark(ulong id) : base(id)
        {
        }

        protected override string GetStringRepresentation()
        {
            return "Landmark";
        }
    }
}
