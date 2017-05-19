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
        private int[] indices = null;
        private VertexPositionNormalTexture[] vertices;
        private CImportedModel CModel;
        private int[] vibrations;
        private Effect bEffect;

        public float WaterHeight = -9;
        public byte WaterOpacity = 100;
        public int Resolution = 40; // vertices per direction
        public float Frequency = 1.5f;
        public float Amplitude = 0.2f;
        public float Bias = 0.5f;

        public override void Init()
        {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            base.Init();
            bEffect = Game1.Inst.Content.Load<Effect>("Effects/Water");
            bEffect.Parameters["Frequency"].SetValue(Frequency);
            bEffect.Parameters["Amplitude"].SetValue(Amplitude);
            bEffect.Parameters["Bias"].SetValue(Bias);
        }

        public void Load()
        {
            Model model = null;
            foreach (var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>())
            {
                if (renderable.Value.GetType() != typeof(CHeightmap))
                    continue;
                CHeightmap heightmap = (CHeightmap) renderable.Value;
                
                var terrainHeight = heightmap.Image.Height;
                var terrainWidth = heightmap.Image.Width;
                
                int counter = 0;
                indices = new int[(Resolution - 1) * (Resolution - 1) * 6];
                vertices = new VertexPositionNormalTexture[Resolution * Resolution];
                // Create vertices
                for (int x = 0; x < Resolution; x++)
                {
                    for (int y = 0; y < Resolution; y++)
                    {
                        vertices[x + y * Resolution].Position = new Vector3((terrainWidth/Resolution)*x, heightmap.LowestPoint + WaterHeight, (terrainHeight/Resolution)*y);
                        var color = Color.Blue;
                        color.A = WaterOpacity;
                        vertices[x + y * Resolution].TextureCoordinate = new Vector2(x * 0.005f, y * 0.005f);
                    }
                }
                for (int y = 0; y < Resolution - 1; y++)
                {
                    for (int x = 0; x < Resolution - 1; x++)
                    {
                        int topLeft = x + y * Resolution;
                        int topRight = (x + 1) + y * Resolution;
                        int lowerLeft = x + (y + 1) * Resolution;
                        int lowerRight = (x + 1) + (y + 1) * Resolution;

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
                // TODO: Match with map parameters
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
           
            // TODO: Match with map
            Game1.Inst.Scene.AddComponent(id, new CTransform() { Position = new Vector3(-590, 0, -590), Rotation = Matrix.Identity, Scale = new Vector3(1f) });
            CModel = new CImportedModel() { model = model };
            Game1.Inst.Scene.AddComponent<C3DRenderable>(id, CModel);
        }
    }
}
