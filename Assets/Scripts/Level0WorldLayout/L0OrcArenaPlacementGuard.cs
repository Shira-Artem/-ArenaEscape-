using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum L0OrcArenaPlacementCategory
{
    None = 0,
    Critical = 1 << 0,
    Camp = 1 << 1,
    SetDressing = 1 << 2,
    Cliff = 1 << 3,
    DistantBackdrop = 1 << 4,
    FarFence = 1 << 5,
    Combat = 1 << 6,
    All = int.MaxValue,
}

/// <summary>
/// Per-build placement context for large Level0 orc-arena visuals.
/// Reservations are deterministic circles in XZ space; conflicts are skipped, never nudged.
/// </summary>
public sealed class L0OrcArenaPlacementGuard
{
    private sealed class Reservation
    {
        public string Id;
        public L0OrcArenaPlacementCategory Category;
        public Vector3 Center;
        public float Radius;
    }

    private sealed class CriticalLane
    {
        public string Id;
        public Vector3 Start;
        public Vector3 End;
        public float HalfWidth;
    }

    private readonly List<Reservation> reservations = new List<Reservation>();
    private readonly List<CriticalLane> criticalLanes = new List<CriticalLane>();

    public string LastFailureReason { get; private set; }

    public bool TryReserve(L0OrcArenaPlacementCategory category, string id, Vector3 center, float radius, L0OrcArenaZone allowedZone)
    {
        LastFailureReason = null;
        radius = Mathf.Max(0f, radius);

        if (category == L0OrcArenaPlacementCategory.None || string.IsNullOrEmpty(id))
        {
            LastFailureReason = "Invalid placement category or id.";
            return false;
        }

        if (ContainsId(id))
        {
            LastFailureReason = "Duplicate reservation id: " + id;
            return false;
        }

        if (!IsFootprintInsideZone(center, radius, allowedZone))
        {
            LastFailureReason = "Footprint is outside zone " + allowedZone + ": " + id;
            return false;
        }

        if (category != L0OrcArenaPlacementCategory.Critical && !IsFootprintOutsideCriticalLanes(center, radius))
        {
            LastFailureReason = "Footprint intersects a critical lane: " + id;
            return false;
        }

        if (IsBackdropCategory(category)
            && !L0OrcArenaConfig.IsBackdropFootprintOutsideVisualForbiddenZones(center, radius))
        {
            LastFailureReason = "Backdrop footprint intersects an entrance/gate/road forbidden zone: " + id;
            return false;
        }

        if (category != L0OrcArenaPlacementCategory.Critical
            && IntersectsReserved(center, radius, L0OrcArenaPlacementCategory.All))
        {
            LastFailureReason = "Footprint intersects an existing reservation: " + id;
            return false;
        }

        reservations.Add(new Reservation
        {
            Id = id,
            Category = category,
            Center = center,
            Radius = radius,
        });
        return true;
    }

    public bool TryReserveBackdrop(
        L0OrcArenaPlacementCategory category,
        string id,
        Vector3 center,
        Vector3 size,
        L0OrcArenaZone allowedZone,
        out float conservativeRadius,
        float extraMargin = 0f)
    {
        conservativeRadius = GetConservativeBackdropRadius(size, extraMargin);
        if (!IsBackdropCategory(category))
        {
            LastFailureReason = "TryReserveBackdrop requires a backdrop category: " + id;
            return false;
        }

        return TryReserve(category, id, center, conservativeRadius, allowedZone);
    }

    public float GetConservativeBackdropRadius(Vector3 size, float extraMargin = 0f)
    {
        float maxHorizontalSize = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.z));
        return maxHorizontalSize * L0OrcArenaConfig.BackdropFootprintScale
            + L0OrcArenaConfig.BackdropFootprintMargin
            + Mathf.Max(0f, extraMargin);
    }

    public bool IsFootprintInsideZone(Vector3 center, float radius, L0OrcArenaZone zone)
    {
        return L0OrcArenaConfig.IsFootprintInsideZone(center, Mathf.Max(0f, radius), zone);
    }

    public bool IsFootprintOutsideCriticalLanes(Vector3 center, float radius)
    {
        radius = Mathf.Max(0f, radius);
        for (int i = 0; i < criticalLanes.Count; i++)
        {
            CriticalLane lane = criticalLanes[i];
            if (L0OrcArenaConfig.DistanceToSegmentXZ(center, lane.Start, lane.End) < lane.HalfWidth + radius)
                return false;
        }

        return true;
    }

    public bool IntersectsReserved(Vector3 center, float radius, L0OrcArenaPlacementCategory categoryMask)
    {
        radius = Mathf.Max(0f, radius);
        for (int i = 0; i < reservations.Count; i++)
        {
            Reservation reservation = reservations[i];
            if ((reservation.Category & categoryMask) == 0)
                continue;

            if (L0OrcArenaConfig.FlatDistance(center, reservation.Center) < radius + reservation.Radius)
                return true;
        }

        return false;
    }

    public void ReserveCriticalCircle(string id, Vector3 center, float radius)
    {
        if (string.IsNullOrEmpty(id) || ContainsId(id))
            return;

        reservations.Add(new Reservation
        {
            Id = id,
            Category = L0OrcArenaPlacementCategory.Critical,
            Center = center,
            Radius = Mathf.Max(0f, radius),
        });
    }

    public void ReserveCriticalLane(string id, Vector3 start, Vector3 end, float halfWidth)
    {
        if (string.IsNullOrEmpty(id) || ContainsId(id))
            return;

        criticalLanes.Add(new CriticalLane
        {
            Id = id,
            Start = start,
            End = end,
            HalfWidth = Mathf.Max(0f, halfWidth),
        });
    }

    public bool HasReservation(string id)
    {
        return !string.IsNullOrEmpty(id) && ContainsId(id);
    }

    private bool ContainsId(string id)
    {
        for (int i = 0; i < reservations.Count; i++)
        {
            if (reservations[i].Id == id)
                return true;
        }

        for (int i = 0; i < criticalLanes.Count; i++)
        {
            if (criticalLanes[i].Id == id)
                return true;
        }

        return false;
    }

    private static bool IsBackdropCategory(L0OrcArenaPlacementCategory category)
    {
        const L0OrcArenaPlacementCategory backdropMask = L0OrcArenaPlacementCategory.Cliff
            | L0OrcArenaPlacementCategory.DistantBackdrop
            | L0OrcArenaPlacementCategory.FarFence;
        return (category & backdropMask) != 0;
    }
}
