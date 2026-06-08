using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Text;
using System.Globalization;

namespace PncNext.Infrastructure.Motion.Protocols.PA
{
    /// <summary>
    /// PA 모션 제어기 전용 프로토콜 처리 클래스 (내부 엔진)
    /// </summary>
    public class PAMotionProtocol
    {
        private const string TERMINATOR = "\r\n";

        // 명령어 상수 정의
        public const string CMD_RND_CDT = "RND_CDT";
        public const string CMD_RND_STOP = "RND_STOP";
        public const string CMD_RND_HALT = "RND_HALT";
        public const string CMD_RND_RST = "RND_RST";
        public const string CMD_RND_INIT = "RND_INIT";
        public const string CMD_RND_HOME = "RND_HOME";
        public const string CMD_RND_MODE = "RND_MODE";
        public const string CMD_RND_PAUSE = "RND_PAUSE";
        public const string CMD_RND_CONTINUE = "RND_CONTINUE";
        public const string CMD_RND_MMI = "RND_MMI";
        public const string CMD_RND_MMA = "RND_MMA";
        public const string CMD_RND_MDA = "RND_MDA";
        public const string CMD_RENABLE = "RENABLE";
        public const string CMD_RDISABLE = "RDISABLE";
        public const string CMD_JOG = "JOG";
        public const string CMD_JSTOP = "JSTOP";
        public const string CMD_RJSS = "RJSS";
        public const string CMD_WJSS = "WJSS";
        public const string CMD_IOT = "IOT";
        public const string CMD_RND_SINIT = "RND_SINIT";

        // [2단계] 파라미터 및 데이터 동기화
        public const string CMD_RCFG = "RCFG";
        public const string CMD_WCFG = "WCFG";
        public const string CMD_RTCP = "RTCP";
        public const string CMD_WTCP = "WTCP";
        public const string CMD_RZOO = "RZOO";
        public const string CMD_WZOO = "WZOO";
        public const string CMD_RTHS = "RTHS";
        public const string CMD_WTHS = "WTHS";
        public const string CMD_RTLS = "RTLS";
        public const string CMD_WTLS = "WTLS";
        public const string CMD_RTMG = "RTMG";
        public const string CMD_WTMG = "WTMG";
        public const string CMD_PTPPO = "PTPPO";
        public const string CMD_WTPPO = "WTPPO";
        public const string CMD_RMAXL = "RMAXL";
        public const string CMD_WMAXL = "WMAXL";
        public const string CMD_RMINL = "RMINL";
        public const string CMD_WMINL = "WMINL";

        // [3단계] 시스템 설정 및 정보 조회
        public const string CMD_RARD = "RARD";
        public const string CMD_WARD = "WARD";
        public const string CMD_RRIOADR = "RRIOADR";
        public const string CMD_WRIOADR = "WRIOADR";
        public const string CMD_VER = "VER";
        public const string CMD_SAVEFLASH = "SAVEFLASH";
        public const string CMD_RND_STIN = "RND_STIN";

        // [2.17, 2.18] 자동 보정 및 설정 변경
        public const string CMD_DO_MEASURE = "DO_MASURE";
        public const string CMD_GET_MEASURE_RESULT = "GET_MASURE_RESULT";
        public const string CMD_RND_SUHO = "RND_SUHO";
        public const string CMD_RND_SABHO = "RND_SABHO";
        public const string CMD_SORZ = "SORZ";
        public const string CMD_WDSSZ = "WDSSZ";
        public const string CMD_SWVF = "SWVF";
        public const string CMD_GWVF = "GWVF";
        public const string CMD_FZS = "FZS";
        public const string CMD_SALF = "SALF";
        public const string CMD_SFSF = "SFSF";
        public const string CMD_WPAR = "WPAR";
        public const string CMD_RPAR = "RPAR";

        // 명령별 기본 타임아웃 설정 (밀리초)
        private readonly Dictionary<string, int> _commandTimeouts = new()
        {
            { CMD_RND_CDT, 2000 },
            { CMD_RND_STOP, 3000 },
            { CMD_RND_HALT, 3000 },
            { CMD_RND_RST, 3000 },
            { CMD_RND_INIT, 3000 }, 
            { CMD_RND_HOME, 600000 },
            { CMD_RND_MODE, 3000 },
            { CMD_RND_PAUSE, 3000 },
            { CMD_RND_CONTINUE, 3000 },
            { CMD_RND_MMI, 10000 },
            { CMD_RND_MMA, 10000 },
            { CMD_RND_MDA, 10000 },
            { CMD_RENABLE, 3000 },
            { CMD_RDISABLE, 3000 },
            { CMD_JOG, 3000 },
            { CMD_JSTOP, 3000 },
            { CMD_RJSS, 3000 },
            { CMD_WJSS, 3000 },
            { CMD_IOT, 3000 },
            { CMD_RND_SINIT, 5000 },
            { CMD_RCFG, 3000 }, { CMD_WCFG, 3000 },
            { CMD_RTCP, 3000 }, { CMD_WTCP, 3000 },
            { CMD_RZOO, 3000 }, { CMD_WZOO, 3000 },
            { CMD_RTHS, 3000 }, { CMD_WTHS, 3000 },
            { CMD_RTLS, 3000 }, { CMD_WTLS, 3000 },
            { CMD_RTMG, 3000 }, { CMD_WTMG, 3000 },
            { CMD_PTPPO, 3000 }, { CMD_WTPPO, 3000 },
            { CMD_RMAXL, 3000 }, { CMD_WMAXL, 3000 },
            { CMD_RMINL, 3000 }, { CMD_WMINL, 3000 },
            { CMD_RARD, 3000 }, { CMD_WARD, 3000 },
            { CMD_RRIOADR, 3000 }, { CMD_WRIOADR, 3000 },
            { CMD_VER, 3000 }, { CMD_SAVEFLASH, 5000 },
            { CMD_RND_STIN, 3000 },
            { CMD_DO_MEASURE, 120000 },
            { CMD_GET_MEASURE_RESULT, 3000 },
            { CMD_RND_SUHO, 3000 },
            { CMD_RND_SABHO, 3000 },
            { CMD_SORZ, 3000 },
            { CMD_WDSSZ, 3000 },
            { CMD_SWVF, 3000 },
            { CMD_GWVF, 3000 },
            { CMD_FZS, 3000 },
            { CMD_SALF, 3000 },
            { CMD_SFSF, 3000 },
            { CMD_WPAR, 3000 },
            { CMD_RPAR, 3000 }
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
            return new MotionCommandInfo {
                CommandKey = CMD_RND_RST,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_RST)
            };
        }

        public MotionCommandInfo EncodeInitController()
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0}{1}", CMD_RND_INIT, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_RND_INIT,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_INIT)
            };
        }

        public MotionCommandInfo EncodeHome()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RND_HOME,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_HOME}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_HOME)
            };
        }

        public MotionCommandInfo EncodeMode(string mode)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", CMD_RND_MODE, mode, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_RND_MODE,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_MODE)
            };
        }

        public MotionCommandInfo EncodePause()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RND_PAUSE,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_PAUSE}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_PAUSE)
            };
        }

        public MotionCommandInfo EncodeContinue()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RND_CONTINUE,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_CONTINUE}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_CONTINUE)
            };
        }

        public MotionCommandInfo EncodeMda(string gcode)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", CMD_RND_MDA, gcode, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_RND_MDA,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_RND_MDA)
            };
        }

        public MotionCommandInfo EncodeServo(bool on)
        {
            string cmd = on ? CMD_RENABLE : CMD_RDISABLE;
            return new MotionCommandInfo {
                CommandKey = cmd,
                Payload = Encoding.ASCII.GetBytes($"{cmd}{TERMINATOR}"),
                TimeoutMs = GetTimeout(cmd)
            };
        }

        public MotionCommandInfo EncodeJog(int axis, int direction)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}{3}", CMD_JOG, axis, direction, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_JOG,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_JOG)
            };
        }

        public MotionCommandInfo EncodeJogStop()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_JSTOP,
                Payload = Encoding.ASCII.GetBytes($"{CMD_JSTOP}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_JSTOP)
            };
        }

        public MotionCommandInfo EncodeSetJogSpeed(int speed)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", CMD_WJSS, speed, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_WJSS,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_WJSS)
            };
        }

        public MotionCommandInfo EncodeGetJogSpeed()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RJSS,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RJSS}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RJSS)
            };
        }

        public MotionCommandInfo EncodeOutput(int bitNo, bool on)
        {
            int signal = on ? 1 : 0;
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}{3}", CMD_IOT, bitNo, signal, TERMINATOR);
            return new MotionCommandInfo {
                CommandKey = CMD_IOT,
                Payload = Encoding.ASCII.GetBytes(payload),
                TimeoutMs = GetTimeout(CMD_IOT)
            };
        }

        public MotionCommandInfo EncodeInitSpindle()
        {
            return new MotionCommandInfo {
                CommandKey = CMD_RND_SINIT,
                Payload = Encoding.ASCII.GetBytes($"{CMD_RND_SINIT}{TERMINATOR}"),
                TimeoutMs = GetTimeout(CMD_RND_SINIT)
            };
        }

        // [2단계] 파라미터 인코딩
        public MotionCommandInfo EncodeReadConfig(int index) => CreateSimpleReadCommand(CMD_RCFG, index);
        public MotionCommandInfo EncodeWriteConfig(int index, double[] vals) => CreateArrayWriteCommand(CMD_WCFG, index, vals);
        
        public MotionCommandInfo EncodeReadTeaching(int index) => CreateSimpleReadCommand(CMD_RTCP, index);
        public MotionCommandInfo EncodeWriteTeaching(int index, double[] vals) => CreateArrayWriteCommand(CMD_WTCP, index, vals);

        public MotionCommandInfo EncodeReadZOriginOffset() => CreateSimpleReadCommand(CMD_RZOO);
        public MotionCommandInfo EncodeWriteZOriginOffset(double val) => CreateSingleValueWriteCommand(CMD_WZOO, val);

        public MotionCommandInfo EncodeReadToolSensingHighSpeed() => CreateSimpleReadCommand(CMD_RTHS);
        public MotionCommandInfo EncodeWriteToolSensingHighSpeed(int val) => CreateSingleValueWriteCommand(CMD_WTHS, val);

        public MotionCommandInfo EncodeReadToolSensingLowSpeed() => CreateSimpleReadCommand(CMD_RTLS);
        public MotionCommandInfo EncodeWriteToolSensingLowSpeed(int val) => CreateSingleValueWriteCommand(CMD_WTLS, val);

        public MotionCommandInfo EncodeReadToolSensingMargin() => CreateSimpleReadCommand(CMD_RTMG);
        public MotionCommandInfo EncodeWriteToolSensingMargin(double val) => CreateSingleValueWriteCommand(CMD_WTMG, val);

        public MotionCommandInfo EncodeReadToolPocketPutOffset() => CreateSimpleReadCommand(CMD_PTPPO);
        public MotionCommandInfo EncodeWriteToolPocketPutOffset(double val) => CreateSingleValueWriteCommand(CMD_WTPPO, val);

        public MotionCommandInfo EncodeReadSoftLimitPositive() => CreateSimpleReadCommand(CMD_RMAXL);
        public MotionCommandInfo EncodeWriteSoftLimitPositive(double[] vals) => CreateArrayWriteCommand(CMD_WMAXL, null, vals);
        public MotionCommandInfo EncodeReadSoftLimitNegative() => CreateSimpleReadCommand(CMD_RMINL);
        public MotionCommandInfo EncodeWriteSoftLimitNegative(double[] vals) => CreateArrayWriteCommand(CMD_WMINL, null, vals);

        // [3단계] 시스템 설정 인코딩
        public MotionCommandInfo EncodeReadControllerIp() => CreateSimpleReadCommand(CMD_RARD);
        public MotionCommandInfo EncodeWriteControllerIp(string ip) => CreateSingleValueWriteCommand(CMD_WARD, ip);
        public MotionCommandInfo EncodeReadIoBoardIp() => CreateSimpleReadCommand(CMD_RRIOADR);
        public MotionCommandInfo EncodeWriteIoBoardIp(string ip) => CreateSingleValueWriteCommand(CMD_WRIOADR, ip);
        public MotionCommandInfo EncodeReadVersion() => CreateSimpleReadCommand(CMD_VER);
        public MotionCommandInfo EncodeSaveFlash() => CreateSimpleReadCommand(CMD_SAVEFLASH);
        public MotionCommandInfo EncodeRestoreToolInfo(int toolNo, double length, bool updated)
        {
            int flag = updated ? 1 : 0;
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2:F3} {3}{4}", CMD_RND_STIN, toolNo, length, flag, TERMINATOR);
            return new MotionCommandInfo { CommandKey = CMD_RND_STIN, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(CMD_RND_STIN) };
        }

        // [2.17, 2.18] 자동 보정 및 설정 인코딩
        public MotionCommandInfo EncodeMeasure(int axisNo, double inPitch, double outPitch, int speed, int count, double maxDist, double offset)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2:F3}, {3:F3}, {4}, {5}, {6:F3}, {7:F3}{8}", 
                CMD_DO_MEASURE, axisNo, inPitch, outPitch, speed, count, maxDist, offset, TERMINATOR);
            return new MotionCommandInfo { CommandKey = CMD_DO_MEASURE, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(CMD_DO_MEASURE) };
        }

        public MotionCommandInfo EncodeGetMeasureResult() => CreateSimpleReadCommand(CMD_GET_MEASURE_RESULT);
        public MotionCommandInfo EncodeSetupSuho() => CreateSimpleReadCommand(CMD_RND_SUHO);
        public MotionCommandInfo EncodeSetupSabho() => CreateSimpleReadCommand(CMD_RND_SABHO);
        public MotionCommandInfo EncodeSetupSorz() => CreateSimpleReadCommand(CMD_SORZ);
        public MotionCommandInfo EncodeSetDiskThickness(double thickness) => CreateSingleValueWriteCommand(CMD_WDSSZ, thickness);
        public MotionCommandInfo EncodeSetM28Type(int type) => CreateSingleValueWriteCommand(CMD_SWVF, type);
        public MotionCommandInfo EncodeGetM28Type() => CreateSimpleReadCommand(CMD_GWVF);
        public MotionCommandInfo EncodeResetHomingStatus() => CreateSimpleReadCommand(CMD_FZS);

        public MotionCommandInfo EncodeSetAirParameters(int usingAir, int interval, int usingPurge, int purgeInterval)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2}, {3}, {4}{5}", 
                CMD_SALF, usingAir, interval, usingPurge, purgeInterval, TERMINATOR);
            return new MotionCommandInfo { CommandKey = CMD_SALF, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(CMD_SALF) };
        }

        public MotionCommandInfo EncodeSetWaterFlowParameters(int usingWater, int startTimeout, int sensingTimeout)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2}, {3}{4}", 
                CMD_SFSF, usingWater, startTimeout, sensingTimeout, TERMINATOR);
            return new MotionCommandInfo { CommandKey = CMD_SFSF, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(CMD_SFSF) };
        }

        public MotionCommandInfo EncodeSetPurgeAirHoldTime(int holdTime) => CreateSingleValueWriteCommand(CMD_WPAR, holdTime);
        public MotionCommandInfo EncodeReadPurgeAirHoldTime() => CreateSimpleReadCommand(CMD_RPAR);

        // 공통 헬퍼 메서드들
        private MotionCommandInfo CreateSimpleReadCommand(string cmd, int? index = null)
        {
            string payload = index.HasValue ? $"{cmd} {index.Value}{TERMINATOR}" : $"{cmd}{TERMINATOR}";
            return new MotionCommandInfo { CommandKey = cmd, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(cmd) };
        }

        private MotionCommandInfo CreateSingleValueWriteCommand(string cmd, object val)
        {
            string payload = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", cmd, val, TERMINATOR);
            return new MotionCommandInfo { CommandKey = cmd, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(cmd) };
        }

        private MotionCommandInfo CreateArrayWriteCommand(string cmd, int? index, double[] vals)
        {
            var sb = new StringBuilder(cmd);
            if (index.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", index.Value);
            
            foreach (var v in vals) sb.AppendFormat(CultureInfo.InvariantCulture, " {0:F3},", v);
            string payload = sb.ToString().TrimEnd(',') + TERMINATOR;
            
            return new MotionCommandInfo { CommandKey = cmd, Payload = Encoding.ASCII.GetBytes(payload), TimeoutMs = GetTimeout(cmd) };
        }

        public MotionCommandInfo EncodeMoveIncremental(double? x, double? y, double? z, double? a, double? b)
        {
            return BuildMoveCommand(CMD_RND_MMI, x, y, z, a, b);
        }

        public MotionCommandInfo EncodeMoveAbsolute(double? x, double? y, double? z, double? a, double? b)
        {
            return BuildMoveCommand(CMD_RND_MMA, x, y, z, a, b);
        }

        private MotionCommandInfo BuildMoveCommand(string cmdKey, double? x, double? y, double? z, double? a, double? b)
        {
            var sb = new StringBuilder(cmdKey);
            if (x.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " {0:F3},", x.Value);
            if (y.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F3},", y.Value);
            if (z.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F3},", z.Value);
            if (a.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F3},", a.Value);
            if (b.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F3}", b.Value);
            sb.Append(TERMINATOR);

            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(sb.ToString()),
                TimeoutMs = GetTimeout(cmdKey)
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

        public string ExtractCommandKey(byte[] response)
        {
            if (response == null || response.Length == 0) return string.Empty;
            string resStr = Encoding.ASCII.GetString(response).Trim();
            string firstWord = resStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)[0];

            return firstWord switch {
                CMD_RND_CDT => CMD_RND_CDT,
                CMD_RND_STOP => CMD_RND_STOP,
                CMD_RND_HOME => CMD_RND_HOME,
                CMD_RND_MODE => CMD_RND_MODE,
                CMD_RND_PAUSE => CMD_RND_PAUSE,
                CMD_RND_CONTINUE => CMD_RND_CONTINUE,
                CMD_RND_MMI => CMD_RND_MMI,
                CMD_RND_MMA => CMD_RND_MMA,
                CMD_RND_MDA => CMD_RND_MDA,
                CMD_RENABLE => CMD_RENABLE,
                CMD_RDISABLE => CMD_RDISABLE,
                CMD_JOG => CMD_JOG,
                CMD_JSTOP => CMD_JSTOP,
                CMD_RJSS => CMD_RJSS,
                CMD_WJSS => CMD_WJSS,
                CMD_IOT => CMD_IOT,
                CMD_RND_SINIT => CMD_RND_SINIT,
                CMD_RCFG => CMD_RCFG,
                CMD_RTCP => CMD_RTCP,
                CMD_RZOO => CMD_RZOO,
                CMD_RTHS => CMD_RTHS,
                CMD_RTLS => CMD_RTLS,
                CMD_RTMG => CMD_RTMG,
                CMD_PTPPO => CMD_PTPPO,
                CMD_RMAXL => CMD_RMAXL,
                CMD_RMINL => CMD_RMINL,
                CMD_RARD => CMD_RARD,
                CMD_RRIOADR => CMD_RRIOADR,
                CMD_VER => CMD_VER,
                CMD_SAVEFLASH => CMD_SAVEFLASH,
                CMD_RND_STIN => CMD_RND_STIN,
                CMD_DO_MEASURE => CMD_DO_MEASURE,
                CMD_GET_MEASURE_RESULT => CMD_GET_MEASURE_RESULT,
                CMD_RND_SUHO => CMD_RND_SUHO,
                CMD_RND_SABHO => CMD_RND_SABHO,
                CMD_SORZ => CMD_SORZ,
                CMD_WDSSZ => CMD_WDSSZ,
                CMD_SWVF => CMD_SWVF,
                CMD_GWVF => CMD_GWVF,
                CMD_FZS => CMD_FZS,
                CMD_SALF => CMD_SALF,
                CMD_SFSF => CMD_SFSF,
                CMD_WPAR => CMD_WPAR,
                CMD_RPAR => CMD_RPAR,
                _ => firstWord
            };
        }

        public void UpdateStateFromResponse(byte[] response, PAMotionControllerState state)
        {
            if (response == null || response.Length == 0) return;
            string resStr = Encoding.ASCII.GetString(response).Trim();
            if (resStr.StartsWith(CMD_RND_CDT)) ParseRndCdt(resStr, state);
        }

        private void ParseRndCdt(string payload, PAMotionControllerState state)
        {
            string dataPart = payload.Replace(CMD_RND_CDT, "").Trim();
            string[] parts = dataPart.Split(',');
            if (parts.Length < 18) return;

            int offset = 0;
            bool isControllerError = false;
            if (parts[0].StartsWith("E", StringComparison.OrdinalIgnoreCase))
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
            if (runStatus == 1) return MotionStatus.Running;
            if (!isHomeComplete) return MotionStatus.NotReady;
            return runStatus switch {
                0 => MotionStatus.Ready,
                1 => MotionStatus.Running,
                2 => MotionStatus.Pause,
                3 => MotionStatus.Error,
                _ => MotionStatus.Ready
            };
        }
    }
}
