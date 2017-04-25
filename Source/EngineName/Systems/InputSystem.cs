using CGLab2.Engine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CGLab2.Engine.Subsystems
{
    public class InputSystem : SubSystem
    {
        public InputSystem(Core core)
        {
            _core = core;
        }
        public override void Draw(GameTime gameTime) { }

        public override void LoadContent() { }

        public override void Update(GameTime gameTime)
        {
            float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            for (int i = 0; i < _core.entityCounter; i++)
            {
                if (_core.inputComponents.ContainsKey(i) && _core.transformComponents.ContainsKey(i))
                {
                    InputComponent inputComponent = _core.inputComponents[i];
                    TransformComponent transformComponent = _core.transformComponents[i];
                    KeyboardState currentState = Keyboard.GetState();

                    float movementSpeed = 25f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    float rotationSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 1f;

                    float yaw = 0, pitch = 0, roll = 0;
                    Vector3 pos = Vector3.Zero;

                    if (currentState.IsKeyDown(inputComponent.ForwardMovementKey))
                        pos.Z -= movementSpeed;
                    if (currentState.IsKeyDown(inputComponent.BackwardMovementKey))
                        pos.Z += movementSpeed;
                    if (currentState.IsKeyDown(inputComponent.LeftMovementKey))
                        pos.X -= movementSpeed;
                    if (currentState.IsKeyDown(inputComponent.RightMovementKey))
                        pos.X += movementSpeed;
                    transformComponent.Position += Vector3.Transform(pos, transformComponent.Rotation);

                    // Clockwise around positive Y-axis
                    if (currentState.IsKeyDown(inputComponent.YRotationPlus))
                        yaw -= rotationSpeed;

                    // Clockwise around negative Y-axis
                    if (currentState.IsKeyDown(inputComponent.YRotationMinus))
                        yaw += rotationSpeed;
                    /*
                    // Clockwise around positive X-axis
                    if (currentState.IsKeyDown(inputComponent.XRotationPlus))
                        pitch -= rotationSpeed;

                    // Clockwise around negative X-axis
                    if (currentState.IsKeyDown(inputComponent.XRotationMinus))
                        pitch += rotationSpeed;
                        *//*
                    // Clockwise around positive Z-axis
                    if (currentState.IsKeyDown(inputComponent.ZRotationPlus))
                        roll += rotationSpeed;

                    // Clockwise around negative Z-axis
                    if (currentState.IsKeyDown(inputComponent.ZRotationMinus))
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
}
