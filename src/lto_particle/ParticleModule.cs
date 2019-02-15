using Modding.Modules;
using Modding.Serialization;
using System.Xml.Serialization;

namespace lto_particle
{
    [Reloadable]
    [XmlRoot("ParticleModule")]
    public class ParticleModule : BlockModule
    {
        [XmlElement("Key", typeof(MKeyReference)), RequireToValidate]
        public MKeyReference key { get; set; }
        
        [XmlElement("HoldSwitch", typeof(MToggleReference)), RequireToValidate]
        public MToggleReference holdSwitch {get; set; }

        [XmlElement("IDSelection", typeof(MSliderReference))]
        public MSliderReference IDSelection { get; set; }

        [XmlArray("ParticleConfigs")]
        [XmlArrayItem("ParticleConfig", typeof(ParticleConfig))]
        [Reloadable]
        public ParticleConfig[] particleConfigs;
        protected override bool Validate(string elemName)
        {
            if (!base.Validate(elemName)) return false;
            for (int i = 0; i < particleConfigs.Length; i++)
            {
                var config = particleConfigs[i];
                foreach (var id in config.childConfigs)
                {
                    if (id.id + 1 > particleConfigs.Length)
                    {
                        return InvalidData(elemName, "particle id " + id.id.ToString() + "does not exist");
                    }
                }
                //if (config.renderMode == "Mesh" && config.particleMesh is null)
                //{
                //    return MissingElement(elemName, "Mesh");
                //}
            }
            return true;
        }
    }
    [Reloadable]
    public class ParticleConfig : Element
    {
        //子级元素粒子发射器
        [XmlArray("ChildConfigs"), CanBeEmpty]
        [XmlArrayItem("ChildConfig"), Reloadable]
        public ChildConfig[] childConfigs { set; get; } = new ChildConfig[0];
        //subEmitter设置
        [XmlElement("SubEmitters"), CanBeEmpty, Reloadable]
        public SubEmittersConfig subEmittersConfig { set; get; } = null;

        [XmlAttribute, Reloadable]
        public string name = "";
        [XmlArray("ColorOverTime")]
        [XmlArrayItem("ColorKey", typeof(ColorKey))]
        [Reloadable]
        public ColorKey[] colorKeys { set; get; }

        [XmlArray("SizeOverTime")]
        [XmlArrayItem("SizeKey", typeof(SizeKey))]
        [Reloadable]
        public SizeKey[] sizeKeys { set; get; }

        [XmlElement("Shader")]
        [Reloadable]
        public string shader { set; get; }

        [XmlElement("Texture", typeof(ResourceReference))]
        [Reloadable]
        public object particleTexture { set; get; }

        [XmlElement("Emission")]
        [Reloadable]
        public EmissionConfig emissionoConfig;
        [XmlElement("MaxParticles")]
        [Reloadable]
        public int maxParticles = 1024;
        [XmlElement("LifeTime")]
        [Reloadable]
        public float lifeTime;
        [XmlElement("SimulationSpace")]
        [Reloadable]
        public string simulationSpace = "Local";
        //速度继承量
        [XmlElement("InheritVelocityModifier"), Reloadable]
        public float inheritVelocityModifier;
        [XmlElement("GravityModifier"), Reloadable]
        public float gravityModifier = 0;
        //发射速度
        [XmlElement("Speed"), Reloadable]
        public float startSpeed;
        //坐标继承模式
        [XmlElement("ScalingMode"), Reloadable]
        public string scalingMode;
        //本地的坐标系
        [XmlElement("LocalPosition"), Reloadable]
        public Modding.Serialization.Vector3 localPosition = new Modding.Serialization.Vector3(0, 0, 0);
        [XmlElement("LocalEularAngles"), Reloadable]
        public Modding.Serialization.Vector3 localEularAngles = new Modding.Serialization.Vector3(0, 0, 0);
        [XmlElement("LocalScale"), Reloadable]
        public Modding.Serialization.Vector3 localScale = new Modding.Serialization.Vector3(1, 1, 1);
        //粒子阻力
        [XmlElement("Dampen"), Reloadable]
        public float dampen;
        //速度限制
        [XmlElement("MaxVelocityLimit"), Reloadable]
        public float maxVelocityLimit;
        [XmlElement("MinVelocityLimit"), Reloadable]
        public float minVelocityLimit;
        [XmlElement("ShapeConfig", typeof(ShapeConfig)), Reloadable]
        public ShapeConfig shapeConfig { set; get; }
        //碰撞模块
        [XmlElement("Collision", typeof(CollisionModuleConfig)), Reloadable]
        public CollisionModuleConfig collisionModuleConfig { set; get; } =null;
        //播放速度
        [XmlElement("PlayBackSpeed"), Reloadable]
        public float playbackSpeed = 1;
        //渲染模式
        [XmlElement("RenderMode"), Reloadable]
        public string renderMode = "Billboard";
        //mesh 如果使用了RenderMode为Mesh则需要指定Mesh
        [XmlElement("Mesh", typeof(ResourceReference))]
        [Reloadable]
        public object particleMesh { set; get; } = null;
        //随机初始角度
        [XmlElement("RandomRotation"), Reloadable]
        public bool randomRotation = false;
    }
    [Reloadable]
    public class ChildConfig
    {
        [XmlAttribute("startDelay"), Reloadable]
        public float startDelay = 0;
        [XmlAttribute("id"), Reloadable]
        public int id;
        [XmlElement("Position"), Reloadable]
        public Vector3? position = null;
        [XmlElement("Rotation"), Reloadable]
        public Vector3? rotation = null;
        [XmlElement("Scale"), Reloadable]
        public Vector3? scale = null;
        public bool EnabledTransform()
        {
            return rotation.HasValue && position.HasValue && scale.HasValue;
        }
    }
    [Reloadable]
    public class EmissionConfig
    {
        [XmlAttribute("rate"), Reloadable]
        public float emissionRate;
        [XmlAttribute("type"), Reloadable]
        public string type = "Time";
        [Reloadable]
        public class Burst
        {
            [XmlAttribute("max"), Reloadable]
            public int max;
            [XmlAttribute("min"), Reloadable]
            public int min;
            [XmlAttribute("time"), Reloadable]
            public float time;
        }
        [XmlArray("Bursts"), CanBeEmpty]
        [XmlArrayItem("Burst"), Reloadable]
        public Burst[] bursts;
    }
    [Reloadable]
    public class SubEmittersConfig
    {
        [XmlAttribute("birth0"), Reloadable]
        public int birth0 = -1;
        [XmlAttribute("birth1"), Reloadable]
        public int birth1 = -1;
        [XmlAttribute("collision0"), Reloadable]
        public int collision0 = -1;
        [XmlAttribute("collision1"), Reloadable]
        public int collision1 = -1;
        [XmlAttribute("death0"), Reloadable]
        public int death0 = -1;
        [XmlAttribute("death1"), Reloadable]
        public int death1 = -1;
    }
    [Reloadable]
    public class ShapeConfig
    {
        //ShapeType
        [XmlElement("ShapeType"), Reloadable]
        public string shapeType;
        //ShapeRandomDirection
        [XmlElement("RandomDirection"), Reloadable]
        public bool randomDirection = false;

        [XmlElement("NormalOffset"), Reloadable]
        public float normalOffset = 0;
        [XmlElement("Box"), Reloadable]
        public Modding.Serialization.Vector3 box = new Modding.Serialization.Vector3(0, 0, 0);
        [XmlElement("Radius"), Reloadable]
        public float radius = 0;
        [XmlElement("Arc"), Reloadable]
        public float arc = 0;
        [XmlElement("Angle"), Reloadable]
        public float angle = 0;
        [XmlElement("Length"), Reloadable]
        public float length = 0;
    }
    [Reloadable]
    public class CollisionModuleConfig
    {
        [XmlElement("Dampen"), Reloadable]
        public float dampen = 0;
        [XmlElement("Bounce"), Reloadable]
        public float bounce = 1;
        [XmlElement("EnableDynamicColliders"), Reloadable]
        public bool enableDynamicColliders = true;
        [XmlElement("EnableInteriorCollisions"), Reloadable]
        public bool enableInteriorCollisions = true;
        [XmlElement("LifetimeLoss"), Reloadable]
        public float lifetimeLoss = 0;
        [XmlElement("MaxCollisionShapes"), Reloadable]
        public int MaxCollisionShapes = 256;
        [XmlElement("MaxKillSpeed"), Reloadable]
        public float maxKillSpeed = 10000;
        [XmlElement("MinKillSpeed"), Reloadable]
        public float minKillSpeed = 1;
        [XmlElement("RadiusScale"), Reloadable]
        public float radiusScale = 1;
        [XmlAttribute("type"), Reloadable]
        public string type = "World";
        [XmlElement("Quality"), Reloadable]
        public string quality = "High";
    }
    [Reloadable]
    public class ColorKey
    {
        [XmlAttribute("r")] [Reloadable] public float r { set; get; }
        [XmlAttribute("g")] [Reloadable] public float g { set; get; }
        [XmlAttribute("b")] [Reloadable] public float b { set; get; }
        [XmlAttribute("a")] [Reloadable] public float a { set; get; } = 1;
        [XmlAttribute("k")] [Reloadable] public float k { set; get; } = 0;
    }
    [Reloadable]
    public class SizeKey
    {
        [XmlAttribute("k")] [Reloadable] public float k { set; get; }
        [XmlAttribute("size")] [Reloadable] public float size { set; get; }
    }
}
