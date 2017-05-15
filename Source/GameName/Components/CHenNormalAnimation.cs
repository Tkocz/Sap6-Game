using EngineName;
using EngineName.Components.Renderable;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Components {
    /// <summary>
    /// Component for normal hen animation. Creates an animation component containing normal animation for hen model.
    /// </summary>
    public class CHenNormalAnimation : CAnimation {
        /// <summary>
        /// Public constructor. Builds animation keyframe list.
        /// </summary>
        public CHenNormalAnimation()
        {
            fileName = "hen";
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_5"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_6"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_7"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_8"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_9"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_10"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_9"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_8"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_7"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_6"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_5"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
            Keyframes.Add(Game1.Inst.Content.Load<Model>("Models/hen_4"));
        }
    }
}