using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using BepInEx;
using HarmonyLib;

[BepInPlugin("com.rushellxyz.simplifiedmovements", "Simplified Movements", "0.0.0")]
public class Plugin : BaseUnityPlugin
{
    void Awake()
    {
        var harmony = new Harmony("com.rushellxyz.simplifiedmovements");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(PlayerScript), "Update")]
class JumpPatch
{
    static void Postfix(PlayerScript __instance)
    {
        if (Input.GetKey(__instance.keys["Jump"]) && !Input.GetKey(KeyCode.LeftControl))
        {
            __instance.bodyScript.Jump();
        }
    }
}

[HarmonyPatch(typeof(BodyScript), "Jump")]
class LogicPatch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            // Always suceed roll pouch
            if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float value1 && 0.214f == value1)
                instruction.operand = float.PositiveInfinity;
            // Ignore walljump min velocity (?)
            if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float value2 && -0.7f == value2)
                instruction.operand = float.NaN;

            yield return instruction;
        }
    }
}

[HarmonyPatch(typeof(BodyScript), "TryClimb")]
class MultiDirClimb
{
    static bool inRecursion;

    static void Prefix(BodyScript __instance)
    {
        if (!inRecursion)
            return;

        __instance.isRight = !__instance.isRight;
    }

    static void Postfix(BodyScript __instance)
    {
        if (!inRecursion)
        {
            inRecursion = true;
            try
            {
                __instance.TryClimb();
            }
            finally
            {
                inRecursion = false;
            }
            return;
        }

        __instance.isRight = !__instance.isRight;

    }
}
