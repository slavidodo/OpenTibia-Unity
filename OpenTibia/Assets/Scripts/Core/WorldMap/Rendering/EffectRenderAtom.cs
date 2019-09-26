using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public sealed class EffectRenderAtom : RenderAtom, ICloneable
    {
        public int lightX { get; set; }
        public int lightY { get; set; }
        public int fieldZ { get; set; }
        public int positionZ { get; set; }

        public EffectRenderAtom(object @object = null, int x = 0, int y = 0, int z = 0, int fieldX = 0, int fieldY = 0, int fieldZ = 0, int positionZ = 0, int lightX = 0, int lightY = 0) {
            Update(@object, x, y, z, fieldX, fieldY);
        }

        public void Update(object @object = null, int x = 0, int y = 0, int z = 0, int fieldX = 0, int fieldY = 0, int fieldZ = 0, int positionZ = 0, int lightX = 0, int lightY = 0) {
            Object = @object;
            this.x = x;
            this.y = y;
            this.z = z;
            this.fieldX = fieldX;
            this.fieldY = fieldY;
            this.fieldZ = fieldZ;
            this.positionZ = positionZ;
            this.lightX = lightX;
            this.lightY = lightY;
        }

        public override void Assign(RenderAtom other) {
            if (!other || other == this)
                return;

            x = other.x;
            y = other.y;
            z = other.z;
            fieldX = other.fieldX;
            fieldY = other.fieldY;
            Object = other.Object;
        }

        public override void Reset() {
            Update();
        }

        public new EffectRenderAtom Clone() {
            return new EffectRenderAtom(Object, x, y, z, fieldX, fieldY, fieldZ, positionZ, lightX, lightY);
        }

        object ICloneable.Clone() {
            return new EffectRenderAtom(Object, x, y, z, fieldX, fieldY, fieldZ, positionZ, lightX, lightY);
        }
    }
}
