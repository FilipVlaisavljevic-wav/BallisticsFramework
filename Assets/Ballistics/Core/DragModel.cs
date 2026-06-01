using UnityEngine;

/// <summary>
/// G1 i G7 drag tablice. Tablice su preuzete iz LGPL biblioteke
/// gehtsoft-usa/go_ballisticcalc, čije podatke izvorno potječu iz BRL
/// (Ballistic Research Laboratory) standarda i službene JBM Ballistics
/// dokumentacije.
/// </summary>
public static class DragModel
{
    // ── G7 referentne tablice (Mach → Cd) ──
    // Standardni G7 projektil: 7.5° boattail, 10 caliber secant ogive
    private static readonly float[] G7_Mach = {
        0.00f, 0.05f, 0.10f, 0.15f, 0.20f, 0.25f, 0.30f, 0.35f, 0.40f, 0.45f,
        0.50f, 0.55f, 0.60f, 0.65f, 0.70f, 0.725f, 0.75f, 0.775f, 0.80f, 0.825f,
        0.85f, 0.875f, 0.90f, 0.925f, 0.95f, 0.975f, 1.00f, 1.025f, 1.05f, 1.075f,
        1.10f, 1.125f, 1.15f, 1.20f, 1.25f, 1.30f, 1.35f, 1.40f, 1.50f, 1.55f,
        1.60f, 1.65f, 1.70f, 1.75f, 1.80f, 1.85f, 1.90f, 1.95f, 2.00f, 2.05f,
        2.10f, 2.15f, 2.20f, 2.25f, 2.30f, 2.35f, 2.40f, 2.45f, 2.50f, 2.55f,
        2.60f, 2.65f, 2.70f, 2.75f, 2.80f, 2.85f, 2.90f, 2.95f, 3.00f, 3.10f,
        3.20f, 3.30f, 3.40f, 3.50f, 3.60f, 3.70f, 3.80f, 3.90f, 4.00f, 4.20f,
        4.40f, 4.60f, 4.80f, 5.00f
    };

    private static readonly float[] G7_Cd = {
        0.1198f, 0.1197f, 0.1196f, 0.1194f, 0.1193f, 0.1194f, 0.1194f, 0.1194f, 0.1193f, 0.1193f,
        0.1194f, 0.1193f, 0.1194f, 0.1197f, 0.1202f, 0.1207f, 0.1215f, 0.1226f, 0.1242f, 0.1266f,
        0.1306f, 0.1368f, 0.1464f, 0.1660f, 0.2054f, 0.2993f, 0.3803f, 0.4015f, 0.4043f, 0.4034f,
        0.4014f, 0.3987f, 0.3955f, 0.3884f, 0.3810f, 0.3732f, 0.3657f, 0.3580f, 0.3440f, 0.3376f,
        0.3315f, 0.3260f, 0.3209f, 0.3160f, 0.3117f, 0.3078f, 0.3042f, 0.3010f, 0.2980f, 0.2951f,
        0.2922f, 0.2892f, 0.2864f, 0.2835f, 0.2807f, 0.2779f, 0.2752f, 0.2725f, 0.2697f, 0.2670f,
        0.2643f, 0.2615f, 0.2588f, 0.2561f, 0.2533f, 0.2506f, 0.2479f, 0.2451f, 0.2424f, 0.2368f,
        0.2313f, 0.2258f, 0.2205f, 0.2154f, 0.2106f, 0.2060f, 0.2017f, 0.1975f, 0.1935f, 0.1861f,
        0.1793f, 0.1730f, 0.1672f, 0.1618f
    };

    // ── G1 referentne tablice (Mach → Cd) ──
    // Standardni G1 projektil: flat base, 2 caliber tangent ogive
    private static readonly float[] G1_Mach = {
        0.00f, 0.05f, 0.10f, 0.15f, 0.20f, 0.25f, 0.30f, 0.35f, 0.40f, 0.45f,
        0.50f, 0.55f, 0.60f, 0.70f, 0.725f, 0.75f, 0.775f, 0.80f, 0.825f, 0.85f,
        0.875f, 0.90f, 0.925f, 0.95f, 0.975f, 1.00f, 1.025f, 1.05f, 1.075f, 1.10f,
        1.125f, 1.15f, 1.20f, 1.25f, 1.30f, 1.35f, 1.40f, 1.45f, 1.50f, 1.55f,
        1.60f, 1.65f, 1.70f, 1.75f, 1.80f, 1.85f, 1.90f, 1.95f, 2.00f, 2.05f,
        2.10f, 2.15f, 2.20f, 2.25f, 2.30f, 2.35f, 2.40f, 2.45f, 2.50f, 2.60f,
        2.70f, 2.80f, 2.90f, 3.00f, 3.10f, 3.20f, 3.30f, 3.40f, 3.50f, 3.60f,
        3.70f, 3.80f, 3.90f, 4.00f, 4.20f, 4.40f, 4.60f, 4.80f, 5.00f
    };

    private static readonly float[] G1_Cd = {
        0.2629f, 0.2558f, 0.2487f, 0.2413f, 0.2344f, 0.2278f, 0.2214f, 0.2155f, 0.2104f, 0.2061f,
        0.2032f, 0.2020f, 0.2034f, 0.2165f, 0.2230f, 0.2313f, 0.2417f, 0.2546f, 0.2706f, 0.2901f,
        0.3136f, 0.3415f, 0.3734f, 0.4084f, 0.4448f, 0.4805f, 0.5136f, 0.5427f, 0.5677f, 0.5883f,
        0.6053f, 0.6191f, 0.6393f, 0.6518f, 0.6589f, 0.6621f, 0.6625f, 0.6607f, 0.6573f, 0.6528f,
        0.6474f, 0.6413f, 0.6347f, 0.6280f, 0.6210f, 0.6141f, 0.6072f, 0.6003f, 0.5934f, 0.5867f,
        0.5804f, 0.5743f, 0.5685f, 0.5630f, 0.5577f, 0.5527f, 0.5481f, 0.5438f, 0.5397f, 0.5325f,
        0.5264f, 0.5211f, 0.5168f, 0.5133f, 0.5105f, 0.5084f, 0.5067f, 0.5054f, 0.5040f, 0.5030f,
        0.5022f, 0.5016f, 0.5010f, 0.5006f, 0.4998f, 0.4995f, 0.4992f, 0.4990f, 0.4988f
    };

    /// <summary>
    /// Vraća referentni Cd s linearnom interpolacijom.
    /// Binarno pretraživanje za pronalazak susjednih točaka u tablici.
    /// </summary>
    public static float LookupReferenceCd(float mach, DragModelType model)
    {
        float[] machTable = (model == DragModelType.G7) ? G7_Mach : G1_Mach;
        float[] cdTable = (model == DragModelType.G7) ? G7_Cd : G1_Cd;

        // Edge cases
        if (mach <= machTable[0]) return cdTable[0];
        if (mach >= machTable[machTable.Length - 1]) return cdTable[cdTable.Length - 1];

        // Binarno pretraživanje — pronaći lo i hi takve da machTable[lo] <= mach <= machTable[hi]
        int lo = 0, hi = machTable.Length - 1;
        while (hi - lo > 1)
        {
            int mid = (lo + hi) / 2;
            if (machTable[mid] <= mach)
                lo = mid;
            else
                hi = mid;
        }

        // Linearna interpolacija između machTable[lo] i machTable[hi]
        float t = (mach - machTable[lo]) / (machTable[hi] - machTable[lo]);
        return Mathf.Lerp(cdTable[lo], cdTable[hi], t);
    }

    /// <summary>
    /// Brzina zvuka u m/s pri zadanoj temperaturi (Kelvin).
    /// c = sqrt(γ · R · T), γ=1.4 za zrak, R=287.05 J/(kg·K).
    /// </summary>
    public static float SpeedOfSound(float temperatureKelvin)
    {
        return Mathf.Sqrt(1.4f * 287.05f * temperatureKelvin);
    }
}