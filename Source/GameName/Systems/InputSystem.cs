using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Thengill.Core;
using Thengill;
using System;
using System.Collections.Generic;
using System.Linq;
using GameName.Components;
using Thengill.Components;
using GameName.Scenes;
using GameName.Scenes.Utils;
using Thengill.Components.Renderable;
using Thengill.Systems;
using Thengill.Utils;

namespace GameName.Systems {
    public class InputSystem : EcsSystem {
        private const float CAMERASPEED = 0.1f;
        private Keys[] lastPressedKeys;
        private Matrix addRot;
        private float yaw = 0, pitch = 0, roll = 0;
        private bool isInAir = false;
        private bool isOnGround = false; // whether the player is on ground (the heightmap, not box)
        private KeyboardState prevState = new KeyboardState();
        private List<int> playersInt = new List<int>();

        public float WaterY { get; set; }
        public Heightmap Heightmap { get; set; }

        public InputSystem() { }

        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("collision", data => {
                if (playersInt.Count == 0)
                {
                    foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                    {
                        playersInt.Add(player);
                    }
                }
                var entity = ((PhysicsSystem.CollisionInfo)data).Entity1;
                if (playersInt.Contains(entity))
                {
                    if (isInAir) {
                        if (Game1.Inst.Scene.EntityHasComponent<CInput>(entity))
                        {
                            isInAir = false;
                            isOnGround = false;
                        }
                        var model = (CImportedModel)Game1.Inst.Scene.GetComponentFromEntity<C3DRenderable>(entity);
                        model.animFn = SceneUtils.playerAnimation(entity, 24, 0.1f);
                    }
                }
            });

            //todo collisionwithground is raised all the time
            //not the best soultion displays another animtion when jumping for both players if network
            Game1.Inst.Scene.OnEvent("collisionwithground", data => {
                if (playersInt.Count == 0)
                {
                    foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                    {
                        playersInt.Add(player);
                    }
                }
                var entity = ((PhysicsSystem.CollisionInfo)data).Entity1;
                if (playersInt.Contains(entity))
                {
                    if (isInAir) {
                        if (Game1.Inst.Scene.EntityHasComponent<CInput>(entity))
                        {
                            isInAir = false;
                            isOnGround = true;
                        }
                        var model = (CImportedModel)Game1.Inst.Scene.GetComponentFromEntity<C3DRenderable>(entity);
                        model.animFn = SceneUtils.playerAnimation(entity, 24, 0.1f);
                    }
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

                var movementSpeed = dt * 100f * body.SpeedMultiplier;
                var rotationSpeed = dt * 2.4f * body.RotationMultiplier;

                Vector3 acceleration = Vector3.Zero;

                if (currentState.IsKeyDown(inputValue.ForwardMovementKey)) {
                    var w = transform.Frame.Forward;

                    if (!isInAir && isOnGround) {
                        var tx = transform.Position.X;
                        var tz = transform.Position.Z;

                        var fv1 = w;
                        fv1.Normalize();

                        var fv2 = fv1;
                        fv2 *= 0.05f;

                        // Compute height delta.
                        var y1 = Heightmap.HeightAt(tx, tz);
                        var y2 = Heightmap.HeightAt(tx+fv2.X, tz+fv2.Z);
                        fv2.Y = (float)Math.Max(0.0f, y2 - y1);

                        fv2.Normalize();

                        var maxAngle = (float)Math.Cos(MathHelper.ToRadians(70.0f));
                        var fac = (float)Math.Max(0.0f, Vector3.Dot(fv1, fv2) - maxAngle);
                        movementSpeed *= fac;
                    }

                    acceleration += movementSpeed * w;
                }

                if (currentState.IsKeyDown(inputValue.BackwardMovementKey)) {
                    var w = transform.Frame.Backward;

                    if (!isInAir && isOnGround) {
                        var tx = transform.Position.X;
                        var tz = transform.Position.Z;

                        var fv1 = w;
                        fv1.Normalize();

                        var fv2 = fv1;
                        fv2 *= 0.05f;

                        // Compute height delta.
                        var y1 = Heightmap.HeightAt(tx, tz);
                        var y2 = Heightmap.HeightAt(tx+fv2.X, tz+fv2.Z);
                        fv2.Y = (float)Math.Max(0.0f, y2 - y1);

                        fv2.Normalize();

                        var maxAngle = (float)Math.Cos(MathHelper.ToRadians(70.0f));
                        var fac = (float)Math.Max(0.0f, Vector3.Dot(fv1, fv2) - maxAngle);
                        movementSpeed *= fac;
                    }

                    acceleration += movementSpeed * w;
                }


                acceleration.Y = 0.0f;
                    body.Velocity += acceleration - dt*body.Velocity*10.0f*new Vector3(1.0f, 0.0f, 1.0f);



                //if (acceleration.X + body.Velocity.X < body.MaxVelocity && acceleration.X + body.Velocity.X > -body.MaxVelocity)
                ///body.Velocity.X += acceleration.X;
                //if (acceleration.Y + body.Velocity.Y < body.MaxVelocity || acceleration.Y + body.Velocity.Y > -body.MaxVelocity)
                  //  body.Velocity.Y += acceleration.Y;
                //if (acceleration.Z + body.Velocity.Z < body.MaxVelocity && acceleration.Z + body.Velocity.Z > -body.MaxVelocity)
                    //body.Velocity.Z += acceleration.Z;

                if (currentState.IsKeyDown(inputValue.LeftMovementKey)) {
                    yaw = rotationSpeed;
                }
                if (currentState.IsKeyDown(inputValue.RightMovementKey)) {
                    yaw = -rotationSpeed;
                }
                if (currentState.IsKeyDown(Keys.K) && !prevState.IsKeyDown(Keys.K)) {
                    if (Game1.Inst.Scene.EntityHasComponent<CPlayer>(input.Key)) {
						SfxUtil.PlaySound("Sounds/Effects/Swing", vol:1);
                        var cp = (CPlayer)Game1.Inst.Scene.GetComponentFromEntity<CPlayer>(input.Key);
                        var p = (CHit)Game1.Inst.Scene.GetComponentFromEntity<CHit>(cp.HitId);
                        if (!p.IsAttacking) {
                            Game1.Inst.Scene.Raise("attack", new HitSystem.HitInfo {
                                EntityID = input.Key,
                                IsAttacking = true,
                                StartTime = t
                            });
                        }
                    }
                }

                if (currentState.IsKeyDown(Keys.Space) && !prevState.IsKeyDown(Keys.Space) && !isInAir) {
                    body.Velocity.Y += 11f;

                    if (transform.Position.Y  > WaterY) {
                        isInAir = true;
                    }
					SfxUtil.PlaySound("Sounds/Effects/Jump", vol:1);
					var model = (CImportedModel)Game1.Inst.Scene.GetComponentFromEntity<C3DRenderable>(input.Key);
					model.animFn = SceneUtils.playerAnimation(input.Key, 12, 0.01f);

                }
                if (currentState.IsKeyDown(Keys.H) && !prevState.IsKeyDown(Keys.H))
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
                if (currentState.IsKeyDown(Keys.J) && !prevState.IsKeyDown(Keys.J))
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
                // This is an ugly test for adding a score
                if (currentState.IsKeyDown(Keys.P) && !prevState.IsKeyDown(Keys.P)) {
                    if (Game1.Inst.Scene.GetType() == typeof(WorldScene)) {
                        var score = (CScore)Game1.Inst.Scene.GetComponentFromEntity<CScore>(input.Key);
                        score.Score++;
                    }
                }
                prevState = currentState;

                addRot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                transform.Heading += yaw;
                transform.Rotation *= addRot;
            }
        }
    }
}
