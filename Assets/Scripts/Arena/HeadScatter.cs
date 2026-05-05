using UnityEngine;

public class HeadScatter : MonoBehaviour
{
    [Header("Head Prefabs")]
    public GameObject[] headPrefabs;
    public Material headMaterial;

    [Header("Size")]
    public float headSize = 10000f;

    [Header("Coverage")]
    public float arenaHalf = 5000f;

    [Header("Depth")]
    [Tooltip("How far below ground the head centre sits. Lower = heads sit higher / more face visible.")]
    public float fixedSink = 420f;

    [Header("Gap Control")]
    [Tooltip("Fraction of narrow bbox axis used as spacing. Higher = more space between heads. 0.75 is a good no-gap-no-crush balance.")]
    [Range(0.30f, 1.20f)]
    public float overlapFactor = 0.75f;

    void Start()
    {
        if (headPrefabs == null || headPrefabs.Length == 0) return;

        float bboxNarrow = MeasureBBoxNarrowAxis();
        if (bboxNarrow <= 0f)
        {
            Debug.LogError("HeadScatter: could not measure head bounds.");
            return;
        }

        float spacing         = bboxNarrow * overlapFactor;
        float verticalSpacing = spacing * 0.866f;
        float groundY         = FindGroundY();

        Debug.Log("HeadScatter: bboxNarrow=" + bboxNarrow.ToString("F1")
                + "  spacing=" + spacing.ToString("F1")
                + "  sink=" + fixedSink.ToString("F1"));

        bool offsetRow = false;
        for (float z = -arenaHalf; z <= arenaHalf; z += verticalSpacing)
        {
            float xOff = offsetRow ? spacing * 0.5f : 0f;
            offsetRow  = !offsetRow;

            for (float x = -arenaHalf; x <= arenaHalf; x += spacing)
            {
                Vector3 pos = new Vector3(
                    x + xOff,
                    groundY - fixedSink,
                    z
                );
                PlaceHead(pos);
            }
        }
    }

    float MeasureBBoxNarrowAxis()
    {
        var probe = Instantiate(headPrefabs[0], Vector3.zero, Quaternion.identity);
        probe.transform.localScale = Vector3.one * headSize;

        var renderers = probe.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) { DestroyImmediate(probe); return 0f; }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        DestroyImmediate(probe);
        return Mathf.Min(b.size.x, b.size.z);
    }

    float FindGroundY()
    {
        if (Physics.Raycast(new Vector3(0f, 1000f, 0f), Vector3.down, out RaycastHit hit, 2000f))
            return hit.point.y;
        return 0f;
    }

    void PlaceHead(Vector3 pos)
    {
        int idx = Random.Range(0, headPrefabs.Length);

        Quaternion rot = Quaternion.Euler(
            180f,
            Random.Range(0f, 360f),
            0f
        );

        GameObject go = Instantiate(headPrefabs[idx], pos, rot, transform);
        go.transform.localScale = Vector3.one * headSize;
        go.isStatic = true;

        if (headMaterial != null)
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                r.sharedMaterial = headMaterial;
    }
}