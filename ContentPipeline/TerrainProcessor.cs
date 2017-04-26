using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;
using TOutput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;

namespace ContentPipeline {
    [ContentProcessor(DisplayName = "SAP6 - Terrain Processor")]
    public class TerrainProcessor : ContentProcessor<TInput, TOutput> {
        public override TOutput Process(TInput input, ContentProcessorContext context) {
            try {
                input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            }
            catch (Exception ex) {
                context.Logger.LogImportantMessage("Could not convert input texture for processing. " + ex.ToString());
                throw ex;
            }
            var bmp = (PixelBitmapContent<Color>)input.Faces[0][0];
            var image = new Color[bmp.Height][];
            for (var y = 0; y < bmp.Height; y++) {
                image[y] = bmp.GetRow(y);
            }
            context.Logger.LogMessage("test");
            return new TOutput();
        }
    }
}