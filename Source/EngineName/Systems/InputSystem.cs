using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Utils;
using EngineName.Components;

namespace EngineName.Systems {
    public class InputSystem : EcsSystem {

        public override void Update(float t, float dt){


            foreach (var input in Game1.Inst.Scene.GetComponents<CInput>()) {
                CBody body = null;
                if(Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)){
                    body = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(input.Key);
                }else{
                    continue;
                }
                CInput inputValue = (CInput)input.Value;
                KeyboardState currentState = Keyboard.GetState();

                if (currentState.IsKeyDown(inputValue.ForwardMovementKey))
                    body.Velocity.Z -= 5f;
                if (currentState.IsKeyDown(inputValue.BackwardMovementKey))
                    body.Position.Z += 5f;
                if (currentState.IsKeyDown(inputValue.LeftMovementKey))
                    body.Position.X -= 5f;
                if (currentState.IsKeyDown(inputValue.RightMovementKey))
                    body.Position.X += 5f;
                
                
            }
        }
    }
}
