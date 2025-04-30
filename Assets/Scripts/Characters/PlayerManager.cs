using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MovementController))]
public class PlayerManager : CharacterManager
{
    [Header("Path Recalculation")]
    [Tooltip("Minimum seconds between path recalculations while holding.")]
    [SerializeField] private float recalcInterval = 0.1f;
    [Tooltip("World-unit threshold before we actually consider the mouse moved.")]
    [SerializeField] private float worldThreshold = 0.2f;

    private Tilemap floorTilemap;
    private MovementController movement;

    private float nextRecalcTime = 0f;
    private Vector3 lastTargetWorld = Vector3.positiveInfinity;

    void Start()
    {
        floorTilemap = FindFirstObjectByType<DungeonGenerator>().FloorTilemap;
        movement = GetComponent<MovementController>();
        UIManager.Instance.UpdatePlayerVitals(Stats.CurrentHP, Stats.CurrentAP, Stats.Hunger, Stats.Thirst);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
            HandleClickAndHold();
        else if (Input.GetMouseButtonUp(0))
            // reset so next click always fires immediately
            nextRecalcTime = 0f;
    }

    private void HandleClickAndHold()
    {
        // 1) only recalc at most once per interval
        if (Time.time < nextRecalcTime) return;
        nextRecalcTime = Time.time + recalcInterval;

        // 2) get exact world-point under mouse
        Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseW.z = 0;

        // 3) bail if we haven’t moved enough in world units
        if (Vector3.Distance(mouseW, lastTargetWorld) < worldThreshold)
            return;
        lastTargetWorld = mouseW;

        // 4) bail if it’s off the floor
        var cell = floorTilemap.WorldToCell(mouseW);
        if (!floorTilemap.HasTile(cell)) return;

        // 5) pivot instantly toward the new click spot
        movement.StopMovement();
        movement.MoveToWorld(mouseW);
    }
}
