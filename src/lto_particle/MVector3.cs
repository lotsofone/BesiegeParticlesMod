using System;
using Modding.Mapper;
using UnityEngine;

namespace lto_particle
{
    class MVector3 : MCustom<Vector3>
    {
        public MVector3(string displayName, string key, Vector3 defaultValue) : base(displayName, key, defaultValue)
        {
        }

        public override Vector3 DeSerializeValue(XData data)
        {
            return ((XVector3)data).Value;
            //try
            //{
            //    return ((XVector3)data).Value;
            //}
            //catch(Exception e)
            //{
            //    //Debug.Log(data.ToString());
            //}
            //return defaultValue;
        }

        public override XData SerializeValue(Vector3 value)
        {
            return new XVector3(base.SerializationKey, value);
        }
    }
}
