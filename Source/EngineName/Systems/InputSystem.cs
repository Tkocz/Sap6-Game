using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using EngineName.Core;
using System;
using System.Linq;
using EngineName.Components;

namespace EngineName.Systems {
    public class InputSystem : EcsSystem {
        private const float CAMERASPEED = 0.1f;
        private Keys[] lastPressedKeys;
        private Matrix addRot;
        private float yaw = 0, pitch = 0, roll = 0;
        private bool isInAir = false;

        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("collisionwithground", data => isInAir = false);
            base.Init();
        }

        public override void Update(float t, float dt)
        {
            KeyboardState currentState = Keyboard.GetState();
            Keys[] pressedKeys = currentState.GetPressedKeys();
            yaw = 0;
            foreach (var input in Game1.Inst.Scene.GetComponents<CInput>()) {
                CBody body = null;
                if (Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)) {
                    body = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(input.Key);
                }
                var transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(input.Key);
                var inputValue = (CInput)input.Value;
                if (Game1.Inst.Scene.EntityHasComponent<CCamera>(input.Key))
                {
                    CCamera cameraComponent = (CCamera)Game1.Inst.Scene.GetComponentFromEntity<CCamera>(input.Key);

                    if (currentState.IsKeyDown(inputValue.CameraMovementForward))
                    {
                        transform.Position += CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                        cameraComponent.Target += CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                    }
                    if (currentState.IsKeyDown(inputValue.CameraMovementBackward))
                    {
                        transform.Position -= CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                        cameraComponent.Target -= CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                    }
                    if (currentState.IsKeyDown(inputValue.CameraMovementLeft))
                    {
                        cameraComponent.Heading -= 0.05f;
                        transform.Position = Vector3.Subtract(cameraComponent.Target, new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), cameraComponent.Height, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f))));
                    }
                    if (currentState.IsKeyDown(inputValue.CameraMovementRight))
                    {
                        cameraComponent.Heading += 0.05f;
                        transform.Position = Vector3.Subtract(cameraComponent.Target, new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), cameraComponent.Height, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f))));
                    }
                }

                //For Network Chat           
                foreach (Keys key in pressedKeys)
                {
                    if (lastPressedKeys != null && lastPressedKeys.Contains(key))
                    {
                        Game1.Inst.RaiseInScene("key_to_write", key);

                    }
                }
                lastPressedKeys = pressedKeys;
                if (!Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)) {
                    continue;
                }
                if (currentState.IsKeyDown(Keys.Escape))
                    Game1.Inst.Exit();

                var movementSpeed = dt * 30f * body.SpeedMultiplier;
                var rotationSpeed = dt * 3f * body.RotationMultiplier;

                Vector3 acceleration = Vector3.Zero;

                if (currentState.IsKeyDown(inputValue.ForwardMovementKey))
                    acceleration += movementSpeed * transform.Frame.Forward;
                if (currentState.IsKeyDown(inputValue.BackwardMovementKey))
                    acceleration += movementSpeed * transform.Frame.Backward;

                if (acceleration.X + body.Velocity.X < body.MaxVelocity || acceleration.X + body.Velocity.X > -body.MaxVelocity)
                    body.Velocity.X += acceleration.X;
                if (acceleration.Y + body.Velocity.Y < body.MaxVelocity || acceleration.Y + body.Velocity.Y > -body.MaxVelocity)
                    body.Velocity.Y += acceleration.Y;
                if (acceleration.Z + body.Velocity.Z < body.MaxVelocity || acceleration.Z + body.Velocity.Z > -body.MaxVelocity)
                    body.Velocity.Z += acceleration.Z;

                if (currentState.IsKeyDown(inputValue.LeftMovementKey)) {
                    yaw = rotationSpeed;
                }
                if (currentState.IsKeyDown(inputValue.RightMovementKey)) {
                    yaw = -rotationSpeed;
                }
                if (currentState.IsKeyDown(Keys.Space) && !isInAir) {
                    body.Velocity.Y += 3f;
                    isInAir = true;
                }
                addRot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                
                transform.Rotation *= addRot;


                //save the currently pressed keys so we can compare on the next update


                /*

                //((LookAtCamera)Camera).Target = new Vector3(m.M41, m.M42*0.0f, m.M43);
                //var ta = ((LookAtCamera)Camera).Target;
                var p = b.Position;
                var c = ((LookAtCamera)Camera).Position;
                var dist = 30f;
                var yDist = -20f;
                var h = b.Heading;

                // Vi positionerar kamera utifrån karaktärens heading (h), p = karaktärerns position, c = kamerans position, t = kamerans target, dist = avstånd till objektet
                // yDist = höjd för kameran, samt t = p -- alltså att kamerans target är position för karaktären.
                // Då gäller c=p-[d*sin(h + pi/2), y, (-d)*cos(h + pi/2)]

                c = Vector3.Subtract(p, new Vector3((float)(dist * Math.Sin(h + Math.PI * 0.5f)), yDist, (float)((-dist) * Math.Cos(h + Math.PI * 0.5f))));

                c.Y = -yDist; // Lock camera to given height
                p.Y = 0; // Target too because it was really ugly otherwise

                ((LookAtCamera)Camera).Target = p;
                ((LookAtCamera)Camera).Position = c;
                
                return Matrix.CreateLookAt(Position, (Vector3)m_Target, Up);

            */

            }
            if (currentState.IsKeyDown(Keys.Escape))
            {
                Environment.Exit(0);
            }
        }
    }
}
