using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "ModelBounding Processor - SAP6")]
    public class ModelBoundingProcessor : ModelProcessor
    {
        List<BoundingSphere> sphereList = new List<BoundingSphere>();
        BoundingSphere master;
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ModelContent model = base.Process(input, context);
            foreach (ModelMeshContent mm in model.Meshes)
            {
                sphereList.Add(mm.BoundingSphere);
            }
            foreach (BoundingSphere bs in sphereList)
                master = BoundingSphere.CreateMerged(master, bs);

            sphereList.Insert(0, master);
            model.Tag = sphereList;
            return model;
        }
    }
}