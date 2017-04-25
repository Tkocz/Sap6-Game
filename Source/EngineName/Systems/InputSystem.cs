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
                if(Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)){
                    var body = Game1.Inst.Scene.GetComponentFromEntity<CBody>(input.Key);
                }else{
                    continue;
                }
                
                KeyboardState currentState = Keyboard.GetState();

                if (currentState.IsKeyDown(input.ForwardMovementKey))
                    body.Position.Z -= movementSpeed;
                if (currentState.IsKeyDown(input.BackwardMovementKey))
                    body.Position.Z += movementSpeed;
                if (currentState.IsKeyDown(input.LeftMovementKey))
                    body.Position.X -= movementSpeed;
                if (currentState.IsKeyDown(input.RightMovementKey))
                    body.Position.X += movementSpeed;
                transformComponent.Position += Vector3.Transform(body.Position, transformComponent.Rotation);

                // Clockwise around positive Y-axis
                if (currentState.IsKeyDown(input.YRotationPlus))
                    yaw -= rotationSpeed;

                // Clockwise around negative Y-axis
                if (currentState.IsKeyDown(input.YRotationMinus))
                    yaw += rotationSpeed;
                /*
                // Clockwise around positive X-axis
                if (currentState.IsKeyDown(input.XRotationPlus))
                    pitch -= rotationSpeed;

                // Clockwise around negative X-axis
                if (currentState.IsKeyDown(input.XRotationMinus))
                    pitch += rotationSpeed;
                    *//*
                // Clockwise around positive Z-axis
                if (currentState.IsKeyDown(input.ZRotationPlus))
                    roll += rotationSpeed;

                // Clockwise around negative Z-axis
                if (currentState.IsKeyDown(input.ZRotationMinus))
                    roll -= rotationSpeed;
                    */
                //float angle = -elapsedGameTime * 0.01f;
                Quaternion rot = Quaternion.CreateFromAxisAngle(transformComponent.Rotation.Right, pitch) *
                    Quaternion.CreateFromAxisAngle(transformComponent.Rotation.Up, yaw) *
                    Quaternion.CreateFromAxisAngle(transformComponent.Rotation.Backward, roll);
                rot.Normalize();
                transformComponent.Rotation *= Matrix.CreateFromQuaternion(rot);
                
            }
        }
    }
}
