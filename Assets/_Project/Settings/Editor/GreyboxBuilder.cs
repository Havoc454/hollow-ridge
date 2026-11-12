using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HollowRidge.EditorTools
{
    /// <summary>
    /// Builds the Phase 1.a movement test course from plain cubes.
    /// Menu: Hollow Ridge > Build Greybox Test Course. Safe to run repeatedly.
    /// The player starts at the origin facing +Z (forward).
    /// </summary>
    public static class GreyboxBuilder
    {
        const string RootName = "Greybox_Course";

        [MenuItem("Hollow Ridge/Build Greybox Test Course")]
        static void Build()
        {
            GameObject old = GameObject.Find(RootName);
            if (old != null) Undo.DestroyObjectImmediate(old);
            GameObject tempCeiling = GameObject.Find("TestCeiling");
            if (tempCeiling != null) Undo.DestroyObjectImmediate(tempCeiling);

            GameObject rootGo = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(rootGo, "Build Greybox Test Course");
            Transform root = rootGo.transform;

            BuildSmallStairs(Group(root, "Stairs_SmallSteps"));
            BuildLargeSteps(Group(root, "Steps_Curb_And_Block"));
            BuildRamps(Group(root, "Ramps"));
            BuildTunnel(Group(root, "Tunnel_LowCeiling"));
            BuildLowGap(Group(root, "Wall_LowGap"));
            BuildCorner(Group(root, "Wall_Corner"));
            BuildPlatforms(Group(root, "Jump_Platforms"));

            EditorSceneManager.MarkSceneDirty(rootGo.scene);
            Selection.activeGameObject = rootGo;
            Debug.Log("Greybox test course built.");
        }

        // ---- Stations (X = left/right, Z = forward from the spawn) ----

        // 8 steps of 0.2 m (below the 0.35 m step offset): should walk straight up.
        static void BuildSmallStairs(Transform g)
        {
            const int steps = 8;
            const float rise = 0.2f, run = 0.4f, width = 4f, x = -12f, z0 = 8f;
            for (int i = 0; i < steps; i++)
            {
                float h = rise * (i + 1);
                Box(g, "Step_" + (i + 1), new Vector3(x, h / 2f, z0 + run * i + run / 2f), new Vector3(width, h, run));
            }
            float top = rise * steps;
            Box(g, "Landing", new Vector3(x, top / 2f, z0 + run * steps + 1.5f), new Vector3(width, top, 3f));
        }

        // A 0.3 m curb (should climb) and a 0.6 m block (should block; jump onto it).
        static void BuildLargeSteps(Transform g)
        {
            Box(g, "Step_0.3m_ShouldClimb", new Vector3(-6f, 0.15f, 10f), new Vector3(4f, 0.3f, 2f));
            Box(g, "Step_0.6m_ShouldBlock", new Vector3(-6f, 0.3f, 14f), new Vector3(4f, 0.6f, 2f));
        }

        // Slope limit is 45 degrees: 20 and 40 should be climbable, 55 should not.
        static void BuildRamps(Transform g)
        {
            Ramp(g, "Ramp_20deg_Gentle", 2f, 8f, 20f, 8f, 4f);
            Ramp(g, "Ramp_40deg_Steep_Climbable", 8f, 8f, 40f, 8f, 4f);
            Ramp(g, "Ramp_55deg_TooSteep", 14f, 8f, 55f, 8f, 4f);
        }

        // Roof underside at 1.2 m: fits crouched (1.0 m), not standing (1.8 m).
        static void BuildTunnel(Transform g)
        {
            const float x = -12f, z0 = 24f, len = 8f;
            Box(g, "Wall_Left", new Vector3(x - 1.4f, 0.75f, z0 + len / 2f), new Vector3(0.3f, 1.5f, len));
            Box(g, "Wall_Right", new Vector3(x + 1.4f, 0.75f, z0 + len / 2f), new Vector3(0.3f, 1.5f, len));
            Box(g, "Roof", new Vector3(x, 1.35f, z0 + len / 2f), new Vector3(3.1f, 0.3f, len));
        }

        // A wall with a 2 m wide opening whose top edge is 1.1 m up.
        static void BuildLowGap(Transform g)
        {
            const float x = -5f, z = 24f;
            Box(g, "Wall_Left", new Vector3(x - 2.5f, 1.5f, z), new Vector3(3f, 3f, 0.5f));
            Box(g, "Wall_Right", new Vector3(x + 2.5f, 1.5f, z), new Vector3(3f, 3f, 0.5f));
            Box(g, "Lintel_Gap1.1m", new Vector3(x, 2.05f, z), new Vector3(2f, 1.9f, 0.5f));
        }

        // Two walls meeting at a right angle. Walk diagonally into the inside corner.
        static void BuildCorner(Transform g)
        {
            const float x0 = 3f, z0 = 24f;
            Box(g, "Wall_AlongX", new Vector3(x0 + 3f, 1.5f, z0), new Vector3(6f, 3f, 0.3f));
            Box(g, "Wall_AlongZ", new Vector3(x0, 1.5f, z0 + 3f), new Vector3(0.3f, 3f, 6f));
        }

        // Jump peak is about 1.1 m: the 0.5 and 1.0 platforms are reachable, 1.5 and 2.0 are not.
        static void BuildPlatforms(Transform g)
        {
            float[] heights = { 0.5f, 1.0f, 1.5f, 2.0f };
            for (int i = 0; i < heights.Length; i++)
            {
                float h = heights[i];
                Box(g, "Platform_" + h.ToString("0.0") + "m", new Vector3(8f + 4f * i, h / 2f, 30f), new Vector3(3f, h, 3f));
            }
        }

        // ---- Helpers ----

        static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static void Box(Transform parent, string name, Vector3 center, Vector3 size, Quaternion? rotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.rotation = rotation ?? Quaternion.identity;
            go.transform.localScale = size;
        }

        // A slab tilted so its upper surface meets the floor at z0 and rises toward +Z.
        static void Ramp(Transform parent, string name, float x, float z0, float angleDeg, float length, float width)
        {
            const float thickness = 0.4f;
            float a = angleDeg * Mathf.Deg2Rad;
            float cy = (length / 2f) * Mathf.Sin(a) - (thickness / 2f) * Mathf.Cos(a);
            float cz = z0 + (length / 2f) * Mathf.Cos(a) + (thickness / 2f) * Mathf.Sin(a);
            Box(parent, name, new Vector3(x, cy, cz), new Vector3(width, thickness, length), Quaternion.Euler(-angleDeg, 0f, 0f));
        }
    }
}