using UnityEngine;

public enum ItemType
{
    Food,
    Trash
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]
public class ItemVisual
{
    public Sprite sprite;
    public string displayName;
    public Vector3 displayScale = Vector3.one;
}
public class Collectible : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private Rarity rarity;

    [Header("Visual Variants")]
    [SerializeField] private ItemVisual[] variants;

    private SpriteRenderer sr;
    private Collider2D col;
    private PolygonCollider2D poly;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();
    }
    void Start()
    {
        SetRandomVisual();
    }
private ItemVisual currentVisual;
    private void SetRandomVisual()
    {
        if (variants == null || variants.Length == 0)
        {
            Debug.LogWarning(gameObject.name + " has no variants assigned.");
            return;
        }

        int randomIndex = Random.Range(0, variants.Length);
        currentVisual = variants[randomIndex];

        sr.sprite = currentVisual.sprite;
        transform.localScale = currentVisual.displayScale;

        RefreshCollider();
    }
    private void RefreshCollider()
    {
        if (poly == null)
            return;

        // Rebuild collider using sprite's Physics Shape
        poly.pathCount = 0;

        Sprite sprite = sr.sprite;

        if (sprite == null)
            return;

        int shapeCount = sprite.GetPhysicsShapeCount();

        for (int i = 0; i < shapeCount; i++)
        {
            var points = new System.Collections.Generic.List<Vector2>();
            sprite.GetPhysicsShape(i, points);

            poly.pathCount = i + 1;
            poly.SetPath(i, points);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player == null || player.hasCollected)
            return;

        player.CollectItem(itemType, rarity);

GameSession.CollectedSprite = currentVisual.sprite;
GameSession.CollectedItemName = currentVisual.displayName;

        poly.enabled = false;

        StartCoroutine(PickupAnimation(player));

        // transform.SetParent(player.HoldPoint);
        // transform.localPosition = Vector3.zero;
        // transform.localRotation = Quaternion.identity;
    }

    private System.Collections.IEnumerator PickupAnimation(PlayerMovement player)
{
    Transform target = player.HoldPoint;

    Vector3 startPos = transform.position;
    Quaternion startRot = transform.rotation;
    Vector3 startScale = transform.localScale;

    // ====================================
    // RARITY SETTINGS
    // ====================================

    float snapDuration = 0.12f;
    float popScale = 1.15f;
    float wobbleAngle = 0f;

    switch (rarity)
    {
        case Rarity.Uncommon:
            popScale = 1.20f;
            break;

        case Rarity.Rare:
            popScale = 1.28f;
            wobbleAngle = 12f;
            break;

        case Rarity.Epic:
            popScale = 1.38f;
            wobbleAngle = 18f;
            snapDuration = 0.10f;
            break;

        case Rarity.Legendary:
            popScale = 1.55f;
            wobbleAngle = 25f;
            snapDuration = 0.08f;
            break;
    }

    // ====================================
    // SNAP TO HAND
    // ====================================

    float time = 0f;

    while (time < snapDuration)
    {
        time += Time.deltaTime;
        float t = Mathf.Clamp01(time / snapDuration);

        transform.position = Vector3.Lerp(startPos, target.position, t);
        transform.rotation = Quaternion.Lerp(startRot, target.rotation, t);

        transform.localScale =
            Vector3.Lerp(startScale, startScale * popScale, t);

        yield return null;
    }

    // ====================================
    // PARENT TO HAND
    // ====================================

    transform.SetParent(target, true);
    transform.localPosition = Vector3.zero;

    // wobble start
    transform.localRotation =
        Quaternion.Euler(0, 0, wobbleAngle);

    // ====================================
    // SETTLE BACK
    // ====================================

    time = 0f;
    float settleDuration = 0.12f;

    while (time < settleDuration)
    {
        time += Time.deltaTime;
        float t = Mathf.Clamp01(time / settleDuration);

        transform.localScale =
            Vector3.Lerp(startScale * popScale, startScale, t);

        transform.localRotation =
            Quaternion.Lerp(
                Quaternion.Euler(0, 0, wobbleAngle),
                Quaternion.identity,
                t
            );

        yield return null;
    }

    transform.localScale = startScale;
    transform.localRotation = Quaternion.identity;
}
}