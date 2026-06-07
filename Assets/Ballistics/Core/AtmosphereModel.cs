using UnityEngine;

public class AtmosphereModel
{
    private const float SEA_LEVEL_PRESSURE_PA = 101325f;
    private const float GAS_CONSTANT_DRY_AIR = 287.05f;
    private const float GAS_CONSTANT_WATER_VAPOR = 461.5f;

    // Zadani atmosferski uvjeti na mjestu ispaljivanja
    public float TemperatureCelsius { get; set; } = 15f;
    public float PressurePascal { get; set; } = SEA_LEVEL_PRESSURE_PA;
    public float HumidityPercent { get; set; } = 50f;

    public Vector3 Wind { get; set; } = Vector3.zero; //duni vjetre

    public float TemperatureKelvin => TemperatureCelsius + 273.15f;

    public float SpeedOfSound => DragModel.SpeedOfSound(TemperatureKelvin);


    private float GetWaterVaporPressure() //Magnus
    {
        float exp = (17.67f * TemperatureCelsius) / (TemperatureCelsius + 243.5f);
        float saturationPressure = 611.2f * Mathf.Exp(exp);
        return (HumidityPercent / 100f) * saturationPressure;
    }
    public float GetAirDensity()
    {
        float pv = GetWaterVaporPressure();
        float pd = PressurePascal - pv;

        return (pd / (GAS_CONSTANT_DRY_AIR * TemperatureKelvin))
             + (pv / (GAS_CONSTANT_WATER_VAPOR * TemperatureKelvin));
    }
}