using System;

namespace OpenTerra.DataModel.Features
{
    /// <summary>
    /// The feature is the root of the data model in OpenTerra.
    /// It represents an element of geographic interest, such as a location, a region, a layer, a network...
    /// </summary>
    public abstract class Feature
    {
        static Random random;

        static Feature()
        {
            random = new Random(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Generate a unique identifier.
        /// </summary>
        /// <returns></returns>
        public static ulong NewId()
        {
            unchecked
            {
                long id = 17;
                id = (id + 23 * random.Next()) << 32;
                id = id + 23 * random.Next();
                return (ulong)id;
            }
        }

        /// <summary>
        /// The unique identifier for this feature.
        /// </summary>
        public readonly ulong Id;

        /// <summary>
        /// The name of this feature (optional).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Should the feature be displayed on the globe ?
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="Feature"/> class with the specified identifier. 
        /// </summary>
        /// <param name="id">The unique identifier assigned to this feature throughout the session.</param>
        public Feature(ulong id)
        {
            this.Id = id;
        }

        public override string ToString()
        {
            return string.Format("[Feature #{0} {1}]", Id, GetStringRepresentation());
        }

        protected virtual string GetStringRepresentation()
        {
            return "(feature)";
        }
    }
}
