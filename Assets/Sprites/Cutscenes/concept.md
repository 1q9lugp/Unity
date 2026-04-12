# OPERATION SHARA — Full Game Design Document & MCP Prompt Sequence

---

## OVERVIEW

| Property | Value |
|---|---|
| Engine | Unity (2D/3D hybrid) |
| Total runtime | ~16 min |
| Act 0 | Intro slideshow cutscene — 1 min |
| Act 1 | 2D space phase — 5 min |
| Act 2 | Transition cutscene — 30 sec |
| Act 3 | 3D FPS arena — 10 min |
| Visual style | Flat PNGs in 3D space, no pixelation |
| Tone | Absurdist cosmic horror-comedy |

---

## VISUAL IDENTITY

**The PNG Chaos Aesthetic**
Enemies and characters are flat 2D PNGs rendered in 3D space. No pixel filter. No retro shader. The humor comes from the contrast between the sincere lore and the fact that you are blasting badly cut-out JPEG lizards with a cosmic shotgun. Enemies look like someone printed a lizard image, cut it out with scissors, and stapled it to a stick. Slight white fringe on the edges is acceptable — even desirable.

**Color palette:**
- Space phase: Deep navy, cold purple, distant star whites
- FPS phase: Warm red sky, cracked grey concrete, sickly green lizard tones

**Music:**
- Act 0: Ambient choir / pleiadian synth (soft, slow)
- Act 1: Ambient space drone with occasional swell
- Transition: SILENCE → hard music drop
- Act 3: Heavy industrial metal, full tempo, no breaks

---

## ACT 0 — THE BRIEFING (Cutscene)

### Description
Slideshow-style intro. Black background. Lo-res images from Ashtar Command / Pleiadian source material fade in one at a time with text underneath. Looks like a late-90s PowerPoint presentation. Ambient synth music plays quietly throughout.

### Slides (8 total)

| # | Image | Text |
|---|---|---|
| 1 | Star field | "In the year of the Great Emergence..." |
| 2 | Lo-res reptilian illustration | "They came from beneath. From the crust of the Earth." |
| 3 | City in chaos (lo-res) | "They did not negotiate." |
| 4 | Starship Shara photo | "But others watched from beyond the stars." |
| 5 | Ashtar Sheran portrait | "Ashtar Sheran, Commander of the Galactic Federation Fleet." |
| 6 | Ptaah portrait | "Ptaah. First Officer. Pilot of the Shara." |
| 7 | Humans boarding ship | "The evacuation was offered. Humanity accepted." |
| 8 | Ship in space | "And so they searched for a new home." |

Each slide: 5 second hold, 1 second cross-fade. Music fades out at slide 8 → silence → scene loads.

---

## ACT 1 — THE WANDERING (2D Space Phase)

### Description
Top-down 2D space. The Starship Shara is a sprite. Parallax star scrolling background. Player navigates to 4 planets in sequence. Between each planet: Reptilian scout ships intercept and must be destroyed. On reaching orbit: a text transmission explains why the planet is unsuitable. After all 4 rejections, Ashtar receives a final transmission and the phase ends.

### Controls
- WASD or Arrow Keys: Move ship
- Mouse aim: Weapon direction
- Left click / Space: Fire photon beam

### Ship Combat
The Shara fires forward-aimed light beams automatically. Player controls direction. Enemy ships are flat PNG sprites. 2-3 hits to kill. They have simple patrol → attack behavior. No permadeath — Shara has a health bar, regenerates slowly between waves.

### Planet Sequence

| Planet | Interception | Rejection Message |
|---|---|---|
| Kepler-186f | 3 scout ships | "Oxygen too thin. Human lungs would fail within hours." |
| Proxima b | 5 scout ships, slightly faster | "Surface radiation is incompatible with human biology." |
| Tau Ceti e | 6 scouts + 1 mini-boss ship | "Atmospheric pressure would crush unshielded humans. Uninhabitable." |
| Gliese 667Cc | 8 scouts in formation | "Already colonized. We would not impose. We move on." |

After planet 4, a final text appears across the ship cockpit screen:

> *"Commander. We have visited every viable candidate in this sector."*
> *"The data is consistent. There is one planet — and only one — whose atmosphere, gravity, water, and memory belong to this species."*

Ashtar's response (player sees this, cannot skip):

> *"Set course for Earth."*

→ Scene transition begins.

---

## ACT 2 — TRANSITION CUTSCENE

### Phase 1 — The Discussion (2D, ~20 sec)
A static 2D scene. Interior of the Shara bridge. Two portrait images: Ashtar (left) and Ptaah (right). Dialogue appears below in a text box, one line at a time, advancing automatically.

```
PTAAH: "The coordinates are locked."
ASHTAR: "How many Reptilians are still on the surface?"
PTAAH: "All of them."
ASHTAR: "Good."
[pause 1.5 sec]
ASHTAR: "Then they will know we are coming."
```

Mid-sentence after "coming" — HARD CUT TO BLACK.

### Phase 2 — The Slam (Black screen, 1.5 sec silence)
Complete silence. Nothing. Let it breathe.

### Phase 3 — The Drop
Music kicks in exactly here. Heavy. No fade in — instant full volume.

On the music's first major downbeat:

```
RECLAIM
```

On the second drop:

```
THE EARTH
```

Text: white, bold, full-screen. No animation. No fade. Just SLAM and SLAM. Exactly like a metal music video title card.

After 2 seconds of the second slide: immediate cut to the FPS camera opening on the arena. Music continues without interruption.

---

## ACT 3 — RECLAIM THE EARTH (3D FPS Arena)

### Description
Single large open arena. Cracked city street surface. Red overcast sky. Ruined buildings as walls — the arena is bounded but wide. Hundreds of Reptilian PNGs are already present in the arena when the player spawns. No wave system. No spawning. They are just there. The player (Ashtar Sheran, first person) works through the crowd at their own pace.

At the center of the arena, stationary and massive: **The Mother Lizard**. She is an enormous PNG — towering, grotesque. She periodically launches slow projectiles outward and emits a reptilian screech. She does not move. She does not need to. She has a large health bar that appears on screen only when first damaged.

When the Mother Lizard dies: brief screen flash, music fades, white text on black:

```
THE EARTH IS RECLAIMED.
```

Game over. Credits roll (scrolling text, same ambient synth from Act 0 returns).

### Player — Ashtar Sheran
First person. You see your hands. No character model visible to player.

**Weapons:**

| Name | Description | Fire Rate | Damage |
|---|---|---|---|
| Photon Shotgun | Burst of 6 light pellets, wide spread | Medium | Medium per pellet, high at close range |
| Solar Ray (Ultimate) | Charged beam. Hold to charge (2 sec), release to fire. Screen flashes white-gold. Massive damage in a line. Long cooldown (20 sec). | Slow | Very High |

Scroll wheel or 1/2 to switch. Solar Ray is for crowd clearing and Mother Lizard damage phases.

### Enemies

**Type 1 — Grunt Lizard**
Standard enemy. A badly cut-out PNG of a bipedal reptilian. Walks toward you. Occasionally stops and spits a green projectile. Low health. Common. They bump into each other.

**Type 2 — Elite Lizard**
Larger PNG. Moves faster. Zigzags slightly. Higher health. Rare — about 1 per 15 grunts.

**The Mother Lizard**
Static. Massive PNG. Centered in arena. Launches slow homing orbs every 4 seconds in random directions. Very high health. No phases — just steadily reduce her HP.

### Enemy Death
When any lizard is killed: the PNG flips backward and falls flat. Optional: a small green splat VFX underneath. No ragdoll needed — the flat fall is funnier and fits the aesthetic.

### Arena Layout

```
[RUINED BUILDING WALL]   [RUINED BUILDING WALL]
         |                         |
         |   [ELITE] [GRUNT x40]   |
[WALL]   |                         |   [WALL]
         |      [MOTHER LIZARD]    |
         |   [GRUNT x30] [ELITE]   |
         |__________________________|
              ^
         [PLAYER SPAWN]
```

Player spawns at the far south end, facing north toward the Mother Lizard in the distance. The crowd is between them.

---

## SCENE STRUCTURE (Unity)

```
Assets/
├── Scenes/
│   ├── Act0_Briefing.unity
│   ├── Act1_Space.unity
│   ├── Act2_Transition.unity
│   └── Act3_Arena.unity
├── Scripts/
│   ├── Cutscene/
│   │   ├── SlideshowController.cs
│   │   ├── DialogueController.cs
│   │   └── TransitionController.cs
│   ├── Space/
│   │   ├── SharaShipController.cs
│   │   ├── EnemyShipAI.cs
│   │   ├── SpaceWeapon.cs
│   │   └── PlanetEncounterManager.cs
│   ├── FPS/
│   │   ├── PlayerController.cs
│   │   ├── PhotonShotgun.cs
│   │   ├── SolarRay.cs
│   │   ├── BillboardEnemy.cs
│   │   ├── GruntAI.cs
│   │   ├── EliteAI.cs
│   │   └── MotherLizardController.cs
│   └── Core/
│       ├── GameManager.cs
│       ├── SceneLoader.cs
│       └── AudioManager.cs
├── Sprites/
│   ├── Characters/ (Ashtar, Ptaah portraits)
│   ├── Ships/ (Shara, enemy scout ships)
│   ├── Enemies/ (Grunt PNG, Elite PNG, Mother PNG)
│   └── UI/
├── Audio/
│   ├── ambient_space.mp3
│   ├── pleiadian_synth.mp3
│   └── reclaim_drop.mp3
└── Materials/
    ├── StarfieldMaterial.mat
    └── BillboardMaterial.mat
```

---

# MCP PROMPT SEQUENCE

Feed these prompts one at a time. Wait for confirmation before proceeding to the next. Each prompt is self-contained and builds on the previous.

---

## PROMPT 1 — Project Structure

```
Create the following folder structure inside the Unity Assets folder:
- Assets/Scenes/
- Assets/Scripts/Cutscene/
- Assets/Scripts/Space/
- Assets/Scripts/FPS/
- Assets/Scripts/Core/
- Assets/Sprites/Characters/
- Assets/Sprites/Ships/
- Assets/Sprites/Enemies/
- Assets/Sprites/UI/
- Assets/Audio/
- Assets/Materials/
- Assets/Prefabs/

Then create 4 empty Unity scenes and save them:
- Assets/Scenes/Act0_Briefing.unity
- Assets/Scenes/Act1_Space.unity
- Assets/Scenes/Act2_Transition.unity
- Assets/Scenes/Act3_Arena.unity

Add all 4 scenes to the Build Settings in order (Act0 = index 0, Act1 = index 1, Act2 = index 2, Act3 = index 3).

Create a Core/GameManager.cs script that is a singleton, persists across scenes with DontDestroyOnLoad, and has a public static method LoadScene(int index) that calls SceneManager.LoadScene(index).

Create a Core/AudioManager.cs script that is also a singleton, persists across scenes, and has:
- public void PlayMusic(AudioClip clip, bool loop = true)
- public void StopMusic()
- public void SetVolume(float vol)
It should use a single AudioSource component attached to the same GameObject.
```

---

## PROMPT 2 — Act 0: Slideshow Cutscene Scene

```
Open the scene Act0_Briefing.unity.

Set the camera to orthographic mode. Set background color to pure black (0,0,0).

Create a Canvas (Screen Space - Overlay) with the following child GameObjects:
1. "SlideImage" — a UI Image component, anchored to fill the full screen, initially with no sprite and alpha 0.
2. "SlideText" — a UI Text (or TextMeshPro) component, anchored to bottom center, font size 28, white color, centered alignment, with a max width of 80% of screen width. Initially empty.
3. "MusicSource" — an empty GameObject with an AudioSource component, not set to play on awake.

Create a script Assets/Scripts/Cutscene/SlideshowController.cs with this behavior:
- Has a serializable struct Slide with fields: Sprite image, string text, float holdDuration (default 5f), float fadeDuration (default 1f)
- Has a public List<Slide> slides that can be filled in the Inspector
- On Start, begins playing through slides sequentially using a coroutine
- Each slide: fade in image (if not null) and text simultaneously over fadeDuration, hold for holdDuration, fade out over fadeDuration, then next slide
- After all slides complete, call GameManager.LoadScene(1) to go to Act1

Attach SlideshowController to a new empty GameObject called "SlideshowManager" in the scene.
```

---

## PROMPT 3 — Act 1: Space Scene Setup

```
Open the scene Act1_Space.unity.

Set the camera to orthographic, size 5, background pure black.

Create a parallax starfield background:
- Create a GameObject "Starfield" with a SpriteRenderer. Generate a 512x512 black texture with ~200 randomly placed 1-pixel white dots procedurally in a script. Attach a script that slowly scrolls the texture downward (simulating forward movement) by offsetting the material UV over time at speed 0.02.
- Create a second "StarfieldFar" layer with the same approach but slower (speed 0.008) and slightly dimmer.

Create a GameObject "SharaShip":
- Add a SpriteRenderer. Assign a placeholder white rectangle sprite for now (will be replaced with real art).
- Attach a Rigidbody2D set to Kinematic.
- Attach a CircleCollider2D.
- Attach a script Assets/Scripts/Space/SharaShipController.cs with:
  - WASD / Arrow key movement
  - Movement speed: 4 units/sec
  - Clamped to screen bounds
  - A public float maxHealth = 100f and float currentHealth
  - A public void TakeDamage(float amount) method

Create a UI Canvas with:
- "HealthBar" — a Slider UI element in the top-left, width 200, height 20, min 0, max 100, set to 100. Colored green fill.

Position SharaShip at the bottom center of the screen (0, -3.5).
```

---

## PROMPT 4 — Act 1: Space Weapon & Enemy Ships

```
In Act1_Space.unity:

Create a projectile prefab Assets/Prefabs/SharaBeam.prefab:
- SpriteRenderer with a small horizontal white capsule sprite (or thin rectangle)
- Rigidbody2D, Kinematic
- Script Assets/Scripts/Space/SpaceProjectile.cs: moves forward (up) at speed 10, destroys self after 3 seconds or on collision with tag "EnemyShip"
- BoxCollider2D as trigger
- Tag it "PlayerProjectile"

Update SharaShipController.cs to:
- Fire a SharaBeam prefab from the ship position every 0.3 seconds while the fire button (Space or Left Mouse) is held
- Reference to SharaBeam prefab assigned in Inspector

Create an enemy ship prefab Assets/Prefabs/EnemyScoutShip.prefab:
- SpriteRenderer with a small red rectangle sprite (placeholder)
- Tag "EnemyShip"
- Rigidbody2D Kinematic
- CircleCollider2D trigger
- Script Assets/Scripts/Space/EnemyShipAI.cs:
  - Spawns above the screen
  - Moves downward toward the player's position
  - When in range (y distance < 2), fires a projectile (small red circle sprite) downward toward player every 1.5 sec
  - If hit by PlayerProjectile: takes damage (3 hits to kill), flashes sprite red, destroys self
  - Enemy projectile: moves downward at speed 5, destroys on collision with SharaShip (calls TakeDamage(10f)) or after 4 sec

The enemy projectile should be a separate small prefab: Assets/Prefabs/EnemyBeam.prefab.
```

---

## PROMPT 5 — Act 1: Planet Encounter Manager

```
In Act1_Space.unity:

Create a script Assets/Scripts/Space/PlanetEncounterManager.cs and attach it to a new empty GameObject "EncounterManager".

This script manages the 4-planet sequence. It has a serializable struct PlanetEncounter with:
- string planetName
- int enemyCount
- bool hasMinisBoss
- string rejectionMessage

Pre-fill 4 encounters:
1. Kepler-186f, 3 enemies, no boss, "Oxygen too thin. Human lungs would fail within hours."
2. Proxima b, 5 enemies, no boss, "Surface radiation is incompatible with human biology."
3. Tau Ceti e, 6 enemies, has mini-boss (1 enemy with 2x health and size), "Atmospheric pressure would crush unshielded humans."
4. Gliese 667Cc, 8 enemies, no boss, "Already colonized. We would not impose."

Sequence logic:
1. Show UI text "[Planet Name] AHEAD" for 2 seconds
2. Spawn enemy wave (enemies appear from top in a spread formation)
3. Wait until all enemies are destroyed
4. Show UI panel with the rejection message text for 4 seconds
5. Move to next encounter
6. After all 4 encounters completed: show final message panel:
   "There is one planet whose atmosphere, gravity, and memory belong to this species."
   (hold 3 sec)
   Then: "Set course for Earth." (hold 2 sec)
   Then: call GameManager.LoadScene(2) to transition to Act 2.

Create the UI elements:
- "EncounterText" — large centered text for planet name announcements
- "MessagePanel" — dark semi-transparent panel with centered text for rejection messages
Both start inactive, activated by the manager as needed.
```

---

## PROMPT 6 — Act 2: Transition Cutscene Scene

```
Open the scene Act2_Transition.unity.

Set camera to orthographic, black background.

Create a Canvas with:
1. "Background" — full screen black UI Image
2. "AshtarPortrait" — UI Image, left half of screen, anchored left-center. Alpha 0 initially.
3. "PtaahPortrait" — UI Image, right half of screen, anchored right-center. Alpha 0 initially.
4. "DialogueBox" — UI panel at bottom, full width, height 100px, dark semi-transparent background
5. "DialogueText" — TextMeshPro inside DialogueBox, white, size 22, left-aligned with padding

Create a script Assets/Scripts/Cutscene/DialogueController.cs:
- Has a list of DialogueLine structs: string speaker, string line, float pauseAfter
- Pre-fill with:
  Line 1: "PTAAH", "The coordinates are locked.", 0.5f
  Line 2: "ASHTAR", "How many Reptilians are still on the surface?", 0.5f
  Line 3: "PTAAH", "All of them.", 1.5f
  Line 4: "ASHTAR", "Good.", 1.0f
  Line 5: "ASHTAR", "Then they will know we are coming.", 0f
- On Start: fade in both portraits over 0.5 sec, then begin dialogue sequence
- Each line: set DialogueText to "[SPEAKER]: [line]", wait pauseAfter seconds, then next line
- After last line completes: wait 0.3 sec, then begin transition

Transition after dialogue:
Phase 1 — Fade everything to black over 0.4 seconds
Phase 2 — Hold black for 1.5 seconds (silence)
Phase 3 — Trigger AudioManager.PlayMusic(reclaimDropClip, loop: true) at full volume instantly (no fade in)
Phase 4 — Show "RECLAIM" text (white, bold, 80pt, center screen) for exactly [beatDuration] seconds (expose beatDuration as a public float, default 1.2f so designer can tune to music)
Phase 5 — Replace with "THE EARTH" text (same style) for beatDuration seconds
Phase 6 — Fade to black over 0.2 sec, then call GameManager.LoadScene(3)

Attach DialogueController to an empty "CutsceneManager" GameObject.
Expose reclaimDropClip as a public AudioClip field in Inspector.
```

---

## PROMPT 7 — Act 3: Arena Scene Setup

```
Open the scene Act3_Arena.unity.

Set the main camera to perspective mode. FOV 75. Position at (0, 1.7, 0) facing north (rotation 0,0,0). The camera IS the player — no separate camera rig needed yet.

Build the arena geometry using Unity primitives (these will be replaced with real art later):
- Floor: a Plane scaled to (20, 1, 20), positioned at (0, 0, 0), tagged "Ground"
- North wall (behind Mother Lizard): a Cube scaled (24, 8, 1) at (0, 4, 10)
- South wall (behind player spawn): a Cube scaled (24, 8, 1) at (0, 4, -10)
- West wall: a Cube scaled (1, 8, 20) at (-11.5, 4, 0)
- East wall: a Cube scaled (1, 8, 20) at (11.5, 4, 0)
- Set all wall and floor materials to a flat grey material (no texture needed yet)

Set the ambient lighting:
- Skybox: remove it, set background to solid color RGB (80, 20, 20) — dark red sky
- Ambient light intensity: 0.6
- Add a Directional Light at rotation (50, -30, 0), intensity 0.8, warm orange-red color

Create a First Person Controller:
- Empty GameObject "Player" at position (0, 0, -8) — south spawn point
- Attach a CharacterController component: height 1.8, radius 0.4
- Attach a script Assets/Scripts/FPS/PlayerController.cs:
  - WASD movement, speed 6 units/sec
  - Mouse look: horizontal rotates Player Y axis, vertical rotates Camera X axis, clamped -80 to +80 degrees
  - No jump needed
  - Public float maxHealth = 100f, currentHealth
  - Public void TakeDamage(float amount)
- Nest the Main Camera as a child of Player at local position (0, 0.8, 0)

Create a HUD Canvas (Screen Space Overlay):
- HealthBar: Slider bottom-left, width 200, height 16, green fill, max 100
- UltimateChargeBar: Slider bottom-left above health, width 200, height 10, gold fill, label "SOLAR RAY"
- CrosshairDot: small 8x8 white circle image, anchored center screen
- MotherLizardHealthBar: Slider top-center, width 400, height 20, red fill, label "MOTHER LIZARD", initially hidden (SetActive false)
- WeaponLabel: bottom-right text, white, shows current weapon name
```

---

## PROMPT 8 — Act 3: Weapons

```
In Act3_Arena.unity, with the Player GameObject selected:

Create a child GameObject "WeaponHolder" at local position (0.3, -0.2, 0.5) — this is where weapon visuals attach.

Create script Assets/Scripts/FPS/PhotonShotgun.cs:
- On left click (or held): fire 6 raycasts in a cone spread (random within 8 degree radius) from camera center
- Each raycast has range 30 units
- If raycast hits a GameObject tagged "Enemy": call its TakeDamage(18f) method
- If hits "MotherLizard" tag: call its TakeDamage(8f) method
- Fire rate: 0.55 seconds between shots (semi-auto, not hold-to-spray)
- Play a screen flash (brief white vignette overlay, fade over 0.08 sec) on each shot
- Play a shotgun sound effect (assign in Inspector)

Create script Assets/Scripts/FPS/SolarRay.cs:
- Activated by right click or key 2
- Hold to charge: a charge bar on HUD fills over 2 seconds
- On release (if fully charged): fire a single long raycast down center screen, range 50 units
- Hits ALL enemies and the MotherLizard along its path (use RaycastAll)
- Damage: 120f to each target hit
- Screen effect: full screen flash white-gold for 0.15 sec on fire
- Cooldown: 20 seconds after firing (charge bar shows cooldown state in red during cooldown)
- If player releases before fully charged: cancel, no fire, reset charge bar
- Play a distinct charge sound and fire sound (assign in Inspector)

Create a PlayerWeaponManager.cs:
- Holds references to PhotonShotgun and SolarRay
- Key 1 or scroll up: activate Shotgun
- Key 2 or scroll down: activate SolarRay
- Default active weapon: Shotgun
- Updates WeaponLabel HUD text with current weapon name

Attach all three scripts to the Player GameObject.
Wire up the HUD UltimateChargeBar reference to SolarRay.
```

---

## PROMPT 9 — Act 3: Enemy Billboard System

```
In Act3_Arena.unity:

Create a base enemy prefab system. First, create a script Assets/Scripts/FPS/BillboardSprite.cs:
- On every Update: rotate this GameObject to always face the Main Camera on the Y axis only (Y-axis billboard, not full billboard — keeps the PNG upright)

Create the Grunt Lizard prefab Assets/Prefabs/GruntLizard.prefab:
- Empty root GameObject tagged "Enemy", layer "Enemy"
- Child GameObject "Sprite" with SpriteRenderer — assign a placeholder green rectangle sprite, scale (1.2, 2.0, 1)
- Attach BillboardSprite.cs to the root
- Attach a CapsuleCollider: height 2, radius 0.4
- Attach script Assets/Scripts/FPS/GruntAI.cs:
  - public float maxHealth = 40f
  - public void TakeDamage(float amount): reduce health, trigger hit flash (lerp SpriteRenderer color to red then back over 0.1 sec), if health <= 0 call Die()
  - Die(): play death animation (rotate sprite -90 degrees on Z over 0.3 sec so it falls flat), disable collider, destroy after 1 second
  - Movement: NavMeshAgent, speed 2.5, stopping distance 3
  - States: Idle → once player within 20 units, switch to Chase → move toward player
  - Attack: when within 5 units, stop and spit projectile at player every 2 seconds
  - Spit projectile: instantiate a small green sphere prefab (GruntSpit.prefab) that moves toward player position at speed 7, destroys on hit or after 4 sec, calls player.TakeDamage(8f) on hit

Create the Elite Lizard prefab Assets/Prefabs/EliteLizard.prefab:
- Same structure as Grunt but scale (1.6, 2.6, 1) — noticeably bigger
- Attach GruntAI.cs but override values: maxHealth 100f, speed 3.5, attack damage 15f, attack interval 1.4f
- Use a darker green or orange-tinted placeholder sprite

Bake a NavMesh for the arena floor in the scene.
```

---

## PROMPT 10 — Act 3: Mother Lizard

```
In Act3_Arena.unity:

Create the Mother Lizard GameObject at position (0, 0, 7) — center of arena, north side.

Structure:
- Root GameObject "MotherLizard" tagged "MotherLizard"
- Child "Sprite" GameObject with SpriteRenderer, scale (6, 9, 1) — she is massive
- Assign a placeholder red rectangle sprite (will be replaced with real art)
- Attach BillboardSprite.cs so she always faces the player
- Attach a BoxCollider: size (5, 8, 1)
- NO NavMeshAgent — she does not move

Create script Assets/Scripts/FPS/MotherLizardController.cs:
- public float maxHealth = 800f, currentHealth = 800f
- bool hasBeenDamaged = false
- public void TakeDamage(float amount):
  - Reduce currentHealth
  - If !hasBeenDamaged: set hasBeenDamaged = true, show MotherLizardHealthBar HUD element, update its max to maxHealth
  - Update MotherLizardHealthBar value
  - Flash sprite red briefly (same as grunt)
  - If currentHealth <= 0: Die()
- Die():
  - Stop all coroutines
  - Play death sequence: scale sprite down to 0 over 1 second (LeanTween or simple lerp)
  - Flash screen white
  - Wait 2 seconds
  - Load end screen (GameManager.LoadScene — create a new simple EndScene or just show a full-screen black Canvas with "THE EARTH IS RECLAIMED." in white text and ambient synth music)

Attack behavior — coroutine that runs every 4 seconds:
  - Launch 3 homing orbs in random outward directions from her position
  - Each orb: small red sphere prefab, moves at speed 3.5, homes toward player position slowly (lerp toward player, turn rate 1.5 deg/sec), calls player.TakeDamage(15f) on hit, destroys after 6 sec or on hit

Attach MotherLizardController to the MotherLizard root GameObject.
Wire up the MotherLizardHealthBar reference in the Inspector.
```

---

## PROMPT 11 — Act 3: Enemy Placement

```
In Act3_Arena.unity:

Place enemies manually in the scene using the following layout. Do not use a spawner — all enemies are pre-placed and present from the start.

Grunt Lizards — place 60 total GruntLizard prefab instances:
- 20 in a loose cluster around position range (-6 to 6, 0, -4 to 2) — front cluster between player spawn and Mother
- 20 in a loose cluster around position range (-7 to 7, 0, 2 to 6) — mid arena
- 20 scattered around the perimeter edges of the arena (near walls, x ±8, z -6 to 8)
Space them randomly within these ranges — they don't need to be in perfect formation, scattered is fine and more natural.

Elite Lizards — place 6 total EliteLizard prefab instances:
- 2 near position (-4, 0, 0) and (4, 0, 0) — flanking the center
- 2 near (-6, 0, 5) and (6, 0, 5) — guarding approaches to the Mother
- 2 near (0, 0, -3) and (0, 0, 1) — directly in the player's main path

Verify the NavMesh is baked and covers the full arena floor so all grunt movement works.

After placing: select all enemy instances and confirm their NavMeshAgent components are active and assigned.
```

---

## PROMPT 12 — Polish & Scene Flow

```
Final pass across all scenes:

1. In Act0_Briefing.unity:
   - Add an AudioSource to SlideshowManager. Assign a looping ambient music clip (label it pleiadian_ambient). Set it to play on Awake at volume 0.7.
   - The SlideshowController should call AudioManager.StopMusic() before calling LoadScene(1) if AudioManager exists.

2. In Act1_Space.unity:
   - Add AudioManager music call on Start: play a looping space ambient clip at volume 0.6.
   - Wire the HealthBar UI slider to SharaShipController's currentHealth — update it every frame.
   - When SharaShip currentHealth reaches 0: freeze the ship, show a "SHARA DAMAGED — RETREATING" message panel for 2 sec, then continue to next encounter anyway (ship cannot die, lore excuse: Pleiadian shields).

3. In Act2_Transition.unity:
   - Confirm the DialogueController has AudioManager.StopMusic() called right before the 1.5 sec silence phase.
   - Confirm AudioManager.PlayMusic(reclaimDropClip) is called AFTER the silence with no fade.

4. In Act3_Arena.unity:
   - Wire HUD HealthBar to PlayerController currentHealth — update every frame.
   - When PlayerController currentHealth reaches 0: don't kill the player. Clamp at 1 HP. Flash red vignette. Display "ASHTAR SHERAN CANNOT FALL" text for 1.5 seconds. This is a design choice — the player is an enlightened cosmic being. They cannot die. This adds to the absurdist tone.
   - Add a subtle screen shake on Solar Ray fire: use Camera.main transform, apply random offset of ±0.1 for 0.3 seconds then return.

5. Final Build Settings check:
   - Confirm scene order: Act0=0, Act1=1, Act2=2, Act3=3
   - Set Act0_Briefing as the startup scene
   - Confirm GameManager singleton is initialized from Act0 and persists through all scenes
```

---

## NOTES FOR REPLACING PLACEHOLDER ART

When real PNGs are ready, replace placeholder sprites in this order:
1. `SharaShip` SpriteRenderer → Starship Shara top-down sprite
2. `EnemyScoutShip` SpriteRenderer → Scout ship top-down sprite
3. `GruntLizard/Sprite` SpriteRenderer → Grunt lizard PNG (badly cropped, embrace the fringe)
4. `EliteLizard/Sprite` SpriteRenderer → Elite lizard PNG (same style, larger, angrier)
5. `MotherLizard/Sprite` SpriteRenderer → Mother lizard PNG (massive, grotesque, front-facing)
6. `SlideshowController` slides list → real lo-res Ashtar/Pleiadian source images
7. `DialogueController` portrait images → Ashtar and Ptaah portrait sprites

For enemy PNGs: import as Sprite, Texture Type = Sprite (2D), Filter Mode = Bilinear (NOT Point — we don't want pixelation). Set the background to transparent in whatever image editor you use. The slight white fringe from a rough cutout is intentional and acceptable.

---

## MUSIC SYNC GUIDE

For the Act 2 transition, the `beatDuration` float in DialogueController should be tuned to match your chosen track:
- Find the BPM of your chosen track
- One beat in seconds = 60 / BPM
- Set `beatDuration` to that value
- The "RECLAIM" slide appears on beat 1, "THE EARTH" slide appears on beat 2

Example: if the track drops at 140 BPM → beatDuration = 60/140 = 0.428 seconds. "RECLAIM" and "THE EARTH" will flash in rapid tight succession, which feels aggressive and correct.

If you want them on half-beats (slower, more dramatic): beatDuration = (60/BPM) * 2.
