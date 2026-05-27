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

        // 명령어 상수 정의
        public const string CMD_RND_CDT = "RND_CDT";
        public const string CMD_RND_STOP = "RND_STOP";
        public const string CMD_RND_HALT = "RND_HALT";
        public const string CMD_RND_RST = "RND_RST";
        public const string CMD_RND_INIT = "RND_INIT";
        public const string CMD_RND_HOME = "RND_HOME";

        // 명령별 기본 타임아웃 설정 (밀리초)
        private readonly Dictionary<string, int> _commandTimeouts = new()
        {
            { CMD_RND_CDT, 2000 },
            { CMD_RND_STOP, 2000 },
            { CMD_RND_HALT, 2000 },
            { CMD_RND_RST, 2000 },
            { CMD_RND_INIT, 2000 }, 
            { CMD_RND_HOME, 300000 } // 5분 (원점 복귀 장시간 소요 대비)
        };

        private int GetTimeout(string command) => _commandTimeouts.TryGetValue(command, out var timeout) ? timeout : 3000;

        public MotionCommandInfo EncodeStop(int mode)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", CMD_RND_STOP, mode, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_RND_STOP,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_STOP)
            };
        }

        public MotionCommandInfo EncodeHalt()
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0}{1}", CMD_RND_HALT, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_RND_HALT,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_HALT)
            };
        }

        public MotionCommandInfo EncodeErrorReset()
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0}{1}", CMD_RND_RST, TERMINATOR);
            return new MotionCommandInfo
            {
                CommandKey = CMD_RND_RST,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_RST)
            };
        }

        public MotionCommandInfo EncodeInitController()
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0}{1}", CMD_RND_INIT, TERMINATOR);
            return new MotionCommandInfo
            {
                CommandKey = CMD_RND_INIT,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_INIT)
            };
        }

        public MotionCommandInfo EncodeHome()
        {
            // 규격: RND_HOME\r\n
            return new MotionCommandInfo {
                CommandKey = CMD_RND_HOME,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_HOME}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_HOME)
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
                CMD_RND_STOP => CMD_RND_STOP,
                CMD_RND_HOME => CMD_RND_HOME,
                _ => firstWord
            };
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

            if (parts.Length < 18) return;

            int offset = 0;
            bool isControllerError = false;
            if (parts.Length > 0 && parts[0].StartsWith("E", StringComparison.OrdinalIgnoreCase))
            {
                offset = 1;
                isControllerError = true;
                state.LastErrorCode = parts[0];
                state.LastErrorMessage = parts[parts.Length - 1];
            }
            else
            {
                state.LastErrorCode = string.Empty;
                state.LastErrorMessage = string.Empty;
            }

            try
            {
                var svParts = parts[8 + offset].Split(':');
                if (svParts.Length >= 4)
                {
                    state.IsServoOn = svParts[1] == "1";
                    state.IsHomComplete = svParts[2] == "1";
                }

                var psParts = parts[1 + offset].Split(':');
                if (psParts.Length >= 4) 
                {
                    int lineIdx = psParts.Length == 4 ? 1 : 3;
                    int errIdx = psParts.Length == 4 ? 2 : 4;
                    int runIdx = psParts.Length == 4 ? 3 : 5;

                    if (long.TryParse(psParts[lineIdx], out long line)) state.MillingLineNumber = line;
                    if (int.TryParse(psParts[errIdx], out int err)) state.GPLErrorCode = err;
                    
                    int run = 0;
                    if (int.TryParse(psParts[runIdx], out run))
                    {
                        state.ControllerState = isControllerError ? MotionStatus.Error : MapToMotionStatus(run, state.IsHomComplete);
                    }
                }

                for (int i = 0; i < 6; i++)
                {
                    var posParts = parts[i + 2 + offset].Split(':');
                    if (posParts.Length >= 2)
                    {
                        if (double.TryParse(posParts[1], CultureInfo.InvariantCulture, out double val))
                            state.Position[i] = val;
                    }
                }

                if (int.TryParse(parts[9 + offset], out int tNo)) state.CurrentToolNo = tNo;
                if (parts.Length > 10 + offset && double.TryParse(parts[10 + offset], CultureInfo.InvariantCulture, out double tLen)) state.CurrentToolLength = tLen;
                if (int.TryParse(parts[11 + offset], out int sSpd)) state.SpindleSpeed = sSpd;
                if (int.TryParse(parts[12 + offset], out int sOv)) state.SpindleOverride = sOv;
                if (int.TryParse(parts[13 + offset], out int mOv)) state.MotorOverride = mOv;
                if (int.TryParse(parts[14 + offset], out int mFd)) state.MotorFeedrate = mFd;

                UpdateIOArray(state.InputSystem, parts[15 + offset], 0);
                UpdateIOArray(state.OutputSystem, parts[16 + offset], 0);
                if (parts.Length > 18 + offset) 
                {
                    UpdateIOArray(state.InputCantops, parts[17 + offset], 0);
                    UpdateIOArray(state.OutputCantops, parts[18 + offset], 0);
                }

                int flagsIdx = parts.Length > 20 ? 19 + offset : 17 + offset;
                if (int.TryParse(parts[flagsIdx], out int flags))
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
            if (string.IsNullOrEmpty(hexValue) || target == null) return;
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

        // 미사용 인터페이스 메서드 Stub
        public MotionCommandInfo EncodeMove(double x, double y, double z, double a, double b) => throw new NotImplementedException();
    }
}
