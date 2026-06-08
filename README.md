# Ballistics Framework for Unity

A modular, physics-based **external ballistics** framework for Unity, written in C#. It simulates realistic projectile trajectories — accounting for Mach-dependent aerodynamic drag, gravity, atmospheric conditions, and wind — and is designed to be dropped into milsim-style games where bullet drop, time of flight and wind drift matter.

Developed as a bachelor's thesis at the Faculty of Electrical Engineering, Computer Science and Information Technology Osijek (FERIT), University of Osijek.

---

## Features

- **G1 / G7 drag models** — Mach-dependent reference drag coefficients via table lookup with linear interpolation, the same standardized approach used by real ballistic calculators.
- **RK4 integrator** — 4th-order Runge-Kutta integration for accurate, deterministic trajectories at large time steps.
- **Moist-air atmosphere** — air density computed from temperature, pressure and relative humidity (ideal-gas law + Magnus formula); temperature-dependent speed of sound.
- **Wind** — drag is computed against the projectile's velocity *relative to the air*, so crosswind produces lateral drift and head/tailwind changes drop and retained velocity.
- **Data-driven projectiles** — every ammunition type is a `ScriptableObject` asset; add new rounds without touching code.
- **Impact kinetic energy** — exposed at the point of impact, the natural handoff to a terminal-ballistics / damage system.
- **Engine-agnostic core** — the drag, atmosphere and solver modules are plain C# (no `MonoBehaviour`), so they can be unit-tested or run server-side for authoritative hit validation.
- **Deterministic range test** — a built-in harness fires horizontally and logs drop and velocity at fixed distances, independent of the visual demo.
- **Visualization** — per-ammo colored trajectory trails and impact markers.

---

## Requirements

- Unity **6.4** (6000.4.x) or newer
- **Universal Render Pipeline (URP)**
- **Input System** package (used by the demo scene)

---

## Installation

Copy the `BallisticsFramework` folder into your project's `Assets/` directory, or add this repository as a Git submodule under `Assets/`.

```
Assets/
└── BallisticsFramework/
    ├── ProjectileData.cs
    ├── DragModel.cs
    ├── AtmosphereModel.cs
    ├── BallisticsSolver.cs
    └── BallisticsManager.cs
```

---

## Architecture

The framework follows a clear separation of concerns. Three of the five modules are pure C# and have **no dependency on Unity**.

| Module | Kind | Responsibility |
| --- | --- | --- |
| `ProjectileData` | ScriptableObject | Per-ammo physical parameters (mass, diameter, muzzle velocity, drag model, ballistic coefficient, trail color). |
| `DragModel` | static C# | G1/G7 reference-Cd tables; Mach-number lookup + interpolation; speed of sound from temperature. |
| `AtmosphereModel` | C# | Moist-air density, speed of sound, and the wind vector for a shot. |
| `BallisticsSolver` | static C# | RK4 integration of the trajectory (drag + gravity using relative velocity); kinetic energy. |
| `BallisticsManager` | MonoBehaviour | Per-shot orchestration, `FixedUpdate` stepping at 100 Hz, raycast hit detection, visualization. |

**Data flow per step:** the projectile speed relative to the air and the speed of sound give the Mach number → `DragModel` returns the reference Cd → with the ballistic coefficient and air density, `BallisticsSolver` computes drag + gravity acceleration and advances the state with RK4. `BallisticsManager` runs this for every active projectile each physics tick.

---

## Quick start

### Using the Unity component

1. Create a projectile asset: **Assets → Create → Ballistics → Projectile Data**, then fill in mass, diameter, muzzle velocity, drag model (G1/G7) and ballistic coefficient.
2. Add a `BallisticsManager` to a GameObject in your scene and set temperature, pressure, humidity and wind in the Inspector.
3. Fire a shot from your weapon script:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    public BallisticsManager ballistics;
    public ProjectileData ammo;
    public Camera cam;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            ballistics.Fire(ammo, cam.transform.position, cam.transform.forward);
    }
}
```

### Using the physics core directly (no Unity scene)

Because the core is engine-agnostic, you can integrate a trajectory yourself — useful for tests or server-side validation:

```csharp
var atmosphere = new AtmosphereModel
{
    TemperatureCelsius = 15f,
    PressurePascal     = 101325f,
    HumidityPercent    = 50f,
    Wind               = new Vector3(5f, 0f, 0f) // 5 m/s crosswind (+X)
};

var state = new ProjectileState
{
    Position = Vector3.zero,
    Velocity = Vector3.forward * ammo.muzzleVelocityMs
};

const float dt = 0.01f; // 100 Hz
for (int i = 0; i < 500; i++)
    state = BallisticsSolver.Step(state, dt, ammo, atmosphere);

Debug.Log($"Speed: {state.Speed:F1} m/s, KE: {state.KineticEnergy(ammo.massKilograms):F0} J");
```

---

## Creating a new projectile

A new round is just a new `ProjectileData` asset — no code changes, no recompilation. Fill in:

| Field | Meaning |
| --- | --- |
| `massKilograms` | Bullet mass (kg). Used for kinetic energy. |
| `diameterMetres` | Bullet diameter / caliber (m). |
| `muzzleVelocityMs` | Initial speed (m/s). |
| `dragModel` | `G1` (flat-base) or `G7` (boat-tail; recommended for modern long-range rounds). |
| `ballisticCoefficient` | Commercial BC in lb/in² (converted to SI internally). |
| `trailColor` | Color of the trajectory trail and impact marker. |

Representative presets used during development:

| Round | Mass (kg) | Diameter (m) | Muzzle vel. (m/s) | Drag model | BC |
| --- | --- | --- | --- | --- | --- |
| 5.56×45 mm M855 | 0.004 | 0.00556 | 940 | G7 | 0.151 |
| 7.62×51 mm M80 | 0.0098 | 0.00782 | 853 | G7 | 0.200 |
| .338 Lapua Magnum | 0.0162 | 0.00859 | 905 | G7 | 0.322 |

> Always verify BC and muzzle velocity against the specific load you are modelling; these are representative figures.

---

## Atmosphere & wind

Environmental conditions are set per shot and held constant for that projectile's flight.

| Field | Meaning |
| --- | --- |
| `TemperatureCelsius` | Air temperature (°C). Drives density and speed of sound. |
| `PressurePascal` | Local pressure (Pa); sea-level standard is 101325. |
| `HumidityPercent` | Relative humidity (0–100). |
| `Wind` | Wind vector in m/s (world space). |

**Wind vector convention** (world axes: X = right, Y = up, Z = downrange):

- **X** — crosswind. `+X` pushes the bullet to the right (lateral drift).
- **Z** — head/tailwind. `+Z` is a tailwind (less drag); `−Z` is a headwind (more drag, more drop).
- **Y** — vertical wind; usually `0`.

Wind is absolute (world space), so whether a given wind acts as crosswind or headwind depends on the firing direction — exactly as in reality.

---

## How it works

The drag acceleration is derived directly from the commercial ballistic coefficient, so the cross-sectional area and mass cancel out of the trajectory math:

```
a_drag = (π · ρ · v_rel² · Cd_ref) / (8 · BC_SI)
```

where `ρ` is air density, `v_rel` is the speed relative to the air, `Cd_ref` is the reference drag coefficient looked up from the G1/G7 table at the current Mach number, and `BC_SI` is the ballistic coefficient converted to kg/m² (commercial BC × 703.069). Gravity uses the standard value 9.80665 m/s². The full acceleration (gravity + drag) is integrated with RK4.

---

## Validation

The physics core is exercised by a deterministic range test (`BallisticsRangeTest`) that fires horizontally and records drop and velocity at 100 / 300 / 500 m. Trajectories were compared against reference values from the [JBM Ballistics](https://www.jbmballistics.com/) calculator; retained velocity stays within a few percent out to 500 m for the test loads.

---

## Demo

A first-person demo scene shows the framework in use: targets at 100, 300 and 500 m, selectable ammunition types, ADS zoom, and on-impact readout of range, drop, velocity and kinetic energy, with colored trails per round.

---

## Roadmap

Not yet implemented; identified as future work:

- **Performance** — parallelize the per-projectile step with the Unity **Job System** + **Burst** compiler and add **object pooling** for large numbers of simultaneous projectiles. (Current implementation manages active projectiles with a simple list, which is sufficient for the demo.)
- **Extended external ballistics** — Coriolis and Eötvös effects, spin drift (gyroscopic).
- **Terminal ballistics** — penetration, fragmentation and armor interaction, consuming the impact kinetic energy the framework already exposes.
- **Adaptive time step** in the transonic region where the drag coefficient changes fastest.

---


## Author

**Filip Vlaisavljević** — Faculty of Electrical Engineering, Computer Science and Information Technology Osijek (FERIT), University of Osijek.

Developed as part of the bachelor's thesis *"Modeliranje i simulacija balističkih putanja u Unity okruženju"* ("Modeling and simulation of ballistic trajectories in the Unity environment").
