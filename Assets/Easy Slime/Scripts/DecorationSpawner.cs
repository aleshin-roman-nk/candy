using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

using Random = UnityEngine.Random;

namespace EasySlime
{
    public class DecorationSpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("The slime that will have the decorations on it.")]
        Slime m_Slime = null;

        [SerializeField, Tooltip("Overall scale of the decorations. Affects size, normal offset and press offset.")]
        float m_Scale = 1.0f;

        [SerializeField, Range(0f, 1f), Tooltip("Overall fill of all the decorations. This is multiplied by the highest individual decoration fill to calculate the total decorations spawned.")]
        float m_Fill = 1.0f;

        [SerializeField, Tooltip("Decoration info. Make sure to at least set the mesh & material, as well as fill and scale values both greater than 0.")]
        List<DecorationInfo> m_DecorationInfos = new List<DecorationInfo>();

        public Slime Slime { get => m_Slime; set => m_Slime = value; }
        public float Scale { get => m_Scale; set => m_Scale = value; }
        public float Fill { get => m_Fill; set => m_Fill = value; }
        public List<DecorationInfo> DecorationInfos { get => m_DecorationInfos; set => m_DecorationInfos = value; }

        [System.Serializable]
        public struct DecorationInfo
        {
            [Tooltip("Mesh used for the decoration.")]
            public Mesh mesh;

            [Range(0f, 1f), Tooltip("Individual fill for this decoration. The highest individual decoration fill is multiplied by the overall fill to calculate the total decorations spawned.")]
            public float fill;

            [Tooltip("Material used for the decoration. Ensure GPU instancing is enabled on the material if GPU instancing is desired.")]
            public Material material;

            [Tooltip("The amount the decoration is offset along the slime's vertex normal. Affected by the overall scale.")]
            public float normalOffset;

            [Tooltip("The amount the decoration is offset inwards along the slime's inverted normal based on how much that point is pressed in. Affected by the overall scale.")]
            public float pressOffset;

            [Tooltip("Minimum rotation in degrees of each spawned decoration. The Z axis is rotation around the slime's vertex normal.")]
            public Vector3 minRotation;

            [Tooltip("Maximum rotation in degrees of each spawned decoration. The Z axis is rotation around the slime's vertex normal.")]
            public Vector3 maxRotation;

            [Tooltip("Scale of each decoration. Affected by the overall scale.")]
            public float scale;

			public override bool Equals(object obj)
			{
				return obj is DecorationInfo info &&
					   EqualityComparer<Mesh>.Default.Equals(mesh, info.mesh) &&
					   fill == info.fill &&
					   EqualityComparer<Material>.Default.Equals(material, info.material) &&
					   normalOffset == info.normalOffset &&
					   pressOffset == info.pressOffset &&
					   minRotation.Equals(info.minRotation) &&
					   maxRotation.Equals(info.maxRotation) &&
					   scale == info.scale;
			}

			public override int GetHashCode()
			{
				return System.HashCode.Combine(mesh, fill, material, normalOffset, pressOffset, minRotation, maxRotation, scale);
			}

			public static bool operator ==(DecorationInfo lhs, DecorationInfo rhs)
            {
                return lhs.mesh == rhs.mesh &&
                       lhs.fill == rhs.fill &&
                       lhs.material == rhs.material &&
                       lhs.normalOffset == rhs.normalOffset &&
                       lhs.pressOffset == rhs.pressOffset &&
                       lhs.minRotation == rhs.minRotation &&
                       lhs.maxRotation == rhs.maxRotation &&
                       lhs.scale == rhs.scale;
            }

            public static bool operator !=(DecorationInfo lhs, DecorationInfo rhs)
            {
                return !(lhs == rhs);
            }
        }

        struct DecorationData
        {
            public int vertexIndex;
            public float3 rotation;
        }

        List<DecorationInfo> m_InternalDecorationInfos = new List<DecorationInfo>();
        List<int> m_RandomIndices = new List<int>();
        List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();
        List<NativeArray<DecorationData>> m_DecorationDatas = new List<NativeArray<DecorationData>>();
        List<NativeArray<ParticleSystem.Particle>> m_ParticleArrays = new List<NativeArray<ParticleSystem.Particle>>();

		private void OnDestroy()
        {
            for (int i = 0; i < m_ParticleArrays.Count; i++)
            {
                m_ParticleArrays[i].Dispose();
            }

            for (int i = 0; i < m_DecorationDatas.Count; i++)
            {
                m_DecorationDatas[i].Dispose();
            }
        }

		public void RebuildIfNeeded(bool force = false)
        {
            bool rebuild = false;

            if (m_Slime == null)
                return;

            if (m_InternalDecorationInfos.Count != m_DecorationInfos.Count)
            {
                rebuild = true;
            }
            else
            {
                for (int i = 0; i < m_InternalDecorationInfos.Count; i++)
                {
                    if (m_InternalDecorationInfos[i] != m_DecorationInfos[i])
                    {
                        rebuild = true;
                        break;
                    }
                }
            }

            if (rebuild || force)
            {
                m_InternalDecorationInfos.Clear();
                for (int i = 0; i < m_DecorationInfos.Count; i++)
                {
                    m_InternalDecorationInfos.Add(m_DecorationInfos[i]);
                }
            }

            float highestFill = 0.0f;
            foreach (var d in m_InternalDecorationInfos)
            {
                if (d.fill > highestFill)
                    highestFill = d.fill;
            }

            int maxCount = Mathf.RoundToInt(m_Slime.Vertices.Length * Mathf.Clamp01(highestFill));

            if (maxCount != m_RandomIndices.Count)
                rebuild = true;

            if (!rebuild && !force)
                return;

            List<int> validVertices = new List<int>();

            for (int i = 0; i < m_Slime.Vertices.Length; i++)
            {
                validVertices.Add(i);
            }

            m_RandomIndices.Clear();

            for (int i = 0; i < maxCount; i++)
            {
                int index = Random.Range(0, validVertices.Count);
                int vertexIndex = validVertices[index];
                validVertices.RemoveAt(index);

                m_RandomIndices.Add(vertexIndex);
            }

            for (int i = 0; i < m_ParticleSystems.Count; i++)
            {
                Destroy(m_ParticleSystems[i].gameObject);
            }
            m_ParticleSystems.Clear();

            for (int i = 0; i < m_InternalDecorationInfos.Count; i++)
            {
                var go = new GameObject("Particle System");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                var ps = go.AddComponent<ParticleSystem>();
                m_ParticleSystems.Add(ps);

                ps.Stop();

                var main = ps.main;
                main.playOnAwake = false;
                main.loop = false;
                main.startSpeed = new ParticleSystem.MinMaxCurve(0);
                main.startSize = new ParticleSystem.MinMaxCurve(m_InternalDecorationInfos[i].scale);//, 0.4f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(float.PositiveInfinity);
                main.simulationSpace = ParticleSystemSimulationSpace.Custom;
                main.customSimulationSpace = m_Slime.transform;
                main.startRotation3D = true;
                main.startRotation = new ParticleSystem.MinMaxCurve(0f);

                var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
                psRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                psRenderer.mesh = m_InternalDecorationInfos[i].mesh;
                psRenderer.material = m_InternalDecorationInfos[i].material;
                psRenderer.enableGPUInstancing = true;
                psRenderer.alignment = ParticleSystemRenderSpace.Local;
                psRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }

            float totalFill = 0.0f;
            for (int i = 0; i < m_InternalDecorationInfos.Count; i++)
            {
                totalFill += Mathf.Clamp01(m_InternalDecorationInfos[i].fill);
            }


            for (int i = 0; i < m_DecorationDatas.Count; i++)
            {
                m_DecorationDatas[i].Dispose();
            }
            m_DecorationDatas.Clear();

            int randomIndexRemaining = m_RandomIndices.Count;
            for (int i = 0; i < m_ParticleSystems.Count; i++)
            {
                int particleCount = 0;

                if (totalFill > 0.0f)
                    particleCount = Mathf.RoundToInt(m_RandomIndices.Count * (Mathf.Clamp01(m_InternalDecorationInfos[i].fill) / totalFill));

                m_DecorationDatas.Add(new NativeArray<DecorationData>(math.min(particleCount, randomIndexRemaining), Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

                randomIndexRemaining -= particleCount;
            }

            int total = 0;

            for (int i = 0; i < m_ParticleSystems.Count; i++)
            {
                for (int k = 0; k < m_DecorationDatas[i].Length; k++)
                {
                    var decorationData = m_DecorationDatas[i];
                    var data = decorationData[k];

                    data.vertexIndex = m_RandomIndices[total];

                    Vector3 minRotation = m_InternalDecorationInfos[i].minRotation;
                    Vector3 maxRotation = m_InternalDecorationInfos[i].maxRotation;

                    data.rotation = new Vector3(Random.Range(minRotation.x, maxRotation.x), Random.Range(minRotation.y, maxRotation.y), Random.Range(minRotation.z, maxRotation.z));

                    decorationData[k] = data;

                    total++;
                }
            }

            for (int i = 0; i < m_ParticleSystems.Count; i++)
            {
                var ps = m_ParticleSystems[i];

                var main = ps.main;
                main.maxParticles = m_DecorationDatas[i].Length;

                ps.Emit(main.maxParticles);
            }
        }


        void Update()
        {
            if (m_Slime == null)
                return;

            RebuildIfNeeded();

            transform.position = m_Slime.transform.position;
            transform.rotation = m_Slime.transform.rotation;

            if (m_InternalDecorationInfos.Count == 0)
                return;

            for (int k = 0; k < m_ParticleSystems.Count; k++)
            {
                var ps = m_ParticleSystems[k];

                var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
                psRenderer.material = m_InternalDecorationInfos[k].material;
                psRenderer.mesh = m_InternalDecorationInfos[k].mesh;

                int intendedParticleCount = Mathf.RoundToInt(Mathf.Clamp01(m_Fill) * ps.main.maxParticles);

                if (m_ParticleArrays.Count <= k)
                {
                    m_ParticleArrays.Add(new NativeArray<ParticleSystem.Particle>(intendedParticleCount, Allocator.Persistent, NativeArrayOptions.ClearMemory));
                    ps.Clear();
                    ps.Emit(intendedParticleCount);
                }
                else if (m_ParticleArrays[k].Length != intendedParticleCount)
                {
                    m_ParticleArrays[k].Dispose();
                    m_ParticleArrays[k] = new NativeArray<ParticleSystem.Particle>(intendedParticleCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                    ps.Clear();
                    ps.Emit(intendedParticleCount);
                }

                var particles = m_ParticleArrays[k];

                ps.GetParticles(particles);

                UpdateParticles job;
                job.normalOffset = m_InternalDecorationInfos[k].normalOffset * m_Scale;
                job.pressOffset = m_InternalDecorationInfos[k].pressOffset * m_Scale;
                job.particles = particles;
                job.size = m_InternalDecorationInfos[k].scale * m_Scale;
                job.decorationData = m_DecorationDatas[k];
                job.vertices = m_Slime.Vertices;
                job.Schedule(particles.Length, 100).Complete();

                ps.SetParticles(particles);
            }
        }

        [BurstCompile]
        struct UpdateParticles : IJobParallelFor
        {
            public NativeArray<ParticleSystem.Particle> particles;

            [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<Slime.VertexData> vertices;
            [ReadOnly] public NativeArray<DecorationData> decorationData;

            [ReadOnly] public float normalOffset;
            [ReadOnly] public float pressOffset;
            [ReadOnly] public float size;

            public void Execute(int index)
            {
                var data = decorationData[index];
                var vertexData = vertices[data.vertexIndex];

                var particle = particles[index];

                particle.position = vertexData.position;

                // Position is vertex position plus an offset based on the inverted surface normal of the vertex (pressOffset) and a flat offset based on the normalOffset
                float pressIn01 = vertexData.uv0.z;
                float3 vertexNormal = vertexData.normal;
                particle.position += (Vector3)(-vertexNormal * pressIn01 * pressOffset);

                particle.position += (Vector3)(vertexNormal * normalOffset);

                // Rotation
                // Particles are rotated so they orient relative to the vertex normal
                if (math.any(vertexNormal != float3.zero))
                {
                    particle.rotation3D = math.degrees(toEuler(math.mul(quaternion.LookRotationSafe(vertexData.normal, math.up()), quaternion.Euler(math.radians(data.rotation)))));
                }

                // Scale
                particle.startSize = size;

                particles[index] = particle;
            }

            static float3 toEuler(quaternion q, math.RotationOrder order = math.RotationOrder.Default)
            {
                const float epsilon = 1e-6f;

                //prepare the data
                var qv = q.value;
                var d1 = qv * qv.wwww * new float4(2.0f); //xw, yw, zw, ww
                var d2 = qv * qv.yzxw * new float4(2.0f); //xy, yz, zx, ww
                var d3 = qv * qv;
                var euler = new float3(0.0f);

                const float CUTOFF = (1.0f - 2.0f * epsilon) * (1.0f - 2.0f * epsilon);

                switch (order)
                {
                    case math.RotationOrder.ZYX:
                        {
                            var y1 = d2.z + d1.y;
                            if (y1 * y1 < CUTOFF)
                            {
                                var x1 = -d2.x + d1.z;
                                var x2 = d3.x + d3.w - d3.y - d3.z;
                                var z1 = -d2.y + d1.x;
                                var z2 = d3.z + d3.w - d3.y - d3.x;
                                euler = new float3(math.atan2(x1, x2), math.asin(y1), math.atan2(z1, z2));
                            }
                            else //zxz
                            {
                                y1 = math.clamp(y1, -1.0f, 1.0f);
                                var abcd = new float4(d2.z, d1.y, d2.y, d1.x);
                                var x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                                var x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                                euler = new float3(math.atan2(x1, x2), math.asin(y1), 0.0f);
                            }

                            break;
                        }

                    case math.RotationOrder.ZXY:
                        {
                            var y1 = d2.y - d1.x;
                            if (y1 * y1 < CUTOFF)
                            {
                                var x1 = d2.x + d1.z;
                                var x2 = d3.y + d3.w - d3.x - d3.z;
                                var z1 = d2.z + d1.y;
                                var z2 = d3.z + d3.w - d3.x - d3.y;
                                euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
                            }
                            else //zxz
                            {
                                y1 = math.clamp(y1, -1.0f, 1.0f);
                                var abcd = new float4(d2.z, d1.y, d2.y, d1.x);
                                var x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                                var x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                                euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
                            }

                            break;
                        }

                    case math.RotationOrder.YXZ:
                        {
                            var y1 = d2.y + d1.x;
                            if (y1 * y1 < CUTOFF)
                            {
                                var x1 = -d2.z + d1.y;
                                var x2 = d3.z + d3.w - d3.x - d3.y;
                                var z1 = -d2.x + d1.z;
                                var z2 = d3.y + d3.w - d3.z - d3.x;
                                euler = new float3(math.atan2(x1, x2), math.asin(y1), math.atan2(z1, z2));
                            }
                            else //yzy
                            {
                                y1 = math.clamp(y1, -1.0f, 1.0f);
                                var abcd = new float4(d2.x, d1.z, d2.y, d1.x);
                                var x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                                var x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                                euler = new float3(math.atan2(x1, x2), math.asin(y1), 0.0f);
                            }

                            break;
                        }

                    case math.RotationOrder.YZX:
                        {
                            var y1 = d2.x - d1.z;
                            if (y1 * y1 < CUTOFF)
                            {
                                var x1 = d2.z + d1.y;
                                var x2 = d3.x + d3.w - d3.z - d3.y;
                                var z1 = d2.y + d1.x;
                                var z2 = d3.y + d3.w - d3.x - d3.z;
                                euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
                            }
                            else //yxy
                            {
                                y1 = math.clamp(y1, -1.0f, 1.0f);
                                var abcd = new float4(d2.x, d1.z, d2.y, d1.x);
                                var x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                                var x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                                euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
                            }

                            break;
                        }

                    case math.RotationOrder.XZY:
                        {
                            var y1 = d2.x + d1.z;
                            if (y1 * y1 < CUTOFF)
                            {
                                var x1 = -d2.y + d1.x;
                                var x2 = d3.y + d3.w - d3.z - d3.x;
                                var z1 = -d2.z + d1.y;
                                var z2 = d3.x + d3.w - d3.y - d3.z;
                                euler = new float3(math.atan2(x1, x2), math.asin(y1), math.atan2(z1, z2));
                            }
                            else //xyx
                            {
                                y1 = math.clamp(y1, -1.0f, 1.0f);
                                var abcd = new float4(d2.x, d1.z, d2.z, d1.y);
                                var x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                                var x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                                euler = new float3(math.atan2(x1, x2), math.asin(y1), 0.0f);
                            }

                            break;
                        }

                    case math.RotationOrder.XYZ:
                        {
                            var y1 = d2.z - d1.y;
                            if (y1 * y1 < CUTOFF)
                            {
                                var x1 = d2.y + d1.x;
                                var x2 = d3.z + d3.w - d3.y - d3.x;
                                var z1 = d2.x + d1.z;
                                var z2 = d3.x + d3.w - d3.y - d3.z;
                                euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
                            }
                            else //xzx
                            {
                                y1 = math.clamp(y1, -1.0f, 1.0f);
                                var abcd = new float4(d2.z, d1.y, d2.x, d1.z);
                                var x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                                var x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                                euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
                            }

                            break;
                        }
                }

                return eulerReorderBack(euler, order);
            }

            static float3 eulerReorderBack(float3 euler, math.RotationOrder order)
            {
                switch (order)
                {
                    case math.RotationOrder.XZY:
                        return euler.xzy;
                    case math.RotationOrder.YZX:
                        return euler.zxy;
                    case math.RotationOrder.YXZ:
                        return euler.yxz;
                    case math.RotationOrder.ZXY:
                        return euler.yzx;
                    case math.RotationOrder.ZYX:
                        return euler.zyx;
                    case math.RotationOrder.XYZ:
                    default:
                        return euler;
                }
            }
        }
    }
}

