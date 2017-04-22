using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Systems {
    public class MapSystem : EcsSystem {
        private GraphicsDevice mGraphicsDevice;

        public override void Init() {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            base.Init();
        }
        public void Load() {

            // for each heightmap component, create Model instance to enable Draw calls when rendering
            foreach (var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                if (renderable.Value.GetType() != typeof(CHeightmap))
                    continue;
                CHeightmap heightmap = (CHeightmap)renderable.Value;
                /* use each color channel for different data, e.g. 
                 * R for height, 
                 * G for texture/material/terrain type, 
                 * B for fixed spawned models/entities (houses, trees etc.), 
                 * A for additional data
                */
                int terrainWidth = heightmap.image.Width;
                int terrainHeight = heightmap.image.Height;
                // get colors from image, put into matrix (maybe not neccessary)
                Color[] colorArray = new Color[terrainWidth * terrainHeight];
                heightmap.colorMap = new Color[terrainWidth, terrainHeight];
                heightmap.image.GetData(colorArray);
                for (int x = 0; x < terrainWidth; x++) {
                    for (int y = 0; y < terrainHeight; y++) {
                        heightmap.colorMap[x, y] = colorArray[x + y * terrainWidth];
                    }
                }
                //
                int[] indices = new int[(terrainWidth-1) * (terrainHeight-1) * 6];
                VertexPositionNormalColor[] vertices = new VertexPositionNormalColor[terrainWidth * terrainHeight];
                // Create vertices
                for (int x = 0; x < terrainWidth; x++) {
                    for (int y = 0; y < terrainHeight; y++) {
                        int height = heightmap.colorMap[x, y].R;
                        vertices[x + y * terrainWidth].Position = new Vector3(x, height, y);
                        vertices[x + y * terrainWidth].Color = Color.Red;
                    }
                }
                // Create indices
                int counter = 0;
                for (int y = 0; y < terrainHeight - 1; y++) {
                    for (int x = 0; x < terrainWidth - 1; x++) {
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

                for (int i = 0; i < indices.Length / 3; i++) {
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
                // set buffers
                var vertexBuffer = new VertexBuffer(mGraphicsDevice, VertexPositionNormalColor.VertexDeclaration, vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(mGraphicsDevice, typeof(int), indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);
                // create model instance
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
                meshPart.Effect = new BasicEffect(mGraphicsDevice) { VertexColorEnabled = true };
                mesh.Name = "Map";
                ModelBone bone = new ModelBone();
                bone.Name = "Map";
                bone.AddMesh(mesh);
                bone.Transform = Matrix.Identity;
                mesh.ParentBone = bone;

                bones.Add(bone);
                meshes.Add(mesh);

                heightmap.model = new Model(mGraphicsDevice, bones, meshes);
                heightmap.model.Tag = "Map";
            }
        }
        public override void Update(float t, float dt) {
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            base.Draw(t, dt);
        }
    }
}
