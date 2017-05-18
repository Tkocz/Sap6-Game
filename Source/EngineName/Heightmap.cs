namespace EngineName {

// TODO: I'm skipping comments here, will (maybe) add them later. Ask Philip if anything is unclear.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Utils;
using EngineName.Components;


public class Heightmap {
    private Texture2D mTex;

    private int mStepX;
    private int mStepY;

    private bool mSmooth;

    private float mScale;
    private float mYScale;

    private Color[] mPixels;
    public Model Model { get; private set; }

    private Heightmap(Texture2D tex, int stepX, int stepY, bool smooth, float scale, float yScale) {
        mTex    = tex;
        mStepX  = stepX;
        mStepY  = stepY;
        mSmooth = smooth;
        mScale  = scale;
        mYScale = yScale;

        // Probably bad idea to do this amount of work in ctor but who really cares?
        Init();
    }

    public float HeightAt(float x, float z) {
        var xScale = 1.0f/mTex.Width;
        var yScale = 1.0f/255.0f;
        var zScale = 1.0f/mTex.Height;

        var i0 = (x/mScale + 0.5f)/xScale;
        var j0 = (z/mScale + 0.5f)/zScale;

        var i = (int)i0;
        var j = (int)j0;

        i /= mStepX;
        j /= mStepY;

        i *= mStepX;
        j *= mStepY;

        var fi = (float)(i0 - i)/mStepX;
        var fj = (float)(j0 - j)/mStepY;

        var k0 = i            + j           *mTex.Width;
        var k1 = (i + mStepX) + j           *mTex.Width;
        var k2 = i            + (j + mStepY)*mTex.Width;
        var k3 = (i + mStepX) + (j + mStepY)*mTex.Width;

        var n = mTex.Width*mTex.Height - 1;
        if (k0 < 0 || k1 < 0 || k2 < 0 || k3 < 0
         || k0 > n || k1 > n || k2 > n || k3 > n)
        {
            return 0.0f;
        }

        var c0 = mPixels[k0].B;
        var c1 = mPixels[k1].B;
        var c2 = mPixels[k2].B;
        var c3 = mPixels[k3].B;

        Func<float, float, float, float> f = (a, b, r) => (1.0f-r)*a + r*b;

        var c = f(f(c0, c1, fi), f(c2, c3, fi), fj);
        var y = mScale*(yScale*c - 0.5f)*mYScale;

        return y;
    }

    public static Heightmap Load(string name,
                                 int stepX=1,
                                 int stepY=1,
                                 bool smooth=true,
                                 float scale=1.0f,
                                 float yScale=1.0f)
    {
        var tex = Game1.Inst.Content.Load<Texture2D>(name);
        return new Heightmap(tex, stepX, stepY, smooth, scale, yScale);
    }

    private void CreateModel(int[] indices, VertexPositionNormalColor[] verts) {
        for (var i = 0; i < indices.Length-2; i += 3) {
            var i0 = indices[i];
            var i1 = indices[i+1];
            var i2 = indices[i+2];

            Vector3 a = verts[i0].Position - verts[i2].Position;
            Vector3 b = verts[i0].Position - verts[i1].Position;
            Vector3 n = Vector3.Cross(a, b);

            verts[i0].Normal += n;
            verts[i1].Normal += n;
            verts[i2].Normal += n;
        }

        for (var i = 0; i < verts.Length; i++) {
            verts[i].Normal.Normalize();
        }

        var device = Game1.Inst.GraphicsDevice;

        var ibo = new IndexBuffer(device, typeof (int), indices.Length, BufferUsage .None);
        ibo.SetData(indices);

        var vbo = new VertexBuffer(device,
                                   VertexPositionNormalColor.VertexDeclaration,
                                   verts.Length,
                                   BufferUsage.None);
        vbo.SetData(verts);

        var meshes = new List<ModelMesh>();
        var parts  = new List<ModelMeshPart>();
        var bones = new List<ModelBone>();

        parts.Add(new ModelMeshPart {
            VertexBuffer = vbo,
            NumVertices  = verts.Length,

            IndexBuffer    = ibo,
            PrimitiveCount = indices.Length / 3,
        });

        var mesh = new ModelMesh(device, parts);

        parts[0].Effect = new BasicEffect(device);

        var bone = new ModelBone {
            Name = "Heightmap",
            Transform = Matrix.Identity
        };

        bone.AddMesh(mesh);

        mesh.ParentBone = bone;

        bones.Add(bone);
        meshes.Add(mesh);

        Model = new Model(device, bones, meshes);
    }

    private void Init() {
        mPixels = new Color[mTex.Width*mTex.Height];
        mTex.GetData<Color>(mPixels);

        var blurPixels = new int[mPixels.Length];

        Func<int, int> calcBlur = (int k) => {
            const int n = 16;
            int val = 0;
            int count = 0;

            for (var i = -n; i <= n; i++) {
                var a = k + i*mTex.Height;
                for (var j = -n; j <= n; j++) {
                    var k0 = a + j;
                    if (k0 < 0 || k0 >= mPixels.Length) {
                        continue;
                    }

                    count++;
                    val += mPixels[k0].B;
                }
            }

            return val/count;
        };

        Parallel.For(0, mPixels.Length, i => {
            blurPixels[i] = calcBlur(i);
        });

        for (var i = 0; i < mPixels.Length; i++) {
            mPixels[i] = new Color(0, 0, blurPixels[i]);
        }

        var indices = new List<int>();
        var verts   = new List<VertexPositionNormalColor>();

        // Used for smooth normals.
        var indexCache = new Dictionary<Int64, int>();

        var indexCounter = 0;

        var xScale = 1.0f/(mTex.Width-1);
        var yScale = 1.0f/255.0f;
        var zScale = 1.0f/(mTex.Height-1);

        var rnd = new Random();

        Func<float, float, float, Color> calcColor = (x, y, z) => {
            Func<float, float, float, float> f1 = (a, b, r) => (1.0f-r)*a + r*b;
            Func<Color, Color, float, Color> f = (a, b, r) =>
                new Color(f1(a.R/255.0f, b.R/255.0f, r),
                          f1(a.G/255.0f, b.G/255.0f, r),
                          f1(a.B/255.0f, b.B/255.0f, r),
                          f1(a.A/255.0f, b.A/255.0f, r));

            var grassColor = new Color(0.5f, 0.66f, 0.5f);
            var sandColor = new Color(0.9f, 0.8f, 0.6f);

            var color = grassColor;

            if (y < 0.0f) {
                y += 0.4f;
                color = f(grassColor,
                          sandColor,
                          2.0f/(1.0f + (float)Math.Pow(MathHelper.E, 40.0f*y)) - 1.0f);
            }

            var c1 = 0.05f * (float)(rnd.NextDouble() - 0.5f);
            var c2 = 0.05f * (float)(rnd.NextDouble() - 0.5f);
            var c3 = 0.05f * (float)(rnd.NextDouble() - 0.5f);
            color = new Color(color.R/255.0f + c1, color.G/255.0f + c2, color.B/255.0f + c3, 1.0f);

            return color;
        };

        Func<int, int, int> calcVert = (i, j) => {
            Int64 key = ((Int64)i) << 32 | (Int64)j;

            var k = i + j*mTex.Width;

            var x = mScale*(xScale*i - 0.5f);
            var y = mScale*(yScale*mPixels[k].B - 0.5f)*mYScale;
            var z = mScale*(zScale*j - 0.5f);

            // var u = (float)i / (mTex.Width  - 1);
            // var v = (float)j / (mTex.Height - 1);

            var vert = new VertexPositionNormalColor {
                Position = new Vector3(x, y, z),
                Normal   = Vector3.Zero,
                Color    = calcColor(x/mScale, (y/mScale)/mYScale, z/zScale)
            };

            if (mSmooth) {
                if (!indexCache.ContainsKey(key)) {
                    verts.Add(vert);
                    indexCache[key] = indexCounter++;
                }

                return indexCache[key];
            }

            verts.Add(vert);
            return indexCounter++;
        };

        Action<int, int, int, int, int, int> calcTri = (i0, j0, i1, j1, i2, j2) => {
            var v0 = calcVert(i0, j0);
            var v1 = calcVert(i1, j1);
            var v2 = calcVert(i2, j2);

            indices.Add(v0);
            indices.Add(v1);
            indices.Add(v2);
        };

        for (var i = 0; i < mTex.Width-mStepX; i += mStepX) {
            for (var j = 0; j < mTex.Height-mStepY; j += mStepY) {
                if (rnd.Next(0, 2) == 1) {
                    calcTri(i       , j       , i+mStepX, j       , i+mStepX, j+mStepY);
                    calcTri(i+mStepX, j+mStepY, i       , j+mStepY, i       , j);
                }
                else {
                    calcTri(i       , j+mStepY, i       , j       , i+mStepX, j);
                    calcTri(i+mStepX, j       , i+mStepX, j+mStepY, i       , j+mStepY);
                }
            }
        }

        CreateModel(indices.ToArray(), verts.ToArray());
    }
}

}
