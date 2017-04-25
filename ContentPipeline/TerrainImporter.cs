using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework;

namespace ContentPipeline {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>

    [ContentImporter(".png", DisplayName = "Terrain Importer", DefaultProcessor = "")]
    public class TerrainImporter : ContentImporter<TInput> {

        public override TInput Import(string filename, ContentImporterContext context) {

            // TODO: process the input object, and return the modified data.
            context.Logger.LogMessage("bajs" + filename);
            return Color.Black;
        }

    }

}
