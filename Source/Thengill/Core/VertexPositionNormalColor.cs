using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Thengill.Core {
    public struct VertexPositionNormalColor {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public VertexPositionNormalColor(Vector3 Position, Vector3 Normal, Color Color) {
            this.Position = Position;
            this.Normal = Normal;
            this.Color = Color;
        }
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}
