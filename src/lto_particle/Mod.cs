using Modding;
using Modding.Modules;
using UnityEngine;
using Modding.Mapper;

namespace lto_particle
{
    public class Mod : ModEntryPoint
	{
		public override void OnLoad()
		{
            // Called when the mod is loaded.
            CustomMapperTypes.AddMapperType<Vector3, MVector3, Vector3Selector>();
            CustomModules.AddBlockModule<ParticleModule, ParticleModuleBehavior>("ParticleModule", true);
            
		}
	}
}
