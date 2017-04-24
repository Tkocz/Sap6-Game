using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineName.Utils
{
    public static class GenericUtil
    {
        public static BoundingBox BuildBoundingBoxForVertex(VertexPositionNormalColor[] vertexData, Matrix meshTransform)
        {
            // Create initial variables to hold min and max xyz values for the mesh
            Vector3 meshMax = new Vector3(float.MinValue);
            Vector3 meshMin = new Vector3(float.MaxValue);

            // Find minimum and maximum xyz values for this mesh part
            Vector3 vertPosition = new Vector3();

            for (int i = 0; i < vertexData.Length; i++)
            {
                vertPosition = vertexData[i].Position;
                // update our values from this vertex
                meshMin = Vector3.Min(meshMin, vertPosition);
                meshMax = Vector3.Max(meshMax, vertPosition);
            }


            // transform by mesh bone matrix
            meshMin = Vector3.Transform(meshMin, meshTransform);
            meshMax = Vector3.Transform(meshMax, meshTransform);

            // Create the bounding box
            BoundingBox box = new BoundingBox(meshMin, meshMax);
            return box;
        }
    }
}
