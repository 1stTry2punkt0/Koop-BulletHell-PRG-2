# Koop-BulletHell-PRG-2 - school project

## Welcome to "Koop-BulletHell-PRG-2" (Working Tile)!<br>
Kooop-BulletHell-PRG-2 us a two player top-down online bullet-hell game developed as part of a school project.<br>
The game is built in Unity and uses FishNet to handle online multiplayer functionality.<br>

The project features a dynamic enemy wave system, where each wave can be individually customized.<br>
A variety of enemy types, including a challenging boss enemy, ensure that every wave feels unique and engaging.<br>

As player progress, the game gradually increases in difficulty through lower enemy spawn intervals, causing enemies to spawn faster,<br>
as well as time-limited boss challenges. This encourages teamwork and coordination.<br>
Players defeat enemies to collect experience points, level up and gain stat boosts and upgrades.<br>
Each enemy type has its own unique fighting style, keeping combat freah and challenging.<br>

By defeating enemies, players earn points and can compete for a spot in the highscore leaderboard.<br>

### TEAM:
Philon Hauk<br>
Lucas Pietruschka

**PROJECT CREATED: January 7, 2026**<br>
**PROJECT END: January 23, 2026**

### PROJECT TASK:
The goal of this project is the development of a playable two-player top-down arcade bullet-hell game.<br>
The game must function as an online multiplayer experience (host + client) and meet the technical requirements listed below.<br>

**1. Multiplayer Foundation**
  - FishNet is correctly set up in the Unity project
  - A host/ client connection is possible
  - Two instances can connect reliable
  - The NetworkManager is correctly configured<br>
  **2. Player Control & Synchronization**
  - Top-down movement
  - Players are implemented as NetworkObjects with NetworkBehaviour
  - Ownership is set correctly (only the local player is controllable)
  - Player movement is synchronized in a server-authoritive manner
  - At least one SyncVar<br>
  **3. Shooting & Bullet-hell mechanics**
  - Network-enabled projectile system
  - At least two different bullet patterns
  - Projectiles are synchronized correctly 
  - Fire rate / cooldown system implemented
  - Hit detection and damage work correctly on both clients<br>
  **4. Enemy, Wave, or Boss System**
  - At least two different enemy types
  - Enemies are spawned server-side
  -Either:
      - a boss enemy or
      - a clearly recognizable wave structure
  - Enemy and boss bullets are consistently visible on both clients<br>
  **5. Health , Damage & Game Flow**
  - Players have a health or life system
  - Damage and hits are synchronzed correctly
  - Clear game flow: Start -> Gameplay -> Game Over / Victory -> Endscreen or Restart<br>
  **6. HUD & Scoring**
  - HUD includes at least:
      - HP / lives
      - Score
  - Points are awarded for kills and / or survival time
  - Score is synchronized and displayed correctly
  - Highscore list implemented using PHP & SQL (server-side)<br>
  **7. Additional Features**<br>
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
  - [ServerRpc] <br>
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
  - collectedLevelUps (LootManager)<br>
**3. Bullet logic**
    - A specific player or enemy sends the required bullet information to the projectile spawner, which then spawns the bullet on the server for that particular player or enemy.
    - Hit detection is handled by the bullet itself, using a SphereCast filtered by a LayerMask that is set by the Projectile Spawner when the bullet is spawned. Depending on what the bullet hits,
      it triggers the appropriate logic on the target. If it hits nothing, the bullet is despawned.<br>
**4. Enemy logic**
  - Enemies use the NavMesh Agent to navigate around obstacles and the PlayerTracher script to identify the nearest target. If a player is dead, enemies stop targeting them and switch to the remaining player.
  - Each enemy type has a unique attack range. Once a player is within the range the enemy begins its attack behavior.
      - The charged attack enemy displays a scaled cone as visual indicator, waits for a short delay and then performs a stronger attack, dealing more damage.
      - The meele enemy attacks when positioned directly in front of the player.
      - The ranged enemy uses the same projectile spawner as the player, firing its own unique attack when the player is within range.
      - The mini boss ranged enemy: a slightly stronger version of the regular ranged enemy, featuring a more complex bullet pattern.
      - The boss enemy has four distinct ranged attacks and randomly selects one of them when player is in range.<br>
  - Enemy spawning is carefully controlled:
      - Enemies only spawn on the NavMesh
      - Avoid obstacles
      - Stay out of both players camera view
      - Spawn intervals are adjustable through the WaveController
  - Enemy Range, Damage, Hp and more is adjustable through scritable objects<br>

  ### Instructions for starting the host and client
  

  ### Description of data persistence
  Highscore are stored in a database using PHP and SQL

  ### Overview of implemented bonus 
  - Combined wave system with boss enemies
  - Added 2 additional enemy types 
  - Implemented level-ups and power-ups
  - Created additional and more complex bullet patterns for the boss enemy and players.
  - Developed a fully adjustable wave system, allowing control over enemy spawns with weighted enimies, configurable spawn intervals, wave duration settings, and more
  - Added timed or normal boss waves, enabling the boss to be defeated either within a set time limit or with unlimited time based solely on its health.
  - Fully animated player and enemies, with animations synchronized across all clients.

### Known bugs and limitations
