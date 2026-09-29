# Last Zone - consolidated scripts (Unity + Netcode for GameObjects)

Copy the `Assets/_Project` folder into your Unity project. 26 scripts.
Requirements: Unity 2021.3+ / Unity 6, Netcode for GameObjects (com.unity.netcode.gameobjects, tested against 1.x API),
Unity Transport, UGUI. Player Settings > Active Input Handling = "Input Manager (Old)" or "Both".

## Build Settings
Scenes in order: Menu, Island (add Boot first if you use it).

## Menu scene
- ONE NetworkManager + UnityTransport (Scene Management on, Player Prefab EMPTY, Default Network Prefabs List containing Player, Bot, Pickup).
  Do NOT put a second NetworkManager in Island.
- Bootstrap object. Canvas: MenuScreen (assign lobbyScreen) + LobbyScreen (assign panels + InputField).
  Play button -> MenuScreen.OnPlayClicked. Host/Join/Back -> LobbyScreen.

## Island scene (remove the default Main Camera / AudioListener)
Each of these needs a NetworkObject component: GameManager, SpawnManager, ZoneController.
- SpawnManager: spawnPoints[], playerPrefab, botPrefab, botCount.
- ZoneController: ZoneSchedule asset, mapRadius.
- Canvas with GameHUD (assign zoneController; sliders Max Value = 1). Optional DebugHUD (F3 toggles).

## Player prefab
NetworkObject, CharacterController, ClientNetworkTransform (NOT NetworkTransform), PlayerController, PlayerHealth, PlayerInput.
Child "Camera": Camera + AudioListener + PlayerCamera.
Child "Weapon": Weapon (assign WeaponDef, muzzle, optional LineRenderer). Weapon is NOT a separate network prefab.
Optional head collider on a child named "Head" for headshots.

## Bot prefab
NetworkObject, CharacterController, NetworkTransform (server authoritative), PlayerHealth, BotController, child Weapon.
No PlayerController / PlayerInput / PlayerCamera / ClientNetworkTransform.

## Pickup prefab
NetworkObject + Collider (Is Trigger) + Pickup.

## Controls
WASD move, Shift sprint, C crouch, Z prone, Space jump, RMB aim, LMB fire, R reload, Esc frees cursor, F3 debug.

## What was fixed vs the original chat output
Compile errors: DieServerRpc signature; Weapon.InvokeRpc (doesn't exist); BotController calling private FireServerRpc;
PlayerInput missing `using ...Weapons`; GameHUD/DebugHUD missing `using LastZone.Core`; SpawnAsPlayer -> SpawnAsPlayerObject;
DebugHUD used a non-existent MessageSender.CurrentRtt; AuthService/ProfileService/MatchmakingService were undefined (stubs added).
Runtime bugs: health/armor/ammo/zone/match state were not networked (now NetworkVariables); ServerRpc used for server-to-server damage
(now plain server methods); no position sync (ClientNetworkTransform); jump/gravity did nothing; movement ignored camera yaw;
every player's camera stayed active; camera followed itself; bot weapons read the host's mouse; host was ignored by bots;
Pickup looked for PlayerHealth on the pickup itself and nothing triggered it; zone stage 0 never shrank, radius jumped, and it
crashed with IndexOutOfRange after the last stage; HUD tried to reference a not-yet-spawned player; DebugHUD disabled itself
permanently; MenuScreen/LobbyScreen loaded Island with SceneManager (bypasses NGO) - host now uses NetworkManager.SceneManager;
GameManager used DontDestroyOnLoad/Destroy on a NetworkObject; Bootstrap reloaded Menu; duplicate NetworkManager in Island;
CompareTag("Head") throws if the tag doesn't exist (now name-based).

## Known limitations
Client-trusted aim direction (fine for prototype); no NavMesh for bots; dead players aren't despawned/spectating;
kills/damage stats and ResultsScreen aren't wired; weapon spread fields and DamageModel are unused; shotgun/weapon switching not implemented.
