using UnityEngine;

/// <summary>
/// Управляет режимом огненных стрел — статические флаги, доступные ArrowProjectile.
/// </summary>
public static class FireArrowMode
{
    public static bool Active       { get; private set; }
    public static int  ArrowsLeft  { get; private set; }

    public static void Activate(int count)
    {
        Active      = true;
        ArrowsLeft  = count;
    }

    /// <summary> Вызывается при выстреле огненной стрелой. </summary>
    public static bool ConsumeArrow()
    {
        if (!Active || ArrowsLeft <= 0) { Active = false; return false; }
        ArrowsLeft--;
        if (ArrowsLeft <= 0) Active = false;
        return true;
    }
}
