# Player Login E2E Test Plan

Based on the analysis of the `PlayerLogInHandler.cs` and related components (including packet parsing, repository interfaces, waiting queue management, and login command execution), here is a comprehensive list of end-to-end (e2e) testing use cases for the login flow in the NeoServer application. The use cases cover happy path scenarios, edge cases, error conditions, security considerations, integration points, user type variations, network conditions, and failure modes. Each use case includes preconditions, steps, expected outcomes, and relevant assertions/validations.

## 1. Happy Path: Successful Login for Returning Player
**Preconditions:**
- Server is in Open state.
- Valid account exists in database with correct password.
- Character exists under the account.
- No IP bans.
- Account not banned.
- No other characters from the account are online (or account allows multiple).
- Waiting queue allows login.
- Player location is valid.
- Database and game server are operational.

**Steps:**
1. Client sends login packet with valid account, password, character name, correct challenge timestamp/number, supported version, and RSA-encrypted data.
2. Handler verifies packet, checks IP ban (none), validates online status (not online), retrieves player record, checks waiting queue (allowed), sends OTC features if applicable, executes login command.

**Expected Outcomes:**
- Player is loaded, added to game, placed on map, joins channels, online status updated in DB.
- Connection remains open; no disconnect packet sent.

**Assertions/Validations:**
- Verify player appears in game world at correct location.
- Check DB: online status = true, last login updated.
- Assert no disconnect packet received.
- Validate VIP list loaded, channels joined, global events triggered if record broken.

## 2. Happy Path: Successful Login with OTCv8 Client
**Preconditions:**
- Same as Use Case 1, plus client is OTCv8 with valid version.

**Steps:**
1. Client sends packet with OtcV8Version > 0.
2. Handler processes as in Use Case 1, additionally sending FeaturesPacket and OpcodeMessagePacket.

**Expected Outcomes:**
- Login succeeds with OTC features enabled.

**Assertions/Validations:**
- Verify FeaturesPacket sent with GameEnvironmentEffect, GameExtendedOpcode, GameExtendedClientPing, GameItemTooltip flags matching server config values.
- Confirm OpcodeMessagePacket sent.

## 3. Happy Path: Successful Login with OTC Linux Client
**Preconditions:**
- Same as Use Case 1, plus client operating system >= OtcLinux.

**Steps:**
1. Client sends packet with OperatingSystem >= OperatingSystem.OtcLinux.
2. Handler processes as in Use Case 1, sending OpcodeMessagePacket.

**Expected Outcomes:**
- Login succeeds with OTC Linux features enabled.

**Assertions/Validations:**
- Confirm OpcodeMessagePacket sent.

## 4. Happy Path: Reconnecting to Existing Online Character
**Preconditions:**
- Player already online with same character name.
- Account allows multiple online (or same character reconnect).

**Steps:**
1. Client sends login packet for already-online character.
2. Handler detects existing connection, disconnects old one, proceeds with login.

**Expected Outcomes:**
- Old connection closed; new login succeeds.

**Assertions/Validations:**
- Verify old connection receives disconnect (if applicable).
- New player instance active in game.

## 5. Edge Case: Empty Account Name
**Preconditions:**
- Packet has empty/whitespace account name.

**Steps:**
1. Client sends packet with invalid account.

**Expected Outcomes:**
- Connection disconnected with message "You must enter your account name."

**Assertions/Validations:**
- Assert GameServerDisconnectPacket sent with correct message.
- Connection closed.

## 6. Edge Case: Empty Character Name
**Preconditions:**
- Packet has empty/whitespace character name.

**Steps:**
1. Client sends packet with valid account/password but empty character name.

**Expected Outcomes:**
- Disconnected with "Account name or password is not correct." (since GetPlayer returns null).

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 7. Edge Case: Invalid Challenge (Timestamp Mismatch)
**Preconditions:**
- Challenge timestamp or number does not match connection's values.

**Steps:**
1. Client sends packet with mismatched challenge.

**Expected Outcomes:**
- Disconnected with "Login challenge is not valid."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 8. Edge Case: Unsupported Client Version (Too Low)
**Preconditions:**
- Packet version < server min version.

**Steps:**
1. Client sends packet with old version.

**Expected Outcomes:**
- Disconnected with "Only clients with protocol X allowed!"

**Assertions/Validations:**
- Correct disconnect message; connection closed.

## 9. Edge Case: Unsupported Client Version (Too High)
**Preconditions:**
- Packet version > server max version.

**Steps:**
1. Same as above.

**Expected Outcomes:**
- Same disconnect as too low.

**Assertions/Validations:**
- Same as above.

## 10. Error Condition: Server Stopped
**Preconditions:**
- Game state = Stopped.

**Steps:**
1. Client sends login packet.

**Expected Outcomes:**
- Immediate disconnect (no message).

**Assertions/Validations:**
- Connection closed without packet.

## 11. Error Condition: Server Opening
**Preconditions:**
- Game state = Opening.

**Steps:**
1. Client sends packet.

**Expected Outcomes:**
- Disconnected with "Gameworld is starting up. Please wait."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 12. Error Condition: Server Maintaining
**Preconditions:**
- Game state = Maintaining.

**Steps:**
1. Client sends packet.

**Expected Outcomes:**
- Disconnected with "Gameworld is under maintenance. Please re-connect in a while."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 13. Error Condition: Server Closed
**Preconditions:**
- Game state = Closed.

**Steps:**
1. Client sends packet.

**Expected Outcomes:**
- Disconnected with "Server is currently closed.\nPlease try again later."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 14. Error Condition: IP Banned
**Preconditions:**
- IP exists in ban list with active ban.

**Steps:**
1. Client connects from banned IP.

**Expected Outcomes:**
- Disconnected with ban message including expiry date and reason.

**Assertions/Validations:**
- Disconnect packet contains exact message: "Your IP address {IP} has been banished until {MM/dd/yyyy}.\nReason: {Reason}"; connection closed.

## 15. Error Condition: Invalid Credentials
**Preconditions:**
- Account/password/character combination invalid.

**Steps:**
1. Client sends valid packet but wrong credentials.

**Expected Outcomes:**
- Disconnected with "Account name or password is not correct."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 16. Error Condition: Account Banned
**Preconditions:**
- Account has BanishedAt set.

**Steps:**
1. Valid credentials for banned account.

**Expected Outcomes:**
- Disconnected with "Your account is banned."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 17. Error Condition: Player Already Online (Single Character Account)
**Preconditions:**
- Another character from same account online, account does not allow multiple.

**Steps:**
1. Attempt login with different character.

**Expected Outcomes:**
- Disconnected with "You may only login with one character of your account at the same time."

**Assertions/Validations:**
- Disconnect packet sent; connection closed.

## 18. Error Condition: Waiting Queue Full
**Preconditions:**
- Waiting queue denies login, returns slot number.

**Steps:**
1. Valid login attempt during high load.

**Expected Outcomes:**
- WaitingInLinePacket sent with queue message and retry time; connection closed.

**Assertions/Validations:**
- Packet contains exact message: "There are too many players online.\nYou are at place {currentSlot} on waiting list."; connection closed.

## 19. Security Consideration: RSA Decryption Failure
**Preconditions:**
- Encrypted data corrupted or invalid RSA.

**Steps:**
1. Client sends packet with bad encrypted data.

**Expected Outcomes:**
- Packet parsing fails; handler may not proceed or disconnect.

**Assertions/Validations:**
- Verify no further processing; possible disconnect if verification fails.

## 20. Security Consideration: XTEA Key Mismatch
**Preconditions:**
- XTEA keys not set correctly.

**Steps:**
1. Packet sent without proper XTEA setup.

**Expected Outcomes:**
- Potential failure in decryption or later communication.

**Assertions/Validations:**
- Assert secure channel not established.

## 21. Integration Point: Database Query Failure (Account Retrieval)
**Preconditions:**
- Database unavailable during GetPlayer call.

**Steps:**
1. Valid packet sent.

**Expected Outcomes:**
- Exception or null return; disconnected with "Account name or password is not correct."

**Assertions/Validations:**
- Disconnect packet sent; log database error.

## 22. Integration Point: Database Query Failure (Online Player Check)
**Preconditions:**
- Database fails during GetOnlinePlayer.

**Steps:**
1. Valid packet.

**Expected Outcomes:**
- Possible exception; handler may crash or disconnect.

**Assertions/Validations:**
- Verify error handling; connection closed.

## 23. Integration Point: Player Loading Failure
**Preconditions:**
- PlayerLoader fails (e.g., invalid data).

**Steps:**
1. Valid login.

**Expected Outcomes:**
- Login command fails; disconnected with parsed error message.

**Assertions/Validations:**
- Disconnect packet with command failure reason.

## 24. Integration Point: Map Placement Failure
**Preconditions:**
- Map.PlaceCreature fails.

**Steps:**
1. Valid login.

**Expected Outcomes:**
- Player not placed; potential inconsistency.

**Assertions/Validations:**
- Verify player not in world; check logs.

## 25. Variation: New Player Login
**Preconditions:**
- First login for character.

**Steps:**
1. Valid new player credentials.

**Expected Outcomes:**
- Successful login; last login set to now.

**Assertions/Validations:**
- DB last login updated correctly.

## 26. Variation: Game Master Login
**Preconditions:**
- Packet has GameMaster = true.

**Steps:**
1. GM account login.

**Expected Outcomes:**
- Login succeeds (GM flag may affect permissions, but not login flow).

**Assertions/Validations:**
- Player loaded with GM status.

## 27. Variation: Multiple Characters Allowed
**Preconditions:**
- Account.AllowManyOnline = true; multiple characters online.

**Steps:**
1. Login additional character.

**Expected Outcomes:**
- Success; no disconnect.

**Assertions/Validations:**
- Multiple players online from account.

## 28. Network Condition: Packet Corruption (Incomplete Data)
**Preconditions:**
- Packet truncated or malformed.

**Steps:**
1. Send corrupted packet.

**Expected Outcomes:**
- Parsing fails; possible exception or disconnect.

**Assertions/Validations:**
- Connection closed; no crash.

## 29. Network Condition: Timeout During Login
**Preconditions:**
- Network delay causes timeout.

**Steps:**
1. Send packet; simulate delay.

**Expected Outcomes:**
- Connection may close before completion.

**Assertions/Validations:**
- Verify partial state not persisted.

## 30. Failure Mode: Login Command Failure (Invalid Location)
**Preconditions:**
- PlayerLocationResolver returns Zero.

**Steps:**
1. Valid packet.

**Expected Outcomes:**
- Command fails; disconnected with error.

**Assertions/Validations:**
- Disconnect packet with location error.

## 31. Failure Mode: Creature Manager Failure
**Preconditions:**
- AddPlayer or other CM method fails.

**Steps:**
1. Valid login.

**Expected Outcomes:**
- Login fails; disconnected.

**Assertions/Validations:**
- Player not added to game.

## 32. Failure Mode: Database Update Failure (Online Status)
**Preconditions:**
- UpdatePlayerOnlineStatus fails.

**Steps:**
1. Valid login.

**Expected Outcomes:**
- Login succeeds but DB inconsistent.

**Assertions/Validations:**
- Check DB online status; log error.
