# PA Controller Command Response Specification (Data-Enabled Commands)

This document details the response formats for commands in the `PAAsyncComm` class that return data beyond a simple acknowledgment.

---

## 1. Machine Status & State Commands

### CMD_RND_CDT (Read Combined Data)
**Command String**: `RND_CDT`  
**Description**: Returns a comprehensive snapshot of the machine status and state in a single message.  
**Format**: `RND_CDT [Field0],[Field1],...,[Field20]\r\n`

| Index | Field Name | Format/Type | Description |
| :--- | :--- | :--- | :--- |
| 0 | Run Mode | `MD:MODE` | `OFF`, `AUTO`, `STEP`, `MDA` |
| 1 | Program Status | `PS:File:B:L:E:R` | Filename, BlockNo, LineNo, ErrorCode, RunStatus |
| 2-6 | Positions | `X:pos`, ..., `B:pos` | Machine positions for axes X, Y, Z, A, B |
| 7 | AL Position | `C:pos` | AutoLoader (C-axis) position |
| 8 | Servo Status | `SV:P:H:E` | Power (0/1), HomeState (0/1), ErrorState (0/1) |
| 9 | Tool No | `int` | Current active tool number |
| 10 | Tool Length | `double` | Current tool length offset |
| 11 | Spindle Speed | `int` | Current spindle RPM |
| 12 | Spindle Override| `int` | Spindle speed override % |
| 13 | Motor Override | `int` | Feedrate override % |
| 14 | Motor Feedrate | `int` | Current commanded feedrate |
| 15 | System Input | `Hex (2 chars)` | System-level inputs (bit-mapped) |
| 16 | System Output | `Hex (2 chars)` | System-level outputs (bit-mapped) |
| 17 | Cantops Input | `Hex (8 chars)` | Board-level inputs (bit-mapped) |
| 18 | Cantops Output | `Hex (8 chars)` | Board-level outputs (bit-mapped) |
| 19 | Bit Flags | `int` | Status bits (See [Status Bitmask Table](#status-bitmask-table)) |
| 20 | Error Message | `string` | Human-readable error message from controller |

### CMD_RND_STATE (Read Machine State)
**Command String**: `RND_STATE`  
**Format**: `RND_STATE [Val0],[Val1],...,[Val16]\r\n`  
**Fields**: Similar to `RND_CDT` but specifically focused on IO and hardware states.
- `0`: IO Board Link (0/1)
- `1`: Spindle Board Link (0/1)
- `2`: Tool No
- `3`: Tool Length
- `4`: Spindle Run (0/1)
- `5`: Spindle Speed
- `6`: Spindle Override
- `7`: Motor Override
- `8`: Motor Feedrate
- `9`: System Input (Hex)
- `10`: System Output (Hex)
- `11`: Cantops Input (Hex)
- `12`: Cantops Output (Hex)
- `13`: Tool Length Update Flag
- `14`: Tool Change Flag
- `15`: EMO Button Flag
- `16`: Error Message

### CMD_RND_STATUS (Read Execution Status)
**Command String**: `RND_STATUS`  
**Format**: `RND_STATUS Key:Value,Key:Value,...\r\n`  
**Key-Value Pairs**:
- `MD`: Mode (e.g., `AUTO`)
- `PS`: `Filename:BlockNo:LineNo:GplError:RunStatus`
- `PT`: Program Cycle Time
- `X/Y/Z/A/B/C`: Axis positions
- `VX/VY/VZ/VA/VB/VC`: Axis velocities
- `SV`: Servo bits (`Power:Home:Error`)
- `ZR`: Z-Axis Ready state

---

## 2. Configuration & Parameter Commands

### CMD_RCFG (Read Coordinate Offset)
**Command String**: `RCFG`  
**Response**: `RCFG x,y,z,a,b,u,v,w\r\n`  
**Parsing**: Returns 8 double values. The system maps these to X, Y, Z, A, B offsets.

### CMD_RTCP (Read Teaching Point)
**Command String**: `RTCP`  
**Response**: `RTCP x,y,z,a,b\r\n`  
**Parsing**: Returns 5 double values for the requested teaching point index.

### CMD_RMAXL / CMD_RMINL (Read Soft Limits)
**Command String**: `RMAXL` / `RMINL`  
**Response**: `RMAXL x,y,z,a,b,c\r\n`  
**Parsing**: Returns 6 double values representing the positive/negative software limits.

### Miscellaneous Configs
- **CMD_RJSS**: `RJSS [int]` (Jog Speed)
- **CMD_RZOO**: `RZOO [double]` (Z-Origin Offset)
- **CMD_RTHS/RTLS**: `RTHS [double]` (Tool Sensing High/Low Speed)
- **CMD_RTMG**: `RTMG [double]` (Tool Sensing Margin)
- **CMD_RTPPO**: `RTPPO [double]` (Tool Pocket Put Offset)
- **CMD_GWVF**: `GWVF [int]` (Wet/Dry Mode: 1=Wet, 0=Dry)

---

## 3. Diagnostic & System Commands

### CMD_SMCT (Power-On Test)
**Command String**: `SMCT`  
**Response**: `SMCT [int]\r\n`  
**Description**: Returns a 32-bit error mask. `0` means success. Non-zero indicates specific hardware failures (e.g., sensors, comms).

### CMD_RISM (Machine Check)
**Command String**: `RISM`  
**Response**: `RISM [int]\r\n`  
**Description**: Returns diagnostic error code (0 = OK).

### CMD_VER (Version)
**Command String**: `VER`  
**Response**: `VER [string]\r\n`  
**Description**: Returns the controller's firmware version string.

### CMD_RDT (Read Controller Time)
**Command String**: `RDT`  
**Response**: `RDT MM-DD-YYYY HH:mm:ss\r\n`  
**Description**: Returns the current system time from the controller.

---

## Appendix: Status Bitmask Table (Index 19 of CDT)
| Mask | Property |
| :--- | :--- |
| `0x0001` | IO Board Connected |
| `0x0002` | Spindle Board Connected |
| `0x0004` | Spindle Running |
| `0x0008` | Tool Length Update Required |
| `0x0010` | Tool Change in Progress |
| `0x0020` | EMO Button Pressed |
| `0x0040` | Motor Moving |
| `0x0080` | AutoLoader Moving |
| `0x0100` | M00 Command Active |
| `0x0200` | Spindle Clamped |
| `0x0400` | Purge Air On |
| `0x0800` | Air Recharge Active |
