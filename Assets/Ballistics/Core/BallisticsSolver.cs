using UnityEngine;

/// <summary>
/// Stanje projektila u jednom trenutku.
/// Pozicija u metrima (Unity koord: X=desno, Y=gore, Z=naprijed),
/// brzina u m/s.
/// </summary>
public struct ProjectileState
{
    public Vector3 Position;
    public Vector3 Velocity;

    public float Speed => Velocity.magnitude;
    public float KineticEnergy(float massKg) => 0.5f * massKg * Velocity.sqrMagnitude;
}

/// <summary>
/// RK4 integrator. Statička klasa — bez stanja, samo računa.
/// Koristi se: state = BallisticsSolver.Step(state, dt, projectile, atmosphere);
/// </summary>
public static class BallisticsSolver
{
    private static readonly Vector3 GRAVITY = new Vector3(0f, -9.80665f, 0f);

    /// <summary>
    /// Pomakni simulaciju za dt sekundi.
    /// </summary>
    public static ProjectileState Step(
        ProjectileState state,
        float dt,
        ProjectileData projectile,
        AtmosphereModel atmosphere)
    {
        // Pre-izračunaj atmosferske vrijednosti — iste su tijekom svih 4 RK4 sub-koraka
        float airDensity   = atmosphere.GetAirDensity();
        float speedOfSound = atmosphere.SpeedOfSound;

        // Četiri uzorka derivacije
        Derivative k1 = Evaluate(state,                          projectile, airDensity, speedOfSound);
        Derivative k2 = Evaluate(Apply(state, k1, dt * 0.5f),    projectile, airDensity, speedOfSound);
        Derivative k3 = Evaluate(Apply(state, k2, dt * 0.5f),    projectile, airDensity, speedOfSound);
        Derivative k4 = Evaluate(Apply(state, k3, dt),           projectile, airDensity, speedOfSound);

        // Ponderirani prosjek (1, 2, 2, 1) / 6
        Vector3 dPos = (k1.dPosition + 2f * k2.dPosition + 2f * k3.dPosition + k4.dPosition) / 6f;
        Vector3 dVel = (k1.dVelocity + 2f * k2.dVelocity + 2f * k3.dVelocity + k4.dVelocity) / 6f;

        return new ProjectileState
        {
            Position = state.Position + dPos * dt,
            Velocity = state.Velocity + dVel * dt
        };
    }

    private struct Derivative
    {
        public Vector3 dPosition; // = trenutna brzina
        public Vector3 dVelocity; // = trenutna akceleracija (gravitacija + drag)
    }

    private static Derivative Evaluate(
     ProjectileState state, ProjectileData projectile,
     float airDensity, float speedOfSound)
    {
        Vector3 dragAccel = Vector3.zero;
        float speed = state.Speed;

        if (speed > 0.01f)
        {
            float mach = speed / speedOfSound;

            float refCd = DragModel.LookupReferenceCd(mach, projectile.dragModel);

            // Komercijalni BC (lbs/in²) → SI (kg/m²)
            float bcMetric = projectile.ballisticCoefficient * 703.069f;

            // Direktna formula: a = (π · ρ · v² · Cd_ref) / (8 · BC_metric)
            float dragAccelMag = (Mathf.PI * airDensity * speed * speed * refCd) / (8f * bcMetric);

            dragAccel = -(state.Velocity / speed) * dragAccelMag;

        }

        return new Derivative
        {
            dPosition = state.Velocity,
            dVelocity = GRAVITY + dragAccel
        };
    }

    private static ProjectileState Apply(ProjectileState s, Derivative d, float dt)
    {
        return new ProjectileState
        {
            Position = s.Position + d.dPosition * dt,
            Velocity = s.Velocity + d.dVelocity * dt
        };
    }
}