
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;
#endif

namespace Clayxels{

	/* This class is your main interface to work with Clayxels, it is designed to work in editor and in game.
		Each container nests one or more ClayObject as children in its hierarchy to generate the final clay result.
	*/
	[ExecuteInEditMode]
	public class ClayContainer : MonoBehaviour{

		// private class, containers use chunks to work with big voxel grids and save memory
		class ClayxelChunk{
			public ComputeBuffer pointCloudDataBuffer;
			public ComputeBuffer indirectDrawArgsBuffer;
			public ComputeBuffer pointCloudDataBuffer2;
			public ComputeBuffer indirectDrawArgsBuffer2;
			public ComputeBuffer computeIndirectDrawArgsBuffer;
			public ComputeBuffer computePointCloudDataBuffer;
			public ComputeBuffer renderIndirectDrawArgsBuffer;
			public ComputeBuffer renderPointCloudDataBuffer;
			public Vector3Int coords = new Vector3Int();
			public Vector3 center = new Vector3();
			public Material clayxelMaterial;
			public Material clayxelPickingMaterial;
			public MaterialPropertyBlock materialProperties;

			#if DRAW_DEBUG
				public ComputeBuffer debugGridOutPointsBuffer;
			#endif
		}

		/* Color of the bounds shown by each container */
		public static Color boundsColor = new Color(0.5f, 0.5f, 1.0f, 0.1f);

		/* Shortcut used to pick-select clayObjects in scene */
		public static string pickingKey = "p";
		
		/* CustomMaterial: specify a material that is not the default one. It will need a special shader as shown in the examples provided.*/
		public Material customMaterial = null;

		/* Store the generated mesh as an asset inside the project folders, used by ClayContainer.freezeToMesh().*/
		public string meshAssetPath = "";

		/* Detail value used when freezing to mesh. If the valye is -100, the clayxelDetail attribute value will be used instead.*/
		public int freezeMeshDetail = -100;
		
		/* Enable this when animating or moving clayObjects at runtime. 
			When a container moves, it inhibits any update of the nested ClayObjects. 
			This attribute makes sure that even if this container is moving, the ClayObjects will also be computed for each frame.*/
		public bool forceUpdate = false;

		// private use for a ClayObject to notify its parent container that something got updated
		public bool needsUpdate = true;

		public ClayContainer instanceOf = null;
		
		static ComputeBuffer solidsPosBuffer;
		static ComputeBuffer solidsRotBuffer;
		static ComputeBuffer solidsScaleBuffer;
		static ComputeBuffer solidsBlendBuffer;
		static ComputeBuffer solidsTypeBuffer;
		static ComputeBuffer solidsColorBuffer;
		static ComputeBuffer solidsAttrsBuffer;
		static ComputeBuffer solidsClayObjectIdBuffer;
		static ComputeBuffer solidsUpdatedBuffer;
		static ComputeBuffer solidsPerChunkBuffer;
		static ComputeBuffer meshIndicesBuffer = null;
		static ComputeBuffer meshVertsBuffer = null;
		static ComputeBuffer meshColorsBuffer = null;
		
		static List<Vector3> solidsPos;
		static List<Quaternion> solidsRot;
		static List<Vector3> solidsScale;
		static List<float> solidsBlend;
		static List<int> solidsType;
		static List<Vector3> solidsColor;
		static List<Vector4> solidsAttrs;
		static List<int> solidsClayObjectId;

		static ComputeShader claycoreCompute;
		static ComputeBuffer gridDataBuffer;
		static ComputeBuffer triangleConnectionTable;
		static ComputeBuffer prefilteredSolidIdsBuffer;
		static ComputeBuffer solidsFilterBuffer;
		static ComputeBuffer numSolidsPerChunkBuffer;

		static ComputeBuffer fieldCache1Buffer = null;
		static ComputeBuffer fieldCache2Buffer = null;
		
		static int maxSolids = 512;
		static int maxSolidsPerVoxel = 128;
		static int chunkMaxOutPoints = (256*256*256) / 8;
		static int inspectorUpdated;
		static int[] tmpChunkData;
		static public bool globalDataNeedsInit = true;
		static List<string> solidsCatalogueLabels = new List<string>();
		static List<List<string[]>> solidsCatalogueParameters = new List<List<string[]>>();
		static List<ComputeBuffer> globalCompBuffers = new List<ComputeBuffer>();
		static int lastUpdatedContainerId = -1;
		static int maxThreads = 8;
		static int[] solidsInSingleChunkArray;
		static int updateFrameSkip = 0;
		static string renderPipe = "";
		static RenderTexture pickingRenderTexture = null;
		static RenderTargetIdentifier pickingRenderTextureId;
		static CommandBuffer pickingCommandBuffer;
		static Texture2D pickingTextureResult;
		static Rect pickingRect;
		static float pickingMousePosX = -1;
		static float pickingMousePosY = -1;
		static int pickedClayObjectId = -1;
		static int pickedClayxelId = -1;
		static GameObject pickedObj = null;
		static bool pickingMode = false;
		static bool pickingShiftPressed = false;
		static float[] fieldCacheInitValues = new float[]{};
		static int maxChunkX = 3;
		static int maxChunkY = 3;
		static int maxChunkZ = 3;
		static int totalMaxChunks = 1;
		static int cacheEnabled = 0;
		static int chunksUpdatePerFrame = 64;
		static int[] dummyData = new int[]{0, 1, 0, 0};

		[SerializeField] int clayxelDetail = 88;
		[SerializeField] int chunksX = 1;
		[SerializeField] int chunksY = 1;
		[SerializeField] int chunksZ = 1;
		[SerializeField] Material material = null;
		[SerializeField] ShadowCastingMode castShadows = ShadowCastingMode.On;
		[SerializeField] bool receiveShadows = true;

		int chunkSize = 8;
		bool memoryOptimized = false;
		float globalSmoothing = 0.0f;
		Dictionary<int, int> solidsUpdatedDict = new Dictionary<int, int>();
		List<ClayxelChunk> chunks = new List<ClayxelChunk>();
		List<ComputeBuffer> compBuffers = new List<ComputeBuffer>();
		bool needsInit = true;
		bool invalidated = false;
		int[] countBufferArray = new int[1]{0};
		ComputeBuffer countBuffer;
		ComputeBuffer indirectChunkArgs1Buffer;
		ComputeBuffer indirectChunkArgs2Buffer;
		ComputeBuffer updateChunksBuffer;
		Vector3 boundsScale = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 boundsCenter = new Vector3(0.0f, 0.0f, 0.0f);
		Bounds renderBounds = new Bounds();
		Vector3[] vertices = new Vector3[1];
		int[] meshTopology = new int[1];
		bool solidsHierarchyNeedsScan = false;
		List<WeakReference> clayObjects = new List<WeakReference>();
		List<Solid> solids = new List<Solid>();
		int numChunks = 0;
		float deltaTime = 0.0f;
		bool meshCached = false;
		int numThreadsComputeStartRes;
		int numThreadsComputeFullRes;
		float splatRadius = 0.0f;
		int clayxelId = -1;
		int updateFrame = 0;
		float splatSize = 1.0f;
		int bufferSwapId = 0;
		int chunkIter = 0;

		enum Kernels{
			computeGrid,
			cacheDistField,
			clearCachedDistField,
			generatePointCloud,
			debugDisplayGridPoints,
			computeGridForMesh,
			computeMesh,
			filterSolidsPerChunk
		}
		
		/* Skip N frames before updating to reduce stress on GPU and increase FPS count. 
			See ClayxelPrefs.cs */
		public static void setUpdateFrameSkip(int frameSkip){
			ClayContainer.updateFrameSkip = frameSkip;
		}

		/* When using multiple chunks, specify how many to update for each frame to reduce stress on GPU and increase FPS count.
			Specify a high number like 64 to update all chunks in one frame.
			The number of chunks in a container is determined by the attributes chunksX * chunksY * chunksZ.
			See ClayxelPrefs.cs */
		public static void setChunksUpdatePerFrame(int num){
			ClayContainer.chunksUpdatePerFrame = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* How many soldis can this container work with in total.
			Valid values: 64, 128, 256, 512, 1024, 4096, 16384
			See ClayxelPrefs.cs */
		public static void setMaxSolids(int num){
			ClayContainer.maxSolids = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* How many chunks can this container work with in total.
			Keep these to a minimum when enabling CLAYXELS_CACHEON or video memory might run out.
			See ClayxelPrefs.cs */
		public static void setMaxChunks(int x, int y, int z){
			ClayContainer.maxChunkX = x;
			ClayContainer.maxChunkY = y;
			ClayContainer.maxChunkZ = z;
			ClayContainer.totalMaxChunks = x * y * z;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* How many solids can stay one next to another while occupying the same voxel.
			Keeping this value low will increase overall performance but will cause disappearing clayxels if the number is exceeded.
			Valid values: 32, 64, 128, 256, 512, 1024, 2048
			See ClayxelPrefs.cs */
		public static void setMaxSolidsPerVoxel(int num){
			ClayContainer.maxSolidsPerVoxel = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* Sets how finely detailed are your clayxels, range 0 to 100.*/
		public void setClayxelDetail(int value){
			if(value == this.clayxelDetail || this.meshCached || this.needsInit){
				return;
			}

			this.switchComputeData();

			if(value < 0){
				value = 0;
			}
			else if(value > 100){
				value = 100;
			}

			this.clayxelDetail = value;

			this.chunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);
			
			float voxelSize = (float)this.chunkSize / 256;

			this.splatRadius = (voxelSize * 0.5f) * 1.8f;
			this.splatSize = this.splatRadius * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			this.globalSmoothing = this.splatRadius * 2.0f;
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.localScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			float chunkOffset = this.chunkSize - voxelSize; // removes the seam between chunks

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.center = new Vector3(
					(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.x),
					(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.y),
					(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.z));
			}

			this.forceUpdateAllSolids();

			this.needsUpdate = true;
		}

		/* Get the value specified by setClayxelDetail()*/		
		public int getClayxelDetail(){
			return this.clayxelDetail;
		}

		/* Determines how much work area you have for your sculpt within this container.
			These values are not expressed in scene units, 
			the final size of this container is determined by the value specified with setClayxelDetail().
			Performance tip: The bigger the bounds, the slower this container will be to compute clay in-game.*/
		public void setBoundsScale(int x, int y, int z){
			this.chunksX = x;
			this.chunksY = y;
			this.chunksZ = z;
			this.limitChunkValues();

			this.needsInit = true;
		}

		/* Get the values specified by setBoundsScale()*/		
		public Vector3Int getBoundsScale(){
			return new Vector3Int(this.chunksX, this.chunksY, this.chunksZ);
		}

		/* How many solids can a container work with.*/
		public int getMaxSolids(){
			return ClayContainer.maxSolids;
		}

		/* How many solids are currently used in this container.*/
		public int getNumSolids(){
			return this.solids.Count;
		}

		/* How many ClayObjects currently in this container, each ClayObject will spawn a certain amount of Solids.*/
		public int getNumClayObjects(){
			return  this.clayObjects.Count;
		}

		/* Invoke this after adding a new ClayObject in scene to have the container notified instantly.*/
		public void scanClayObjectsHierarchy(){
			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();
			this.solids.Clear();

			this.scanRecursive(this.transform);

			this.solidsHierarchyNeedsScan = false;

			if(this.numChunks == 1){
				this.countBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.countBufferArray);
			}
		}

		/* Get and own the list of solids in this container. 
			Useful when you don't want a heavy hierarchy of ClayObject in scene (ex. working with particles). */
		public List<Solid> getSolids(){
			return this.solids;
		}

		/* If you work directly with the list of solids in this container, invoke this to notify when a solid has changed.*/
		public void solidUpdated(int id){
			if(id < ClayContainer.maxSolids){
				this.solidsUpdatedDict[id] = 1;

				this.needsUpdate = true;
			}
		}

		/* If you are manipulating the internal list of solids, use this after you add or remove solids in the list.*/
		public void updatedSolidCount(){
			if(this.numChunks == 1){
				this.countBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.countBufferArray);
			}
			
			for(int i = 0; i < this.solids.Count; ++i){
				Solid solid = this.solids[i];
				solid.id = i;
				
				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
				else{
					break;
				}
			}
		}

		/* Set a material with a clayxels-compatible shader or set it to null to return to the standard clayxels shader.*/
		public void setCustomMaterial(Material material){
			this.customMaterial = material;
			this.material = material;

			this.initMaterialProperties();
		}

		/* Automatically invoked once when the game starts, 
			you only need to invoke this yourself if you change what's declared in ClayXelsPrefs.cs at runtime.*/
		static public void initGlobalData(){
			if(!ClayContainer.globalDataNeedsInit){
				return;
			}

			ClayxelsPrefs.apply();

			string renderPipeAsset = "";
			if(GraphicsSettings.renderPipelineAsset != null){
				renderPipeAsset = GraphicsSettings.renderPipelineAsset.GetType().Name;
			}
			
			if(renderPipeAsset == "HDRenderPipelineAsset"){
				ClayContainer.renderPipe = "hdrp";
			}
			else if(renderPipeAsset == "UniversalRenderPipelineAsset"){
				ClayContainer.renderPipe = "urp";
			}
			else{
				ClayContainer.renderPipe = "builtin";
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					ClayContainer.setupScenePicking();
					ClayContainer.pickingMode = false;
					ClayContainer.pickedObj = null;
				}
			#endif

			ClayContainer.reloadSolidsCatalogue();

			ClayContainer.globalDataNeedsInit = false;

			ClayContainer.lastUpdatedContainerId = -1;

			ClayContainer.releaseGlobalBuffers();

			UnityEngine.Object clayCore = Resources.Load("clayCoreLock");
			if(clayCore == null){
				clayCore = Resources.Load("clayCore");
			}

			ClayContainer.claycoreCompute = (ComputeShader)Instantiate(clayCore);

			ClayContainer.gridDataBuffer = new ComputeBuffer(256 * 256 * 256, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.gridDataBuffer);

			ClayContainer.prefilteredSolidIdsBuffer = new ComputeBuffer((64 * 64 * 64) * ClayContainer.maxSolidsPerVoxel, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);

			int maxSolidsPerVoxelMask = ClayContainer.maxSolidsPerVoxel / 32;
			ClayContainer.solidsFilterBuffer = new ComputeBuffer((64 * 64 * 64) * maxSolidsPerVoxelMask, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsFilter", ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "solidsFilter", ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "solidsFilter", ClayContainer.solidsFilterBuffer);

			ClayContainer.claycoreCompute.SetInt("maxSolidsPerVoxel", maxSolidsPerVoxel);
			ClayContainer.claycoreCompute.SetInt("maxSolidsPerVoxelMask", maxSolidsPerVoxelMask);
			
			ClayContainer.triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.triangleConnectionTable);

			ClayContainer.triangleConnectionTable.SetData(MeshUtils.TriangleConnectionTable);
			
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "triangleConnectionTable", ClayContainer.triangleConnectionTable);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "triangleConnectionTable", ClayContainer.triangleConnectionTable);

			ClayContainer.claycoreCompute.SetInt("maxSolids", ClayContainer.maxSolids);

			ClayContainer.solidsPosBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsPosBuffer);
			ClayContainer.solidsRotBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsRotBuffer);
			ClayContainer.solidsScaleBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsScaleBuffer);
			ClayContainer.solidsBlendBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsBlendBuffer);
			ClayContainer.solidsTypeBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsTypeBuffer);
			ClayContainer.solidsColorBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsColorBuffer);
			ClayContainer.solidsAttrsBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsAttrsBuffer);
			ClayContainer.solidsClayObjectIdBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsClayObjectIdBuffer);

			ClayContainer.solidsPos = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			ClayContainer.solidsRot = new List<Quaternion>(new Quaternion[ClayContainer.maxSolids]);
			ClayContainer.solidsScale = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			ClayContainer.solidsBlend = new List<float>(new float[ClayContainer.maxSolids]);
			ClayContainer.solidsType = new List<int>(new int[ClayContainer.maxSolids]);
			ClayContainer.solidsColor = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			ClayContainer.solidsAttrs = new List<Vector4>(new Vector4[ClayContainer.maxSolids]);
			ClayContainer.solidsClayObjectId = new List<int>(new int[ClayContainer.maxSolids]);

			int numKernels = Enum.GetNames(typeof(Kernels)).Length;
			for(int i = 0; i < numKernels; ++i){
				ClayContainer.claycoreCompute.SetBuffer(i, "gridData", ClayContainer.gridDataBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsPos", ClayContainer.solidsPosBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsRot", ClayContainer.solidsRotBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsScale", ClayContainer.solidsScaleBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsBlend", ClayContainer.solidsBlendBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsType", ClayContainer.solidsTypeBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsColor", ClayContainer.solidsColorBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsAttrs", ClayContainer.solidsAttrsBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsClayObjectId", ClayContainer.solidsClayObjectIdBuffer);
			}

			ClayContainer.numSolidsPerChunkBuffer = new ComputeBuffer(64, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);

			ClayContainer.solidsUpdatedBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsUpdatedBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsUpdated", ClayContainer.solidsUpdatedBuffer);

			int maxChunks = 64;
			ClayContainer.solidsPerChunkBuffer = new ComputeBuffer(ClayContainer.maxSolids * maxChunks, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);

			ClayContainer.solidsInSingleChunkArray = new int[ClayContainer.maxSolids];
			for(int i = 0; i < ClayContainer.maxSolids; ++i){
				ClayContainer.solidsInSingleChunkArray[i] = i;
			}

			if(ClayContainer.chunksUpdatePerFrame < ClayContainer.totalMaxChunks){
				// if we spread the chunk update over multiple frames, we need to process all of them always
				// failing to do so will cause chunks displaying garbage leftover from previous frames
				ClayContainer.claycoreCompute.SetInt("alwaysProcessAllChunks", 1);
			}
			else{
				// if we update all chunks in one frame, then we can detect which chunks contain moving solids and only update those
				ClayContainer.claycoreCompute.SetInt("alwaysProcessAllChunks", 0);
			}

			ClayContainer.tmpChunkData = new int[ClayContainer.chunkMaxOutPoints * 4];

			if(ClayContainer.cacheEnabled == 1){
				ClayContainer.setupDistFieldCache();
			}

			ClayContainer.meshIndicesBuffer = null;
			ClayContainer.meshVertsBuffer = null;
			ClayContainer.meshColorsBuffer = null;

			#if DRAW_DEBUG
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.debugDisplayGridPoints, "gridData", ClayContainer.gridDataBuffer);
			#endif
		}

		/* Automatically invoked once when the game starts, 
			you only need to invoke this yourself if you change chunkSize or chunksX,Y,Z attributes.*/
		public void init(){
			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.reinstallEditorEvents();
				}
			#endif

			if(ClayContainer.globalDataNeedsInit){
				ClayContainer.initGlobalData();
			}

			this.needsInit = false;

			if(this.instanceOf != null){
				return;
			}

			this.memoryOptimized = false;
			
			if(this.gameObject.GetComponent<MeshFilter>() != null){
				this.meshCached = true;
				this.releaseBuffers();
				return;
			}
			
			this.chunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);

			this.limitChunkValues();

			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();

			this.releaseBuffers();

			this.numThreadsComputeStartRes = 64 / ClayContainer.maxThreads;
			this.numThreadsComputeFullRes = 256 / ClayContainer.maxThreads;

			this.splatRadius = (((float)this.chunkSize / 256) * 0.5f) * 1.8f;
			this.splatSize = this.splatRadius * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			this.initChunks();

			this.globalSmoothing = this.splatRadius * 2.0f;
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);
			
			this.countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
			this.compBuffers.Add(this.countBuffer);

			this.solidsHierarchyNeedsScan = true;
			this.needsUpdate = true;
			ClayContainer.lastUpdatedContainerId = -1;

			this.initMaterialProperties();

			this.scanClayObjectsHierarchy();
			this.computeClay();

			this.bufferSwapId = 0;
			this.chunkIter = 0;
			this.updateFrame = 0;

			if(this.clayObjects.Count > 0){
				this.optimizeMemory();
			}
		}

		/* Spawn a new ClayObject in scene under this container.*/
		public ClayObject addClayObject(){
			GameObject clayObj = new GameObject("clay_cube+");
			clayObj.transform.parent = this.transform;
			clayObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			ClayObject clayObjComp = clayObj.AddComponent<ClayObject>();
			clayObjComp.clayxelContainerRef = new WeakReference(this);
			clayObjComp.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			this.collectClayObject(clayObjComp);

			this.needsUpdate = true;

			return clayObjComp;
		}

		/* Get a ClayObject inside this container by id.*/
		public ClayObject getClayObject(int id){
			return (ClayObject)this.clayObjects[id].Target;
		}

		/* Scan for ClayObjects in this container at the next update.*/
		public void scheduleClayObjectsScan(){
			this.solidsHierarchyNeedsScan = true;
		}

		/* Invoke this when you need all solids in a container to be updated, ex. if you change the material attributes.*/
		public void forceUpdateAllSolids(){
			for(int i = 0; i < this.solids.Count; ++i){
				int id = this.solids[i].id;
				if(id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[id] = 1;
				}
				else{
					break;
				}
			}

			this.needsUpdate = true;
		}

		/* Notify this container that one of the nested ClayObject has changed.*/
		public void clayObjectUpdated(ClayObject clayObj){
			if(!this.transform.hasChanged || this.forceUpdate){
				for(int i = 0; i < clayObj.getNumSolids(); ++i){
					int id = clayObj.getSolid(i).id;
					if(id < ClayContainer.maxSolids){
						this.solidsUpdatedDict[id] = 1;
					}
				}

				this.needsUpdate = true;
			}
		}
		
		/* Get the material currently in use by this container. 
			Even if you are using a customMaterial, this is the real instanceOf of the material in scene.
			After changing the material, invoke updatedMaterialProperties() and then forceUpdateAllSolids().*/
		public Material getMaterial(){
			return this.material;
		}

		/* Each chunk in this container will be notified that something changed in the material.
			Also invoke forceUpdateAllSolids() if you have multiple chunks.*/
		public void updatedMaterialProperties(){
			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];

				for(int propertyId = 0; propertyId < this.material.shader.GetPropertyCount(); ++propertyId){
					ShaderPropertyType type = this.material.shader.GetPropertyType(propertyId);
					string name = this.material.shader.GetPropertyName(propertyId);

					if(type == ShaderPropertyType.Color || type == ShaderPropertyType.Vector){
						chunk.clayxelMaterial.SetVector(name, this.material.GetVector(name));
					}
					else if(type == ShaderPropertyType.Float || type == ShaderPropertyType.Range){
						chunk.clayxelMaterial.SetFloat(name, this.material.GetFloat(name));
					}
					else if(type == ShaderPropertyType.Texture){
						chunk.clayxelMaterial.SetTexture(name, this.material.GetTexture(name));
					}
				}
			}
		}

		/* Automatically invoked when the game starts.
			If this container will not receive any more editing for a while,
			then use this method to resize the memory used by this container to make it weight just as a frozen mesh.
			Memory will automatically be expanded again if you tweak one of the ClayObjects in this container.*/
		public void optimizeMemory(){
			if(this.memoryOptimized){
				return;
			}

			this.memoryOptimized = true;

			int[] indirectDrawBufferData = new int[4]{0, 0, 0, 0};

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.renderIndirectDrawArgsBuffer.GetData(indirectDrawBufferData);
				
				int pointCount = indirectDrawBufferData[0];
				
				chunk.renderPointCloudDataBuffer.GetData(ClayContainer.tmpChunkData, 0, 0, pointCount *4);
				
				chunk.pointCloudDataBuffer.Release();

				this.compBuffers.Remove(chunk.pointCloudDataBuffer);
				
				if(chunk.pointCloudDataBuffer2 != null){
					chunk.pointCloudDataBuffer2.Release();
					this.compBuffers.Remove(chunk.pointCloudDataBuffer2);
				}

				chunk.pointCloudDataBuffer = null;
				chunk.pointCloudDataBuffer2 = null;
				
				if(pointCount == 0){
					pointCount = 1;
				}

				chunk.pointCloudDataBuffer = new ComputeBuffer(pointCount, sizeof(int) * 4);
				chunk.renderPointCloudDataBuffer = chunk.pointCloudDataBuffer;
				this.compBuffers.Add(chunk.pointCloudDataBuffer);

				chunk.renderPointCloudDataBuffer.SetData(ClayContainer.tmpChunkData, 0, 0, pointCount * 4);

				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);
			}

			this.bufferSwapId = 0;
			this.chunkIter = 0;
			this.updateFrame = 0;
		}

		/* Force this container to compute the final clay result now.
			Useful if you have set frame skips or limited the chunks to update per frame.*/
		public void computeClay(){
			if(this.memoryOptimized){
				this.expandMemory();
			}
			
			if(this.solidsHierarchyNeedsScan){
				this.scanClayObjectsHierarchy();
			}
			
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}

			this.updateSolids();
			
			if(this.numChunks == 1){
				this.computeChunk(0);
			}
			else{
				for(int i = 0; i < this.numChunks; ++i){
					this.computeChunk(i);
				}
			}

			this.needsUpdate = false;
			this.chunkIter = 0;
			this.updateFrame = 0;
		}

		/* */
		public void setCastShadows(bool state){
			if(state){
				this.castShadows = ShadowCastingMode.On;
			}
			else{
				this.castShadows = ShadowCastingMode.Off;		
			}
		}

		/* */
		public bool getCastShadows(){
			if(this.castShadows == ShadowCastingMode.On){
				return true;
			}

			return false;
		}

		/* */
		public void setReceiveShadows(bool state){
			this.receiveShadows = state;
		}

		/* */
		public bool getReceiveShadows(){
			return this.receiveShadows;
		}

		/* Schedule a draw call, 
			this is only useful if you disable this container's Update and want to manually draw its content.*/
		public void drawClayxels(ClayContainer instance){
			instance.renderBounds.center = instance.transform.position;

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				this.drawChunk(chunkIt, instance);
			}
		}

		/*  If CLAYXELS_CACHEON is set to 1 inside claySDF.compute,
			 this bool will force containers to 1,1,1 boundsScale to prevent big memory usage.
			 When disabling this, try to not exceed ClayContainer.setMaxChunks(3,3,3).*/
		public static bool safeCacheMemoryLimit = true;

		/*  Is CLAYXELS_CACHEON set to 1 inside claySDF.compute?
			 Caching will enable the use of ClayContainer.cacheDistField() .*/
		public static bool isCacheEnabled(){
			if(ClayContainer.cacheEnabled > 0){
				return true;
			}

			return false;
		}

		/* If CLAYXELS_CACHEON is set to 1 in claySDF.compute, use this method to cache whatever clay you have visible at this moment in time.
			After caching you can disable all the ClayObjects that contributed to your results.
			Introducing new ClayObjects after caching will result in your sculpt to keep updating as if all your ClayObjects were 
			still live in the scene. */
		public void cacheClay(){
			this.updateSolids();

			this.chunkIter = 0;
			this.updateFrame = 0;
			this.bufferSwapId = 0;
			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];

				chunk.renderPointCloudDataBuffer = chunk.pointCloudDataBuffer;
				chunk.renderIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;

				chunk.computePointCloudDataBuffer = chunk.pointCloudDataBuffer;
				chunk.computeIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;
				
				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);

				this.computeChunkCache(i);
			}
		}

		/* If CLAYXELS_CACHEON is set to 1 in claySDF.compute, use this method to clear the cahced clay.*/
		public void clearCachedClay(){
			int threads = 64 / ClayContainer.maxThreads;

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];

				uint indirectChunkId = sizeof(int) * ((uint)i * 3);

				ClayContainer.claycoreCompute.SetInt("chunkId", i);
				ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

				ClayContainer.claycoreCompute.Dispatch((int)Kernels.clearCachedDistField, threads, threads, threads);
			}
		}

		/* Returns a mesh at the specified level of detail, clayxelDetail will range from 0 to 100.
			Useful to generate mesh colliders, to improve performance leave colorizeMesh and watertight to false.*/
		public Mesh generateMesh(int clayxelDetail, bool colorizeMesh = false, bool watertight = false){
			this.switchComputeData();

			int prevDetail = this.clayxelDetail;

			if(clayxelDetail != this.clayxelDetail){
				this.setClayxelDetail(clayxelDetail);
			}
			else{
				this.forceUpdateAllSolids();
			}

			if(ClayContainer.meshIndicesBuffer == null){
				ClayContainer.meshIndicesBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 3, ComputeBufferType.Counter);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshIndicesBuffer);
				
				ClayContainer.meshVertsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(float) * 3);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshVertsBuffer);

				ClayContainer.meshColorsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(float) * 4);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshColorsBuffer);
			}

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutIndices", ClayContainer.meshIndicesBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutPoints", ClayContainer.meshVertsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutColors", ClayContainer.meshColorsBuffer);

			List<Vector3> totalVertices = null;
			List<int> totalIndices = null;
			List<Color> totalColors = null;

			if(this.numChunks > 1){
				totalVertices = new List<Vector3>();
				totalIndices = new List<int>();

				if(colorizeMesh){
					totalColors = new List<Color>();
				}
			}

			int totalNumVerts = 0;

			Mesh mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				ClayxelChunk chunk = this.chunks[chunkIt];

				ClayContainer.meshIndicesBuffer.SetCounterValue(0);

				ClayContainer.claycoreCompute.SetInt("chunkId", chunkIt);

				ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

				if(watertight){
					ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeGridForMesh, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes);
				}
				else{
					ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeGrid, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes);
				}

				ClayContainer.claycoreCompute.SetInt("outMeshIndexOffset", totalNumVerts);
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeMesh, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes);

				int numTris = this.getBufferCount(ClayContainer.meshIndicesBuffer);
				int numVerts = numTris * 3;

				if(numVerts > ClayContainer.chunkMaxOutPoints){
					Debug.Log("Clayxels: the generated mesh is too dense, try a lower clayxelDetail.");
					mesh = null;

					break;
				}

				totalNumVerts += numVerts;
				
				if(mesh != null){
					if(this.numChunks > 1){
						Vector3[] vertices = new Vector3[numVerts];
						ClayContainer.meshVertsBuffer.GetData(vertices);

						int[] indices = new int[numVerts];
						ClayContainer.meshIndicesBuffer.GetData(indices);

						totalVertices.AddRange(vertices);
						totalIndices.AddRange(indices);

						if(colorizeMesh){
							Color[] colors = new Color[numVerts];
							ClayContainer.meshColorsBuffer.GetData(colors);

							totalColors.AddRange(colors);
						}
					}
				}
			}

			if(mesh != null){
				if(this.numChunks > 1){
					mesh.vertices = totalVertices.ToArray();
					mesh.triangles = totalIndices.ToArray();

					if(colorizeMesh){
						mesh.colors = totalColors.ToArray();
					}
				}
				else{
					Vector3[] vertices = new Vector3[totalNumVerts];
					ClayContainer.meshVertsBuffer.GetData(vertices);

					mesh.vertices = vertices;

					int[] indices = new int[totalNumVerts];
					ClayContainer.meshIndicesBuffer.GetData(indices);

					mesh.triangles = indices;

					if(colorizeMesh){
						Color[] colors = new Color[totalNumVerts];
						meshColorsBuffer.GetData(colors);

						mesh.colors = colors;
					}
				}
			}

			if(prevDetail != this.clayxelDetail){
				this.setClayxelDetail(prevDetail);
			}

			return mesh;
		}

		/* Freeze this container to a mesh.*/
		public void freezeToMesh(){
			if(this.needsInit){
				this.init();
			}

			this.meshCached = true;
			this.enabled = false;

			if(this.gameObject.GetComponent<MeshFilter>() == null){
				this.gameObject.AddComponent<MeshFilter>();
			}
			
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				render = this.gameObject.AddComponent<MeshRenderer>();

				if(ClayContainer.renderPipe == "hdrp"){
					render.material = new Material(Shader.Find("Clayxels/ClayxelHDRPMeshShader"));
				}
				else if(ClayContainer.renderPipe == "urp"){
					render.material = new Material(Shader.Find("Clayxels/ClayxelURPMeshShader"));
				}
				else{
					render.material = new Material(Shader.Find("Clayxels/ClayxelBuiltInMeshShader"));
				}
			}
			
			this.switchComputeData();
			this.forceUpdateAllSolids();
			this.updateSolids();

			int detail = this.freezeMeshDetail;
			if(detail == -100){
				detail = this.getClayxelDetail();
			}
			else if(detail < 0){
				detail = 0;
			}
			else if(detail > 100){
				detail = 100;
			}

			bool vertexColors = true;
			bool watertight = true;
			Mesh mesh = this.generateMesh(detail, vertexColors, watertight);
			MeshUtils.weldVertices(mesh);
			mesh.Optimize();
			mesh.RecalculateNormals();
			
			this.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

			this.releaseBuffers();
		}

		/* Transfer every material attribue found with the same name from this container's material, to the generated mesh material. */
		public void transferMaterialPropertiesToMesh(){
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				return;
			}
			
			for(int propertyId = 0; propertyId < this.material.shader.GetPropertyCount(); ++propertyId){
				ShaderPropertyType type = this.material.shader.GetPropertyType(propertyId);
				string name = this.material.shader.GetPropertyName(propertyId);
				
				if(render.sharedMaterial.shader.FindPropertyIndex(name) != -1){
					if(type == ShaderPropertyType.Color || type == ShaderPropertyType.Vector){
						render.sharedMaterial.SetVector(name, this.material.GetVector(name));
					}
					else if(type == ShaderPropertyType.Float || type == ShaderPropertyType.Range){
						render.sharedMaterial.SetFloat(name, this.material.GetFloat(name));
					}
					else if(type == ShaderPropertyType.Texture){
						render.sharedMaterial.SetTexture(name, this.material.GetTexture(name));
					}
				}
			}

		}

		/* Is this container frozen to mesh? */
		public bool isFrozenToMesh(){
			// we need to perform this check each time
			if(this.gameObject.GetComponent<MeshFilter>() != null){
				this.meshCached = true;
			}

			return this.meshCached;
		}

		/* Disable the frozen mesh and get back to live clayxels. */
		public void disableFrozenMesh(){
			this.meshCached = false;
			this.needsInit = true;
			this.enabled = true;

			if(this.gameObject.GetComponent<MeshFilter>() != null){
				DestroyImmediate(this.gameObject.GetComponent<MeshFilter>());
			}
		}

		/* Access the point cloud buffer that is about to be drawn.
			To correctly access the point cloud data you should refer to the function 
			clayxelVertNormalBlend inside clayxelSRPUtils.cginc .*/
		public List<ComputeBuffer> getPointCloudBuffers(){
			List<ComputeBuffer> pointCloudBuffers = new List<ComputeBuffer>();

			for(int i = 0; i < this.numChunks; ++i){
				ComputeBuffer buff = this.chunks[i].renderPointCloudDataBuffer;
				pointCloudBuffers.Add(buff);
			}

			return pointCloudBuffers;
		}

		// end of public interface ///////////////////////////////////////

		static void parseSolidsAttrs(string content, ref int lastParsed){
			string[] lines = content.Split(new[]{ "\r\n", "\r", "\n" }, StringSplitOptions.None);
			for(int i = 0; i < lines.Length; ++i){
				string line = lines[i];
				if(line.Contains("label: ")){
					if(line.Split('/').Length == 3){// if too many comment slashes, it's a commented out solid,
						lastParsed += 1;

						string[] parameters = line.Split(new[]{"label:"}, StringSplitOptions.None)[1].Split(',');
						string label = parameters[0].Trim();
						
						ClayContainer.solidsCatalogueLabels.Add(label);

						List<string[]> paramList = new List<string[]>();

						for(int paramIt = 1; paramIt < parameters.Length; ++paramIt){
							string param = parameters[paramIt];
							string[] attrs = param.Split(':');
							string paramId = attrs[0];
							string[] paramLabelValue = attrs[1].Split(' ');
							string paramLabel = paramLabelValue[1];
							string paramValue = paramLabelValue[2];

							paramList.Add(new string[]{paramId.Trim(), paramLabel.Trim(), paramValue.Trim()});
						}

						ClayContainer.solidsCatalogueParameters.Add(paramList);
					}
				}
			}
		}

		// for internal use by the inspector
		static public void reloadSolidsCatalogue(){
			ClayContainer.solidsCatalogueLabels.Clear();
			ClayContainer.solidsCatalogueParameters.Clear();

			int lastParsed = -1;
			try{
				string claySDF = ((TextAsset)Resources.Load("claySDF", typeof(TextAsset))).text;
				ClayContainer.parseSolidsAttrs(claySDF, ref lastParsed);

				string numThreadsDef = "MAXTHREADS";
				ClayContainer.maxThreads = (int)char.GetNumericValue(claySDF[claySDF.IndexOf(numThreadsDef) + numThreadsDef.Length + 1]);

				string cacheDef = "CLAYXELS_CACHEON";
				ClayContainer.cacheEnabled = (int)char.GetNumericValue(claySDF[claySDF.IndexOf(cacheDef) + cacheDef.Length + 1]);
			}
			catch{
				Debug.Log("error trying to parse parameters in claySDF.compute, solid #" + lastParsed);
			}
		}

		// for internal use by the inspector
		public string[] getSolidsCatalogueLabels(){
			return ClayContainer.solidsCatalogueLabels.ToArray();
		}

		// for internal use by the inspector
		public List<string[]> getSolidsCatalogueParameters(int solidId){
			return ClayContainer.solidsCatalogueParameters[solidId];
		}

		void OnDestroy(){
			this.invalidated = true;

			this.releaseBuffers();

			if(UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length == 0){
				ClayContainer.releaseGlobalBuffers();
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.removeEditorEvents();
				}
			#endif
		}

		void computeClayStep(){
			if(this.memoryOptimized){
				this.expandMemory();
			}
			
			if(this.solidsHierarchyNeedsScan){
				this.scanClayObjectsHierarchy();
			}
			
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}

			if(this.chunkIter == 0){
				this.updateSolids();
			}

			if(this.numChunks == 1){
				this.computeChunk(0);
				this.needsUpdate = false;
			}
			else{
				for(int i = 0; i < ClayContainer.chunksUpdatePerFrame; ++i){
					this.computeChunk(this.chunkIter);
					this.chunkIter += 1;	
					
					if(this.chunkIter >= this.numChunks){
						this.needsUpdate = false;
						this.chunkIter = 0;
						
						if(ClayContainer.chunksUpdatePerFrame < this.numChunks){
							this.swapBuffer();
						}

						break;
					}
				}
			}
		}

		void releaseBuffers(){
			for(int i = 0; i < this.compBuffers.Count; ++i){
				this.compBuffers[i].Release();
			}

			this.compBuffers.Clear();
		}

		static void releaseGlobalBuffers(){
			for(int i = 0; i < ClayContainer.globalCompBuffers.Count; ++i){
				ClayContainer.globalCompBuffers[i].Release();
			}

			ClayContainer.globalCompBuffers.Clear();

			ClayContainer.globalDataNeedsInit = true;
		}

		void limitChunkValues(){
			if(this.chunksX > ClayContainer.maxChunkX){
				this.chunksX = ClayContainer.maxChunkX;
			}
			if(this.chunksY > ClayContainer.maxChunkY){
				this.chunksY = ClayContainer.maxChunkY;
			}
			if(this.chunksZ > ClayContainer.maxChunkZ){
				this.chunksZ = ClayContainer.maxChunkZ;
			}
			if(this.chunksX < 1){
				this.chunksX = 1;
			}
			if(this.chunksY < 1){
				this.chunksY = 1;
			}
			if(this.chunksZ < 1){
				this.chunksZ = 1;
			}

			if(this.chunkSize < 4){
				this.chunkSize = 4;
			}
			else if(this.chunkSize > 255){
				this.chunkSize = 255;
			}
		}

		void initChunks(){
			this.numChunks = 0;
			this.chunks.Clear();

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.localScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			for(int z = 0; z < this.chunksZ; ++z){
				for(int y = 0; y < this.chunksY; ++y){
					for(int x = 0; x < this.chunksX; ++x){
						this.initNewChunk(x, y, z);
						this.numChunks += 1;
					}
				}
			}

			this.updateChunksBuffer = new ComputeBuffer(this.numChunks, sizeof(int));
			this.compBuffers.Add(this.updateChunksBuffer);

			this.indirectChunkArgs1Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs1Buffer);

			this.indirectChunkArgs2Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs2Buffer);

			int[] indirectChunk1 = new int[this.numChunks * 3];
			int[] indirectChunk2 = new int[this.numChunks * 3];

			int indirectChunkSize1 = 64 / ClayContainer.maxThreads;
			int indirectChunkSize2 = 256 / ClayContainer.maxThreads;
			
			int[] updateChunks = new int[this.numChunks];

			for(int i = 0; i < this.numChunks; ++i){
				int indirectChunkId = i * 3;
				indirectChunk1[indirectChunkId] = indirectChunkSize1;
				indirectChunk1[indirectChunkId + 1] = indirectChunkSize1;
				indirectChunk1[indirectChunkId + 2] = indirectChunkSize1;

				indirectChunk2[indirectChunkId] = indirectChunkSize2;
				indirectChunk2[indirectChunkId + 1] = indirectChunkSize2;
				indirectChunk2[indirectChunkId + 2] = indirectChunkSize2;

				updateChunks[i] = 1;
			}

			this.updateChunksBuffer.SetData(updateChunks);
			this.indirectChunkArgs1Buffer.SetData(indirectChunk1);
			this.indirectChunkArgs2Buffer.SetData(indirectChunk2);
		}

		void initNewChunk(int x, int y, int z){
			ClayxelChunk chunk = new ClayxelChunk();
			this.chunks.Add(chunk);

			float seamOffset = this.chunkSize / 256.0f; // removes the seam between chunks
			float chunkOffset = this.chunkSize - seamOffset;
			float gridCenterOffset = (this.chunkSize * 0.5f);
			chunk.coords = new Vector3Int(x, y, z);
			chunk.center = new Vector3(
				(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * x),
				(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * y),
				(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * z));

			chunk.pointCloudDataBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 4);
			this.compBuffers.Add(chunk.pointCloudDataBuffer);

			chunk.indirectDrawArgsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(chunk.indirectDrawArgsBuffer);

			chunk.indirectDrawArgsBuffer.SetData(new int[]{0, 2, 0, 0});

			chunk.pointCloudDataBuffer2 = null;
			chunk.indirectDrawArgsBuffer2 = null;

			if(ClayContainer.chunksUpdatePerFrame < ClayContainer.totalMaxChunks){
				chunk.pointCloudDataBuffer2 = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 4);
				this.compBuffers.Add(chunk.pointCloudDataBuffer2);

				chunk.indirectDrawArgsBuffer2 = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
				this.compBuffers.Add(chunk.indirectDrawArgsBuffer2);

				chunk.indirectDrawArgsBuffer2.SetData(new int[]{0, 1, 0, 0});
			}

			chunk.renderPointCloudDataBuffer = chunk.pointCloudDataBuffer;
			chunk.renderIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;
			chunk.computePointCloudDataBuffer = chunk.pointCloudDataBuffer;
			chunk.computeIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;

			chunk.clayxelPickingMaterial = new Material(Shader.Find("Clayxels/ClayxelPickingShader"));
			chunk.clayxelPickingMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);

			chunk.materialProperties = new MaterialPropertyBlock();

			#if DRAW_DEBUG
				chunk.clayxelMaterial = new Material(Shader.Find("Clayxels/ClayxelDebugShader"));

				chunk.debugGridOutPointsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(float) * 3, ComputeBufferType.Counter);
				this.compBuffers.Add(chunk.debugGridOutPointsBuffer);

				chunk.clayxelMaterial.SetBuffer("debugChunkPoints", chunk.debugGridOutPointsBuffer);
			#endif
		}

		void initMaterialProperties(){
			#if DRAW_DEBUG
				return;
			#endif

			bool isPrefab = false;

			#if UNITY_EDITOR
				isPrefab = PrefabUtility.IsPartOfAnyPrefab(this.gameObject);
			#endif
			
			if(this.customMaterial != null){
				if(isPrefab){// use shared material
					this.material = this.customMaterial;
				}
				else{
					bool createNewMaterialInstance = true;
					if(this.material != null){
						if(this.material.shader.name == this.customMaterial.shader.name){
							// if a modified version of this customMaterial was already in use, don't destroy it
							createNewMaterialInstance = false;
						}
					}

					if(createNewMaterialInstance){
						this.material = new Material(this.customMaterial);
					}
				}
			}
			else{
				if(isPrefab){
					Debug.Log("Clayxels prefab " + this.gameObject.name + " needs a shared customMaterial or prefab will revert to a default material.");
				}

				if(this.material != null){// if this is a duplicated container
					this.material = new Material(this.material);
				}
				else{// brand new container
					if(ClayContainer.renderPipe == "hdrp"){
						this.material = new Material(Shader.Find("Clayxels/ClayxelHDRPShader"));
					}
					else if(ClayContainer.renderPipe == "urp"){
						this.material = new Material(Shader.Find("Clayxels/ClayxelURPShader"));
					}
					else{
						this.material = new Material(Shader.Find("Clayxels/ClayxelBuiltInShader"));
					}
				}
			}

			if(ClayContainer.renderPipe == "hdrp"){
				this.material.hideFlags = HideFlags.HideAndDontSave;// required in hdrp
			}

			if(this.customMaterial == null){
				// set the default clayxel texture to a dot on the standard material
				Texture texture = this.material.GetTexture("_MainTex");
				if(texture == null){
					this.material.SetTexture("_MainTex", (Texture)Resources.Load("clayxelDot"));
				}
			}
			

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.clayxelMaterial = new Material(this.material);
				
				chunk.clayxelMaterial.SetInt("solidHighlightId", -1);
				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);
				chunk.clayxelMaterial.SetFloat("chunkSize", (float)this.chunkSize);
				chunk.clayxelMaterial.SetVector("chunkCenter",  chunk.center);
			}
		}

		void scanRecursive(Transform trn){
			ClayObject clayObj = trn.gameObject.GetComponent<ClayObject>();
			if(clayObj != null){
				if(clayObj.isValid() && trn.gameObject.activeSelf){
					this.collectClayObject(clayObj);
				}
			}

			for(int i = 0; i < trn.childCount; ++i){
				GameObject childObj = trn.GetChild(i).gameObject;
				if(childObj.activeSelf){
					this.scanRecursive(childObj.transform);
				}
			}
		}

		void collectClayObject(ClayObject clayObj){
			if(clayObj.getNumSolids() == 0){
				clayObj.init();
			}

			clayObj.clayObjectId = this.clayObjects.Count;
			this.clayObjects.Add(new WeakReference(clayObj));

			for(int i = 0; i < clayObj.getNumSolids(); ++i){
				Solid solid = clayObj.getSolid(i);
				solid.id = this.solids.Count;
				solid.clayObjectId = clayObj.clayObjectId;
				this.solids.Add(solid);

				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
				else{
					break;
				}
			}

			clayObj.transform.hasChanged = true;
			clayObj.setClayxelContainer(this);
		}

		int getBufferCount(ComputeBuffer buffer){
			ComputeBuffer.CopyCount(buffer, this.countBuffer, 0);
			this.countBuffer.GetData(this.countBufferArray);
			int count = this.countBufferArray[0];

			return count;
		}

		#if DRAW_DEBUG
		void debugGridPoints(ClayxelChunk chunk){
			chunk.debugGridOutPointsBuffer.SetCounterValue(0);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.debugDisplayGridPoints, "debugGridOutPoints", chunk.debugGridOutPointsBuffer);
			ClayContainer.claycoreCompute.Dispatch((int)Kernels.debugDisplayGridPoints, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes);
		}
		#endif

		void computeChunk(int chunkId){
			ClayxelChunk chunk = this.chunks[chunkId];

			uint indirectChunkId = sizeof(int) * ((uint)chunkId * 3);

			ClayContainer.claycoreCompute.SetInt("chunkId", chunkId);
			ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "indirectDrawArgs", chunk.computeIndirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGrid, this.indirectChunkArgs1Buffer, indirectChunkId);

			#if DRAW_DEBUG
				this.debugGridPoints(chunk);
				return;
			#endif

			// generate point cloud
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "indirectDrawArgs", chunk.computeIndirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "pointCloudData", chunk.computePointCloudDataBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.generatePointCloud, this.indirectChunkArgs2Buffer, indirectChunkId);
		}

		void computeChunkCache(int chunkId){
			ClayxelChunk chunk = this.chunks[chunkId];

			uint indirectChunkId = sizeof(int) * ((uint)chunkId * 3);

			ClayContainer.claycoreCompute.SetInt("chunkId", chunkId);
			ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.cacheDistField, this.indirectChunkArgs1Buffer, indirectChunkId);
		}

		void updateSolids(){
			int solidCount = this.solids.Count;
			if(solidCount > ClayContainer.maxSolids){
				solidCount = ClayContainer.maxSolids;
			}

			foreach(int i in this.solidsUpdatedDict.Keys){
				Solid solid = this.solids[i];

				int clayObjId = solid.clayObjectId;
				if(solid.clayObjectId > -1){
					ClayObject clayObj = (ClayObject)this.clayObjects[solid.clayObjectId].Target;
					clayObj.pullUpdate();
				}
				else{
					clayObjId = 0;
				}

				if(solid.scale.x < 0.0001f){
					solid.scale.x = 0.0001f;
				}
				if(solid.scale.y < 0.0001f){
					solid.scale.y = 0.0001f;
				}
				if(solid.scale.z < 0.0001f){
					solid.scale.z = 0.0001f;
				}
				
				ClayContainer.solidsPos[i] = solid.position;
				ClayContainer.solidsRot[i] = solid.rotation;
				ClayContainer.solidsScale[i] = solid.scale;
				ClayContainer.solidsBlend[i] = solid.blend;
				ClayContainer.solidsType[i] = solid.primitiveType;
				ClayContainer.solidsColor[i] = solid.color;
				ClayContainer.solidsAttrs[i] = solid.attrs;
				ClayContainer.solidsClayObjectId[i] = clayObjId;
			}

			if(this.solids.Count > 0){
				ClayContainer.solidsPosBuffer.SetData(ClayContainer.solidsPos);
				ClayContainer.solidsRotBuffer.SetData(ClayContainer.solidsRot);
				ClayContainer.solidsScaleBuffer.SetData(ClayContainer.solidsScale);
				ClayContainer.solidsBlendBuffer.SetData(ClayContainer.solidsBlend);
				ClayContainer.solidsTypeBuffer.SetData(ClayContainer.solidsType);
				ClayContainer.solidsColorBuffer.SetData(ClayContainer.solidsColor);
				ClayContainer.solidsAttrsBuffer.SetData(ClayContainer.solidsAttrs);
				ClayContainer.solidsClayObjectIdBuffer.SetData(ClayContainer.solidsClayObjectId);
			}

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			if(this.numChunks > 1){
				ClayContainer.claycoreCompute.SetInt("numSolidsUpdated", this.solidsUpdatedDict.Count);
				ClayContainer.solidsUpdatedBuffer.SetData(this.solidsUpdatedDict.Keys.ToArray());
				
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.filterSolidsPerChunk, this.chunksX, this.chunksY, this.chunksZ);
			}

			this.solidsUpdatedDict.Clear();
		}

		void logFPS(){
			this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;
			float fps = 1.0f / this.deltaTime;
			Debug.Log(fps);
		}

		void switchComputeData(){
			int id = this.GetInstanceID();
			if(ClayContainer.lastUpdatedContainerId == id){
				return;
			}

			ClayContainer.lastUpdatedContainerId = id;

			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			ClayContainer.claycoreCompute.SetInt("numChunksX", this.chunksX);
			ClayContainer.claycoreCompute.SetInt("numChunksY", this.chunksY);
			ClayContainer.claycoreCompute.SetInt("numChunksZ", this.chunksZ);

			this.forceUpdateAllSolids();

			if(this.numChunks == 1){
				this.countBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.countBufferArray);

				ClayContainer.solidsPerChunkBuffer.SetData(ClayContainer.solidsInSingleChunkArray);
			}
			else{
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "updateChunks", this.updateChunksBuffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs1", this.indirectChunkArgs1Buffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs2", this.indirectChunkArgs2Buffer);
			}
		}

		void drawChunk(int chunkId, ClayContainer instance){
			ClayxelChunk chunk = this.chunks[chunkId];
			// ClayxelChunk chunkInstance = instance.chunks[chunkId];

			#if DRAW_DEBUG 
				int pnts = this.getBufferCount(chunk.debugGridOutPointsBuffer);
				
				Graphics.DrawProcedural(chunk.clayxelMaterial, 
					this.renderBounds,
					MeshTopology.Points, pnts, 1);
				return;
			#endif

			chunk.materialProperties.SetMatrix("objectMatrix", instance.transform.localToWorldMatrix);
			chunk.materialProperties.SetFloat("splatRadius", this.splatSize);

			#if UNITY_EDITOR
				// update some properties of the material only while in editor to avoid disappearing clayxels on certain editor events
				if(!Application.isPlaying){
					this.updateMaterialInEditor(chunk, this.splatSize);
				}
			#endif

			Graphics.DrawProceduralIndirect(chunk.clayxelMaterial, 
				instance.renderBounds,
				MeshTopology.Triangles, chunk.renderIndirectDrawArgsBuffer, 0,
				null, chunk.materialProperties,
				this.castShadows, this.receiveShadows, this.gameObject.layer);
		}

		void Start(){
			if(this.needsInit){
				this.init();
			}
		}

		bool checkNeedsInit(){
			// we need to perform these checks because prefabs will reset some of these attributes upon instancing
			if(this.needsInit || this.numChunks == 0 || this.material == null){
				return true;
			}

			return false;
		}

		void Update(){
			if(this.instanceOf != null){
				this.renderBounds = instanceOf.renderBounds;
				this.splatSize = this.instanceOf.splatRadius * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);
				this.instanceOf.drawClayxels(this);

				return;
			}

			if(this.checkNeedsInit()){
				this.init();
				this.updateFrame = 0;
			}
			else{
				// inhibit updates if this transform is the trigger
				if(this.transform.hasChanged){
					this.needsUpdate = false;
					this.transform.hasChanged = false;

					// if this transform moved and also one of the solids moved, then we still need to update
					if(this.forceUpdate){
						this.needsUpdate = true;
					}
				}
			}
			
			if(!Application.isPlaying){
				if(this.needsUpdate){
					// always keep everything up to date in editor, skipping frames should only be used in game
					this.computeClay();
				}
			}
			else{
				if(this.updateFrame == 0 && this.needsUpdate){
					this.computeClayStep();
				}
				
				if(this.chunkIter == 0 && ClayContainer.updateFrameSkip > 0){
					this.updateFrame = (this.updateFrame + 1) % ClayContainer.updateFrameSkip;
				}
			}
			
			this.splatSize = this.splatRadius * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);
			
			this.drawClayxels(this);

			#if CLAYXELS_GPU_FIX1
				// some old GPU models will strugle and this dummy getdata does somehow improve things  
				this.chunks[0].indirectDrawArgsBuffer.GetData(ClayContainer.dummyData);
			#endif
		}

		static void setupDistFieldCache(){
			uint encodeR6G6B6A14(float r, float g, float b, float a){
				uint ri = (uint)(r * 63.0f);
				uint gi = (uint)(g * 63.0f);
				uint bi = (uint)(b * 63.0f);
				uint ai = (uint)(((a + 1.0f) * 0.5f) * 16383.0f);
				
			 	uint rgba = (((ri<<6|gi)<<6|bi)<<14)|ai;

			 	return rgba;
			}

			if(ClayContainer.safeCacheMemoryLimit){
				if(ClayContainer.totalMaxChunks > 1){
					// prevent video memory from becoming huge 
					ClayContainer.maxChunkX = 1;
					ClayContainer.maxChunkY = 1;
					ClayContainer.maxChunkZ = 1;
					ClayContainer.totalMaxChunks = 1;
				}
			}

			int cacheSize1 = 64 * 64 * 64;
			int cacheSize2 = 256 * 256 * 256;
			
			ClayContainer.fieldCache1Buffer = new ComputeBuffer(cacheSize1 * ClayContainer.totalMaxChunks, sizeof(float));
			ClayContainer.globalCompBuffers.Add(ClayContainer.fieldCache1Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "fieldCache1", ClayContainer.fieldCache1Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "fieldCache1", ClayContainer.fieldCache1Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.clearCachedDistField, "fieldCache1", ClayContainer.fieldCache1Buffer);

			ClayContainer.fieldCache2Buffer = new ComputeBuffer(cacheSize2 * ClayContainer.totalMaxChunks, sizeof(uint));
			ClayContainer.globalCompBuffers.Add(ClayContainer.fieldCache2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "fieldCache2", ClayContainer.fieldCache2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "fieldCache2", ClayContainer.fieldCache2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.clearCachedDistField, "fieldCache2", ClayContainer.fieldCache2Buffer);

			float[] cache1 = new float[cacheSize1 * ClayContainer.totalMaxChunks];
			uint[] cache2 = new uint[cacheSize2 * ClayContainer.totalMaxChunks];
			for(int i = 0; i < cacheSize1 * ClayContainer.totalMaxChunks; ++i){
				cache1[i] = 1.0f;
			}

			uint defaultValue = encodeR6G6B6A14(1.0f, 1.0f, 1.0f, 1.0f);
			for(int i = 0; i < cacheSize2 * ClayContainer.totalMaxChunks; ++i){
				cache2[i] = defaultValue;
			}

			ClayContainer.fieldCache1Buffer.SetData(cache1);
			ClayContainer.fieldCache2Buffer.SetData(cache2);

			ClayContainer.claycoreCompute.SetInt("fieldCacheSize1", cacheSize1);
			ClayContainer.claycoreCompute.SetInt("fieldCacheSize2", cacheSize2);
		}

		void expandMemory(){
			this.memoryOptimized = false;

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				
				chunk.renderPointCloudDataBuffer.Release();
				this.compBuffers.Remove(chunk.renderPointCloudDataBuffer);
				
				chunk.pointCloudDataBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 4);
				this.compBuffers.Add(chunk.pointCloudDataBuffer);

				if(ClayContainer.chunksUpdatePerFrame < ClayContainer.totalMaxChunks){
					chunk.pointCloudDataBuffer2 = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 4);
					this.compBuffers.Add(chunk.pointCloudDataBuffer2);
				}

				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);

				chunk.renderPointCloudDataBuffer = chunk.pointCloudDataBuffer;
				chunk.renderIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;
				chunk.computePointCloudDataBuffer = chunk.pointCloudDataBuffer;
				chunk.computeIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;
			}

			this.bufferSwapId = 0;
			this.chunkIter = 0;
			this.updateFrame = 0;
		}

		void swapBuffer(){
			if(this.bufferSwapId == 0){
				this.bufferSwapId = 1;

				for(int i = 0; i < this.numChunks; ++i){
					ClayxelChunk chunk = this.chunks[i];
					
					chunk.renderPointCloudDataBuffer = chunk.pointCloudDataBuffer;
					chunk.renderIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;

					chunk.computePointCloudDataBuffer = chunk.pointCloudDataBuffer2;
					chunk.computeIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer2;

					chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);
				}
			}
			else{
				this.bufferSwapId = 0;

				for(int i = 0; i < this.numChunks; ++i){
					ClayxelChunk chunk = this.chunks[i];
					chunk.renderPointCloudDataBuffer = chunk.pointCloudDataBuffer2;	
					chunk.renderIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer2;

					chunk.computePointCloudDataBuffer = chunk.pointCloudDataBuffer;
					chunk.computeIndirectDrawArgsBuffer = chunk.indirectDrawArgsBuffer;

					chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);
				}
			}
		}

		// All functions past this point are used only in editor
		#if UNITY_EDITOR
		void Awake(){
			// ClayContainer.checkEditorUIScaling();

			if(!Application.isPlaying){
				// this is needed to trigger a re-init after playing in editor
				ClayContainer.globalDataNeedsInit = true;
				this.needsInit = true;
			}
		}

		public static float getEditorUIScale(){
			PropertyInfo p =
				typeof(GUIUtility).GetProperty("pixelsPerPoint", BindingFlags.Static | BindingFlags.NonPublic);

			float editorUiScaling = 1.0f;
			if(p != null){
				editorUiScaling = (float)p.GetValue(null, null);
			}

			return editorUiScaling;
		}

		void updateMaterialInEditor(ClayxelChunk chunk, float splatSize){
			if(ClayContainer.pickedClayxelId == this.clayxelId){
				chunk.clayxelMaterial.SetInt("solidHighlightId", ClayContainer.pickedClayObjectId);
			}
			else{
				chunk.clayxelMaterial.SetInt("solidHighlightId", -1);
			}

			chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);
			chunk.clayxelMaterial.SetFloat("chunkSize", (float)this.chunkSize);
			chunk.clayxelMaterial.SetVector("chunkCenter",  chunk.center);
			
			chunk.clayxelPickingMaterial.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
			chunk.clayxelPickingMaterial.SetFloat("splatRadius",  splatSize);
			chunk.clayxelPickingMaterial.SetBuffer("chunkPoints", chunk.renderPointCloudDataBuffer);
			chunk.clayxelPickingMaterial.SetFloat("chunkSize", (float)this.chunkSize);
			chunk.clayxelPickingMaterial.SetVector("chunkCenter",  chunk.center);
		}

		[MenuItem("GameObject/3D Object/Clayxel Container" )]
		public static ClayContainer createNewContainer(){
			 GameObject newObj = new GameObject("ClayxelContainer");
			 ClayContainer newClayContainer = newObj.AddComponent<ClayContainer>();

			 UnityEditor.Selection.objects = new GameObject[]{newObj};

			 return newClayContainer;
		}

		bool editingThisContainer = false;

		void OnValidate(){
			// called when editor value on this object is changed
			this.numChunks = 0;
		}

		void removeEditorEvents(){
			AssemblyReloadEvents.beforeAssemblyReload -= this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged -= this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged -= this.onSelectionChanged;

			Undo.undoRedoPerformed -= this.onUndoPerformed;
		}

		void reinstallEditorEvents(){
			this.removeEditorEvents();

			AssemblyReloadEvents.beforeAssemblyReload += this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged += this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged += this.onSelectionChanged;

			Undo.undoRedoPerformed += this.onUndoPerformed;
		}

		void onBeforeAssemblyReload(){
			// called when this script recompiles

			if(Application.isPlaying){
				return;
			}

			this.releaseBuffers();
			ClayContainer.releaseGlobalBuffers();

			ClayContainer.globalDataNeedsInit = true;
			this.needsInit = true;
		}

		void onUndoPerformed(){

			if(Undo.GetCurrentGroupName() == "changed clayobject" ||
				Undo.GetCurrentGroupName() == "changed clayxel container"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "changed clayxel grid"){
				this.init();
			}
			else if(Undo.GetCurrentGroupName() == "added clayxel solid"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "Selection Change"){
				if(!UnityEditor.Selection.Contains(this.gameObject)){
					if(UnityEditor.Selection.gameObjects.Length > 0){
						ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
						if(clayObj != null){
							if(clayObj.getClayContainer() == this){
								this.needsUpdate = true;
							}
						}
					}
				}
			}
			
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			ClayContainer.getSceneView().Repaint();
		}

		void onHierarchyChanged(){
			if(this.meshCached){
				return;
			}

			if(this.invalidated){
				// scene is being cleared
				return;
			}
			
			this.solidsHierarchyNeedsScan = true;
			this.needsUpdate = true;
			this.onSelectionChanged();
			
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			ClayContainer.getSceneView().Repaint();
			#if DEBUG_CLAYXEL_REPAINT_WARN
			Debug.Log("onHierarchyChanged!");
			#endif
		}

		public static void inspectorUpdate(){
			ClayContainer.inspectorUpdated = UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length;
		}

		void onSelectionChanged(){
			// for some reason this callback is also triggered by the inspector
			// so we first have to check if this is really a selection change or an inspector update. wtf. 

			if(ClayContainer.inspectorUpdated > 0){
				ClayContainer.inspectorUpdated -= 1;
				return;
			}

			if(this.invalidated){
				return;
			}

			if(this.meshCached){
				return;
			}

			if(this.instanceOf != null){
				return;
			}

			bool wasEditingThis = this.editingThisContainer;
			this.editingThisContainer = false;
			if(UnityEditor.Selection.Contains(this.gameObject)){
				// check if this container got selected
				this.editingThisContainer = true;
			}

			if(!this.editingThisContainer){
				// check if one of the clayObjs in container has been selected
				for(int i = 0; i < this.clayObjects.Count; ++i){
					ClayObject clayObj = (ClayObject)this.clayObjects[i].Target;

					if(clayObj != null){
						if(UnityEditor.Selection.Contains(clayObj.gameObject)){
							this.editingThisContainer = true;
							return;
						}
					}
				}

				if(wasEditingThis){// if we're changing selection, optimize the buffers of this container
					this.computeClay();
					this.optimizeMemory();
				}
			}
			
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}
		}

		static void setupScenePicking(){
			SceneView sceneView = (SceneView)SceneView.sceneViews[0];
			SceneView.duringSceneGui -= ClayContainer.onSceneGUI;
			SceneView.duringSceneGui += ClayContainer.onSceneGUI;

			ClayContainer.pickingCommandBuffer = new CommandBuffer();
			
			ClayContainer.pickingTextureResult = new Texture2D(1, 1, TextureFormat.ARGB32, false);

			ClayContainer.pickingRect = new Rect(0, 0, 1, 1);

			if(ClayContainer.pickingRenderTexture != null){
				ClayContainer.pickingRenderTexture.Release();
				ClayContainer.pickingRenderTexture = null;
			}

			ClayContainer.pickingRenderTexture = new RenderTexture(1024, 768, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			ClayContainer.pickingRenderTexture.Create();
			ClayContainer.pickingRenderTextureId = new RenderTargetIdentifier(ClayContainer.pickingRenderTexture);
		}

		public static void startPicking(){
			ClayContainer.pickingMode = true;
			ClayContainer.pickedObj = null;

			ClayContainer.getSceneView().Repaint();
		}

		static void clearPicking(){
			ClayContainer.pickingMode = false;
			ClayContainer.pickedObj = null;
			ClayContainer.pickedClayxelId = -1;
			ClayContainer.pickedClayObjectId = -1;

			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
		}

		static void onSceneGUI(SceneView sceneView){
			if(Application.isPlaying){
				return;
			}

			if(!UnityEditorInternal.InternalEditorUtility.isApplicationActive){
				// this callback keeps running even in the background
				return;
			}

			Event ev = Event.current;

			if(ev.isKey){
				if(ev.keyCode.ToString().ToLower() == ClayContainer.pickingKey){
					ClayContainer.startPicking();
				}
				// test cache
				// else if(ev.keyCode == KeyCode.C){
				// 	UnityEngine.Object.FindObjectsOfType<ClayContainer>()[0].cacheClay();
				// 	UnityEngine.Object.FindObjectsOfType<ClayContainer>()[0].computeClay();
				// }
				// else if(ev.keyCode == KeyCode.X){
				// 	UnityEngine.Object.FindObjectsOfType<ClayContainer>()[0].clearCachedClay();
				// }

				return;
			}
			
			if(!ClayContainer.pickingMode){
				return;
			}
			
			if(ClayContainer.pickedObj != null){
				if(ClayContainer.pickingShiftPressed){
					List<UnityEngine.Object> sel = new List<UnityEngine.Object>();
	   			for(int i = 0; i < UnityEditor.Selection.objects.Length; ++i){
	   				sel.Add(UnityEditor.Selection.objects[i]);
	   			}
	   			sel.Add(ClayContainer.pickedObj);
	   			UnityEditor.Selection.objects = sel.ToArray();
	   		}
	   		else{
					UnityEditor.Selection.objects = new GameObject[]{ClayContainer.pickedObj};
				}
			}
			
			if(ev.type == EventType.MouseMove){
				float uiScale = ClayContainer.getEditorUIScale();
				ClayContainer.pickingMousePosX = (int)ev.mousePosition.x * uiScale;
				ClayContainer.pickingMousePosY = (int)ev.mousePosition.y * uiScale;
				
				if(ClayContainer.pickedObj != null){
					ClayContainer.clearPicking();
				}
			}
			else if(ev.type == EventType.MouseDown && !ev.alt){
				if(ClayContainer.pickingMousePosX < 0 || ClayContainer.pickingMousePosX >= sceneView.camera.pixelWidth || 
					ClayContainer.pickingMousePosY < 0 || ClayContainer.pickingMousePosY >= sceneView.camera.pixelHeight){
					ClayContainer.clearPicking();
					return;
				}

				ev.Use();

				if(ClayContainer.pickedClayxelId > -1 && ClayContainer.pickedClayObjectId > -1){
					ClayContainer[] clayxels = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
					GameObject newSel = clayxels[ClayContainer.pickedClayxelId].getClayObject(ClayContainer.pickedClayObjectId).gameObject;
					UnityEditor.Selection.objects = new GameObject[]{newSel};

					ClayContainer.pickedObj = newSel;
					ClayContainer.pickingShiftPressed = ev.shift;
				}
				else{
					ClayContainer.clearPicking();
				}
			}
			else if((int)ev.type == 7){ // on repaint
				if(ClayContainer.pickingMousePosX < 0 || ClayContainer.pickingMousePosX >= sceneView.camera.pixelWidth || 
					ClayContainer.pickingMousePosY < 0 || ClayContainer.pickingMousePosY >= sceneView.camera.pixelHeight){
					return;
				}

				ClayContainer.pickedClayObjectId = -1;
		  		ClayContainer.pickedClayxelId = -1;

				ClayContainer.pickingCommandBuffer.Clear();
				ClayContainer.pickingCommandBuffer.SetRenderTarget(ClayContainer.pickingRenderTextureId);
				ClayContainer.pickingCommandBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);

				ClayContainer[] clayxels = UnityEngine.Object.FindObjectsOfType<ClayContainer>();

				for(int i = 0; i < clayxels.Length; ++i){
					ClayContainer clayxel = clayxels[i];
					if(!clayxel.meshCached){
						clayxels[i].drawClayxelPicking(i, ClayContainer.pickingCommandBuffer);
					}
				}

				Graphics.ExecuteCommandBuffer(ClayContainer.pickingCommandBuffer);

				
				ClayContainer.pickingRect.Set(
					(int)(1024.0f * ((float)ClayContainer.pickingMousePosX / (float)sceneView.camera.pixelWidth )), 
					(int)(768.0f * ((float)ClayContainer.pickingMousePosY / (float)sceneView.camera.pixelHeight)), 
					1, 1);

				RenderTexture oldRT = RenderTexture.active;
				RenderTexture.active = ClayContainer.pickingRenderTexture;
				ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
				ClayContainer.pickingTextureResult.Apply();
				RenderTexture.active = oldRT;
				
				Color pickCol = ClayContainer.pickingTextureResult.GetPixel(0, 0);
				
				int pickId = (int)((pickCol.r + pickCol.g * 255.0f + pickCol.b * 255.0f) * 255.0f);
		  		ClayContainer.pickedClayObjectId = pickId - 1;
		  		ClayContainer.pickedClayxelId = (int)(pickCol.a * 255.0f) - 1;
			}

			ClayContainer.getSceneView().Repaint();
		}

		void drawClayxelPicking(int clayxelId, CommandBuffer pickingCommandBuffer){
			if(this.needsInit){
				return;
			}

			this.clayxelId = clayxelId;

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				ClayxelChunk chunk = this.chunks[chunkIt];

				chunk.clayxelPickingMaterial.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
				chunk.clayxelPickingMaterial.SetInt("clayxelId", clayxelId);

				pickingCommandBuffer.DrawProceduralIndirect(Matrix4x4.identity, chunk.clayxelPickingMaterial, -1, 
					MeshTopology.Triangles, chunk.indirectDrawArgsBuffer);
			}
		}

		void OnDrawGizmos(){
			if(Application.isPlaying){
				return;
			}

			if(!this.editingThisContainer){
				return;
			}

			Gizmos.color = ClayContainer.boundsColor;
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(this.boundsCenter, this.boundsScale);

			// debug chunks
			// Vector3 boundsScale2 = new Vector3(this.chunkSize, this.chunkSize, this.chunkSize);
			// for(int i = 0; i < this.numChunks; ++i){
			// 	Gizmos.DrawWireCube(this.chunks[i].center, boundsScale2);
			// }
		}

		static public void reloadAll(){
			ClayContainer.globalDataNeedsInit = true;

			ClayContainer[] clayxelObjs = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < clayxelObjs.Length; ++i){
				clayxelObjs[i].init();
			}
			
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			((SceneView)SceneView.sceneViews[0]).Repaint();
		}

		public static SceneView getSceneView(){
			return (SceneView)SceneView.sceneViews[0];
		}

		public bool shouldRetopoMesh = false;
		public int retopoMaxVerts = -1;
		
		public void storeMesh(){
			AssetDatabase.CreateAsset(this.gameObject.GetComponent<MeshFilter>().sharedMesh, "Assets/" + this.meshAssetPath + ".mesh");
			AssetDatabase.SaveAssets();
			
			UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath("Assets/" + this.meshAssetPath + ".mesh");
			for(int i = 0; i < data.Length; ++i){
				if(data[i].GetType() == typeof(Mesh)){
					this.gameObject.GetComponent<MeshFilter>().sharedMesh = (Mesh)data[i];

					break;
				}
			}
		}

		#endif// end if UNITY_EDITOR
	}
}
