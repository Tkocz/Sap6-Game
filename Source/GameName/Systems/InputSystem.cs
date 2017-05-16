using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using EngineName.Core;
using EngineName;
using System;
using System.Linq;
using EngineName.Components;
using GameName.Scenes;
using EngineName.Systems;

namespace GameName.Systems {
    public class InputSystem : EcsSystem {
        private const float CAMERASPEED = 0.1f;
        private Keys[] lastPressedKeys;
        private Matrix addRot;
        private float yaw = 0, pitch = 0, roll = 0;
        private bool isInAir = false;
        private KeyboardState prevState = new KeyboardState();
        public InputSystem() { }

        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("collisionwithground", data => {
                int playerID;
                if (Game1.Inst.Scene.GetType() == typeof(WorldScene)) {
                    playerID = ((WorldScene)Game1.Inst.Scene).GetPlayerEntityID();
                }else {
                    return;
                }
                if (((PhysicsSystem.CollisionInfo)data).Entity1 == playerID
                      ||
                     ((PhysicsSystem.CollisionInfo)data).Entity2 == playerID) {
                    isInAir = false;
                }
            });
            
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
                /*if (Game1.Inst.Scene.EntityHasComponent<CCamera>(input.Key))
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
                }*/

                //For Network Chat           
                foreach (Keys key in pressedKeys)
                {
                    if (lastPressedKeys != null && !lastPressedKeys.Contains(key))
                    {
                        Game1.Inst.RaiseInScene("key_to_write", key);

                    }
                }
                lastPressedKeys = pressedKeys;
                if (!Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)) {
                    continue;
                }
                if (currentState.IsKeyDown(Keys.Escape))
                    Game1.Inst.Exit(); // TODO: We Should leave the scene

                var movementSpeed = dt * 3f * body.SpeedMultiplier;
                var rotationSpeed = dt * 3f * body.RotationMultiplier;

                Vector3 acceleration = Vector3.Zero;

                if (currentState.IsKeyDown(inputValue.ForwardMovementKey))
                    acceleration += movementSpeed * transform.Frame.Forward;
                if (currentState.IsKeyDown(inputValue.BackwardMovementKey))
                    acceleration += movementSpeed * transform.Frame.Backward;

                if (acceleration.X + body.Velocity.X < body.MaxVelocity && acceleration.X + body.Velocity.X > -body.MaxVelocity)
                    body.Velocity.X += acceleration.X;
                //if (acceleration.Y + body.Velocity.Y < body.MaxVelocity || acceleration.Y + body.Velocity.Y > -body.MaxVelocity)
                  //  body.Velocity.Y += acceleration.Y;
                if (acceleration.Z + body.Velocity.Z < body.MaxVelocity && acceleration.Z + body.Velocity.Z > -body.MaxVelocity)
                    body.Velocity.Z += acceleration.Z;

                if (currentState.IsKeyDown(inputValue.LeftMovementKey)) {
                    yaw = rotationSpeed;
                }
                if (currentState.IsKeyDown(inputValue.RightMovementKey)) {
                    yaw = -rotationSpeed;
                }
                if (currentState.IsKeyDown(Keys.Space) && !isInAir) {
                    body.Velocity.Y += 10f;
                    isInAir = true;
                }
                if (currentState.IsKeyDown(Keys.LeftShift) && !prevState.IsKeyDown(Keys.LeftShift))
                {
                    if (Game1.Inst.Scene.GetType() == typeof(WorldScene))
                    {
                        var scene = (WorldScene)Game1.Inst.Scene;
                        var list = Game1.Inst.Scene.GetComponents<CPickUp>();
                        var inv = (CInventory)Game1.Inst.Scene.GetComponentFromEntity<CInventory>(input.Key);
                        foreach (var ball in list)
                        {
                            var ballBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(ball.Key);

                            if (body.ReachableArea.Intersects(ballBody.Aabb) && !inv.isFull)
                            {
                                var b = new CInventoryItem(ballBody);
                                inv.items.Add(b);
                                inv.IdsToRemove.Add(ball.Key);
                                prevState = currentState;
                                // Return so only one item will be picked up.
                                return;
                            }
                        }
                    }
                }
                if (currentState.IsKeyDown(Keys.LeftControl) && !prevState.IsKeyDown(Keys.LeftControl))
                {
                    if (Game1.Inst.Scene.EntityHasComponent<CInventory>(input.Key))
                    {
                        var inv = (CInventory)Game1.Inst.Scene.GetComponentFromEntity<CInventory>(input.Key);
                        if (inv.items.Count > 0)
                        {
                            var item = inv.items.ElementAt(inv.items.Count - 1);

                            var ts = dt * 100f * item.itemBody.SpeedMultiplier;
                            var newItem = item;
                            newItem.itemBody.Velocity += /*transform.Rotation.Forward
                                              */ new Vector3(item.itemBody.Aabb.Max.X * 2 + .5f, 0f, item.itemBody.Aabb.Max.Z * 2 + .5f) * ts;

                            inv.items.Remove(item);
                            inv.itemsToRemove.Add(newItem);
                        }
                    }
                }
                prevState = currentState;

                addRot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                transform.Heading += yaw;
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
        }
    }
}
