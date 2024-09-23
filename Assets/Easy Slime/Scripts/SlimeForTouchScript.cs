using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Jobs;
using UnityEngine.EventSystems;

namespace EasySlimeTouchScript
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(BoxCollider))]
	public class SlimeForTouchScript : MonoBehaviour/*, IPointerDownHandler, IPointerUpHandler, IDragHandler*/
	{
		[SerializeField, Tooltip("The camera that will be used for input and rendering of the slime.")]
		Camera m_Camera = null;

		[SerializeField, Tooltip("Master size scaling of the effect. Scaling is applied to certain settings so that the effect can be scaled all at once.")]
		float m_Scaling = 1.0f;

		[Header("Mesh Generation")]
		[SerializeField, Tooltip("Whether or not the plane mesh will be generated and regenerated if screen size or scaling is changed.")]
		bool m_AutoGenerateMesh = true;
		[SerializeField, Tooltip("The distance between the vertices generated for the plane (Affected by Scaling).")]
		float m_VertexDistance = 0.19f;
		[SerializeField, Tooltip("The border size that goes outside of the screen (Affected by Scaling).")]
		float m_Border = 0.4f;

		[Header("Slime Settings")]
		[SerializeField, Tooltip("The speed at which pressing inwards goes from the min to max radius.")]
		float m_PressForce = 3.0f;

		[SerializeField, Tooltip("The maximum depth the slime can be pressed inwards (Affected by Scaling).")]
		float m_PressDepth = 1.9f;

		[SerializeField, Tooltip("The minimum radius of the press (Affected by Scaling).")]
		float m_PressRadiusMin = 0.66f;

		[SerializeField, Tooltip("The maximum radius of the press (Affected by Scaling).")]
		float m_PressRadiusMax = 0.99f;

		[SerializeField, Tooltip("The amount of drag movement of the slime in relation to the amount the input touch/cursor moved.")]
		float m_DragForce = 1.0f;

		[SerializeField, Tooltip("The distance from the input touch/cursor that vertices are affected by dragging (Affected by Scaling).")]
		float m_DragRadius = 2.46f;

		[SerializeField, Tooltip("The maximum distance a vertex can be displaced from its original location. Used to stop the mesh becoming too deformed, which would cause rendering artifacts (Affected by Scaling).")]
		float m_DragElasticityDistance = 1.89f;

		[SerializeField, Tooltip("The speed at which vertex positions are updated towards their target positions.")]
		float m_UpdateSpeed = 10.0f;

		[SerializeField, Tooltip("Whether or not the slime should return to its original shape over time.")]
		bool m_RegenerateOverTime = true;

		[SerializeField, Tooltip("The speed at which the slime returns to its original shape.")]
		float m_RegenerateSpeed = 0.3f;

		[Header("Audio")]
		[SerializeField, Tooltip("Audio that is played when input is pressed down.")]
		AudioSource m_SFXOnTouchDown = null;

		[SerializeField, Tooltip("Audio that is played while input is dragging across the slime.")]
		AudioSource m_SFXContinuous = null;

		Mesh m_Mesh = null;
		int m_xSize, m_ySize;
		float m_PreviousScaling = 1.0f;
		Dictionary<int, List<int>> m_VertexToTriangles = new Dictionary<int, List<int>>();

		NativeArray<VertexData> m_Vertices;
		NativeArray<float3> m_TargetPositions;
		NativeArray<float3> m_OriginalPositions;
		NativeArray<float> m_Elasticities;
		NativeArray<float> m_Locks;
		NativeArray<float3> m_TriangleNormals;
		NativeArray<int> m_VertexTriangles;
		NativeArray<int> m_Triangles;

		NativeList<PointerData> m_Pointers = default;

		public Camera Camera { get => m_Camera; set => m_Camera = value; }
		public float Scaling { get => m_Scaling; set => m_Scaling = value; }
		public bool AutoGenerateMesh { get => m_AutoGenerateMesh; set => m_AutoGenerateMesh = value; }
		public float VertexDistance { get => m_VertexDistance; set => m_VertexDistance = value; }
		public float Border { get => m_Border; set => m_Border = value; }
		public float PressForce { get => m_PressForce; set => m_PressForce = value; }
		public float PressDepth { get => m_PressDepth; set => m_PressDepth = value; }
		public float PressRadiusMin { get => m_PressRadiusMin; set => m_PressRadiusMin = value; }
		public float PressRadiusMax { get => m_PressRadiusMax; set => m_PressRadiusMax = value; }
		public float DragForce { get => m_DragForce; set => m_DragForce = value; }
		public float DragRadius { get => m_DragRadius; set => m_DragRadius = value; }
		public float DragElasticityDistance { get => m_DragElasticityDistance; set => m_DragElasticityDistance = value; }
		public float UpdateSpeed { get => m_UpdateSpeed; set => m_UpdateSpeed = value; }
		public bool RegenerateOverTime { get => m_RegenerateOverTime; set => m_RegenerateOverTime = value; }
		public float RegenerateSpeed { get => m_RegenerateSpeed; set => m_RegenerateSpeed = value; }
		public AudioSource SFXOnTouchDown { get => m_SFXOnTouchDown; set => m_SFXOnTouchDown = value; }
		public AudioSource SFXContinuous { get => m_SFXContinuous; set => m_SFXContinuous = value; }

		public NativeArray<VertexData> Vertices => m_Vertices;
		public NativeArray<float3> TargetPositions => m_TargetPositions;
		public NativeArray<float3> OriginalPositions => m_OriginalPositions;
		public NativeArray<float> Elasticities => m_Elasticities;
		public NativeArray<float> Locks => m_Locks;
		public NativeArray<float3> TriangleNormals => m_TriangleNormals;
		public NativeArray<int> VertexTriangles => m_VertexTriangles;
		public NativeArray<int> Triangles => m_Triangles;
		public NativeList<PointerData> Pointers => m_Pointers;

		public struct PointerData
		{
			public int pointerId;
			public Vector2 position;
			public Vector2 previousPosition;
			public Vector2 delta;
		}

		// Must match the vertex layout below
		[System.SerializableAttribute, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct VertexData
		{
			public float3 position;
			public float3 normal;
			public float4 uv0;
		}

		// Must match the vertex struct layout above
		VertexAttributeDescriptor[] m_VertexLayout = new VertexAttributeDescriptor[]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4)
		};

		public void ToggleRegenerateOverTime()
		{
			m_RegenerateOverTime = !m_RegenerateOverTime;
		}

		public void RebuildMesh(bool force = true)
		{
			float height;
			float width;

			if (m_Camera.orthographic)
			{
				height = 2 * m_Camera.orthographicSize;
				width = height * m_Camera.aspect;
			}
			else
			{
				Vector3[] frustumCorners = new Vector3[4];
				m_Camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), m_Camera.transform.InverseTransformPoint(transform.position).z, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
				height = math.abs(frustumCorners[0].y - frustumCorners[2].y);
				width = math.abs(frustumCorners[0].x - frustumCorners[2].x);
			}

			if (m_Scaling <= 0.0f)
			{
				Debug.LogError("Scaling must be greater than zero");
				return;
			}

			float border = m_Border * m_Scaling;

			if (m_VertexDistance <= 0.0f)
			{
				Debug.LogError("VertexDistance must be greater than zero");
				return;
			}

			float vertexDistance = m_VertexDistance * m_Scaling;

			int requiredXSize = Mathf.RoundToInt((width + border * 2f) / vertexDistance);
			int requiredYSize = Mathf.RoundToInt(((height + border * 2f) / (Mathf.Sqrt(3f) / 2f)) / vertexDistance);

			int maxSize = 1000;

			if (requiredXSize > maxSize || requiredYSize > maxSize)
			{
				Debug.LogError("Requested mesh size too large");
				return;
			}

			if (!force && requiredXSize == m_xSize && requiredYSize == m_ySize && m_PreviousScaling == m_Scaling)
			{
				return;
			}

			m_PreviousScaling = m_Scaling;

			Vector4 localBounds = new Vector4(-width * 0.5f, -height * 0.5f, width * 0.5f, height * 0.5f) / m_Scaling;// ; new Vector4(-width * 0.5f - border, -height * 0.5f - border, xSize * m_Resolution - width * 0.5f - border, ySize * m_Resolution - height * 0.5f - border);
			Shader.SetGlobalVector("_Slime_Bounds", localBounds);

			m_xSize = requiredXSize;
			m_ySize = requiredYSize;

			// Resize collider for input raycast handling
			GetComponent<BoxCollider>().size = new Vector3(width, height, 0.1f);

			int vertexCount = (m_xSize + 1) * (m_ySize + 1);
			int triangleCount = m_xSize * m_ySize * 6;

			ResizeMeshData(vertexCount, triangleCount);

			// Generate vertices, triangles and uvs for the fullscreen plane mesh
			// Plane is made of equilateral triangles (rather than right angled) so deformation results in nicer geometry for rendering

			// Vertex position and UV
			for (int i = 0, y = 0; y <= m_ySize; y++)
			{
				for (int x = 0; x <= m_xSize; x++, i++)
				{
					Vector3 position = new Vector3(x * vertexDistance - width * 0.5f - border + (y % 2 == 0 ? vertexDistance * 0.5f : 0.0f), (y * vertexDistance) * (Mathf.Sqrt(3f) / 2f) - border - height * 0.5f);

					float u = position.x / m_Scaling;
					float v = position.y / m_Scaling;

					m_OriginalPositions[i] = position;
					m_TargetPositions[i] = position;
					m_Elasticities[i] = 1.0f;

					m_Locks[i] = (x == 0 || x == m_xSize || y == 0 || y == m_ySize) ? 0.0f : 1.0f;

					var vertexData = m_Vertices[i];
					vertexData.position = position;
					vertexData.uv0 = new float4(u, v, 0, 0);
					vertexData.normal = new float3(0, 0, 1);
					m_Vertices[i] = vertexData;
				}
			}

			// Triangle indices
			for (int ti = 0, vi = 0, y = 0; y < m_ySize; y++, vi++)
			{
				for (int x = 0; x < m_xSize; x++, ti += 6, vi++)
				{
					if (y % 2 == 0)
					{
						m_Triangles[ti] = vi;
						m_Triangles[ti + 1] = vi + m_xSize + 2;
						m_Triangles[ti + 2] = vi + 1;

						m_Triangles[ti + 3] = vi;
						m_Triangles[ti + 4] = vi + m_xSize + 1;
						m_Triangles[ti + 5] = vi + m_xSize + 2;
					}
					else
					{
						m_Triangles[ti] = vi;
						m_Triangles[ti + 1] = vi + m_xSize + 1;
						m_Triangles[ti + 2] = vi + 1;

						m_Triangles[ti + 3] = vi + 1;
						m_Triangles[ti + 4] = vi + m_xSize + 1;
						m_Triangles[ti + 5] = vi + m_xSize + 2;
					}
				}
			}

			UpdateMeshDataAndAccelerationStructure();
		}

		public void ResizeMeshData(int vertexCount, int triangleCount)
		{
			CleanUpInternalMeshData();

			m_TargetPositions = new NativeArray<float3>(vertexCount, Allocator.Persistent);
			m_OriginalPositions = new NativeArray<float3>(vertexCount, Allocator.Persistent);
			m_Elasticities = new NativeArray<float>(vertexCount, Allocator.Persistent);
			m_Locks = new NativeArray<float>(vertexCount, Allocator.Persistent);
			m_Vertices = new NativeArray<VertexData>(vertexCount, Allocator.Persistent);
			m_Triangles = new NativeArray<int>(triangleCount, Allocator.Persistent);

			// Data for assisting with normal calculations
			m_TriangleNormals = new NativeArray<float3>(m_Triangles.Length / 3, Allocator.Persistent);

			// Mapping from vertex to which triangles it's connected to (each vertex is connected to max 6 triangles)
			m_VertexTriangles = new NativeArray<int>(vertexCount * 6, Allocator.Persistent);
		}

		public void UpdateMeshDataAndAccelerationStructure()
		{
			// Rebuild acceleration structure
			m_VertexToTriangles.Clear();

			for (int i = 0; i < m_Triangles.Length; i++)
			{
				int vertexIndex = m_Triangles[i];
				int triangleIndex = Mathf.FloorToInt(i / 3);

				if (!m_VertexToTriangles.ContainsKey(vertexIndex))
				{
					m_VertexToTriangles.Add(vertexIndex, new List<int>());
				}

				m_VertexToTriangles[vertexIndex].Add(triangleIndex);
			}

			int vertexTrianglecount = 0;
			for (int i = 0; i < m_Vertices.Length; i++)
			{
				var trianglesForThisVertex = m_VertexToTriangles[i];
				for (int k = 0; k < 6; k++)
				{
					m_VertexTriangles[vertexTrianglecount] = trianglesForThisVertex[k % trianglesForThisVertex.Count];
					vertexTrianglecount++;
				}
			}

			// Assign vertices, uvs and triangles to internal mesh
			m_Mesh.SetVertexBufferParams(m_Vertices.Length, m_VertexLayout);
			m_Mesh.SetVertexBufferData(m_Vertices, 0, 0, m_Vertices.Length);
			m_Mesh.SetIndexBufferParams(m_Triangles.Length, IndexFormat.UInt32);
			m_Mesh.SetIndexBufferData(m_Triangles, 0, 0, m_Triangles.Length);
			m_Mesh.SetSubMesh(0, new SubMeshDescriptor(0, m_Triangles.Length, MeshTopology.Triangles));
			m_Mesh.RecalculateBounds();
		}

		void DisposeIfCreated<T>(ref NativeArray<T> array) where T : struct
		{
			if (array.IsCreated)
			{
				array.Dispose();
			}
		}

		void CleanUpInternalMeshData()
		{
			DisposeIfCreated(ref m_Vertices);
			DisposeIfCreated(ref m_TriangleNormals);
			DisposeIfCreated(ref m_VertexTriangles);
			DisposeIfCreated(ref m_Triangles);
			DisposeIfCreated(ref m_TargetPositions);
			DisposeIfCreated(ref m_OriginalPositions);
			DisposeIfCreated(ref m_Elasticities);
			DisposeIfCreated(ref m_Locks);
		}

		private void OnEnable()
		{
			m_Pointers = new NativeList<PointerData>(20, Allocator.Persistent);

			m_Mesh = new Mesh();
			m_Mesh.name = "EasySlime Mesh";
			m_Mesh.MarkDynamic();

			ResizeMeshData(0, 0);

			GetComponent<MeshFilter>().sharedMesh = m_Mesh;

			if (m_AutoGenerateMesh)
				RebuildMesh();

			if (m_SFXContinuous != null)
			{
				m_SFXContinuous.loop = true;
				m_SFXContinuous.volume = 0.0f;
				m_SFXContinuous.Play();
				m_SFXContinuous.Pause();
			}
		}

		private void OnDisable()
		{
			m_Pointers.Dispose();

			CleanUpInternalMeshData();

			Destroy(m_Mesh);
		}

		void Update()
		{
			// Update pointer deltas
			for (int i = 0; i < m_Pointers.Length; i++)
			{
				PointerData data = m_Pointers[i];
				data.delta = data.position - data.previousPosition;
				data.previousPosition = data.position;
				m_Pointers[i] = data;
			}

			HandleSFXContinuous();

			if (m_AutoGenerateMesh)
				RebuildMesh(false);

			bool anyInput = m_Pointers.Length > 0;

			JobHandle jobHandle = default;

			if (anyInput)
			{
				// Interact with the slime
				InputModifyMeshDataJob job;
				job.targetPositions = m_TargetPositions;
				job.originalPositions = m_OriginalPositions;
				job.elasticities = m_Elasticities;
				job.locks = m_Locks;
				job.dragRadius = m_DragRadius * m_Scaling;
				job.pressRadiusMin = m_PressRadiusMin * m_Scaling;
				job.pressRadiusMax = m_PressRadiusMax * m_Scaling;
				job.dragForce = m_DragForce;
				job.dragElasticityDistance = m_DragElasticityDistance * m_Scaling;
				job.pressDepth = m_PressDepth * m_Scaling;
				job.pressForce = m_PressForce;
				job.deltaTime = Time.deltaTime;
				job.inputDataList = m_Pointers;
				jobHandle = job.Schedule(m_Vertices.Length, 100);
			}


			// Reform slime back to original shape and move vertices towards target vertex positions
			{
				ProcessMeshDataJob job;
				job.deltaTime = Time.deltaTime;
				job.targetPositions = m_TargetPositions;
				job.vertices = m_Vertices;
				job.elasticities = m_Elasticities;
				job.originalPositions = m_OriginalPositions;
				job.dragElasticityDistance = m_DragElasticityDistance * m_Scaling;
				job.regenSpeed = m_RegenerateOverTime ? m_RegenerateSpeed : 0.0f;
				job.updateSpeed = m_UpdateSpeed;
				job.pressDepth = m_PressDepth * m_Scaling;
				jobHandle = job.Schedule(m_Vertices.Length, 100, jobHandle);
			}

			// Recalculate mesh normals first per triangle then per vertex
			{
				TriangleNormalsJob normalsJob;
				normalsJob.vertices = m_Vertices;
				normalsJob.triangleNormals = m_TriangleNormals;
				normalsJob.triangles = m_Triangles;
				jobHandle = normalsJob.Schedule(m_TriangleNormals.Length, 100, jobHandle);
			}

			{
				VertexNormalsJob normalsJob;
				normalsJob.vertices = m_Vertices;
				normalsJob.triangleNormals = m_TriangleNormals;
				normalsJob.vertexTriangles = m_VertexTriangles;
				jobHandle = normalsJob.Schedule(m_Vertices.Length, 100, jobHandle);
			}

			jobHandle.Complete();

			// Update mesh data with new modified mesh vertices
			var meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;
			m_Mesh.SetVertexBufferData(m_Vertices, 0, 0, m_Vertices.Length, 0, meshFlags);
		}

		void HandleSFXContinuous()
		{
			// SFX continuous plays if there is a moving input pointer
			if (m_SFXContinuous != null)
			{
				float volumeTarget = 0.0f;

				for (int i = 0; i < m_Pointers.Length; i++)
				{
					PointerData pointer = m_Pointers[i];

					if (pointer.delta.magnitude > 0.0f)
					{
						volumeTarget = 1.0f;
						break;
					}
				}

				m_SFXContinuous.volume = Mathf.Lerp(m_SFXContinuous.volume, volumeTarget, Time.deltaTime * 10.0f);

				if (m_SFXContinuous.volume > 0.0f && !m_SFXContinuous.isPlaying)
				{
					m_SFXContinuous.UnPause();
				}

				if (m_SFXContinuous.volume == 0.0f && m_SFXContinuous.isPlaying)
				{
					m_SFXContinuous.Pause();
				}
			}
		}

		#region MeshManipulationJobs
		[BurstCompile]
		struct InputModifyMeshDataJob : IJobParallelFor
		{
			public NativeArray<float3> targetPositions;
			public NativeArray<float> elasticities;

			[ReadOnly, NativeDisableParallelForRestriction] public NativeList<PointerData> inputDataList;
			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float3> originalPositions;
			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float> locks;

			[ReadOnly] public float deltaTime;

			[ReadOnly] public float pressForce;
			[ReadOnly] public float pressDepth;
			[ReadOnly] public float pressRadiusMin;
			[ReadOnly] public float pressRadiusMax;

			[ReadOnly] public float dragForce;
			[ReadOnly] public float dragRadius;
			[ReadOnly] public float dragElasticityDistance;

			public void Execute(int i)
			{
				var targetPos = targetPositions[i];
				var originalPos = originalPositions[i];
				var elasticity = elasticities[i];
				var lockValue = locks[i];

				for (int k = 0; k < inputDataList.Length; k++)
				{
					var touchInfo = inputDataList[k];

					float2 touchPos = touchInfo.position;
					float distance = math.distance(targetPos.xy, touchPos);

					// Dragging
					if (distance <= dragRadius)
					{
						float targetOffset = EaseOut10(distance / dragRadius) * dragForce;
						float2 offset = touchInfo.delta * elasticity * targetOffset * lockValue;

						elasticity -= math.length(offset) / dragElasticityDistance;

						if (elasticity < 0.0f)
							elasticity = 0.0f;

						targetPos.x += offset.x;
						targetPos.y += offset.y;
					}

					// Pressing
					if (distance <= pressRadiusMax)
					{
						float distanceMin01 = math.saturate(distance / pressRadiusMin);
						float distanceMax01 = math.saturate(distance / pressRadiusMax);

						float diveCurveMin = SmootherStep10(distanceMin01) * lockValue;
						float diveCurveMax = SmootherStep10(distanceMax01) * lockValue;

						float targetMinDepth = originalPos.z + diveCurveMin * (pressDepth * (pressRadiusMin / pressRadiusMax));
						float targetMaxDepth = originalPos.z + diveCurveMax * pressDepth;

						targetPos.z = math.max(targetPos.z, targetMinDepth);

						if (targetMaxDepth > targetPos.z)
							targetPos.z = math.lerp(targetPos.z, targetMaxDepth, deltaTime * pressForce);
					}
				}

				targetPositions[i] = targetPos;
				elasticities[i] = elasticity;
			}

			float SmootherStep10(float t)
			{
				return math.lerp(1.0f, 0.0f, t * t * t * (t * (6f * t - 15f) + 10f));
			}

			float EaseOut10(float t)
			{
				return math.lerp(1.0f, 0.0f, math.sin(t * math.PI * 0.5f));
			}
		}


		// Reform the slime over time back to its original shape
		// Move displaced vertices towards target vertices
		[BurstCompile]
		struct ProcessMeshDataJob : IJobParallelFor
		{
			public NativeArray<VertexData> vertices;
			public NativeArray<float3> targetPositions;
			public NativeArray<float> elasticities;

			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float3> originalPositions;
			[ReadOnly] public float deltaTime;
			[ReadOnly] public float regenSpeed;
			[ReadOnly] public float dragElasticityDistance;
			[ReadOnly] public float updateSpeed;
			[ReadOnly] public float pressDepth;

			public void Execute(int i)
			{
				var vertexData = vertices[i];
				var targetPos = targetPositions[i];
				var originalPos = originalPositions[i];
				var elasticity = elasticities[i];

				// Move target positions slowly back to original positions
				targetPos = math.lerp(targetPos, originalPos, deltaTime * regenSpeed);

				elasticity += deltaTime * regenSpeed;
				if (elasticity > 1.0f)
					elasticity = 1.0f;

				// Move vertices towards target positions
				vertexData.position = math.lerp(vertexData.position, targetPos, deltaTime * updateSpeed);

				// Set UV0.z to the amount that vertex is pressed in, between 0 and 1
				vertexData.uv0.z = math.saturate(math.abs(originalPos.z - vertexData.position.z) / pressDepth);

				vertices[i] = vertexData;
				targetPositions[i] = targetPos;
				elasticities[i] = elasticity;
			}
		}
		#endregion

		#region NormalCalculationJobs
		[BurstCompile]
		struct TriangleNormalsJob : IJobParallelFor
		{
			public NativeArray<float3> triangleNormals;

			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<int> triangles;
			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<VertexData> vertices;

			public void Execute(int i)
			{
				// Goes through in parallel and calculates the normal for every triangle
				int normalTriangleIndex = i * 3;
				int vIndexA = triangles[normalTriangleIndex];
				int vIndexB = triangles[normalTriangleIndex + 1];
				int vIndexC = triangles[normalTriangleIndex + 2];

				triangleNormals[i] = SurfaceNormalFromIndices(vIndexA, vIndexB, vIndexC, ref vertices);
			}

			float3 SurfaceNormalFromIndices(int iA, int iB, int iC, ref NativeArray<VertexData> vertices)
			{
				float3 pointA = vertices[iA].position;
				float3 pointB = vertices[iB].position;
				float3 pointC = vertices[iC].position;

				float3 sideAB = pointB - pointA;
				float3 sideAC = pointC - pointA;

				return math.normalizesafe(math.cross(sideAB, sideAC));
			}
		}

		[BurstCompile]
		struct VertexNormalsJob : IJobParallelFor
		{
			public NativeArray<VertexData> vertices;

			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float3> triangleNormals;
			[ReadOnly, NativeDisableParallelForRestriction] public NativeArray<int> vertexTriangles;

			// Every vertex should have max 6 triangles connected to it
			public void Execute(int i)
			{
				// Goes through every vertex in parallel and looks at connected triangles to calculate its normal

				var normal = float3.zero;

				// Loop through every triangle associated with this vertex
				int startIndex = i * 6;
				for (int k = 0; k < 6; k++)
				{
					int triangleIndex = vertexTriangles[startIndex + k];
					var triangleNormal = triangleNormals[triangleIndex];
					normal += triangleNormal;
				}

				normal = math.normalizesafe(normal);

				var vertexData = vertices[i];
				vertexData.normal = normal;
				vertices[i] = vertexData;
			}
		}
		#endregion

		#region InputHandling
		public void OnDrag(PointerData eventData)
		{
			Vector3 screenPos = eventData.position;
			screenPos.z = m_Camera.transform.InverseTransformPoint(transform.position).z;
			Vector3 pointWorld = m_Camera.ScreenToWorldPoint(screenPos);
			Vector2 pointLocal = transform.InverseTransformPoint(pointWorld);

			for (int i = 0; i < m_Pointers.Length; i++)
			{
				PointerData data = m_Pointers[i];
				if (data.pointerId == eventData.pointerId)
				{
					data.previousPosition = data.position;
					data.position = pointLocal;
					m_Pointers[i] = data;
					break;
				}
			}
		}

		//// by Roman
		//public void OnDrag(PointerData eventData)
		//{
		//	PointerData data;

		//	Vector3 screenPos = eventData.position;
		//	screenPos.z = m_Camera.transform.InverseTransformPoint(transform.position).z;
		//	Vector3 pointWorld = m_Camera.ScreenToWorldPoint(screenPos);
		//	Vector2 pointLocal = transform.InverseTransformPoint(pointWorld);

		//	if (m_Pointers.Length == 0)
		//	{
		//		data.position = pointLocal;
		//		data.previousPosition = pointLocal;
		//		data.delta = Vector2.zero;
		//		data.pointerId = eventData.pointerId;

		//		m_Pointers.Add(data);
		//	}

		//	for (int i = 0; i < m_Pointers.Length; i++)
		//	{
		//		data = m_Pointers[i];
		//		if (data.pointerId == eventData.pointerId)
		//		{
		//			data.previousPosition = data.position;
		//			data.position = pointLocal;
		//			m_Pointers[i] = data;
		//			break;
		//		}
		//	}
		//}

		public void OnPointerDown(PointerData eventData)
		{
			PointerData data;

			Vector3 screenPos = eventData.position;
			screenPos.z = m_Camera.transform.InverseTransformPoint(transform.position).z;
			Vector3 pointWorld = m_Camera.ScreenToWorldPoint(screenPos);
			Vector2 pointLocal = transform.InverseTransformPoint(pointWorld);

			data.position = pointLocal;
			data.previousPosition = pointLocal;
			data.delta = Vector2.zero;
			data.pointerId = eventData.pointerId;

			for (int i = 0; i < m_Pointers.Length; i++)
			{
				if (m_Pointers[i].pointerId == eventData.pointerId)
				{
					m_Pointers[i] = data;
					return;
				}
			}

			m_Pointers.Add(data);

			if (m_SFXOnTouchDown != null)
			{
				m_SFXOnTouchDown.Play();
			}
		}

		public void OnPointerUp(PointerData eventData)
		{
			for (int i = 0; i < m_Pointers.Length; i++)
			{
				if (m_Pointers[i].pointerId == eventData.pointerId)
				{
					m_Pointers.RemoveAt(i);
					break;
				}
			}
		}
		#endregion
	}
}