namespace EngineName {

// TODO: I'm skipping comments here, will (maybe) add them later. Ask Philip if anything is unclear.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

    private void CreateModel(int[] indices, VertexPositionNormalTexture[] verts) {
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

        foreach (var lol in verts) {
            Console.WriteLine(lol.Position);
        }

        var device = Game1.Inst.GraphicsDevice;

        var ibo = new IndexBuffer(device, typeof (int), indices.Length, BufferUsage.None);
        ibo.SetData(indices);

        var vbo = new VertexBuffer(device,
                                   VertexPositionNormalTexture.VertexDeclaration,
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

        var indices = new List<int>();
        var verts   = new List<VertexPositionNormalTexture>();

        // Used for smooth normals.
        var indexCache = new Dictionary<Int64, int>();

        var indexCounter = 0;

        var xScale = 1.0f/mTex.Width;
        var yScale = 1.0f/255.0f;
        var zScale = 1.0f/mTex.Height;

        Func<int, int, int> calcVert = (i, j) => {
            Int64 key = ((Int64)i) << 32 | (Int64)j;

            var k = i + j*mTex.Width;

            var x = mScale*(xScale*i - 0.5f);
            var y = mScale*(yScale*mPixels[k].B - 0.5f)*mYScale;
            var z = mScale*(zScale*j - 0.5f);

            var u = (float)i / (mTex.Width  - 1);
            var v = (float)j / (mTex.Height - 1);

            var vert = new VertexPositionNormalTexture {
                Position          = new Vector3(x, y, z),
                Normal            = Vector3.Zero,
                TextureCoordinate = new Vector2(u, v)
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

        var rnd = new Random();

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
