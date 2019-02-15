using Modding.Mapper;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Selectors;
using System;
using System.Text.RegularExpressions;

namespace lto_particle
{
    internal class Vector3Selector : CustomSelector<Vector3, MVector3>
    {
        private static GameObject _prefab;

        private static bool _failedOnce;

        private DynamicText _fieldName;

        private MValue[] _values;

        private ValueSelector[] _valueSelectors;

        static Vector3Selector()
        {
            Vector3Selector.CreatePrefab();
        }

        public Vector3Selector()
        {
        }

        protected override void CreateInterface()
        {
            Vector3 vector3 = base.CustomMapperType.Value;
            this._values = new MValue[] { new MValue(string.Concat("  ", base.CustomMapperType.DisplayName), "x", vector3.x), new MValue(string.Concat("  ", base.CustomMapperType.DisplayName), "y", vector3.y), new MValue(string.Concat("  ", base.CustomMapperType.DisplayName), "z", vector3.z) };
            if (!Vector3Selector._failedOnce)
            {
                Transform content = base.Content;
                Transform _transform = ((GameObject)UnityEngine.Object.Instantiate(Vector3Selector._prefab, content)).transform;
                _transform.localPosition=Vector3.zero;
                _transform.name = "Vector3Selector";
                this._fieldName = _transform.FindChild("TextHolder/r_Text").GetComponent<DynamicText>();
                Transform[] transformArray = new Transform[] { _transform.FindChild("ValueHolder1"), _transform.FindChild("ValueHolder2"), _transform.FindChild("ValueHolder3") };
                this._valueSelectors = Array.ConvertAll<Transform, ValueSelector>(transformArray, (Transform holder) => holder.GetComponent<ValueSelector>());
                for (int i = 0; i < 3; i++)
                {
                    int num = i;
                    ValueSelector component = transformArray[i].GetComponent<ValueSelector>();
                    component.mValue = this._values[i];
                    this._values[i].ValueChanged += new ValueChangeHandler((float v) => {
                        Vector3 value = this.CustomMapperType.Value;
                        value[num]=v;
                        this.CustomMapperType.Value = value;
                    });
                }
                this.UpdateInterface();
            }
        }

        public static void CreatePrefab()
        {
            //string versionString = VersionNumber.GetVersionString();
            //versionString = string.Concat(Regex.Match(versionString, "^[0-9.]*").Value, ".", Regex.Match(versionString, "[0-9]*$").Value);
            if ((Vector3Selector._prefab != null ? false : !Vector3Selector._failedOnce))
            {
                //Debug.Log(string.Concat("You're running version ", versionString));
                GameObject gameObject = Vector3Selector.GetPool().Get();
                gameObject.name=("Vector3Selector");
                UnityEngine.Object.Destroy(gameObject.GetComponent<ContainerDetails>());
                Vector3Selector._prefab = gameObject;
                UnityEngine.Object.DontDestroyOnLoad(Vector3Selector._prefab);
                UnityEngine.Object.Destroy(gameObject.transform.FindChild("Background").gameObject);
                GameObject _gameObject = gameObject.transform.FindChild("ValueHolder").gameObject;
                GameObject gameObject1 = UnityEngine.Object.Instantiate(_gameObject, gameObject.transform) as GameObject;
                GameObject gameObject2 = UnityEngine.Object.Instantiate(_gameObject, gameObject.transform) as GameObject;
                Vector3 _localPosition = _gameObject.transform.localPosition.y * Vector3.up;
                Transform transform = _gameObject.transform.FindChild("Background");
                Bounds _bounds = transform.GetComponent<Renderer>().bounds;
                float _size = _bounds.size.x * 1.2f;
                _gameObject.transform.localPosition=(((Vector3.right * _size) * -0.5f) + _localPosition);
                gameObject1.transform.localPosition=(((Vector3.right * _size) * 0.5f) + _localPosition);
                gameObject2.transform.localPosition=(((Vector3.right * _size) * 1.5f) + _localPosition);
                _gameObject.name=("ValueHolder1");
                gameObject1.name=("ValueHolder2");
                gameObject2.name=("ValueHolder3");
            }
        }

        private static BMWidgetPool.Pool GetPool()
        {
            return BMWidgetPool.GetPool("Prefabs/BlockMapper/ValueContainer");
        }

        protected override void UpdateInterface()
        {
            MVector3 customMapperType = base.CustomMapperType;
            for (int i = 0; i < 3; i++)
            {
                this._values[i].SetValue(customMapperType.Value[i]);
                if (!Vector3Selector._failedOnce)
                {
                    this._valueSelectors[i].Init();
                }
            }
        }
    }
}
