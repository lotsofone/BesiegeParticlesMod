using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace lto_particle
{
    class RetroEffectBlockScript : BlockScript
    {
        MKey startKey;
        MValue channelCount;
        MValue particlePerChannel;
        MSlider changeDuration;
        MSlider axisSpeedRamdon, normalSpeedRandom;
        MSlider dampen;
        MSlider windPower;
        MSlider initialSpeed;

        RetroBehaviour retroBehaviour;
        public override void SafeAwake()
        {
            base.SafeAwake();
            startKey = AddKey("启动", "lto_particle_startKey", KeyCode.P);
            this.channelCount = AddValue("通道数", "lto_particle_channelCount", 4);
            this.particlePerChannel = AddValue("粒子数量", "lto_particle_particlePerChannel", 100);
            this.changeDuration = AddSliderUnclamped("变化时间", "lto_particle_changeDuration", 1, 0, 10);
            this.axisSpeedRamdon = AddSliderUnclamped("线速度波动", "lto_particle_axisSpeedRandom", 0.15f, 0, 1f);
            this.normalSpeedRandom = AddSliderUnclamped("法速度波动", "lto_particle_normalSpeedRandom", 0.0625f, 0, 1f);
            this.dampen = AddSliderUnclamped("阻尼", "lto_particle_dampen", 0.015f, 0, 0.1f);
            this.windPower = AddSliderUnclamped("风力", "lto_particle_windPower", 0.01f, 0f, 0.1f);
            this.initialSpeed = AddSliderUnclamped("发射速度", "lto_particle_initialSpeed", 0.1f, 0f, 1f);
        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            TurnColliders(false); TurnRenderer(false);
            this.retroBehaviour = this.gameObject.AddComponent<RetroBehaviour>();
            this.retroBehaviour.channelCount = Mathf.RoundToInt(channelCount.Value);
            this.retroBehaviour.particleCountPerChannel = Mathf.RoundToInt(particlePerChannel.Value);
            if (this.retroBehaviour.particleCountPerChannel <= 0) this.retroBehaviour.particleCountPerChannel = 1;
            if (this.retroBehaviour.channelCount <= 0) this.retroBehaviour.channelCount = 1;
            this.retroBehaviour.changeDuration = changeDuration.Value;
            if (this.retroBehaviour.changeDuration < 0) this.retroBehaviour.changeDuration = 0;
            this.retroBehaviour.axisSpeedRandom = axisSpeedRamdon.Value;
            this.retroBehaviour.normalSpeedRandom = normalSpeedRandom.Value;
            this.retroBehaviour.dampen = dampen.Value;
            this.retroBehaviour.windPower = windPower.Value;
            this.retroBehaviour.initialSpeed = initialSpeed.Value;
        }
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            if (startKey.IsPressed)
            {
                this.retroBehaviour._starting = !this.retroBehaviour._starting;
            }
        }
        //模拟时关闭物理碰撞箱和本体显示，并消去靶箱
        public void TurnColliders(bool state)
        {
            var colliders = gameObject.GetComponentsInChildren<BoxCollider>(true);
            //Debug.Log(colliders.Length);
            foreach (var collider in colliders)
            {
                if (collider.name == "Box Collider" || collider.name == "Adding Point")
                    collider.enabled = state;
            }
        }
        public void TurnRenderer(bool state)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.name == "Vis")
                    renderer.enabled = state;
            }
        }
    }
    public class RetroBehaviour : MonoBehaviour
    {

        ParticleSystem.Particle[] particles;
        int _particleCount;
        int[] offsets;
        float colorLerp = 0;
        Color fullColor0 = new Color(1f, 0.4f, 0.1f);
        Color fullColor1 = new Color(0.7f, 0.1f, 0f);
        Color halfColor0 = new Color(0.8f, 0.1f, 0);
        public int channelCount = 4;
        public bool _starting = false;
        public float changeDuration = 1f;
        public int particleCountPerChannel = 100;
        public float axisSpeedRandom = 0.15f;
        public float normalSpeedRandom = 0.0625f;
        public float dampen = 0.015f;
        public float windPower = 0.01f;
        public float initialSpeed = 0.1f;

        // Start is called before the first frame update
        public void Start()
        {
            //generate full particles
            var ps = gameObject.GetComponent<ParticleSystem>();
            if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();
            _particleCount = particleCountPerChannel * channelCount;
            SetUpParticle(ps);
            particles = new ParticleSystem.Particle[_particleCount];
            ps.Emit(_particleCount);
            ps.GetParticles(particles);
            //split channels
            offsets = new int[channelCount];
            for (int i = 0; i < channelCount; i++)
            {
                offsets[i] = i * _particleCount / channelCount;
            }
            //rotates
            for (int i = 0; i < channelCount; i++)
            {
                _StartChannel(i);
                _UpdateChannel(i);
            }
            ps.SetParticles(particles, _particleCount);
        }
        void SetUpParticle(ParticleSystem ps)
        {
            var emission = ps.emission;
            emission.rate = 0;
            ps.startLifetime = 10;
            ps.simulationSpace = ParticleSystemSimulationSpace.Local;

            ps.maxParticles = particleCountPerChannel * channelCount;

            //ps.collision.maxKillSpeed = 0f;


            //rotation over time
            var rol = ps.rotationOverLifetime;
            rol.enabled = true;
            rol.separateAxes = true;
            //size over time
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.separateAxes = true;
            //color over time
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
            for (int i = 0; i < 3; i++)
            {
                colorKeys[i] = new GradientColorKey(new Color(0f, 0f, 0f), 0f + i * 0.5f);
                alphaKeys[0] = new GradientAlphaKey(1f, 0f + i * 0.5f);
            }
            gradient.SetKeys(colorKeys, alphaKeys);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = gradient;
            //renderer
            var renderer = GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.alignment = ParticleSystemRenderSpace.Local;
            if (Shader.Find("Particles/Additive") != null) renderer.material.shader = Shader.Find("Particles/Additive");
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, -0.5f, 0) };
            mesh.triangles = new int[]
            { 0, 1, 2,
          0, 2, 3
            };
            renderer.mesh = mesh;
        }

        private void _StartChannel(int channelID)
        {
            int begin = offsets[channelID];
            int end = channelID + 1 == channelCount ? particles.Length : offsets[channelID + 1];

        }

        // Update is called once per frame
        public void Update()
        {
            var ps = gameObject.GetComponent<ParticleSystem>();
            ps.SetParticles(particles, _particleCount);
            //设置颜色渐变
            var colorModule = ps.colorOverLifetime;
            var keys = colorModule.color.gradient.colorKeys;
            var key1 = keys[1];
            var key0 = keys[0];
            if (colorLerp > 0.5)
            {
                key0.color = Color.Lerp(new Color(0.8f, 0.1f, 0), fullColor0, colorLerp * 2 - 1);
            }
            else
            {
                key0.color = Color.Lerp(new Color(0, 0, 0), halfColor0, colorLerp * 2);
            }
            key1.color = Color.Lerp(new Color(0, 0, 0), fullColor1, colorLerp);
            keys[1] = key1;
            keys[0] = key0;
            Gradient grad = new Gradient();
            grad.SetKeys(keys, colorModule.color.gradient.alphaKeys);
            colorModule.color = grad;
            if (changeDuration <= 0)
            {
                colorLerp = _starting ? 1 : 0;
            }
            else
            {
                if (_starting)
                {
                    colorLerp = Mathf.Clamp01(colorLerp + Time.deltaTime * 1f / changeDuration);
                }
                else
                {
                    colorLerp = Mathf.Clamp01(colorLerp - Time.deltaTime * 1f / changeDuration);
                }
            }
            for (int i = 0; i < channelCount; i++)
            {
                _UpdateChannel(i);
            }
        }

        private void _UpdateChannel(int channelID)
        {
            int begin = offsets[channelID];
            int end = channelID + 1 == channelCount ? particles.Length : offsets[channelID + 1];

            var ps = gameObject.GetComponent<ParticleSystem>();

            Vector3 initialPosition = Vector3.zero;
            Vector3 velocity;
            if (ps.GetComponent<Rigidbody>() != null)
            {
                velocity = ps.transform.InverseTransformDirection(ps.GetComponent<Rigidbody>().velocity);
            }
            else
            {
                velocity = new Vector3(0, 0, 1f);
            }
            Vector3 wind = Vector3.Normalize(velocity) * -windPower;

            Vector3 d = new UnityEngine.Vector3(axisSpeedRandom, normalSpeedRandom, normalSpeedRandom);
            d.x -= d.x * UnityEngine.Random.value * 2;
            d.y -= d.y * UnityEngine.Random.value * 2;
            d.z -= d.z * UnityEngine.Random.value * 2;
            d *= 0.5f;
            d = Quaternion.AngleAxis(Vector3.Angle(velocity, Vector3.right), Vector3.Cross(Vector3.right, velocity)) * d;
            Vector3 initialVelocity = Vector3.forward * initialSpeed;
            float startSize = 1;
            float deltaSize = 1f / (end - begin);
            float startLifeTime = ps.startLifetime;
            float deltaLifeTime = startLifeTime / (end - begin);
            for (int i = begin; i < end; i++)
            {
                var p = particles[i];
                particles[i].position = initialPosition;
                particles[i].lifetime = (startLifeTime - deltaLifeTime);
                startLifeTime -= deltaLifeTime;

                initialVelocity = (initialVelocity + wind) * (1f - dampen);
                initialPosition += initialVelocity + d;

                var sz = particles[i].startSize3D;
                sz.y = startSize;
                sz.z = startSize;
                particles[i].startSize3D = sz;
                startSize += deltaSize;
            }
            float rotation = UnityEngine.Random.value * 360;

            for (int i = begin; i < end; i++)
            {
                var p = particles[i];
                p.rotation3D = new Vector3(rotation, 0, 0);
                particles[i] = p;
            }
        }
    }

}
