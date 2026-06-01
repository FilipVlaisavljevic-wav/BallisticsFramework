using UnityEngine;

/// <summary>
/// ISA (International Standard Atmosphere) model.
/// Računa gustoću zraka i brzinu zvuka iz temperature, tlaka i vlažnosti.
/// </summary>
public class AtmosphereModel
{
    private const float SEA_LEVEL_PRESSURE_PA = 101325f;
    private const float SEA_LEVEL_TEMP_KELVIN = 288.15f;
    private const float TEMP_LAPSE_RATE = 0.0065f;
    private const float GAS_CONSTANT_DRY_AIR = 287.05f;
    private const float GAS_CONSTANT_WATER_VAPOR = 461.5f;
    private const float GRAVITY = 9.80665f;
    private const float MOLAR_MASS_AIR = 0.0289644f;
    private const float UNIVERSAL_GAS_CONSTANT = 8.31446f;

    // Trenutni atmosferski uvjeti
    public float TemperatureCelsius { get; set; } = 15f;
    public float PressurePascal { get; set; } = SEA_LEVEL_PRESSURE_PA;
    public float HumidityPercent { get; set; } = 50f;
    public float AltitudeMetres { get; set; } = 0f;

    public float TemperatureKelvin => TemperatureCelsius + 273.15f;

    public float SpeedOfSound => DragModel.SpeedOfSound(TemperatureKelvin);

    /// <summary>
    /// Tlak na nadmorskoj visini (barometrijska formula, troposfera < 11 km).
    /// </summary>
    public float GetPressureAtAltitude(float altitudeMetres)
    {
        return SEA_LEVEL_PRESSURE_PA * Mathf.Pow(
            1f - (TEMP_LAPSE_RATE * altitudeMetres) / SEA_LEVEL_TEMP_KELVIN,
            (GRAVITY * MOLAR_MASS_AIR) / (UNIVERSAL_GAS_CONSTANT * TEMP_LAPSE_RATE)
        );
    }

    /// <summary>
    /// Parcijalni tlak vodene pare (Magnus formula).
    /// </summary>
    private float GetWaterVaporPressure()
    {
        float e0 = 611.2f;
        float exp = (17.67f * TemperatureCelsius) / (TemperatureCelsius + 243.5f);
        float saturationPressure = e0 * Mathf.Exp(exp);
        return (HumidityPercent / 100f) * saturationPressure;
    }

    /// <summary>
    /// Gustoća vlažnog zraka u kg/m³.
    /// To je ρ koji ide u jednadžbu otpora: F_d = 0.5 * ρ * v² * Cd * A
    /// </summary>
    public float GetAirDensity()
    {
        float pressure = AltitudeMetres > 0f ? GetPressureAtAltitude(AltitudeMetres) : PressurePascal;
        float pv = GetWaterVaporPressure();
        float pd = pressure - pv;

        return (pd / (GAS_CONSTANT_DRY_AIR * TemperatureKelvin))
             + (pv / (GAS_CONSTANT_WATER_VAPOR * TemperatureKelvin));
    }
}