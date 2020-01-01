using Modding.Modules;
using UnityEngine;
using Modding;
using Modding.Serialization;
using System;

namespace lto_particle
{
    public class ParticleModuleBehavior : BlockModuleBehaviour<ParticleModule>
    {
        private GameObject particleHandler;

        private MKey fireKey;
        private MToggle holdToShootToggle;
        private MSlider particleIDSlider;

        private bool _isPlaying = false;

        public override void SafeAwake()
        {
            //input
            fireKey = GetKey(Module.key);
            holdToShootToggle = GetToggle(Module.holdSwitch);
            particleIDSlider = GetSlider(base.Module.IDSelection);

            CreateParticleHandler();
            if(particleIDSlider != null)
            {
                particleIDSlider.ValueChanged += (float value) =>
                {
                    this.OnReload();
                };
            }

        }

        //生成对应的object
        private void CreateParticleHandler()
        {
            int configID = (particleIDSlider!=null) ? (int)(particleIDSlider.Value + 0.5) : 0;
            this.particleHandler = CreateParticleObject(configID, base.gameObject);
        }

        private GameObject CreateParticleObject(int configID, GameObject parent)
        {
            //先做configID的越界检查
            if (configID + 1 > base.Module.particleConfigs.Length)
            {
                Debug.Log("[lto particle]Particle config with id "+configID.ToString()+" does not exist");
                return null;
            }
            //创建对象
            GameObject pH = new GameObject();
            var _transform = pH.transform;
            _transform.SetParent(parent.transform);//将Block的transform设为自己的parent
            var particleSystem = pH.GetComponent<ParticleSystem>() ?? pH.AddComponent<ParticleSystem>();
            var particleRenderer = pH.GetComponent<ParticleSystemRenderer>() ?? pH.AddComponent<ParticleSystemRenderer>();
            particleSystem.Stop();
            particleSystem.playOnAwake = false;
            particleSystem.loop = true;
            particleRenderer.sortMode = ParticleSystemSortMode.Distance;
            //创建子对象
            var config = base.Module.particleConfigs[configID];
            foreach(var childConfig in config.childConfigs)
            {
                var childObject = CreateParticleObject(childConfig.id, pH);
                if (childConfig.EnabledTransform())//childConfig中设置了Transform则覆盖掉子config中的Transform
                {
                    childObject.transform.localPosition = (UnityEngine.Vector3)childConfig.position;
                    childObject.transform.localEulerAngles = (UnityEngine.Vector3)childConfig.rotation;
                    childObject.transform.localScale = (UnityEngine.Vector3)childConfig.scale;
                }
                //开启延迟
                var childPS = childObject.GetComponent<ParticleSystem>();
                childPS.startDelay = childConfig.startDelay;
            }
            //subemitter
            var seConfig = config.subEmittersConfig;
            if (seConfig != null)
            {
                ParticleSystem.SubEmittersModule seModule = particleSystem.subEmitters;
                seModule.enabled = true;
                if (seConfig.birth0 >= 0)
                {
                    seModule.birth0 = pH.transform.GetChild(seConfig.birth0).gameObject.GetComponent<ParticleSystem>();
                }
                if (seConfig.birth1 >= 0)
                {
                    seModule.birth1 = pH.transform.GetChild(seConfig.birth1).gameObject.GetComponent<ParticleSystem>();
                }
                if (seConfig.collision0 >= 0)
                {
                    seModule.collision0 = pH.transform.GetChild(seConfig.collision0).gameObject.GetComponent<ParticleSystem>();
                }
                if (seConfig.collision1 >= 0)
                {
                    seModule.collision1 = pH.transform.GetChild(seConfig.collision1).gameObject.GetComponent<ParticleSystem>();
                }
                if (seConfig.death0 >= 0)
                {
                    seModule.death0 = pH.transform.GetChild(seConfig.death0).gameObject.GetComponent<ParticleSystem>();
                }
                if (seConfig.death1 >= 0)
                {
                    seModule.death1 = pH.transform.GetChild(seConfig.death1).gameObject.GetComponent<ParticleSystem>();
                }
            }
            //应用对应的config
            ApplyConfigToParticleObject(pH, config);
            return pH;
        }

        //将config的设置应用于某个ParticleSystem。
        //该函数只影响单个ParticleSystem，且不设置subEmitter
        private void ApplyConfigToParticleObject(GameObject particleObject, ParticleConfig config)
        {
            //ParticleSys.collision
            //ParticleSys.forceOverLifetime;
            //ParticleSys.externalForces;
            //ParticleSys.hideFlags;
            //ParticleSys.limitVelocityOverLifetime;
            //ParticleSys.startDelay;
            //ParticleSys.tag;
            //ParticleSys.textureSheetAnimation;
            //ParticleSys.trigger;
            //ParticleSys.velocityOverLifetime;
            //to make
            ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
            ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();

            //播放速度
            particleSystem.playbackSpeed = config.playbackSpeed;
            //Shape
            {
                var sc = config.shapeConfig;
                ParticleSystem.ShapeModule _shape = particleSystem.shape;
                switch (sc.shapeType)
                {
                    case "Sphere":
                        _shape.shapeType = ParticleSystemShapeType.Sphere; break;
                    case "SphereShell":
                        _shape.shapeType = ParticleSystemShapeType.SphereShell; break;
                    case "Hemisphere":
                        _shape.shapeType = ParticleSystemShapeType.Hemisphere; break;
                    case "HemisphereShell":
                        _shape.shapeType = ParticleSystemShapeType.HemisphereShell; break;
                    case "Cone":
                        _shape.shapeType = ParticleSystemShapeType.Cone; break;
                    case "ConeShell":
                        _shape.shapeType = ParticleSystemShapeType.ConeShell; break;
                    case "Box":
                        _shape.shapeType = ParticleSystemShapeType.Box; break;
                    case "ConeVolume":
                        _shape.shapeType = ParticleSystemShapeType.ConeVolume; break;
                    case "ConeVolumeShell":
                        _shape.shapeType = ParticleSystemShapeType.ConeVolumeShell; break;
                    case "Circle":
                        _shape.shapeType = ParticleSystemShapeType.Circle; break;
                    case "CircleEdge":
                        _shape.shapeType = ParticleSystemShapeType.CircleEdge; break;
                    case "SingleSidedEdge":
                        _shape.shapeType = ParticleSystemShapeType.SingleSidedEdge; break;
                    default:
                        Debug.Log("[lto_particle]Failed to find ShapeType：" + sc.shapeType);
                        _shape.shapeType = ParticleSystemShapeType.ConeShell;
                        break;
                }
                _shape.normalOffset = sc.normalOffset;
                _shape.box = sc.box;
                _shape.radius = sc.radius;
                _shape.arc = sc.arc;
                _shape.angle = sc.angle;
                _shape.length = sc.length;
                _shape.randomDirection = sc.randomDirection;
                _shape.enabled = true;
            }
            //粒子阻力和随机速度限制
            {
                var lv = particleSystem.limitVelocityOverLifetime;
                lv.limit = new ParticleSystem.MinMaxCurve(config.minVelocityLimit, config.maxVelocityLimit);
                lv.dampen = config.dampen;
                lv.enabled = true;
            }
            //transform
            try { 
                Transform _transform = particleObject.transform;
                //var _draggedBlock = base.GetComponent<ParticleBlockScript>();
                //_transform.localPosition = _draggedBlock.position.Value;
                //_transform.localEulerAngles = _draggedBlock.rotation.Value;
                //_transform.localScale = _draggedBlock.scale.Value;

                _transform.localPosition = config.localPosition;
                _transform.localEulerAngles = config.localEularAngles;
                _transform.localScale = config.localScale;
            }
            catch (Exception e)
            {
                Debug.Log("[lto_particle]"+e.Message);
            }
            //发射速度
            particleSystem.startSpeed = config.startSpeed;
            //坐标继承模式
            switch (config.scalingMode)
            {
                case "Hierarchy":
                    particleSystem.scalingMode = ParticleSystemScalingMode.Hierarchy;
                    break;
                case "Shape":
                    particleSystem.scalingMode = ParticleSystemScalingMode.Shape;
                    break;
                default:
                    particleSystem.scalingMode = ParticleSystemScalingMode.Local;
                    break;
            }
            //emission module
            {
                var _e = particleSystem.emission;
                var _ec = config.emissionoConfig;
                _e.rate = _ec.emissionRate;
                _e.type = _ec.type == "Time" ? ParticleSystemEmissionType.Time : ParticleSystemEmissionType.Distance;
                if (_ec.bursts!=null)
                {
                    ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[_ec.bursts.Length];
                    for (int i=0; i<bursts.Length; i++)
                    {
                        bursts[i].maxCount = (short)_ec.bursts[i].max;
                        bursts[i].minCount = (short)_ec.bursts[i].min;
                        bursts[i].time = _ec.bursts[i].time;
                    }
                    _e.SetBursts(bursts);
                }
            }
            //最大粒子数量
            particleSystem.maxParticles = config.maxParticles;
            //粒子存在时间
            particleSystem.startLifetime = config.lifeTime;
            //simulation space
            particleSystem.simulationSpace = config.simulationSpace == "Local" ? ParticleSystemSimulationSpace.Local:ParticleSystemSimulationSpace.World;
            //继承发射器运动速度的程度
            {
                var iv = particleSystem.inheritVelocity;
                if ((config.inheritVelocityModifier > 0.000001|| config.inheritVelocityModifier < -0.000001) && particleSystem.simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    iv.enabled = true;
                    iv.curve = config.inheritVelocityModifier;
                }
                else
                {
                    iv.enabled = false;
                }
            }
            //gravity modifier
            particleSystem.gravityModifier = config.gravityModifier;
            //shader
            {
                Shader particleShader;
                if (config.shader.Equals("default"))
                {
                    particleShader = GameMaterials.Shaders.Particles.AlphaBlended;
                }
                else
                {
                    particleShader = Shader.Find(config.shader);
                    if (!particleShader)
                    {
                        Debug.Log("[lto_particle] Unable to Find Shader " + config.shader);
                        particleShader = GameMaterials.Shaders.Particles.AlphaBlended;
                    }
                }
                particleRenderer.material.shader = particleShader;
            }
            //texture
            particleRenderer.material.mainTexture = (ModTexture)base.GetResource(((ResourceReference)config.particleTexture));
            //size over time
            {
                var s_t = particleSystem.sizeOverLifetime;
                s_t.enabled = true;
                AnimationCurve curve = new AnimationCurve();
                foreach (var key in config.sizeKeys)
                    curve.AddKey(key.k, key.size);
                s_t.size = new ParticleSystem.MinMaxCurve(1, curve);
            }
            //color over time
            {
                //ParticleSys.startColor = new Color (config.colorKeys[0].r, config.colorKeys[0].g, config.colorKeys[0].b);

                var c_t = particleSystem.colorOverLifetime;
                c_t.enabled = true;
                Gradient grad = new Gradient();
                GradientColorKey[] ColorKeys = new GradientColorKey[config.colorKeys.Length];
                GradientAlphaKey[] AlphaKeys = new GradientAlphaKey[config.colorKeys.Length];
                for(int i=0; i< config.colorKeys.Length; i++)
                {
                    var xkey = config.colorKeys[i];
                    ColorKeys[i] = new GradientColorKey(new Color(xkey.r, xkey.g, xkey.b), xkey.k);
                    AlphaKeys[i] = new GradientAlphaKey(xkey.a, xkey.k);
                }
                grad.SetKeys(ColorKeys, AlphaKeys);
                c_t.color = grad;
            }
            //渲染模式
            switch (config.renderMode)
            {
                case "Stretch":
                    particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                    break;
                case "HorizontalBillboard":
                    particleRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                    break;
                case "VerticalBillboard":
                    particleRenderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
                    break;
                case "Mesh":
                    particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                    if (!(config.particleMesh is null))
                    {
                        particleRenderer.mesh = (ModMesh)base.GetResource(((ResourceReference)config.particleMesh));
                    }
                    else
                    {
                        Debug.Log("[lto_particle] no mesh specified for particle " + config.name);
                    }
                    break;
                default:
                    particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                    break;
            }

            //random rotation
            if (config.randomRotation)
            {
                var _rotation = particleSystem.rotationOverLifetime;
                var curve1 = new AnimationCurve();
                var curve2 = new AnimationCurve();
                curve1.AddKey(0, -3000000f);
                curve1.AddKey(0.00001f/config.lifeTime, 0);
                curve2.AddKey(0, 3000000f);
                curve2.AddKey(0.00001f/config.lifeTime, 0);

                _rotation.z = new ParticleSystem.MinMaxCurve(1, curve1, curve2);
                if(config.renderMode == "Mesh")
                {
                    _rotation.separateAxes = true;
                    _rotation.x = new ParticleSystem.MinMaxCurve(1, curve1, curve2);
                    _rotation.y = new ParticleSystem.MinMaxCurve(1, curve1, curve2);
                }
                _rotation.enabled = true;
            }
            //collision
            if (config.collisionModuleConfig!=null)
            {
                var _collision = particleSystem.collision;
                var _collisionConfig = config.collisionModuleConfig;
                _collision.enabled = true;

                _collision.type = _collisionConfig.type == "Planes" ? ParticleSystemCollisionType.Planes : ParticleSystemCollisionType.World;

                _collision.dampen = _collisionConfig.dampen;
                _collision.bounce = _collisionConfig.bounce;
                _collision.enableDynamicColliders = _collisionConfig.enableDynamicColliders;
                _collision.enableInteriorCollisions = _collisionConfig.enableInteriorCollisions;
                _collision.bounce = _collisionConfig.bounce;
                _collision.lifetimeLoss = _collisionConfig.lifetimeLoss;
                _collision.maxCollisionShapes = _collisionConfig.MaxCollisionShapes;
                _collision.maxKillSpeed = _collisionConfig.maxKillSpeed;
                _collision.minKillSpeed = _collisionConfig.minKillSpeed;
                _collision.radiusScale = _collisionConfig.radiusScale;
                _collision.quality = _collisionConfig.quality=="High"? ParticleSystemCollisionQuality.High:
                    (_collisionConfig.quality == "Medium"?ParticleSystemCollisionQuality.Medium:ParticleSystemCollisionQuality.Low);
            }
        }

        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
        }
        public override void OnSimulateStop()
        {
            base.OnSimulateStop();
            _isPlaying = false;
        }

        public override void SimulateUpdateAlways()
        {
            var particleSystem = particleHandler.GetComponent<ParticleSystem>();
            //处理按键控制
            if (this.fireKey.IsPressed && !this.holdToShootToggle.IsActive)
            {
                if (_isPlaying)
                {
                    _isPlaying = false;
                    particleSystem.Stop();
                }
                else
                {
                    _isPlaying = true;
                    particleSystem.Play();
                }
            }
            if (this.fireKey.IsPressed && this.holdToShootToggle.IsActive)
            {
                particleSystem.Play();
                _isPlaying = true;
            }
            if (this.fireKey.IsReleased && this.holdToShootToggle.IsActive)
            {
                _isPlaying = false;
                particleSystem.Stop();
            }
        }
        public override void OnReload()
        {
            GameObject.Destroy(particleHandler);
            CreateParticleHandler();
        }
        public void PreprocessForReloading()
        {

        }
    }
}
