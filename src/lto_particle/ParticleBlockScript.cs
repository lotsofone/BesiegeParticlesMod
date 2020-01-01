using UnityEngine;
using Modding.Blocks;
using Modding;
using System;

namespace lto_particle
{
    class ParticleBlockScript : BlockScript
    {
        public MVector3 position;
        public MVector3 rotation;
        public MVector3 scale;
        public override void SafeAwake()
        {
            //position = (MVector3)AddCustom<Vector3>(new MVector3("Local position", "lto_position", Vector3.zero));
            //rotation = (MVector3)AddCustom<Vector3>(new MVector3("Local rotation", "lto_rotation", Vector3.zero));
            //scale = (MVector3)AddCustom<Vector3>(new MVector3("Local scale", "lto_scale", Vector3.one));
            Rigidbody.drag = 0f;
            base.SafeAwake();
        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            TurnColliders(false);
            TurnRenderer(false);
        }
        //模拟时关闭物理碰撞箱和本体显示，并消去靶箱
        public void TurnColliders(bool state)
        {
            var colliders = gameObject.GetComponentsInChildren<BoxCollider>(true);
            //Debug.Log(colliders.Length);
            foreach (var collider in colliders)
            {
                if(collider.name=="Box Collider"||collider.name=="Adding Point")
                    collider.enabled = state;
            }
        }
        public void TurnRenderer(bool state)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                //Debug.Log(renderer.name);
                if (renderer.name == "Vis")
                    renderer.enabled = state;
            }
        }

        //public override void OnSimulateStop()
        //{
        //    base.OnSimulateStop();
        //    TurnColliders(true);
        //    TurnRenderer(true);
        //}
    }
}
