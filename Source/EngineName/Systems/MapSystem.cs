using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Systems {
    public class MapSystem : EcsSystem {
        public override void Init() {
            base.Init();

            // for each heightmap component, create Model instance to enable Draw calls when rendering
            foreach(var renderable in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                CHeightmap heightmap = (CHeightmap)renderable.Value;
                /* use each color channel for different data, e.g. 
                 * R for height, 
                 * G for texture/material/terrain type, 
                 * B for fixed spawned models/entities (houses, trees etc.), 
                 * A for additional data
                */

                /*
                var indices = new Dictionary<int, int[]>();
                var vertices = new Dictionary<int, VertexPositionNormalTexture[]>();
                CreateIndicesChunk(heightMap, ref indices, 0);
                CalculateHeightData(heightMap);
                CreateVerticesChunks(heightMap, ref vertices, 0, 0);
                for (int j = 0; j < vertices.Values.Count; j++) {
                    var vert = vertices[j];
                    var ind = indices[j];
                    CalculateNormals(ref vert, ref ind);
                    vertices[j] = vert;
                    indices[j] = ind;
                }

                CreateBuffers(heightMapComponent, vertices, indices);*/
            }
        }
        public void Load() {

        }
        public override void Update(float t, float dt) {
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            base.Draw(t, dt);
        }
    }
}
