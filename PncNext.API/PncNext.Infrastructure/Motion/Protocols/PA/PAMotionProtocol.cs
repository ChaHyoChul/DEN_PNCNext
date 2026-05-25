using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Text;
using System.Globalization;

namespace PncNext.Infrastructure.Motion.Protocols.PA
{
    /// <summary>
    /// PA 모션 제어기 비동기 통신 규격(PAAsyncComm.md, 20260514-command-resp.md)을 반영한 프로토콜 구현 클래스
    /// </summary>
    public class PAMotionProtocol : IMotionProtocol
    {
        private const string TERMINATOR = "\r\n";

        public const string CMD_RND_CDT = "RND_CDT";
        public const string CMD_RND_STOP = "RND_STOP";

        private readonly Dictionary<string, int> _commandTimeouts = new()
        {
            { CMD_RND_CDT, 1000000 },
            { CMD_RND_STOP, 2000 },
            { "DEFAULT", 3000 }
        };

        private int GetTimeout(string command) => _commandTimeouts.TryGetValue(command, out var timeout) ? timeout : _commandTimeouts["DEFAULT"];

        public MotionCommandInfo EncodeMove(double x, double y, double z, double a, double b)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, 
                "{0} {1:F3} {2:F3} {3:F3} {4:F3} {5:F3}{6}", 
                CMD_MOV, x, y, z, a, b, TERMINATOR);

            return new MotionCommandInfo {
                CommandKey = CMD_MOV,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_MOV)
            };
        }

        public MotionCommandInfo EncodeStop()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RND_STOP,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_STOP}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_STOP)
            };
        }

        public MotionCommandInfo EncodeStatusRequest()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RND_CDT,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_CDT}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_CDT)
            };
        }

        public string ExtractCommandKey(byte[] response)
        {
            if (response == null || response.Length == 0) return string.Empty;
            string resStr = Encoding.ASCII.GetString(response).Trim();
            string firstWord = resStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)[0];

            return firstWord switch
            {
                CMD_RND_CDT => CMD_RND_CDT,
                CMD_RND_STATUS => CMD_RND_STATUS,
                CMD_RND_STOP => CMD_RND_STOP,
                CMD_MOV => CMD_MOV,
                _ => firstWord
            };
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response).Trim();
            bool isHomeComplete = true; 
            int runStatus = 0;
            bool isControllerError = false;

            if (resStr.StartsWith(CMD_RND_CDT))
            {
                var dataPart = resStr.Replace(CMD_RND_CDT, "").Trim();
                var parts = dataPart.Split(',');
                
                int offset = 0;
                if (parts.Length > 0 && parts[0].StartsWith("E", StringComparison.OrdinalIgnoreCase))
                {
                    offset = 1;
                    isControllerError = true;
                }

                if (parts.Length > 1 + offset && parts[1 + offset].StartsWith("PS:"))
                {
                    var psParts = parts[1 + offset].Split(':');
                    if (psParts.Length >= 6) int.TryParse(psParts[5], out runStatus);
                }

                if (parts.Length > 8 + offset && parts[8 + offset].StartsWith("SV:"))
                {
                    var svParts = parts[8 + offset].Split(':');
                    if (svParts.Length >= 3) isHomeComplete = svParts[2] == "1";
                }
                
                return isControllerError ? MotionStatus.Error : MapToMotionStatus(runStatus, isHomeComplete);
            }

            return MotionStatus.Ready;
        }

        public MotionCommandInfo EncodeCustom(string command, params object[] args)
        {
            string formatted = args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, command, args) : command;
            string firstWord = formatted.Split(' ')[0];

            return new MotionCommandInfo {
                CommandKey = firstWord,
                Payload = Encoding.ASCII.GetBytes($"{formatted}{TERMINATOR}"),
                TimeoutMs = GetTimeout(firstWord)
            };
        }

        public void UpdateStateFromResponse(byte[] response, PAMotionControllerState state)
        {
            if (response == null || response.Length == 0) return;
            string resStr = Encoding.ASCII.GetString(response).Trim();

            if (resStr.StartsWith(CMD_RND_CDT))
            {
                ParseRndCdt(resStr, state);
            }
        }

        private void ParseRndCdt(string payload, PAMotionControllerState state)
        {
            string dataPart = payload.Replace(CMD_RND_CDT, "").Trim();
            string[] parts = dataPart.Split(',');

            // 정상 응답은 18개, 에러 응답은 20개 필드로 구성됨
            if (parts.Length < 18) return;

            // 제어기 에러 상태 체크
            int offset = 0;
            bool isControllerError = false;
            if (parts[0].StartsWith("E", StringComparison.OrdinalIgnoreCase))
            {
                offset = 1;
                isControllerError = true;
                state.LastErrorCode = parts[0];
                // 에러 발생 시 마지막 필드가 에러 메시지
                state.LastErrorMessage = parts[parts.Length - 1];
            }
            else
            {
                state.LastErrorCode = string.Empty;
                state.LastErrorMessage = string.Empty;
            }

            try
            {
                // 1. Servo Status (Index 8 + offset)
                var svParts = parts[8 + offset].Split(':');
                if (svParts.Length >= 4)
                {
                    state.IsServoOn = svParts[1] == "1";
                    state.IsHomComplete = svParts[2] == "1";
                }

                // 2. Program Status (Index 1 + offset)
                var psParts = parts[1 + offset].Split(':');
                //if (psParts.Length >= 6)
                if (psParts.Length == 4) 
                {
                    // LineNumber
                    if (long.TryParse(psParts[1], out long line)) state.MillingLineNumber = line;
                    // GPL ErrorCode
                    if (int.TryParse(psParts[2], out int err)) state.GPLErrorCode = err;
                    // RunMode
                    int run = 0;
                    if (int.TryParse(psParts[3], out run))
                    {
                        state.ControllerState = isControllerError ? MotionStatus.Error : MapToMotionStatus(run, state.IsHomComplete);
                    }
                }

                // 3. Positions (Index 2+offset ~ 7+offset)
                for (int i = 0; i < 6; i++)
                {
                    var posParts = parts[i + 2 + offset].Split(':');
                    if (posParts.Length >= 2)
                    {
                        if (double.TryParse(posParts[1], CultureInfo.InvariantCulture, out double val))
                            state.Position[i] = val;
                    }
                }

                // 4. Tool & Spindle (Index 9+offset ~ 14+offset)
                if (int.TryParse(parts[9 + offset], out int tNo)) state.CurrentToolNo = tNo;
                if (double.TryParse(parts[10 + offset], out double tLength)) state.CurrentToolLength = tLength;
                if (int.TryParse(parts[11 + offset], out int sSpd)) state.SpindleSpeed = sSpd;
                if (int.TryParse(parts[12 + offset], out int sOv)) state.SpindleOverride = sOv;
                if (int.TryParse(parts[13 + offset], out int mOv)) state.MotorOverride = mOv;
                if (int.TryParse(parts[14 + offset], out int mFd)) state.MotorFeedrate = mFd;

                // 5. I/O => System + Cantops (System 15+offset, 16+offset, Cantops 17+offset, 18+offset)
                UpdateIOArray(state.InputSystem, parts[15 + offset], 0);  // System Input 
                UpdateIOArray(state.OutputSystem, parts[16 + offset], 0); // System Output 
                UpdateIOArray(state.InputSystem, parts[17 + offset], 0);  // Cantops Input 
                UpdateIOArray(state.OutputSystem, parts[18 + offset], 0); // Cantops Output 

                // 6. Flags (Index 17+offset)
                if (int.TryParse(parts[19 + offset], out int flags))
                {
                    state.IoBoardState = (flags & 0x0001) != 0 ? 1 : 0;
                    state.SpindleBoardState = (flags & 0x0002) != 0 ? 1 : 0;
                    state.IsSpindleRun = (flags & 0x0004) != 0;
                    state.IsToolLengthUpdate = (flags & 0x0008) != 0;
                    state.IsDuringToolChange = (flags & 0x0010) != 0;
                    state.IsEmoButtonPressed = (flags & 0x0020) != 0;
                    state.IsMotorMoving = (flags & 0x0040) != 0;
                    state.IsMotorMovingAutoloader = (flags & 0x0080) != 0;
                    state.IsM00Command = (flags & 0x0100) != 0;
                    state.IsSpindleClampState = (flags & 0x0200) != 0;
                    state.IsPurgeAirState = (flags & 0x0400) != 0;
                    state.IsAirRechargeStarte = (flags & 0x0800) != 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RND_CDT 파싱 오류: {ex.Message}");
            }
        }

        private void UpdateIOArray(bool[] target, string hexValue, int offset)
        {
            if (string.IsNullOrEmpty(hexValue)) return;
            try
            {
                long val = Convert.ToInt64(hexValue, 16);
                for (int i = 0; i < 32; i++)
                {
                    if (offset + i < target.Length)
                        target[offset + i] = (val & (1L << i)) != 0;
                }
            }
            catch { }
        }

        private MotionStatus MapToMotionStatus(int runStatus, bool isHomeComplete)
        {
            if (!isHomeComplete) return MotionStatus.NotReady;

            return runStatus switch
            {
                0 => MotionStatus.Ready,
                1 => MotionStatus.Running,
                2 => MotionStatus.Pause,
                3 => MotionStatus.Error,
                _ => MotionStatus.Ready
            };
        }
    }
}
