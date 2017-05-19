namespace EngineName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Represents a height map created from a two-dimensional texture.</summary>
public class Heightmap {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The heightmap texture.</summary>
    private Texture2D mTex;

    /// <summary>The number of pixels to "skip" along the x-axis to create a more "low poly"
    ///          heightmap.</summary>
    private int mStepX;

    /// <summary>The number of pixels to "skip" along the y-axis to create a more "low poly"
    ///          heightmap.</summary>
    private int mStepY;

    /// <summary>Indicates whether the heightmap should have smooth shading.</summary>
    private bool mSmooth;

    /// <summary>The scale of the heightmap (along all axes).</summary>
    private float mScale;

    /// <summary>The scale of the heightmap (along the y-axis).</summary>
    private float mYScale;

    /// <summary>A one-dimensional array of heights representing the heightmap. Height(x, y) =
    ///          mHeights[x+y*mTex.Width]</summary>
    private float[] mHeights;

    private Color[] mPixels;

    /// <summary>Whether the direction of the diagonal of each quad sould be randomized.</summary>
    private bool mRandomTris;

    /// <summary>The blur factor (higher value makes heightmap smoother).</summary>
    private int mBlur;

    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    /// <summary>The renderable heightmap 3d model.</summary>
    public Model Model { get; private set; }

    /*--------------------------------------
     * NON-PUBLIC CONSTRUCTORS
     *------------------------------------*/

    /// <summary>Creates a new height map.</summary>
    /// <param name="tex">The heightmap texture.</param>
    /// <param name="stepX">The number of pixels to "skip" along the x-axis to create a more "low
    ///                      poly" heightmap.</param>
    /// <param name="stepY">The number of pixels to "skip" along the y-axis to create a more "low
    ///                      poly" heightmap.</param>
    /// <param name="smooth">Indicates whether the heightmap should have smooth shading.</param>
    /// <param name="scale">The scale of the heightmap (along all axes).</param>
    /// <param name="yScale">The scale of the heightmap (along the y-axis).</param>
    /// <param name="randomTris">Whether the direction of the diagonal of each quad sould be
    ///                          randomized.</param>
    /// <param name="blur">The blur factor (higher value makes heightmap smoother).</param>
    private Heightmap(Texture2D tex,
                      int       stepX,
                      int       stepY,
                      bool      smooth,
                      float     scale,
                      float     yScale,
                      bool      randomTris,
                      int       blur)
    {
        mTex        = tex;
        mStepX      = stepX;
        mStepY      = stepY;
        mSmooth     = smooth;
        mScale      = scale;
        mYScale     = yScale;
        mRandomTris = randomTris;
        mBlur       = blur;

        // Probably bad idea to do this amount of work in ctor but who really cares? This ctor is
        // private anyway.
        Init();
    }

    public Color ColorAt(int x, int y) {
        return mPixels[x + y*mTex.Width];
    }

		public Vector2 GetDimensions()
		{
			return new Vector2(mTex.Width, mTex.Height);
		}

		public float GetScale()
		{
			return mScale;
		}


    /// <summary>Figures out the height at the specified x- and z-coordinates. The coordinates
    ///          should be specified in world-space scale.</summary>
    /// <param name="x">The x-coordinate of the position to calculate the height at.</param>
    /// <param name="y">The x-coordinate of the position to calculate the height at.</param>
    public float HeightAt(float x, float z) {
        // TODO: This function could probably be optimized.

        // So basically, what's going on here is, we transform the given position in world-space to
        // heightmap-space and figure out over which quad (k0, k1, k2 and k3 'vertex' indices) we
        // are. Then we 2d-lerp over the quad to figure out the y-coordinate, which we transform
        // back into world-space and return.

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

        var h0 = mHeights[k0];
        var h1 = mHeights[k1];
        var h2 = mHeights[k2];
        var h3 = mHeights[k3];

        Func<float, float, float, float> f = (a, b, r) => (1.0f-r)*a + r*b;

        var h = f(f(h0, h1, fi), f(h2, h3, fi), fj);
        var y = mScale*(yScale*h - 0.5f)*mYScale - 0.05f; // 5cm under the surface

        return y;
    }

    /// <summary>Loads a heightmap and creates a model.</summary>
    /// <param name="name">The heightmap texture asset name.</param>
    /// <param name="stepX">The number of pixels to "skip" along the x-axis to create a more "low
    ///                      poly" heightmap.</param>
    /// <param name="stepY">The number of pixels to "skip" along the y-axis to create a more "low
    ///                      poly" heightmap.</param>
    /// <param name="smooth">Indicates whether the heightmap should have smooth shading.</param>
    /// <param name="scale">The scale of the heightmap (along all axes).</param>
    /// <param name="yScale">The scale of the heightmap (along the y-axis).</param>
    /// <param name="randomTris">Whether the direction of the diagonal of each quad sould be
    ///                          randomized.</param>
    /// <param name="blur">The blur factor (higher value makes heightmap smoother).</param>
    public static Heightmap Load(string name,
                                 int    stepX      = 1,
                                 int    stepY      = 1,
                                 bool   smooth     = true,
                                 float  scale      = 1.0f,
                                 float  yScale     = 1.0f,
                                 bool   randomTris = true,
                                 int    blur       = 16)
    {
        var tex = Game1.Inst.Content.Load<Texture2D>(name);
        return new Heightmap(tex, stepX, stepY, smooth, scale, yScale, randomTris, blur);
    }

    /// <summary>Creates the MonoGame/XNA renderable model.</summary>
    /// <param name="indices">The triangle indices.</param>
    /// <param name="verts">The model vertices.</param>
    private void CreateModel(int[] indices, VertexPositionNormalColor[] verts) {
        //--------------------
        // Compute normals.
        //--------------------

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

        //--------------------
        // Setup model.
        //--------------------

        // TODO: The code below is retarded. Blame XNA/MonoGame. :-(

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
            Name      = "Heightmap",
            Transform = Matrix.Identity
        };

        bone.AddMesh(mesh);

        mesh.ParentBone = bone;

        bones.Add(bone);
        meshes.Add(mesh);

        Model = new Model(device, bones, meshes);
        Model.Tag = "Heightmap";
    }

    /// <summary>Initializes the heightmap.</summary>
    private void Init() {
        //--------------------
        // Copy pixel data.
        //--------------------

        mHeights = new float[mTex.Width*mTex.Height];

        mPixels = new Color[mHeights.Length];
        mTex.GetData<Color>(mPixels);

        for (var i = 0; i < mHeights.Length; i++) {
            // Keep red channel as height. Why red channel? Because it was used in the previous
            // implementation. Works well.
            mHeights[i] = mPixels[i].R;
        }

        //--------------------
        // Blur heightmap.
        //--------------------

        var blurPixels = new float[mHeights.Length];

        Func<int, float> calcBlur = (int k) => {
            var val   = 0.0f;
            var count = 0;

            for (var i = -mBlur; i <= mBlur; i++) {
                var a = k + i*mTex.Height;
                for (var j = -mBlur; j <= mBlur; j++) {
                    var k0 = a + j;
                    if (k0 < 0 || k0 >= mHeights.Length) {
                        continue;
                    }

                    count++;
                    val += mHeights[k0];
                }
            }

            return val/count;
        };

        // Go to work, little CPUs! *cracks whip*
        Parallel.For(0, mHeights.Length, i => {
            blurPixels[i] = calcBlur(i);
        });

        // Copy back blurred values into heightmap values. We have to double-buffer to get a correct
        // result (because of the nature of blurring).
        for (var i = 0; i < mHeights.Length; i++) {
            mHeights[i] = blurPixels[i];
        }

        //--------------------
        // Setup vertices.
        //--------------------

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
            // The logic below is a bit messy - it's the result of some experimentation. Feel free
            // to tear it apart and come up with something better. :-) Basically, it computes and
            // interpolates between colors depending on the heightmap height at the given position.

            Func<float, float, float, float> f1 = (a, b, r) => (1.0f-r)*a + r*b;
            Func<Color, Color, float, Color> f = (a, b, r) =>
                new Color(f1(a.R/255.0f, b.R/255.0f, r),
                          f1(a.G/255.0f, b.G/255.0f, r),
                          f1(a.B/255.0f, b.B/255.0f, r),
                          f1(a.A/255.0f, b.A/255.0f, r));

            var r1 = 0.1f * (float)(rnd.NextDouble() - 0.5f);
            var r2 = 0.1f * (float)(rnd.NextDouble() - 0.5f);
            var r3 = 0.1f * (float)(rnd.NextDouble() - 0.5f);

            var rockColor  = new Color(0.6f+r1, 0.6f+r1, 0.65f+r1);
            var grassColor = new Color(0.2f+0.3f*r1, 0.4f+0.3f*r2, 0.3f+0.3f*r3);
            var sandColor  = new Color(0.3f+0.3f*r1, 0.1f+0.3f*r2, 0.0f+0.3f*r2);

            var color = grassColor;

            if (y < 0.0f) {
                y += 0.4f;
                var r = 2.0f/(1.0f + (float)Math.Pow(MathHelper.E, 40.0f*y)) - 1.0f;
                r = Math.Max(Math.Min(r, 1.0f), 0.0f);
                color = f(grassColor,
                          sandColor,
                          r);
            }

            if (y > 0.05f) {
                y -= 0.05f;
                var r = 2.0f/(1.0f + (float)Math.Pow(MathHelper.E, -90.0f*y)) - 1.0f;
                r = Math.Max(Math.Min(r, 1.0f), 0.0f);
                color = f(grassColor,
                          rockColor,
                          r);
            }

            return color;
        };

        Func<int, int, int> calcVert = (i, j) => {
            Int64 key = ((Int64)i) << 32 | (Int64)j;

            var k = i + j*mTex.Width;

            var x = mScale*(xScale*i - 0.5f);
            var y = mScale*(yScale*mHeights[k] - 0.5f)*mYScale;
            var z = mScale*(zScale*j - 0.5f);

            // var u = (float)i / (mTex.Width  - 1);
            // var v = (float)j / (mTex.Height - 1);

            var vert = new VertexPositionNormalColor {
                Position = new Vector3(x, y, z),
                Normal   = Vector3.Zero,
                Color    = calcColor(x/mScale, (y/mScale)/mYScale, z/mScale)
            };

            if (mSmooth) {
                // So, if we're doing a smooth heightmap, cache the vertex index so we can reuse it.
                // This means triangles end up sharing vertices, in which case the normal
                // computation will give smooth triangles.

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
                // If random tris are enabled, randomize the diagonal of each triangle.
                if (!mRandomTris || rnd.Next(0, 2) == 1) {
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
