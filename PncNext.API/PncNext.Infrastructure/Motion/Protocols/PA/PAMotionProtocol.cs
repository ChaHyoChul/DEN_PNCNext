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
        public const string CMD_RND_STATUS = "RND_STATUS";
        public const string CMD_RND_STOP = "RND_STOP";
        public const string CMD_MOV = "MOV";

        private readonly Dictionary<string, int> _commandTimeouts = new()
        {
            { CMD_RND_CDT, 1000000 },
            { CMD_RND_STATUS, 1000 },
            { CMD_RND_STOP, 2000 },
            { CMD_MOV, 5000 },
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
            bool isHomeComplete = true; // 기본값은 완료로 가정 (상태 파싱에서 업데이트됨)
            int runStatus = 0;

            // 1. RND_CDT 응답 파싱
            if (resStr.StartsWith(CMD_RND_CDT))
            {
                var parts = resStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Program Status (Index 2 in split array)
                if (parts.Length > 2 && parts[2].StartsWith("PS:"))
                {
                    var psParts = parts[2].Split(':');
                    if (psParts.Length >= 6) int.TryParse(psParts[5], out runStatus);
                }

                // Servo Status (Index 9 in split array) - SV:P:H:E
                if (parts.Length > 9 && parts[9].StartsWith("SV:"))
                {
                    var svParts = parts[9].Split(':');
                    if (svParts.Length >= 3) isHomeComplete = svParts[2] == "1";
                }
                
                return MapToMotionStatus(runStatus, isHomeComplete);
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

            if (parts.Length < 20) return;

            try
            {
                // 1. Servo Status (Index 8) - SV:P:H:E
                var svParts = parts[8].Split(':');
                if (svParts.Length >= 4)
                {
                    state.IsServoOn = svParts[1] == "1";
                    state.IsHomComplete = svParts[2] == "1";
                }

                // 2. Program Status (Index 1) - PS:File:B:L:E:R
                var psParts = parts[1].Split(':');
                if (psParts.Length >= 6)
                {
                    if (long.TryParse(psParts[3], out long line)) state.MillingLineNumber = line;
                    if (int.TryParse(psParts[4], out int err)) state.GPLErrorCode = err;
                    
                    int run = 0;
                    if (int.TryParse(psParts[5], out run))
                    {
                        // 원점 복귀 여부와 제어기 실행 상태를 결합하여 MotionStatus 결정
                        state.ControllerState = MapToMotionStatus(run, state.IsHomComplete);
                    }
                }

                // 3. Positions (Index 2~7)
                for (int i = 0; i < 6; i++)
                {
                    var posParts = parts[i + 2].Split(':');
                    if (posParts.Length >= 2)
                    {
                        if (double.TryParse(posParts[1], CultureInfo.InvariantCulture, out double val))
                            state.Position[i] = val;
                    }
                }

                // 4. Tool & Spindle
                if (int.TryParse(parts[9], out int tNo)) state.CurrentToolNo = tNo;
                if (int.TryParse(parts[11], out int sSpd)) state.SpindleSpeed = sSpd;
                if (int.TryParse(parts[12], out int sOv)) state.SpindleOverride = sOv;
                if (int.TryParse(parts[13], out int mOv)) state.MotorOverride = mOv;
                if (int.TryParse(parts[14], out int mFd)) state.MotorFeedrate = mFd;

                // 5. I/O & Flags
                UpdateIOArray(state.Input, parts[15], 0);
                UpdateIOArray(state.Input, parts[17], 8);
                UpdateIOArray(state.Output, parts[16], 0);
                UpdateIOArray(state.Output, parts[18], 8);

                if (int.TryParse(parts[19], out int flags))
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

        /// <summary>
        /// 제어기의 원시 실행 상태와 원점 복귀 여부를 조합하여 도메인 MotionStatus로 매핑합니다.
        /// </summary>
        private MotionStatus MapToMotionStatus(int runStatus, bool isHomeComplete)
        {
            // 1순위: 원점 복귀가 되지 않았다면 무조건 NotReady
            if (!isHomeComplete) return MotionStatus.NotReady;

            // 2순위: 제어기 실행 상태에 따른 매핑
            return runStatus switch
            {
                0 => MotionStatus.Ready,    // Idle -> Ready
                1 => MotionStatus.Running,  // Running
                2 => MotionStatus.Pause,    // Pause
                3 => MotionStatus.Error,    // Error
                _ => MotionStatus.Ready
            };
        }
    }
}
