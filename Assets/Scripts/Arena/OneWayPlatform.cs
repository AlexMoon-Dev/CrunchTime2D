using UnityEngine;

/// <summary>
/// Companion script for PlatformEffector2D platforms.
/// Ensures the DropThrough layer switch from PlayerController works correctly
/// by configuring which layers this platform collides with.
///
/// Setup: attach alongside PlatformEffector2D + Collider2D.
/// Set Collider2D to "Used By Effector".
/// PlatformEffector2D: Surface Arc = 180, Use One Way = true.
/// </summary>
[RequireComponent(typeof(PlatformEffector2D), typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
    private void Awake()
    {
        var effector = GetComponent<PlatformEffector2D>();
        effector.useOneWay          = true;
        effector.surfaceArc         = 180f;
        effector.useOneWayGrouping  = true;

        // DropThrough layer should NOT collide with this platform
        // Set up in Unity's Physics 2D collision matrix:
        // Layer "DropThrough" vs Layer "Platform" → unchecked
        // TODO: configure in Project Settings > Physics 2D > Layer Collision Matrix
    }
}
