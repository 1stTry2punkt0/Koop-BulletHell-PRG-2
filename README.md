# Koop-BulletHell-PRG-2 - school project

## Welcome to "Koop-BulletHell-PRG-2" (Working Tile)!
Kooop-BulletHell-PRG-2 us a two player top-down online bullet-hell game developed as part of a school project.
The game is built in Unity and uses FishNet to handle online multiplayer functionality.

The project features a dynamic enemy wave system, where each wave can be individually customized.
A variety of enemy types, including a challenging boss enemy, ensure that every wave feels unique and engaging.

As player progress, the game gradually increases in difficulty through lower enemy spawn intervals, causing enemies to spawn faster,
as well as time-limited boss challenges. This encourages teamwork and coordination. 
Players defeat enemies to collect experience points, level up and gain stat boosts and upgrades. 
Each enemy type has its own unique fighting style, keeping combat freah and challenging.

By defeating enemies, players earn points and can compete for a spot in the highscore leaderboard.

### TEAM:
Philon Hauk 
Lucas Pietruschka

**PROJECT CREATED: January 7, 2026**
**PROJECT END: January 23, 2026**

### PROJECT TASK:
The goal of this project is the development of a playable two-player top-down arcade bullet-hell game.
The game must function as an online multiplayer experience (host + client) and meet the technical requirements listed below.

**1. Multiplayer Foundation**
  - FishNet is correctly set up in the Unity project
  - A host/ client connection is possible
  - Two instances can connect reliable
  - The NetworkManager is correctly configured
**2. Player Control & Synchronization**
  - Top-down movement
  - Players are implemented as NetworkObjects with NetworkBehaviour
  - Ownership is set correctly (only the local player is controllable)
  - Player movement is synchronized in a server-authoritive manner
  - At least one SyncVar 
**3. Shooting & Bullet-hell mechanics**
  - Network-enabled projectile system
  - At least two different bullet patterns
  - Projectiles are synchronized correctly 
  - Fire rate / cooldown system implemented
  - Hit detection and damage work correctly on both clients 
**4. Enemy, Wave, or Boss System**
  - At least two different enemy types
  - Enemies are spawned server-side
  -Either:
      - a boss enemy or
      - a clearly recognizable wave structure
  - Enemy and boss bullets are consistently visible on both clients
**5. Health , Damage & Game Flow**
  - Players have a health or life system
  - Damage and hits are synchronzed correctly
  - Clear game flow: Start -> Gameplay -> Game Over / Victory -> Endscreen or Restart
**6. HUD & Scoring**
  - HUD includes at least:
      - HP / lives
      - Score
  - Points are awarded for kills and / or survival time
  - Score is synchronized and displayed correctly
  - Highscore list implemented using PHP & SQL (server-side)
**7. Additional Features**
**8. Readme must include**
  -  Short description of the game
  -  Instructions for starting the host and client
  -  Technical overview
      - Used RPCs
      - Used SyncVars
      - Bullet logic 
      - Enemy logic
  - Description of data persistence
      - e.g., PHP/SQL, JSON, PlayerPrefs
  - Overview of implemented bonus features
  - Known bugs or limitations

### Technical Overview 
**1. Used RPCs**
  - [ObserverRpc]
  - [TargetRpc]
  - [ServerRpc]
**2. Used SyncVars**
  - currentHealth (PlayerActions)
  - IsAlive (PlayerActions)
  - score (PlayerActions)
  - syncSpeed (PlayerMovement)
  - playerName (PlayerMovement)
  - syncRotation (PlayerMovement)
  - isMoving (PlayerMovement)
  - reamainingWaveTime (WaveController)
  - currentWave (WaveController)
  - betweenWaveTime (WaveController)
  - gameOver (WaveController)
  - bossUsesTimer (WaveController)
  - isBossWave (WaveController)
  - bossAlive (WaveController)
  - bossName (WaveController)
  - playerEXP (LootManager)
  - collectedLevelUps (LootManager)
**3. Bullet logic**
    - A specific player or enemy sends the required bullet information to the projectile spawner, which then spawns the excat bullet pattern on the server for that particular player or enemy.
    - Hit detection is handled by the bullet itself, using a SphereCast filtered by a LayerMask that is set by the Projectile Spawner when the bullet is spawned. Depending on what the bullet hits,
      it triggers the appropriate logic on the target. If it hits nothing, the bullet is despawned.
**4. Enemy logic**
  - Enemies use the NavMesh Agent and the PlayerTracher script to identify the nearest target. If a player is dead, enemies stop targeting them and switch to the remaining player.
  - Each enemy has a unique attack range. Once a player is within the range the enemy begins its attack behavior.
      - The charged attack enemy displays a scaled cone as visual indicator, waits for a short delay and then performs a stronger attack, dealing more damage.
      - The meele enemy attacks when positioned directly in front of the player.
      - The ranged enemy uses the same projectile spawner as the player, firing its own unique attack when the player is within range.
      - The boss enemy has four distinct ranged attacks and randomly selects one of them when player is in range.
  - Enemy spawning is carefully controlled:
      - Enemies only spawn on the NavMesh
      - Avoid obstacles
      - Stay out of both players camera view
      - Spawn intervals are adjustable through the WaveController
  - Enemy Range, Damage, Hp and more is adjustable through scritable objects

  ### Overview of implemented bonus 
  - Combined wave system with a boss enemy 
  - Added 2 additional enemy types 
  - Implemented level-ups and power-ups
  - Created additional and more complex bullet patterns for the boss enemy
  - Developed a fully adjustable wave system, allowing control over enemy spawns with weighted enimies, configurable spawn intervals, wave duration settings, and more
  - Added timed or normal boss waves, enabling the boss to be defeated either within a set time limit or with unlimited time based solely on its health.
  - Fully animated player and enemies, with animations synchronized across all clients.


