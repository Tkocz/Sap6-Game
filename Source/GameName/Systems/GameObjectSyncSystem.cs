#pragma warning disable CS0414
#pragma warning disable CS0219
#pragma warning disable CS0169

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thengill;
using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Core;
using Thengill.Systems;
using GameName.Components;
using GameName.Scenes.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameName.Systems
{
    public class GameObjectSyncSystem : EcsSystem
    {


        private Dictionary<int, CBody> prevCBody = new Dictionary<int, CBody>();
        private Dictionary<int, CTransform> prevTransform = new Dictionary<int, CTransform>();
        private Dictionary<int, CBody> newCBody = new Dictionary<int, CBody>();
        private Dictionary<int, CTransform> newTransform = new Dictionary<int, CTransform>();
        private Random rnd = new Random();
        private int haveSent;
        private bool isMaster;
        private int counter = 0;
        private const float updateInterval = (float)1 / 20;
        private float remaingTime = 0;

        public GameObjectSyncSystem(bool ismaster)
        {
            isMaster = ismaster;
        }
        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("entityupdate", data =>
            {
                var entity = (EntitySync) data;
                addOrUpdatCObjects(entity);
            });
        }


        /// <summary> Add or updates objects retrieved by other players </summary>
        private void addOrUpdatCObjects(EntitySync data)
        {
            //Add entity
            if (!Game1.Inst.Scene.EntityHasComponent<CTransform>(data.ID))
            {
                var currentScene = Game1.Inst.Scene;
                //calculate BoundindBox since we have the data do this
                data.CBody.Aabb = new BoundingBox(-data.CBody.Radius * Vector3.One, data.CBody.Radius * Vector3.One);
                Game1.Inst.Scene.AddComponent(data.ID, data.CBody);
                Game1.Inst.Scene.AddComponent(data.ID, data.CTransform);
                Func<float, Matrix> aniFunc = null;
                if (data.IsPlayer)
                {
                    Game1.Inst.Scene.AddComponent(data.ID, new CPlayer());
                    aniFunc = SceneUtils.playerAnimation(data.ID, 24, 0.1f);
                }

                if (data.ModelFileName == "hen")
                {
                    aniFunc = SceneUtils.wiggleAnimation(data.ID);
                    // TODO: Make animals have different animations based on state
                    CAnimation normalAnimation = new CHenNormalAnimation { animFn = aniFunc };
                    // Set a random offset to animation so not all animals are synced
                    normalAnimation.CurrentKeyframe = rnd.Next(normalAnimation.Keyframes.Count - 1);
                    // Random animation speed between 0.8-1.0
                    normalAnimation.AnimationSpeed = (float)rnd.NextDouble() * 0.2f + 0.8f;
                    currentScene.AddComponent<C3DRenderable>(data.ID, normalAnimation);
                }
                else
                {
                    
                    Game1.Inst.Scene.AddComponent<C3DRenderable>(data.ID, new CImportedModel
                    {
                        model = Game1.Inst.Content.Load<Model>("Models/" + data.ModelFileName),
                        fileName = data.ModelFileName,
                        animFn = aniFunc

                    });
                }
               
                Game1.Inst.Scene.AddComponent(data.ID, new CSyncObject {Owner = false});

                newCBody.Add(data.ID, data.CBody);
                newTransform.Add(data.ID, data.CTransform);
                prevCBody.Add(data.ID, data.CBody);
                prevTransform.Add(data.ID, data.CTransform);
            }
            else
            {

                prevCBody[data.ID] = newCBody[data.ID];
                prevTransform[data.ID] = newTransform[data.ID];
                if (string.IsNullOrEmpty(data.ModelFileName))
                {
                    newCBody[data.ID].Velocity = data.CBody.Velocity;
                    newTransform[data.ID].Position = data.CTransform.Position;
                    newTransform[data.ID].Rotation = data.CTransform.Rotation;
                }
                else {
                    newTransform[data.ID] = data.CTransform;
                    newCBody[data.ID] = data.CBody;
                }
            }
        }
        
        /// <summary> Sync objects thats have the Component CSyncObject to other players </summary>
        private void syncObjects(float dt)
        {
           
            foreach (var pair in Game1.Inst.Scene.GetComponents<CSyncObject>())
            {

                var sync = (CSyncObject)pair.Value;
                if (sync.Owner)
                {
                    var model = (CImportedModel)Game1.Inst.Scene.GetComponentFromEntity<C3DRenderable>(pair.Key);
                    var ctransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(pair.Key);
                    var cbody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(pair.Key);
                    var isPlayer = Game1.Inst.Scene.EntityHasComponent<CPlayer>(pair.Key);

                    if ((int)dt % 60 == 0 || haveSent < 10)// Send whole object 10 time to be safe then time every minute
                        Game1.Inst.Scene.Raise("sendentity", new EntitySync() { CBody = cbody, CTransform = ctransform, ID = pair.Key, ModelFileName = model.fileName, IsPlayer = isPlayer });
                    else
                        Game1.Inst.Scene.Raise("sendentitylight", new EntitySync() { CBody = cbody, CTransform = ctransform, ID = pair.Key, ModelFileName = model.fileName, IsPlayer = isPlayer });
                }
                if (haveSent < 10)
                {
                    haveSent++;
                }
            }
        }


        public override void Update(float t, float dt)
        {
            remaingTime += dt;
            if (remaingTime > updateInterval)
            {
                syncObjects(dt);
                remaingTime = 0;
            }

            //Todo Impliment Prediction for player movement
            /*foreach (var pair in Game1.Inst.Scene.GetComponents<CSyncObject>())
            {
                var sync = (CSyncObject)pair.Value;
                if (!sync.Owner)
                {


                }
            }*/
            foreach (var key in newCBody.Keys)
            {

                var cbody = (CBody) Game1.Inst.Scene.GetComponentFromEntity<CBody>(key);
                var transform = (CTransform) Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);
                var newtransform = newTransform[key];
                var newcbody = newCBody[key];
                
               
                //Smoothing
                cbody.Velocity = Vector3.Lerp(cbody.Velocity, newcbody.Velocity, 0.1f);
                transform.Position = Vector3.Lerp(transform.Position, newtransform.Position, 0.1f);
                transform.Scale = Vector3.Lerp(transform.Scale, newtransform.Scale, 0.1f);
                transform.Frame = newtransform.Frame;
                var rotation = Quaternion.Lerp(Quaternion.CreateFromRotationMatrix(transform.Rotation),
                    Quaternion.CreateFromRotationMatrix(newtransform.Rotation), 0.1f);
                transform.Rotation = Matrix.CreateFromQuaternion(rotation);


            }
        }

    }
}

#pragma warning restore CS0414
#pragma warning restore CS0219
#pragma warning restore CS0169
