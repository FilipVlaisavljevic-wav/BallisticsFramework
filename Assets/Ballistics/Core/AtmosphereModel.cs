using UnityEngine;

/// <summary>
/// Model gustoće vlažnog zraka i brzine zvuka.
/// Gustoću računa iz zadanog tlaka, temperature i relativne vlažnosti
/// pomoću jednadžbe stanja idealnog plina (parcijalni tlakovi suhog zraka i vodene pare).
/// </summary>
public class AtmosphereModel
{
    private const float SEA_LEVEL_PRESSURE_PA = 101325f;
    private const float GAS_CONSTANT_DRY_AIR = 287.05f;
    private const float GAS_CONSTANT_WATER_VAPOR = 461.5f;

    // Zadani atmosferski uvjeti na mjestu ispaljivanja
    public float TemperatureCelsius { get; set; } = 15f;
    public float PressurePascal { get; set; } = SEA_LEVEL_PRESSURE_PA;
    public float HumidityPercent { get; set; } = 50f;

    public float TemperatureKelvin => TemperatureCelsius + 273.15f;

    public float SpeedOfSound => DragModel.SpeedOfSound(TemperatureKelvin);

    /// <summary>
    /// Parcijalni tlak vodene pare (Magnusova formula).
    /// </summary>
    private float GetWaterVaporPressure()
    {
        float exp = (17.67f * TemperatureCelsius) / (TemperatureCelsius + 243.5f);
        float saturationPressure = 611.2f * Mathf.Exp(exp);
        return (HumidityPercent / 100f) * saturationPressure;
    }

    /// <summary>
    /// Gustoća vlažnog zraka u kg/m³.
    /// ρ = p_d/(R_d·T) + p_v/(R_v·T)
    /// </summary>
    public float GetAirDensity()
    {
        float pv = GetWaterVaporPressure();
        float pd = PressurePascal - pv;

        return (pd / (GAS_CONSTANT_DRY_AIR * TemperatureKelvin))
             + (pv / (GAS_CONSTANT_WATER_VAPOR * TemperatureKelvin));
    }
}