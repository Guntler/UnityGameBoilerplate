using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UtilitiesGame
{
    [Serializable]
    public struct StickSector
    {
        [SerializeField]
        string SectorId;
        [SerializeField]
        float AngleA;
        [SerializeField]
        float AngleB;
    }

    /**
     * https://answers.unity.com/questions/1105437/rotate-vector3-manually.html
     */
    public static Vector3 RotateVector(Vector3 aVec, Vector3 aAngles)
    {
        aAngles *= Mathf.Deg2Rad;
        float sx = Mathf.Sin(aAngles.x);
        float cx = Mathf.Cos(aAngles.x);
        float sy = Mathf.Sin(aAngles.y);
        float cy = Mathf.Cos(aAngles.y);
        float sz = Mathf.Sin(aAngles.z);
        float cz = Mathf.Cos(aAngles.z);
        aVec = new Vector3(aVec.x * cz - aVec.y * sz, aVec.x * sz + aVec.y * cz, aVec.z);
        aVec = new Vector3(aVec.x, aVec.y * cx - aVec.z * sx, aVec.y * sx + aVec.z * cx);
        aVec = new Vector3(aVec.x * cy + aVec.z * sy, aVec.y, -aVec.x * sy + aVec.z * cy);
        return aVec;
    }

    public static bool FloatApprox(float a, float b, float tolerance = 0.005f)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }

    public static bool FloatApprox(Vector3 a, Vector3 b, float tolerance = 0.005f)
    {
        return ((Mathf.Abs(a.x - b.x) < tolerance) && (Mathf.Abs(a.y - b.y) < tolerance) && (Mathf.Abs(a.z - b.z) < tolerance));
    }

    public static bool FloatApprox(Vector3 a, Vector3 b, float toleranceX = 0.005f, float toleranceY = 0.005f, float toleranceZ = 0.005f)
    {
        return ((Mathf.Abs(a.x - b.x) < toleranceX) && (Mathf.Abs(a.y - b.y) < toleranceY) && (Mathf.Abs(a.z - b.z) < toleranceZ));
    }

    public static bool ColorApprox(Color a, Color b, float toleranceR = 0.005f, float toleranceG = 0.005f, float toleranceB = 0.005f, float toleranceA = 0.005f)
    {
        return ((Mathf.Abs(a.r - b.r) < toleranceR) && (Mathf.Abs(a.g - b.g) < toleranceG) && (Mathf.Abs(a.b - b.b) < toleranceB) && (Mathf.Abs(a.a - b.a) < toleranceA));
    }

    public static IEnumerator LerpColor(Material m, Color target, float rate, float delay)
    {
        while (!UtilitiesGame.ColorApprox(m.color, target, 0.1f, 0.1f, 0.1f, 0.1f)) {
            if (!m)
                break;
            m.color = Color.Lerp(m.color, target, rate * Time.deltaTime);
            yield return new WaitForSeconds(delay);
        }

        if (m)
            m.color = target;
        yield return null;
    }

    public static IEnumerator LerpColor(Image i, Color target, float rate, float delay)
    {
        while (!UtilitiesGame.ColorApprox(i.color, target, 0.1f, 0.1f, 0.1f, 0.1f)) {
            if (!i)
                break;
            i.color = Color.Lerp(i.color, target, rate * Time.deltaTime);
            yield return new WaitForSeconds(delay);
        }

        if (i)
            i.color = target;
        yield return null;
    }

    public static IEnumerator LerpColor(Text t, Color target, float rate, float delay)
    {
        while (!UtilitiesGame.ColorApprox(t.color, target, 0.1f, 0.1f, 0.1f, 0.1f)) {
            if (!t)
                break;
            t.color = Color.Lerp(t.color, target, rate * Time.deltaTime);
            yield return new WaitForSeconds(delay);
        }

        if (t)
            t.color = target;
        yield return null;
    }

    public static IEnumerator LerpVolume(AudioSource src, float targetVolume, float rate, float delay)
    {
        while (!UtilitiesGame.FloatApprox(src.volume, targetVolume, 0.005f)) {
            if (!src)
                break;

            float sign = Mathf.Sign(targetVolume - src.volume);
            src.volume += sign * Time.deltaTime * rate;
            yield return new WaitForSeconds(delay);
        }

        if (src)
            src.volume = targetVolume;
        yield return null;
    }

    /// <summary>
    /// Creates and combines the InstanceID of an owner object with a specified objString. Often used for timers.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="objString"></param>
    public static void MakeGameObjectString(GameObject owner, ref string objString)
    {
        objString = objString + owner.GetInstanceID();
    }
}
