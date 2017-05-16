using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Systems;
using GameName.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameName.Systems
{
    public class GameObjectSync : EcsSystem
    {


        private Dictionary<int, CBody> prevCBody = new Dictionary<int, CBody>();
        private Dictionary<int, CTransform> prevTransform = new Dictionary<int, CTransform>();
        private Dictionary<int, CBody> newCBody = new Dictionary<int, CBody>();
        private Dictionary<int, CTransform> newTransform = new Dictionary<int, CTransform>();
        private Random rnd = new Random();
        private bool isMaster;

        public GameObjectSync(bool ismaster)
        {
            isMaster = ismaster;
        }
        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("entityupdate", data =>
            {
                var entity = (NetworkSystem.EntitySync) data;
                addOrUpdatCObjects(entity);
            });
        }


        private void addOrUpdatCObjects(NetworkSystem.EntitySync data)
        {
            //Add entity 
            if (!Game1.Inst.Scene.EntityHasComponent<CTransform>(data.ID))
            {
                //calculate BoundindBox since we have the data do this
                data.CBody.Aabb = new BoundingBox(-data.CBody.Radius * Vector3.One, data.CBody.Radius * Vector3.One);
                Game1.Inst.Scene.AddComponent(data.ID, data.CBody);
                Game1.Inst.Scene.AddComponent(data.ID, data.CTransform);

                if (data.ModelFileName == "hen")
                {
                    CAnimation normalAnimation = new CHenNormalAnimation();
                    // Set a random offset to animation so not all animals are synced
                    normalAnimation.CurrentKeyframe = rnd.Next(normalAnimation.Keyframes.Count - 1);
                    Game1.Inst.Scene.AddComponent<C3DRenderable>(data.ID, normalAnimation);
                }
                else
                {
                    Game1.Inst.Scene.AddComponent<C3DRenderable>(data.ID, new CImportedModel
                    {
                        model = Game1.Inst.Content.Load<Model>("Models/" + data.ModelFileName),
                        fileName = data.ModelFileName
                    });

                }
                if(data.IsPlayer)
                   Game1.Inst.Scene.AddComponent(data.ID,new CPlayer());
                Game1.Inst.Scene.AddComponent(data.ID, new CSyncObject { Owner = false });
                newCBody.Add(data.ID, data.CBody);
                newTransform.Add(data.ID, data.CTransform);
                prevCBody.Add(data.ID, data.CBody);
                prevTransform.Add(data.ID, data.CTransform);
            }
            else
            {
                if (string.IsNullOrEmpty(data.ModelFileName))
                    return;
                /*if (!newCBody.ContainsKey(data.ID))
                {
                    newCBody[data.ID] = data.CBody;
                    newTransform[data.ID] = data.CTransform;
                }*/
                prevCBody[data.ID] = newCBody[data.ID];
                prevTransform[data.ID] = newTransform[data.ID];
                newTransform[data.ID] = data.CTransform;
                newCBody[data.ID] = data.CBody;
            }
        }
        private void syncObjects()
        {
            var counter = 0;
            foreach (var pair in Game1.Inst.Scene.GetComponents<CSyncObject>())
            {
                counter++;
                var sync = (CSyncObject)pair.Value;
                if (sync.Owner)
                {
                    string filename = "";
                    var model = (CImportedModel)Game1.Inst.Scene.GetComponentFromEntity<C3DRenderable>(pair.Key);
                    var ctransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(pair.Key);
                    var cbody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(pair.Key);
                    var isPlayer = Game1.Inst.Scene.EntityHasComponent<CPlayer>(pair.Key);
                    Game1.Inst.Scene.Raise("sendentity", new NetworkSystem.EntitySync() {CBody = cbody,CTransform = ctransform, ID =  pair.Key, ModelFileName = model.fileName,IsPlayer = isPlayer });
                }
            }
        }
        private const float updateInterval = (float)1 / 20;
        private float remaingTime = 0;

        public override void Update(float t, float dt)
        {
            remaingTime += dt;
            if (remaingTime > updateInterval)
            {
                syncObjects();
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
                //Smooth
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
