using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework;
using EngineName.Core;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Texture2DContent;
using TOutput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace ContentPipeline
{
	[ContentProcessor(DisplayName = "SAP6 - Terrain Processor")]
	public class TerrainProcessor : ContentProcessor<TInput, TOutput>
	{
		public override TOutput Process(TInput input, ContentProcessorContext context)
		{
			try
			{
				input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
			}
			catch (Exception ex)
			{
				context.Logger.LogImportantMessage("Could not convert input texture for processing. " + ex.ToString());
				throw ex;
			}
			var bmp = (PixelBitmapContent<Color>)input.Faces[0][0];
			var image = new Color[bmp.Height][];
			for (var y = 0; y < bmp.Height; y++)
			{
				image[y] = bmp.GetRow(y);
			}
			var heightmapData = CalculateHeightData(image);
			var indices = new Dictionary<int, int[]>();
			var vertices = new Dictionary<int, VertexPositionNormalColor[]>();
			CreateIndicesChunk(heightmapData, ref indices, 0);
			CreateVerticesChunks(heightmapData, ref vertices, 0, 0);







			//PositionCollection pc = new PositionCollection(); pc.Add()
			//MeshContent mc = new MeshContent() {  }
			//GeometryContent g = new GeometryContent() {  }a

			context.Logger.LogMessage("test");
			return new TOutput();
		}

		private void CreateIndicesChunk(float[,] heightmapData, ref Dictionary<int, int[]> indicesdict, int reCurisiveCounter)
		{
			int chunksplit = 1;
			int terrainWidthChunk = heightmapData.GetLength(0) / chunksplit;
			int terrainHeightChunk = heightmapData.GetLength(1) / chunksplit;

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
				CreateIndicesChunk(heightmapData, ref indicesdict, reCurisiveCounter + 1);


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

				vertices[index1].Normal += normal;
				vertices[index2].Normal += normal;
				vertices[index3].Normal += normal;
			}

			for (int i = 0; i < vertices.Length; i++)
				vertices[i].Normal.Normalize();
		}

		private ModelMesh CreateModelPart(VertexPositionNormalColor[] vertices, int[] indices)
		{

			var vertexBuffer = new VertexContent(mGraphicsDevice, VertexPositionNormalColor.VertexDeclaration, vertices.Length, BufferUsage.None);
			vertexBuffer.SetData(vertices);
			var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(int), indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);
			return new MeshContent { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, NumVertices = indices.Length, PrimitiveCount = indices.Length / 3 };
		}

		private float[,] CalculateHeightData(Color[][] input)
		{


			int terrainWidth = input.GetLength(0);
			int terrainHeight = input.GetLength(1);

			var HeightData = new float[terrainWidth, terrainHeight];
			for (int x = 0; x < terrainWidth; x++)
				for (int y = 0; y < terrainHeight; y++)
					HeightData[x, y] = input[x][y].R;

			return HeightData;
		}

		private void CreateVerticesChunks(float [,] cheightmap,
			ref Dictionary<int, VertexPositionNormalColor[]> vertexdict, int reCurisiveCounter, int xOffset)
		{

			int chunksplit = 1;
			int terrainWidth = cheightmap.GetLength(0) / chunksplit;
			int terrainHeight = cheightmap.GetLength(1) / chunksplit;
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
					int height = (int)cheightmap[globalx, globaly];
					vertices[x + y * terrainWidth].Position = new Vector3(globalx, height, globaly);
					vertices[x + y * terrainWidth].Color = Color.Red;
					//vertices[x + y * terrainWidth].TextureCoordinate = new Vector2((float) x / terrainWidth,
					//(float) y / terrainHeight);
				}
			}
			vertexdict.Add(reCurisiveCounter, vertices);


			if (reCurisiveCounter + 1 < chunksplit * chunksplit)
				CreateVerticesChunks(cheightmap, ref vertexdict, reCurisiveCounter + 1, xOffset);
		}



		private ModelContent generateTerrain()
		{

			// for each heightmap component, create Model instance to enable Draw calls when rendering
			foreach (var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>())
			{
				if (renderable.Value.GetType() != typeof(CHeightmap))
					continue;
				CHeightmap heightmap = (CHeightmap)renderable.Value;
				/* use each color channel for different data, e.g. 
				 * R for height, 
				 * G for texture/material/terrain type, 
				 * B for fixed spawned models/entities (houses, trees etc.), 
				 * A for additional data
				*/


				List<ModelMeshContent> meshes = new List<ModelMeshContent>();
				var bones = new List<ModelBoneContentCollection>();

				var indices = new Dictionary<int, int[]>();
				var vertices = new Dictionary<int, VertexPositionNormalColor[]>();

				CreateIndicesChunk(heightmap, ref indices, 0);
				CalculateHeightData(heightmap);
				CreateVerticesChunks(heightmap, ref vertices, 0, 0);
				basicEffect.Texture = heightmap.Image;
				for (int j = 0; j < vertices.Values.Count; j++)
				{
					var vert = vertices[j];
					var ind = indices[j];
					CalculateNormals(ref vert, ref ind);
					vertices[j] = vert;
					indices[j] = ind;

					var modelpart = CreateModelPart(vert, ind);
					List<ModelMeshPartContent> meshParts = new List<ModelMeshPartContent>();
					meshParts.Add(modelpart);

					ModelMesh modelMesh = new ModelMeshContent(meshParts);
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
			return null;
		}

		private MeshContent buildGround(CHeightmap heightmap, int height)
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
				var currY = heightmap.HeightData[x, z];
				var nextY = heightmap.HeightData[x + 1, z];
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
				currY = heightmap.HeightData[x, z];
				nextY = heightmap.HeightData[x + 1, z];
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
				var currY = heightmap.HeightData[x, z];
				var nextY = heightmap.HeightData[x, z + 1];
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
				currY = heightmap.HeightData[x, z];
				nextY = heightmap.HeightData[x, z + 1];
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

			var vertexBuffer = new VertexContent();
			vertexBuffer.(vertices);

			var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);

			var groundMeshPart = new MeshContent { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, NumVertices = vertices.Length, PrimitiveCount = vertices.Length / 3 };

			return groundMeshPart;
	}

	public struct VertexPositionNormalColor
	{
		public Vector3 Position;
		public Color Color;
		public Vector3 Normal;
		public VertexPositionNormalColor(Vector3 Position, Vector3 Normal, Color Color)
		{
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