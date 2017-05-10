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

namespace EngineName.Systems
{
	public class MapSystem : EcsSystem
	{
		private GraphicsDevice mGraphicsDevice;
		private int chunksplit = 1;
		private BasicEffect basicEffect;
        private float[,] mHeightData;

        public override void Init()
		{
			mGraphicsDevice = Game1.Inst.GraphicsDevice;
			basicEffect = new BasicEffect(mGraphicsDevice);
			basicEffect.VertexColorEnabled = true;
			base.Init();
		}
        // Note: X is correct axis, but Y in this case actually is Z in game world (Y is for height)
        public float HeightPosition(float x, float y) {
            foreach (var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>())
            {
                if (renderable.Value.GetType() != typeof(CHeightmap))
                    continue;
                var heightmap = (CHeightmap)renderable.Value;
                var key = renderable.Key;
                CTransform transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);

                x -= transform.Position.X;
                y -= transform.Position.Z;

                if (x < 0 || x > heightmap.Image.Width || y < 0 || y > heightmap.Image.Height)
                    return 0;
                // get four closest vertices
                int lowX = (int)Math.Floor(x);
                int highX = (int)Math.Floor(x + 1);
                int lowY = (int)Math.Floor(y);
                int highY = (int)Math.Floor(y + 1);

                var A = mHeightData[lowX, lowY];
                var B = mHeightData[highX, lowY];
                var C = mHeightData[lowX, highY];
                var D = mHeightData[highX, highY];

                // lerp func
                Func<float, float, float, float> f = (a, b, sigma) => (1.0f-sigma)*a + sigma*b;

                // fractional parts
                var fX = x - lowX;
                var fY = y - lowY;

                // 2d-interpolate over the square
                var h = f(f(A, B, fX), f(C, D, fX), fY);

                // P = (x, y)
                // f(a,b,x) = xa + (1-x)b
                // Pz = f(f(A,B,Px), f(C, D, Px), Py

                return h;
            }
            return 0;
        }

		private Color materialPick(int decimalCode)
		{
            var sand = Color.FromNonPremultiplied(194, 178, 128, 255);
            var grass = Color.ForestGreen;
            float sandStop = 225;
            float grassStart = 235;
            if (decimalCode < sandStop)
                return sand;
            if(decimalCode < grassStart) {
                var progress = (decimalCode - sandStop) / (grassStart - sandStop);
                return Color.Lerp(sand, grass, progress);
            }
            return grass;
			switch (decimalCode)
			{
				case 255:
					return Color.Brown;
				case 250:
					return Color.Pink;
				case 240:
					return Color.Yellow;
				default:
					return Color.Pink;
			}
		}


		private void CreateIndicesChunk(CHeightmap heightMap, ref Dictionary<int, int[]> indicesdict, int reCurisiveCounter)
		{
			int terrainWidthChunk = heightMap.Image.Width / chunksplit;
			int terrainHeightChunk = heightMap.Image.Height / chunksplit;

			// indicies
			int counter = 0;
			var indices = new int[(terrainWidthChunk - 1) * (terrainHeightChunk - 1) * 6];
			for (int y = 0; y < terrainHeightChunk - 1; y++)
			{
				for (int x = 0; x < terrainWidthChunk - 1; x++)
				{
					int topLeft = x + y * terrainWidthChunk;
					int topRight = (x + 1) + y * terrainWidthChunk;
					int lowerLeft = x + (y + 1) * terrainWidthChunk;
					int lowerRight = (x + 1) + (y + 1) * terrainWidthChunk;

					indices[counter++] = topLeft;
					indices[counter++] = lowerRight;
					indices[counter++] = lowerLeft;

					indices[counter++] = topLeft;
					indices[counter++] = topRight;
					indices[counter++] = lowerRight;
				}
			}
			indicesdict.Add(reCurisiveCounter, indices);
			if (reCurisiveCounter + 1 < chunksplit * chunksplit)
				CreateIndicesChunk(heightMap, ref indicesdict, reCurisiveCounter + 1);
		}

		private void CalculateNormals(ref VertexPositionNormalColor[] vertices, ref int[] indices)
		{
			for (int i = 0; i < vertices.Length; i++)
				vertices[i].Normal = new Vector3(0, 0, 0);

			for (int i = 0; i < indices.Length / 3; i++)
			{
				int index1 = indices[i * 3];
				int index2 = indices[i * 3 + 1];
				int index3 = indices[i * 3 + 2];

				Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
				Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
				Vector3 normal = Vector3.Cross(side1, side2);
				normal.Normalize();
				vertices[index1].Normal += normal;
				vertices[index2].Normal += normal;
				vertices[index3].Normal += normal;
			}
		}

		private ModelMeshPart CreateModelPart(VertexPositionNormalColor[] vertices, int[] indices)
		{

			var vertexBuffer = new VertexBuffer(mGraphicsDevice, VertexPositionNormalColor.VertexDeclaration, vertices.Length, BufferUsage.None);
			vertexBuffer.SetData(vertices);
			var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(int), indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);
			return new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, NumVertices = indices.Length, PrimitiveCount = indices.Length / 3 };
		}

		private void CalculateHeightData(CHeightmap compHeight, int key)
		{
            CTransform transformComponent = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);

			int terrainWidth = compHeight.Image.Width;
			int terrainHeight = compHeight.Image.Height;

			var colorMap = new Color[terrainWidth * terrainHeight];
			compHeight.Image.GetData(colorMap);

			compHeight.HeightData = new Color[terrainWidth, terrainHeight];
            mHeightData = new float[terrainWidth, terrainHeight];
            Random rn = new Random();
            for (int x = 0; x < terrainWidth; x++) {
                for (int y = 0; y < terrainHeight; y++) {
                    compHeight.HeightData[x, y] = colorMap[x + y * terrainWidth];
                    mHeightData[x, y] = colorMap[x + y * terrainWidth].R + transformComponent.Position.Y + (0.5f-(float)(rn.NextDouble() * 1f));
                }
            }

			float minHeight = float.MaxValue;
			float maxHeight = float.MinValue;
			for (int x = 0; x < terrainWidth; x++)
			{
				for (int y = 0; y < terrainHeight; y++)
				{
					if (compHeight.HeightData[x, y].R < minHeight)
						compHeight.LowestPoint = compHeight.HeightData[x, y].R;
					if (compHeight.HeightData[x, y].R > maxHeight)
						compHeight.HeighestPoint = compHeight.HeightData[x, y].R;
				}
			}
		}

		private void CreateVerticesChunks(CHeightmap cheightmap,
			ref Dictionary<int, VertexPositionNormalColor[]> vertexdict, int reCurisiveCounter, int xOffset)
		{


			int terrainWidth = cheightmap.Image.Width / chunksplit;
			int terrainHeight = cheightmap.Image.Height / chunksplit;
			int globaly = 0;
			int globalx = 0;
			int yOffset = 0;
			if (reCurisiveCounter % chunksplit == 0 && reCurisiveCounter != 0)
			{
				xOffset += terrainWidth;
				xOffset--;
			}
			var vertices = new VertexPositionNormalColor[terrainWidth * terrainHeight];
			for (int x = 0; x < terrainWidth; x++)
			{
				for (int y = 0; y < terrainHeight; y++)
				{
					yOffset = terrainHeight * (reCurisiveCounter % chunksplit);
					if (yOffset > 0)
						yOffset = yOffset - reCurisiveCounter % chunksplit;
					globalx = x + xOffset;
					globaly = y + yOffset;
					float height = mHeightData[globalx, globaly];
					vertices[x + y * terrainWidth].Position = new Vector3(globalx, height, globaly);
					vertices[x + y * terrainWidth].Color = materialPick(cheightmap.HeightData[globalx, globaly].G);

					//vertices[x + y * terrainWidth].TextureCoordinate = new Vector2((float) x / terrainWidth,
					//(float) y / terrainHeight);
				}
			}
			vertexdict.Add(reCurisiveCounter, vertices);


			if (reCurisiveCounter + 1 < chunksplit * chunksplit)
				CreateVerticesChunks(cheightmap, ref vertexdict, reCurisiveCounter + 1, xOffset);
		}



		public void Load()
		{

			// for each heightmap component, create Model instance to enable Draw calls when rendering
			foreach (var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>())
			{
				if (renderable.Value.GetType() != typeof(CHeightmap))
					continue;
				CHeightmap heightmap = (CHeightmap)renderable.Value;
                int key = renderable.Key;
				/* use each color channel for different data, e.g.
				 * R for height,
				 * G for texture/material/terrain type,
				 * B for fixed spawned models/entities (houses, trees etc.),
				 * A for additional data
				*/


				List<ModelMesh> meshes = new List<ModelMesh>();
				var bones = new List<ModelBone>();

				var indices = new Dictionary<int, int[]>();
				var vertices = new Dictionary<int, VertexPositionNormalColor[]>();

				CreateIndicesChunk(heightmap, ref indices, 0);
				CalculateHeightData(heightmap, key);
				CreateVerticesChunks(heightmap, ref vertices, 0, 0);
                //basicEffect.Texture = heightmap.Image;
                basicEffect.DiffuseColor = new Vector3(1, 1, 1);
                basicEffect.SpecularPower = 20f;
                basicEffect.SpecularColor = new Vector3(0.25f);
                for (int j = 0; j < vertices.Values.Count; j++)
				{
					var vert = vertices[j];
					var ind = indices[j];
					CalculateNormals(ref vert, ref ind);
                    /*
                    for(int i = 0; i < vertices[j].Length; i++)
                        vertices[j][i].Color = Color.ForestGreen;
                    */
					vertices[j] = vert;
					indices[j] = ind;

					var modelpart = CreateModelPart(vert, ind);
					List<ModelMeshPart> meshParts = new List<ModelMeshPart>();
					meshParts.Add(modelpart);

					ModelMesh modelMesh = new ModelMesh(mGraphicsDevice, meshParts);
					modelMesh.BoundingSphere = new BoundingSphere();
					ModelBone modelBone = new ModelBone();
					modelBone.AddMesh(modelMesh);
					modelBone.Transform = Matrix.CreateTranslation(new Vector3(0, 0, 0)); // changing object world (frame) / origo

					modelMesh.ParentBone = modelBone;
					bones.Add(modelBone);
					meshes.Add(modelMesh);
					modelMesh.BoundingSphere = BoundingSphere.CreateFromBoundingBox(GenericUtil.BuildBoundingBoxForVertex(vert, Matrix.Identity));
					modelpart.Effect = basicEffect;
				}
				ModelMeshPart ground = buildGround(heightmap, 20);
				List<ModelMeshPart> groundMeshParts = new List<ModelMeshPart>();
				groundMeshParts.Add(ground);
				ModelMesh groundMesh = new ModelMesh(mGraphicsDevice, groundMeshParts);
				groundMesh.BoundingSphere = new BoundingSphere();
				ModelBone groundBone = new ModelBone();
				groundBone.AddMesh(groundMesh);
				groundBone.Transform = Matrix.CreateTranslation(new Vector3(0, 0, 0));
				groundMesh.ParentBone = groundBone;
				groundMesh.Name = "FrontFace";
				bones.Add(groundBone);
				meshes.Add(groundMesh);
				ground.Effect = basicEffect;

				heightmap.model = new Model(mGraphicsDevice, bones, meshes);
				heightmap.model.Tag = "Map";
			}
		}

		private ModelMeshPart buildGround(CHeightmap heightmap, int height)
		{
			int width = heightmap.Image.Width;
			int depth = heightmap.Image.Height;

			// Normals
			Vector3 RIGHT = new Vector3(1, 0, 0); // +X
			Vector3 LEFT = new Vector3(-1, 0, 0); // -X
			Vector3 UP = new Vector3(0, 1, 0); // +Y
			Vector3 DOWN = new Vector3(0, -1, 0); // -Y
			Vector3 FORWARD = new Vector3(0, 0, 1); // +Z
			Vector3 BACKWARD = new Vector3(0, 0, -1); // -Z

			Color groundColor = Color.SaddleBrown;
			var vertexList = new List<VertexPositionNormalColor>();
			// Front and back
			for (int x = 0; x < width - 1; x++)
			{
				// Front Face
				var z = depth - 1;
				var currY = heightmap.HeightData[x, z].R;
				var nextY = heightmap.HeightData[x + 1, z].R;
				var FRONT_TOP_LEFT = new Vector3(x, currY, z);
				var FRONT_TOP_RIGHT = new Vector3(x + 1, nextY, z);
				var FRONT_BOTTOM_LEFT = new Vector3(x, -height, z);
				var FRONT_BOTTOM_RIGHT = new Vector3(x + 1, -height, z);
				vertexList.Add(new VertexPositionNormalColor(FRONT_TOP_LEFT, FORWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_BOTTOM_RIGHT, FORWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_BOTTOM_LEFT, FORWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_TOP_LEFT, FORWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_TOP_RIGHT, FORWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_BOTTOM_RIGHT, FORWARD, groundColor));

				//Back face
				z = 0;
				currY = heightmap.HeightData[x, z].R;
				nextY = heightmap.HeightData[x + 1, z].R;
				var BACK_TOP_RIGHT = new Vector3(x, currY, z);
				var BACK_TOP_LEFT = new Vector3(x + 1, nextY, z);
				var BACK_BOTTOM_RIGHT = new Vector3(x, -height, z);
				var BACK_BOTTOM_LEFT = new Vector3(x + 1, -height, z);
				vertexList.Add(new VertexPositionNormalColor(BACK_TOP_LEFT, BACKWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_BOTTOM_RIGHT, BACKWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_BOTTOM_LEFT, BACKWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_TOP_LEFT, BACKWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_TOP_RIGHT, BACKWARD, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_BOTTOM_RIGHT, BACKWARD, groundColor));
			}
			// Left and right
			for (int z = 0; z < depth - 1; z++)
			{
				// Left face
				var x = 0;
				var currY = heightmap.HeightData[x, z].R;
				var nextY = heightmap.HeightData[x, z + 1].R;
				var BACK_TOP_LEFT = new Vector3(x, currY, z);
				var BACK_TOP_RIGHT = new Vector3(x, nextY, z + 1);
				var BACK_BOTTOM_LEFT = new Vector3(x, -height, z);
				var BACK_BOTTOM_RIGHT = new Vector3(x, -height, z + 1);
				vertexList.Add(new VertexPositionNormalColor(BACK_TOP_LEFT, LEFT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_BOTTOM_RIGHT, LEFT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_BOTTOM_LEFT, LEFT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_TOP_LEFT, LEFT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_TOP_RIGHT, LEFT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(BACK_BOTTOM_RIGHT, LEFT, groundColor));
				// Right face
				x = depth - 1;
				currY = heightmap.HeightData[x, z].R;
				nextY = heightmap.HeightData[x, z + 1].R;
				var FRONT_TOP_RIGHT = new Vector3(x, currY, z);
				var FRONT_TOP_LEFT = new Vector3(x, nextY, z + 1);
				var FRONT_BOTTOM_RIGHT = new Vector3(x, -height, z);
				var FRONT_BOTTOM_LEFT = new Vector3(x, -height, z + 1);
				vertexList.Add(new VertexPositionNormalColor(FRONT_TOP_LEFT, RIGHT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_BOTTOM_RIGHT, RIGHT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_BOTTOM_LEFT, RIGHT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_TOP_LEFT, RIGHT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_TOP_RIGHT, RIGHT, groundColor));
				vertexList.Add(new VertexPositionNormalColor(FRONT_BOTTOM_RIGHT, RIGHT, groundColor));
			}

			var vertices = vertexList.ToArray();
			// indicies
			var indexLength = (width * 6 * 2) + (depth * 6 * 2);
			List<short> indexList = new List<short>(indexLength);
			for (short i = 0; i < indexLength; ++i)
				indexList.Add(i);
			var indices = indexList.ToArray();

			var vertexBuffer = new VertexBuffer(mGraphicsDevice, VertexPositionNormalColor.VertexDeclaration, vertices.Length, BufferUsage.None);
			vertexBuffer.SetData(vertices);

			var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);

			var groundMeshPart = new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, NumVertices = vertices.Length, PrimitiveCount = vertices.Length / 3 };

			return groundMeshPart;
		}

		public override void Update(float t, float dt)
		{
			base.Update(t, dt);
		}
		public override void Draw(float t, float dt)
		{
			base.Draw(t, dt);
		}
	}
}
