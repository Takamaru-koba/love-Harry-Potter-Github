using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCamouflageFixedFull_V3 : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // player target
    public Camera playerCamera;              // player's camera (assign in inspector)
    public AudioSource aus;
    private bool isPlayingAudio;
    public AudioClip audioFile;
    [Header("Visibility / Sampling")]
    [Tooltip("Max number of mesh vertices to sample per check. More = more accurate but slower.")]
    public int maxVertexSamples = 128;
    [Tooltip("How often (seconds) to run visibility checks. 0 => every frame.")]
    public float sampleInterval = 0.08f;
    [Tooltip("Layers to raycast against (include environment + enemy).")]
    public LayerMask raycastMask = ~0;
    [Tooltip("Layers to IGNORE as meaningful hits (e.g., Player/camera colliders).")]
    public LayerMask ignoreLayers = 0;
    [Tooltip("Draw debug rays (green visible, red blocked)")]
    public bool showDebugRays = false;

    [Header("Camouflage")]
    [Tooltip("Candidate objects enemy can mimic")]
    public GameObject[] roomObjects;
    [Tooltip("Only mimic objects within this radius (world units)")]
    public float mimicRadius = 10f;

    [Header("Debounce / Timing")]
    [Tooltip("Seconds the enemy must be continuously visible before mimicking")]
    public float seenDelay = 0.08f;
    [Tooltip("Seconds the enemy must be continuously invisible before restoring")]
    public float unseenDelay = 0.12f;
    [Header("Movement")]
    [Tooltip("Enemy will not approach closer than this world distance to the player.")]
    public float minDistance = 2.0f;

    [Header("Agent / Behavior")]
    public bool instantStop = true;           // immediately zero velocity when seen
    public Color debugVisibleColor = Color.green;
    public Color debugBlockedColor = Color.red;

    // internals
    NavMeshAgent agent;

    // cached original parts (enemy)
    Renderer[] originalRenderers;
    MeshFilter[] originalMeshFilters;
    SkinnedMeshRenderer[] originalSkinnedRenderers;

    // baked mesh temp for skinned meshes
    Mesh bakedMeshTemp;

    // clone & restore
    GameObject currentMimicClone = null;
    List<Renderer> disabledRenderers = new List<Renderer>();

    // timing
    float lastSampleTime = -999f;
    float seenTimer = 0f;
    float unseenTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        isPlayingAudio = false;
        originalRenderers = GetComponentsInChildren<Renderer>();
        originalMeshFilters = GetComponentsInChildren<MeshFilter>();
        originalSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        bakedMeshTemp = new Mesh();

        if (playerCamera == null) playerCamera = Camera.main;
        if (sampleInterval < 0f) sampleInterval = 0f;
        if (maxVertexSamples < 1) maxVertexSamples = 1;

        lastSampleTime = Time.time - sampleInterval; // allow immediate check
    }

    void Update()
    {
        if (player == null || playerCamera == null) return;

        float dt = Time.time - lastSampleTime;
        if (dt < sampleInterval) return;
        lastSampleTime = Time.time;

        // IMPORTANT: visibility is checked against the "active target":
        // - if a clone is present -> check the clone's renderers/meshes
        // - otherwise -> check the enemy's original renderers/meshes
        bool visible = IsAnyMeshVertexVisibleToCamera_ForActiveTarget();

        if (visible)
        {
            seenTimer += dt;
            unseenTimer = 0f;
        }
        else
        {
            unseenTimer += dt;
            seenTimer = 0f;
        }

        // create mimic only after seenDelay continuous visibility
        if (seenTimer >= seenDelay)
        {
            if (currentMimicClone == null)
            {
                // stop agent immediately
                agent.isStopped = true;
                if (instantStop)
                {
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();
                }

                GameObject target = PickNearbyObject();
                if (target != null) ApplyMimicByCloning(target);
                else Debug.Log("[Camouflage] No nearby object to mimic (increase mimicRadius or assign roomObjects).");
            }
        }

        // destroy mimic only after unseenDelay continuous invisibility
        if (currentMimicClone != null && unseenTimer >= unseenDelay && Vector3.Distance(transform.position, player.position) > minDistance )
        {
            RestoreOriginalAppearance();
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // If not mimicking and not visible, ensure agent moves
        if (currentMimicClone == null && !visible)
        {
            agent.isStopped = false;
            if (isPlayingAudio == false)
            {
                isPlayingAudio = true;
                agent.SetDestination(player.position);
                StartCoroutine(LoopAudio());
            }
            
        } else
        {
            isPlayingAudio = false;
        }

        if (Vector3.Distance(transform.position, player.position) <= minDistance) {
            if (currentMimicClone == null)
            {
                // stop agent immediately
                agent.isStopped = true;
                if (instantStop)
                {
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();
                }

                GameObject target = PickNearbyObject();
                if (target != null) ApplyMimicByCloning(target);
                else Debug.Log("[Camouflage] No nearby object to mimic (increase mimicRadius or assign roomObjects).");
            }
        }
    }

    public IEnumerator LoopAudio()
    {
        if (agent.isStopped == false)
        {
            yield return new WaitForSeconds(0.5f);
            aus.PlayOneShot(audioFile);
            StartCoroutine(LoopAudio());
        }
        
    }

    // --------------------------
    // VISIBILITY: perform checks against the active target (clone or original)
    // --------------------------
    bool IsAnyMeshVertexVisibleToCamera_ForActiveTarget()
    {
        // decide which components to use
        Renderer[] activeRenderers = (currentMimicClone != null) ? currentMimicClone.GetComponentsInChildren<Renderer>() : originalRenderers;
        MeshFilter[] activeMeshFilters = (currentMimicClone != null) ? currentMimicClone.GetComponentsInChildren<MeshFilter>() : originalMeshFilters;
        SkinnedMeshRenderer[] activeSkinned = (currentMimicClone != null) ? currentMimicClone.GetComponentsInChildren<SkinnedMeshRenderer>() : originalSkinnedRenderers;

        // cheap early-out: if none of the active renderers are in any camera frustum, skip
        bool anyRendererInFrustum = false;
        for (int i = 0; i < activeRenderers.Length; ++i)
        {
            if (activeRenderers[i] != null && activeRenderers[i].isVisible)
            {
                anyRendererInFrustum = true;
                break;
            }
        }
        if (!anyRendererInFrustum) return false;

        List<Vector3> worldVertices = new List<Vector3>(maxVertexSamples + 8);

        // MeshFilters first (static meshes)
        foreach (var mf in activeMeshFilters)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            AddSampledVerts(mf.sharedMesh.vertices, mf.transform, worldVertices);
            if (worldVertices.Count >= maxVertexSamples) break;
        }

        // Skinned meshes
        if (worldVertices.Count < maxVertexSamples)
        {
            foreach (var smr in activeSkinned)
            {
                if (smr == null || smr.sharedMesh == null) continue;
                bakedMeshTemp.Clear();
                smr.BakeMesh(bakedMeshTemp);
                AddSampledVerts(bakedMeshTemp.vertices, smr.transform, worldVertices);
                if (worldVertices.Count >= maxVertexSamples) break;
            }
        }

        // fallback: bounds samples from activeRenderers
        if (worldVertices.Count == 0)
        {
            foreach (var r in activeRenderers)
            {
                if (r == null) continue;
                var b = r.bounds;
                worldVertices.Add(b.center);
                worldVertices.Add(b.min);
                worldVertices.Add(b.max);
                if (worldVertices.Count >= maxVertexSamples) break;
            }
        }

        if (worldVertices.Count > maxVertexSamples)
            worldVertices.RemoveRange(maxVertexSamples, worldVertices.Count - maxVertexSamples);

        Vector3 camPos = playerCamera.transform.position;

        // IMPORTANT: Raycasts must hit the clone (or enemy). We already set clone layer to enemy's layer.
        for (int i = 0; i < worldVertices.Count; i++)
        {
            Vector3 wv = worldVertices[i];
            Vector3 vp = playerCamera.WorldToViewportPoint(wv);

            // must be in front of camera and within viewport
            if (vp.z <= 0f) continue;
            if (vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;

            Vector3 dir = (wv - camPos);
            float dist = dir.magnitude;
            if (dist <= Mathf.Epsilon) continue;

            Ray ray = new Ray(camPos + playerCamera.transform.forward * 0.01f, dir.normalized);
            RaycastHit[] hits = Physics.RaycastAll(ray, dist + 0.001f, raycastMask);
            if (hits != null && hits.Length > 0)
            {
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                bool foundMeaningful = false;
                foreach (var h in hits)
                {
                    if (h.collider == null) continue;
                    if (h.collider.isTrigger) continue;
                    if (((1 << h.collider.gameObject.layer) & ignoreLayers) != 0) continue;

                    foundMeaningful = true;

                    // If we're mimicking, the relevant "hitThis" is whether the ray hits the clone (or enemy)
                    bool hitThis = false;
                    if (currentMimicClone != null)
                    {
                        // check if hit is on the clone hierarchy
                        hitThis = (h.transform == currentMimicClone.transform) || h.transform.IsChildOf(currentMimicClone.transform);
                    }
                    else
                    {
                        hitThis = (h.transform == transform) || h.transform.IsChildOf(transform);
                    }

                    if (showDebugRays)
                    {
                        Debug.DrawLine(camPos, h.point, hitThis ? debugVisibleColor : debugBlockedColor, sampleInterval > 0f ? sampleInterval : 0.02f);
                    }

                    if (hitThis) return true;
                    else break; // blocked by other object
                }

                // if everything hit was ignored (rare), treat that as visible
                if (!foundMeaningful)
                {
                    if (showDebugRays)
                        Debug.DrawRay(camPos, dir.normalized * Mathf.Min(dist, 20f), debugVisibleColor, sampleInterval > 0f ? sampleInterval : 0.02f);
                    return true;
                }
            }
            else
            {
                // nothing hit: visible
                if (showDebugRays)
                    Debug.DrawRay(camPos, dir.normalized * Mathf.Min(dist, 20f), debugVisibleColor, sampleInterval > 0f ? sampleInterval : 0.02f);
                return true;
            }
        }

        return false;
    }

    void AddSampledVerts(Vector3[] localVerts, Transform srcTransform, List<Vector3> outList)
    {
        if (localVerts == null || localVerts.Length == 0) return;
        int need = maxVertexSamples - outList.Count;
        if (need <= 0) return;

        int total = localVerts.Length;
        if (total <= need)
        {
            for (int i = 0; i < total; i++)
                outList.Add(srcTransform.TransformPoint(localVerts[i]));
        }
        else
        {
            float step = total / (float)need;
            float idx = 0f;
            for (int i = 0; i < need; i++, idx += step)
            {
                int vi = Mathf.Min(total - 1, Mathf.FloorToInt(idx));
                outList.Add(srcTransform.TransformPoint(localVerts[vi]));
            }
        }
    }

    // --------------------------
    // CAMOUFLAGE: clone & align properly, hide original renderers
    // --------------------------
    GameObject PickNearbyObject()
    {
        if (roomObjects == null || roomObjects.Length == 0) return null;
        List<GameObject> nearby = new List<GameObject>();
        foreach (var obj in roomObjects)
        {
            if (obj == null) continue;
            if (Vector3.Distance(transform.position, obj.transform.position) <= mimicRadius)
                nearby.Add(obj);
        }
        if (nearby.Count == 0) return null;
        int idx = UnityEngine.Random.Range(0, nearby.Count);
        Debug.Log($"[Camouflage] Picked mimic target: {nearby[idx].name}");
        return nearby[idx];
    }

    void ApplyMimicByCloning(GameObject target)
    {
        if (target == null) return;

        // instantiate clone (world transform preserved)
        GameObject clone = Instantiate(target);
        clone.name = $"MimicClone_{target.name}_{name}";

        // compute world-space bounds
        Bounds targetBounds = CalculateBoundsRecursive(clone);
        Bounds enemyBounds = CalculateRendererBounds(originalRenderers);

        // avoid zero-size
        Vector3 tSize = targetBounds.size;
        for (int i = 0; i < 3; i++) if (Mathf.Abs(tSize[i]) < 1e-4f) tSize[i] = 1e-4f;
        Vector3 eSize = enemyBounds.size;
        for (int i = 0; i < 3; i++) if (Mathf.Abs(eSize[i]) < 1e-4f) eSize[i] = 1e-4f;

        // compute per-axis scale factor to match enemy bounds (in world space)
        Vector3 scaleFactor = new Vector3(eSize.x / tSize.x, eSize.y / tSize.y, eSize.z / tSize.z);

        // apply scale (in local space) by scaling localScale
        clone.transform.localScale = Vector3.Scale(clone.transform.localScale, scaleFactor);

        // recompute clone bounds after scaling
        Bounds cloneBounds = CalculateBoundsRecursive(clone);

        // compute translation so clone bounds center matches enemy bounds center
        Vector3 desiredCenter = enemyBounds.center;
        Vector3 currentCenter = cloneBounds.center;
        Vector3 translation = desiredCenter - currentCenter;
        clone.transform.position += translation;

        // parent clone to enemy but preserve world transform
        clone.transform.SetParent(transform, worldPositionStays: true);

        // remove physics & animators and set layer
        RemovePhysicsAndAnimatorsRecursively(clone);
        SetLayerRecursively(clone, gameObject.layer);

        // hide enemy's original renderers (store which were disabled so we can restore)
        disabledRenderers.Clear();
        foreach (var r in originalRenderers)
        {
            if (r == null) continue;
            if (r.enabled)
            {
                r.enabled = false;
                disabledRenderers.Add(r);
            }
        }

        currentMimicClone = clone;
        Debug.Log($"[Camouflage] Applied mimic clone of '{target.name}' as child of '{name}'.");
    }

    void RestoreOriginalAppearance()
    {
        if (currentMimicClone != null)
        {
            Destroy(currentMimicClone);
            currentMimicClone = null;
        }

        // re-enable previously disabled renderers
        foreach (var r in disabledRenderers)
        {
            if (r != null) r.enabled = true;
        }
        disabledRenderers.Clear();

        Debug.Log("[Camouflage] Restored original appearance.");
    }

    void RemovePhysicsAndAnimatorsRecursively(GameObject go)
    {
        var colliders = go.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) Destroy(c);

        var rbs = go.GetComponentsInChildren<Rigidbody>();
        foreach (var r in rbs) Destroy(r);

        var anims = go.GetComponentsInChildren<Animator>();
        foreach (var a in anims) a.enabled = false;

        var monos = go.GetComponentsInChildren<MonoBehaviour>();
        foreach (var m in monos)
        {
            try
            {
                if (m != null && m != this) m.enabled = false;
            }
            catch { }
        }
    }

    Bounds CalculateBoundsRecursive(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        bool init = false;
        foreach (var r in rends)
        {
            if (!init)
            {
                b = r.bounds;
                init = true;
            }
            else b.Encapsulate(r.bounds);
        }
        if (!init) b = new Bounds(go.transform.position, Vector3.one * 0.1f);
        return b;
    }

    Bounds CalculateRendererBounds(Renderer[] rends)
    {
        Bounds b = new Bounds(transform.position, Vector3.zero);
        bool init = false;
        foreach (var r in rends)
        {
            if (r == null) continue;
            if (!init) { b = r.bounds; init = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!init) b = new Bounds(transform.position, Vector3.one * 0.1f);
        return b;
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}
