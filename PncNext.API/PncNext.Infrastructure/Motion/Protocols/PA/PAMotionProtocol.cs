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

        public byte[] EncodeMove(double x, double y, double z, double a, double b)
        {
            string command = string.Format(CultureInfo.InvariantCulture, 
                "MOV {0:F3} {1:F3} {2:F3} {3:F3} {4:F3}{5}", 
                x, y, z, a, b, TERMINATOR);
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStop()
        {
            return Encoding.ASCII.GetBytes($"RND_STOP{TERMINATOR}");
        }

        public byte[] EncodeStatusRequest()
        {
            // 효율적인 상태 조회를 위해 RND_CDT를 기본 상태 요청 명령으로 사용합니다.
            return Encoding.ASCII.GetBytes($"RND_CDT{TERMINATOR}");
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response).Trim();

            // RND_CDT 응답인 경우 내부 파싱 로직에서 상태 추출
            if (resStr.StartsWith("RND_CDT"))
            {
                var parts = resStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 2 && parts[2].StartsWith("PS:"))
                {
                    var psParts = parts[2].Split(':');
                    if (psParts.Length >= 6 && int.TryParse(psParts[5], out int runStatus))
                    {
                        return MapRunStatus(runStatus);
                    }
                }
            }

            // 기존 RND_STATUS 응답 처리 유지
            if (resStr.Contains("PS:"))
            {
                var allValues = resStr.Replace("RND_STATUS", "").Trim().Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (allValues.Length >= 5)
                {
                    if (int.TryParse(allValues[allValues.Length - 1], out int runStatus))
                    {
                        return MapRunStatus(runStatus);
                    }
                }
            }

            return MotionStatus.Idle;
        }

        public byte[] EncodeCustom(string command, params object[] args)
        {
            string formatted = args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, command, args) : command;
            return Encoding.ASCII.GetBytes($"{formatted}{TERMINATOR}");
        }

        /// <summary>
        /// 응답 메시지로부터 PAMotionControllerState 객체를 직접 업데이트합니다. (PA 전용 기능)
        /// </summary>
        public void UpdateStateFromResponse(byte[] response, PAMotionControllerState state)
        {
            if (response == null || response.Length == 0) return;

            string resStr = Encoding.ASCII.GetString(response).Trim();

            if (resStr.StartsWith("RND_CDT"))
            {
                ParseRndCdt(resStr, state);
            }
        }

        private void ParseRndCdt(string payload, PAMotionControllerState state)
        {
            // 포맷: RND_CDT [MD],[PS],[X],[Y],[Z],[A],[B],[C],[SV],[ToolNo],[ToolLen],[Spd],[SpdOv],[MotOv],[MotFd],[SysIn],[SysOut],[CanIn],[CantOut],[Flags],[ErrMsg]
            // 헤더 제거 후 쉼표로 분리
            string dataPart = payload.Replace("RND_CDT", "").Trim();
            string[] parts = dataPart.Split(',');

            if (parts.Length < 20) return; // 최소 필드 수 확인

            try
            {
                // 1. Program Status (Index 1) - PS:File:B:L:E:R
                var psParts = parts[1].Split(':');
                if (psParts.Length >= 6)
                {
                    if (long.TryParse(psParts[3], out long line)) state.MillingLineNumber = line;
                    if (int.TryParse(psParts[4], out int err)) state.ErrorCode = err;
                    if (int.TryParse(psParts[5], out int run)) state.ControllerState = run;
                }

                // 2. Positions (Index 2~7) - X:pos ~ C:pos
                for (int i = 0; i < 6; i++)
                {
                    var posParts = parts[i + 2].Split(':');
                    if (posParts.Length >= 2)
                    {
                        if (double.TryParse(posParts[1], CultureInfo.InvariantCulture, out double val))
                            state.Position[i] = val;
                    }
                }

                // 3. Servo Status (Index 8) - SV:P:H:E
                var svParts = parts[8].Split(':');
                if (svParts.Length >= 4)
                {
                    state.IsServoOn = svParts[1] == "1";
                    state.IsHomComplete = svParts[2] == "1";
                }

                // 4. Tool & Spindle (Index 9~14)
                if (int.TryParse(parts[9], out int tNo)) state.CurrentToolNo = tNo;
                if (int.TryParse(parts[11], out int sSpd)) state.SpindleSpeed = sSpd;
                if (int.TryParse(parts[12], out int sOv)) state.SpindleOverride = sOv;
                if (int.TryParse(parts[13], out int mOv)) state.MotorOverride = mOv;
                if (int.TryParse(parts[14], out int mFd)) state.MotorFeedrate = mFd;

                // 5. I/O Hex (Index 15~18)
                UpdateIOArray(state.Input, parts[15], 0);  // System Input
                UpdateIOArray(state.Input, parts[17], 8);  // Cantops Input (Offset 8)
                UpdateIOArray(state.Output, parts[16], 0); // System Output
                UpdateIOArray(state.Output, parts[18], 8); // Cantops Output (Offset 8)

                // 6. Bit Flags (Index 19)
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
                // 파싱 중 오류 발생 시 로깅 또는 무시 (시스템 안정성)
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

        private MotionStatus MapRunStatus(int runStatus)
        {
            return runStatus switch
            {
                0 => MotionStatus.Idle,
                1 => MotionStatus.Running,
                2 => MotionStatus.Stopped,
                3 => MotionStatus.Error,
                _ => MotionStatus.Idle
            };
        }
    }
}
