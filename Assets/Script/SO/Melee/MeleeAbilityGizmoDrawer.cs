using UnityEngine;

[ExecuteAlways]
public class MeleeAbilityGizmoDrawer : MonoBehaviour
{
    public MeleeAbilitySO ability;
    public bool showWhenNotSelected = false;
    public Color fillColor = new Color(1f, 0f, 0f, 0.12f);
    public Color wireColor = Color.red;
    public float forwardLineLength = 1f;
    public int circleSegments = 40;

    private void OnDrawGizmos()
    {
        if (ability == null) return;
#if UNITY_EDITOR
        // if we don't want to show when not selected, bail when this GameObject is not the active selection
        if (!showWhenNotSelected && UnityEditor.Selection.activeGameObject != gameObject) return;
#endif
        DrawGizmosInternal();
    }

    private void OnDrawGizmosSelected()
    {
        if (ability == null) return;
        DrawGizmosInternal();
    }

    private void DrawGizmosInternal()
    {
        Transform source = transform;
        Vector3 origin = source.TransformPoint(ability.firePointOffset);
        float range = ability.range;
        float halfAngle = Mathf.Clamp(ability.forwardArcHalfAngle, 0f, 180f);
        Vector3 forward = source.forward;

#if UNITY_EDITOR
        // Solid fill using Handles when available (clean and faster)
        UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        UnityEditor.Handles.color = fillColor;
        UnityEditor.Handles.DrawSolidDisc(origin, Vector3.up, range);

        // Wire circle
        UnityEditor.Handles.color = wireColor;
        UnityEditor.Handles.DrawWireDisc(origin, Vector3.up, range);

        // Forward indicator
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawLine(origin, origin + forward * Mathf.Min(forwardLineLength, range));

        // Forward arc visualization if arc < 180
        if (halfAngle < 180f)
        {
            UnityEditor.Handles.color = new Color(wireColor.r, wireColor.g, wireColor.b, 0.9f);
            int arcSegments = Mathf.Max(6, Mathf.RoundToInt(circleSegments * (halfAngle * 2f) / 360f));
            float startAngle = -halfAngle;
            float step = (halfAngle * 2f) / arcSegments;
            Vector3 prev = origin + Quaternion.AngleAxis(startAngle, Vector3.up) * forward.normalized * range;
            for (int i = 1; i <= arcSegments; i++)
            {
                float angle = startAngle + step * i;
                Vector3 next = origin + Quaternion.AngleAxis(angle, Vector3.up) * forward.normalized * range;
                UnityEditor.Handles.DrawLine(prev, next);
                prev = next;
            }

            // Draw arc edges from origin
            Vector3 leftEdge = origin + Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward.normalized * range;
            Vector3 rightEdge = origin + Quaternion.AngleAxis(halfAngle, Vector3.up) * forward.normalized * range;
            UnityEditor.Handles.DrawLine(origin, leftEdge);
            UnityEditor.Handles.DrawLine(origin, rightEdge);
        }
#else
        // Fallback runtime gizmos (approximate)
        Gizmos.color = fillColor;
        int segs = Mathf.Max(12, circleSegments);
        Vector3 prev = origin + Quaternion.AngleAxis(0, Vector3.up) * Vector3.forward * range;
        for (int i = 1; i <= segs; i++)
        {
            float angle = (360f / segs) * i;
            Vector3 next = origin + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * range;
            Gizmos.DrawLine(origin, prev);
            Gizmos.DrawLine(origin, next);
            prev = next;
        }

        Gizmos.color = wireColor;
        prev = origin + Quaternion.AngleAxis(0, Vector3.up) * Vector3.forward * range;
        for (int i = 1; i <= segs; i++)
        {
            float angle = (360f / segs) * i;
            Vector3 next = origin + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * range;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + forward * Mathf.Min(forwardLineLength, range));

        if (halfAngle < 180f)
        {
            Vector3 leftEdge = origin + Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward.normalized * range;
            Vector3 rightEdge = origin + Quaternion.AngleAxis(halfAngle, Vector3.up) * forward.normalized * range;
            Gizmos.DrawLine(origin, leftEdge);
            Gizmos.DrawLine(origin, rightEdge);
        }
#endif
    }
}