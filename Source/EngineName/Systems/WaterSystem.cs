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
        private VertexPositionNormalTexture[] vertices;
        private CImportedModel CModel;
        private int[] vibirations;
        private Effect bEffect;

        public override void Init()
        {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            base.Init();
            bEffect = Game1.Inst.Content.Load<Effect>("Effects/Water");
            bEffect.CurrentTechnique = bEffect.Techniques["BasicColorDrawing"];
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


                var terrainHeight = heightmap.Image.Height;
                var terrainWidth = heightmap.Image.Width;
                var resolution = 40; // vertices per direction
                //
                int counter = 0;
                indices = new int[(resolution - 1) * (resolution - 1) * 6];
                vertices = new VertexPositionNormalTexture[resolution * resolution];
                // Create vertices
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        vertices[x + y * resolution].Position = new Vector3((terrainWidth/resolution)*x, heightmap.LowestPoint + 375, (terrainHeight/resolution)*y);
                        var color = Color.Blue;
                        color.A = 100;
                        vertices[x + y * resolution].TextureCoordinate = new Vector2(x * 0.005f, y * 0.005f);
                    }
                }
                for (int y = 0; y < resolution - 1; y++)
                {
                    for (int x = 0; x < resolution - 1; x++)
                    {
                        int topLeft = x + y * resolution;
                        int topRight = (x + 1) + y * resolution;
                        int lowerLeft = x + (y + 1) * resolution;
                        int lowerRight = (x + 1) + (y + 1) * resolution;

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



                var vertexBuffer = new VertexBuffer(mGraphicsDevice, VertexPositionNormalTexture.VertexDeclaration,
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
                mesh.BoundingSphere = BoundingSphere.CreateFromBoundingBox(new BoundingBox(new Vector3(0, heightmap.LowestPoint, 0), new Vector3(1081, heightmap.HeighestPoint, 1081)));
                ModelBone bone = new ModelBone();
                bone.Name = "water";
                bone.AddMesh(mesh);
                bone.Transform = Matrix.Identity;
                mesh.ParentBone = bone;

                bones.Add(bone);
                meshes.Add(mesh);
                model = new Model(Game1.Inst.GraphicsDevice,bones,meshes);
                model.Tag = "water";

            }
            int id = Game1.Inst.Scene.AddEntity();
           
            Game1.Inst.Scene.AddComponent(id, new CTransform() { Position = new Vector3(-590, -275, -590), Rotation = Matrix.Identity, Scale = new Vector3(1f) });
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
