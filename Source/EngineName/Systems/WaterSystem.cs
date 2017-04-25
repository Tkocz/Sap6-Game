using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EngineName.Components;

namespace EngineName.Systems
{
    public class WaterSystem : EcsSystem
    {
        private GraphicsDevice mGraphicsDevice;
        private int[] indices  =null;
        private VertexPositionNormalColor[] vertices;
        private CImportedModel CModel;
        private int[] vibirations;
        private Effect bEffect;

        public override void Init()
        {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            base.Init();
            bEffect = new BasicEffect(mGraphicsDevice) { VertexColorEnabled = true };
        }

        public void Load()
        {
            Model model = null;
            foreach (var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>())
            {
                if (renderable.Value.GetType() != typeof(CHeightmap))
                    continue;
                CHeightmap heightmap = (CHeightmap) renderable.Value;

                //                heightmap.HeightData
                // Create vertices
                Random rnd = new Random(1990);


                var terrainHeight = 1081;
                var terrainWidth = 1081;
                //
                int counter = 0;
                indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
                vertices = new VertexPositionNormalColor[terrainWidth * terrainHeight];
                vibirations = new int[terrainWidth * terrainHeight];
                // Create vertices
                for (int x = 0; x < terrainWidth; x++)
                {
                    for (int y = 0; y < terrainHeight; y++)
                    {

                        vertices[x + y * terrainWidth].Position = new Vector3(x, heightmap.LowestPoint+10, y);
                        var color = Color.Blue;
                        color.A = 100;
                        vertices[x + y * terrainWidth].Color = color;

                        // Randomly set the direction of the vertex up or down
                        vibirations[x + y * terrainWidth] = rnd.Next(0, 100) > 50 ? -1 : 1;
                    }
                }
                for (int y = 0; y < terrainHeight - 1; y++)
                {
                    for (int x = 0; x < terrainWidth - 1; x++)
                    {
                        int topLeft = x + y * terrainWidth;
                        int topRight = (x + 1) + y * terrainWidth;
                        int lowerLeft = x + (y + 1) * terrainWidth;
                        int lowerRight = (x + 1) + (y + 1) * terrainWidth;

                        indices[counter++] = (int)topLeft;
                        indices[counter++] = (int)lowerRight;
                        indices[counter++] = (int)lowerLeft;

                        indices[counter++] = (int)topLeft;
                        indices[counter++] = (int)topRight;
                        indices[counter++] = (int)lowerRight;
                    }
                }
                // Calculate normals
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



                var vertexBuffer = new VertexBuffer(mGraphicsDevice, VertexPositionNormalColor.VertexDeclaration,
                    vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(int), indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);

                var bones = new List<ModelBone>();
                var meshes = new List<ModelMesh>();

                List<ModelMeshPart> parts = new List<ModelMeshPart>();
                ModelMeshPart meshPart = new ModelMeshPart();
                meshPart.VertexBuffer = vertexBuffer;
                meshPart.IndexBuffer = indexBuffer;
                meshPart.NumVertices = indices.Length;
                meshPart.PrimitiveCount = indices.Length / 3;
                parts.Add(meshPart);

                ModelMesh mesh = new ModelMesh(mGraphicsDevice, parts);
                meshPart.Effect = bEffect;

                mesh.Name = "water";
                ModelBone bone = new ModelBone();
                bone.Name = "Water";
                bone.AddMesh(mesh);
                bone.Transform = Matrix.Identity;
                mesh.ParentBone = bone;

                bones.Add(bone);
                meshes.Add(mesh);
                model = new Model(Game1.Inst.GraphicsDevice,bones,meshes);
 

            }
            int id = Game1.Inst.Scene.AddEntity();
           
            Game1.Inst.Scene.AddComponent(id, new CTransform() { Position = new Vector3(-590, -900, -590) * 0.01f, Rotation = Matrix.Identity, Scale = new Vector3(0.01f) });
            CModel = new CImportedModel() { model = model };
            Game1.Inst.Scene.AddComponent<C3DRenderable>(id, CModel);
        }

        public override void Update(float t, float dt)
        {
            
           /* float windSpeed = 0.99f;
            float maxWaveHeight = 80.1f;

            for (int i = 0; i <= vertices.Length - 1; i++)
            {
                vertices[i].Position.Y += vibirations[i] * windSpeed;

                // Change direction if Y component has ecxeeded the limit
                if (Math.Abs(vertices[i].Position.Y) > maxWaveHeight)
                {
                    vertices[i].Position.Y = Math.Sign(vertices[i].Position.Y) * maxWaveHeight;
                    vibirations[i] *= -1;
                }
            }

            // Normals must be updated!
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

            var vertexBuffer = new VertexBuffer(mGraphicsDevice, VertexPositionNormalColor.VertexDeclaration,vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);
            var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(int), indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);

            var bones = new List<ModelBone>();
            var meshes = new List<ModelMesh>();

            List<ModelMeshPart> parts = new List<ModelMeshPart>();
            ModelMeshPart meshPart = new ModelMeshPart();
            meshPart.VertexBuffer = vertexBuffer;
            meshPart.IndexBuffer = indexBuffer;
            meshPart.NumVertices = indices.Length;
            meshPart.PrimitiveCount = indices.Length / 3;

            parts.Add(meshPart);

            ModelMesh mesh = new ModelMesh(mGraphicsDevice, parts);



            mesh.Name = "water";
            ModelBone bone = new ModelBone();
            bone.Name = "Water";
            bone.AddMesh(mesh);
            bone.Transform = Matrix.Identity;
            mesh.ParentBone = bone;
                        meshPart.Effect = bEffect;
            bones.Add(bone);
            meshes.Add(mesh);
            CModel.model = new Model(Game1.Inst.GraphicsDevice, bones, meshes);
            */
        }
    }
}
