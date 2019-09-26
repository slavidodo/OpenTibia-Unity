using System;
using OpenTibiaUnity.Core.Creatures;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class RenderAtom : ICloneable
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int fieldX { get; set; }
        public int fieldY { get; set; }
        public object Object { get; set; }

        public RenderAtom(object @object = null, int x = 0, int y = 0, int z = 0, int fieldX = 0, int fieldY = 0) {
            Update(@object, x, y, z, fieldX, fieldY);
        }

        public void Update(object @object = null, int x = 0, int y = 0, int z = 0, int fieldX = 0, int fieldY = 0) {
            Object = @object;
            this.x = x;
            this.y = y;
            this.z = z;
            this.fieldX = fieldX;
            this.fieldY = fieldY;
        }

        public virtual void Assign(RenderAtom other) {
            if (!other || other == this)
                return;

            x = other.x;
            y = other.y;
            z = other.z;
            fieldX = other.fieldX;
            fieldY = other.fieldY;
            Object = other.Object;
        }

        public virtual void Reset() {
            Update();
        }

        public virtual RenderAtom Clone() {
            return new RenderAtom(Object, x, y, z, fieldX, fieldY);
        }

        object ICloneable.Clone() {
            return new RenderAtom(Object, x, y, z, fieldX, fieldY);
        }

        public static bool operator !(RenderAtom instance) {
            return instance == null;
        }
    }
}
