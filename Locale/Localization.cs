using System.Globalization;

namespace RainDropWeb.Locale;

public static class Localization
{
    private static readonly Dictionary<string, string> English = new()
    {
        { "INVALID_WAVE_FUNCTION", "Invalid wave function." },
        { "INVALID_WAVE_FREQUENCY", "Invalid wave frequency." },
        { "INVALID_WAVE_OFFSET", "Invalid wave offset." },
        { "INVALID_WAVE_AMP", "Invalid wave amplitude." },
        { "INVALID_WAVE_SYM", "Invalid wave symmetry." },
        { "FREQUENCY_OUT_OF_RANGE", "Frequency must be between 0 and 40 MHz." },
        { "SYM_UNFIT", "Symmetry must be between 0 and 1." },
        { "BYTE_STR_EVEN_LENGTH", "must have an even length." },
        { "NEGATIVE_CH_VOL_ERR", "Negative channel voltage should be between -5 and 0." },
        { "POSITIVE_CH_VOL_ERR", "Positive channel voltage should be between 0 and 5." },
        { "SIZE_ERR", "Size should be 32, 64, 128, 256, 512, 1024 or 2048." },
        { "LEVEL_OUT_OF_RANGE", "Level {0} must be in range [-{1}, {1}]." },
        { "DEVICE_QUERY_ERR", "Error querying number of devices: " },
        { "DEVICE_LIST_ERR", "Error when getting devices list: " },
        { "DEVICE_ALREADY_OPEN", "A device is already open." },
        { "OSC_RANGE_ERR", "Range must be 5 or 25." },
        { "OSC_AVERAGE_ERR", "Average should be positive" },
        { "OSC_DATA_RANGE_ERR", "Data points must be 32, 64, 128, 256, 512, 1024 or 2048." },
        { "CHANNEL_UNAVAILABLE", "No channel is enabled." },
        { "DEVICE_NOT_OPEN", "No device is open." },
        { "UNEXPECTED_WRITTEN_DATA_LENGTH", "Written data length is not expected." },
        { "UNEXPECTED_RECEIVED_DATA_LENGTH", "Received data length is not expected." },
        { "SUPPLY_ADJUSTING", "Another adjustment is in progress." },
        { "OSCILLOSCOPE_AMPLITUDE_POSITIVE", "Amplitude of a channel mush be positive." },
        { "PHASE_OUT_OF_RANGE", "Phase should be between 0 and 360 degrees." },

        // Webpage translations
        { "site-title-bar", "RainDrop Command Center" },
        { "site-title", "RainDrop Command Center" },
        { "site-lang-zh-CN", "中文" },
        { "site-lang-en", "English" },
        { "site-home", "Home" },
        { "site-osc", "Oscilloscope" },
        { "site-supply", "Power source" },
        { "site-wave", "Waveform" },
        {
            "site-msg-dll-error",
            "Dll load failed. Please download and install the driver (ftdi2xx) from <a href=\"https://ftdichip.com/drivers/d2xx-drivers/\">FTDI Official Website</a>."
        },
        {
            "site-msg-io-error",
            "Rain drop has experienced an IO error. Check the device status and try to reconnect in Home tab. You may need to completely power-off the device and re-power-on"
        },
        { "home-available-devices", "Available devices" },
        { "home-click-to-connect", "Click to try to connect to one." },
        { "home-no-device", "None" },
        { "home-unsupported-device", "Unsupported device" },
        { "home-currently-connected", "Currently connected:" },
        { "home-disconnect", "Disconnect" },
        { "home-refresh", "Refresh" },
        { "home-msg-disconnected", "Disconnected." },
        { "home-msg-connected", "Connected to {device}" },
        { "home-msg-fail-to-connect", "Failed to connect to {device}: {error}" },
        { "osc-device-attribute", "Device attributes" },
        { "osc-start", "Start" },
        { "osc-stop", "Stop" },
        { "osc-single", "Single" },
        { "osc-auto-set", "Auto Set" },
        { "osc-base", "Base (s)" },
        { "osc-rate", "Rate (Hz)" },
        { "osc-samples", "Samples" },
        { "osc-range", "Range (V/div)" },
        { "osc-offset", "Offset(V)" },
        { "osc-trigger-attribute", "Trigger attributes" },
        { "osc-trigger-source", "Trigger source" },
        { "osc-channel", "Channel {c}" },
        { "osc-trigger-mode", "Trigger mode" },
        { "osc-trigger-mode-rise", "Rise" },
        { "osc-trigger-mode-fall", "Fall" },
        { "osc-trigger-mode-edge", "Either" },
        { "osc-trigger-voltage", "Trigger voltage (V)" },
        { "osc-trigger-manual", "Manual Trigger" },
        {
            "site-msg-failed-to-retrieve-status",
            "Failed to retrieve device status. Please connect the device and refresh to try again."
        },
        { "osc-msg-invalid-timebase", "Invalid timebase: {time}" },
        { "osc-msg-too-long-timebase", "Too long timebase: {time}" },
        { "osc-msg-too-short-timebase", "Too short timebase: {time}" },
        { "osc-msg-invalid-rate", "Invalid sampling frequency: {frequency}" },
        { "osc-msg-too-low-rate", "{samplingFrequency}Hz is too low and will make the oscilloscope unresponsive" },
        { "osc-msg-too-high-rate", "{samplingFrequency / 1e6}MHz is too high and oscilloscope doesn't support it" },
        { "osc-msg-invalid-voltage", "Invalid voltage: {voltage}" },
        { "osc-msg-too-low-voltage", "{value}V is too low to be accurately displayed" },
        { "osc-msg-invalid-offset", "Invalid offset: {offset}" },
        { "osc-msg-sync-time-fail", "Failed to sync time: {error}" },
        { "osc-msg-sync-channel-fail", "Fail to sync channel {channel}: {error}" },
        { "osc-msg-sync-trigger-fail", "Fail to sync trigger: {error}" },
        { "osc-msg-catch-data-fail", "Failed to get data: {error}" },
        { "osc-msg-start-fail", "Failed to start: {error}" },
        { "osc-msg-stop-fail", "Failed to stop: {error}" },
        { "supply-positive-channel", "Positive Channel" },
        { "supply-output-voltage", "Output Voltage (V)" },
        { "supply-negative-channel", "Negative Channel" },
        { "supply-msg-invalid-value", "Invalid value." },
        { "supply-msg-too-high-vol", "{value}V is too high." },
        { "supply-msg-false-pos-neg-set", "{value}V should be {pos-neg}." },
        { "supply-msg-change-vol-fail", "Failed to change voltage: {error}" },
        { "supply-positive", "positive" },
        { "supply-negative", "negative" },
        { "wave-channel", "Channel {ch}" },
        { "wave-type", "Type" },
        { "wave-frequency", "Frequency (Hz)" },
        { "wave-amplitude", "Amplitude (V)" },
        { "wave-offset", "Offset (V)" },
        { "wave-sym", "Symmetry (%)" },
        { "wave-phase", "Phase (deg)" },
        { "wave-type-dc", "Direct Current" },
        { "wave-type-sine", "Sine" },
        { "wave-type-square", "Square" },
        { "wave-type-triangle", "Triangle" },
        { "wave-type-ramp-up", "Ramp Up" },
        { "wave-type-ramp-down", "Ramp Down" },
        { "wave-type-noise", "Noise" },
        { "wave-type-pulse", "Pulse" },
        { "wave-type-trapezium", "Trapezium" },
        { "wave-type-sine-power", "Sine Power" },
        { "wave-msg-unexpected-channel", "Unexpected Channel {ch}." },
        { "wave-msg-invalid-frequency", "Please Enter Valid Frequency." },
        { "wave-msg-too-high-frequency", "{value}Hz is too high." },
        { "wave-msg-not-positive-frequency", "{value}Hz should be a positive value." },
        { "wave-msg-invalid-amp", "Please Enter Correct Amplitude." },
        { "wave-msg-too-high-amp", "{value}V is too high." },
        { "wave-msg-too-low-amp", "{value}V is too low." },
        { "wave-msg-invalid-offset", "Please Enter Correct Offset." },
        { "wave-msg-too-high-offset", "{value}V is too high." },
        { "wave-msg-too-low-offset", "{value}V is too low." },
        { "wave-msg-invalid-sym", "Please Enter Correct sym." },
        { "wave-msg-too-high-sym", "{value}V is too high.。" },
        { "wave-msg-too-low-sym", "{value}V is too low." },
        { "wave-msg-invalid-phase", "Please Enter Correct Phase" }
    };

    private static readonly Dictionary<string, string> Chinese = new()
    {
        { "INVALID_WAVE_FUNCTION", "无效的波形函数。" },
        { "INVALID_WAVE_FREQUENCY", "无效的频率设定。" },
        { "INVALID_WAVE_OFFSET", "无效的直流偏置。" },
        { "INVALID_WAVE_AMP", "无效的振幅设定。" },
        { "INVALID_WAVE_SYM", "无效的对称度设定。" },
        { "FREQUENCY_OUT_OF_RANGE", "频率必须介于0到40兆赫兹之间。" },
        { "SYM_UNFIT", "对称度必须介于0到1之间。" },
        { "BYTE_STR_EVEN_LENGTH", "必须要有一个确定的长度。" },
        { "NEGATIVE_CH_VOL_ERR", "负电压必须设置在-5到0之间。" },
        { "POSITIVE_CH_VOL_ERR", "正电压必须设置在0到5之间。" },
        { "SIZE_ERR", "值的大小必须为32，64，128，256，514，1024，2048其中之一。" },
        { "LEVEL_OUT_OF_RANGE_1", "设定的电压{0}必须在[-{1}, {1}]范围内。" },
        { "DEVICE_QUERY_ERR", "查询设备数量错误：" },
        { "DEVICE_LIST_ERR", "获取设备列表时出错：" },
        { "DEVICE_OPEN_ERR", "已经打开了一个设备。" },
        { "OSC_RANGE_ERR", "极值应当设置为5或25。" },
        { "OSC_AVERAGE_ERR", "平均次数应当为正数。" },
        { "OSC_DATA_RANGE_ERR", "采样比应当设置为32，64，128，256，514，1024，2048其中之一。" },
        { "CHANNEL_UNAVAILABLE", "示波器暂无通道可用。" },
        { "DEVICE_NOT_OPEN", "没有打开任何设备。" },
        { "UNEXPECTED_WRITTEN_DATA_LENGTH", "写入数据长度不符合要求。" },
        { "UNEXPECTED_RECEIVED_DATA_LENGTH", "收到的数据长度不符合要求。" },
        { "SUPPLY_ADJUSTING", "另一个调整正在进行中。" },
        { "OSCILLOSCOPE_AMPLITUDE_POSITIVE", "通道的振幅必须为正。" },
        { "PHASE_OUT_OF_RANGE", "相位必须介于0到360度之间。" },

        // Webpage translations
        { "site-title-bar", "电实验" },
        { "site-title", "电实验" },
        { "site-lang-zh-CN", "中文" },
        { "site-lang-en", "English" },
        { "site-home", "主页" },
        { "site-osc", "示波器" },
        { "site-supply", "可调电源" },
        { "site-wave", "波形发生器" },
        {
            "site-msg-dll-error",
            "未正确加载设备驱动程序。请到<a href=\"https://ftdichip.com/drivers/d2xx-drivers/\">FTDI官网</a>下载并安装驱动程序 (ftdi2xx)。"
        },
        { "site-msg-io-error", "电实验遇到了IO错误。设备连接可能已经断开。请到主页重新连接设备。设备可能需要断电重启。" },
        { "home-available-devices", "可用设备" },
        { "home-click-to-connect", "单击以连接" },
        { "home-no-device", "无" },
        { "home-unsupported-device", "未知设备" },
        { "home-currently-connected", "当前连接的设备：" },
        { "home-disconnect", "断开连接" },
        { "home-refresh", "刷新" },
        { "home-msg-disconnected", "连接已断开。" },
        { "home-msg-connected", "已连接到 {device}" },
        { "home-msg-fail-to-connect", "无法连接到 {device}： {error}" },
        { "osc-device-attribute", "设备属性" },
        { "osc-start", "开始" },
        { "osc-stop", "停止" },
        { "osc-single", "单次" },
        { "osc-auto-set", "自动设置" },
        { "osc-base", "单格时间 (s)" },
        { "osc-rate", "采样频率 (Hz)" },
        { "osc-samples", "采样数" },
        { "osc-range", "单格电压 (V/div)" },
        { "osc-offset", "直流偏置 (V)" },
        { "osc-trigger-attribute", "触发属性" },
        { "osc-trigger-source", "触发源" },
        { "osc-channel", "{c}通道" },
        { "osc-trigger-mode", "触发模式" },
        { "osc-trigger-mode-rise", "上升沿" },
        { "osc-trigger-mode-fall", "下降沿" },
        { "osc-trigger-mode-edge", "双沿" },
        { "osc-trigger-voltage", "触发电压 (V)" },
        { "osc-trigger-manual", "手动触发" },
        { "site-msg-failed-to-retrieve-status", "无法获取设备信息。请连接设备后刷新重试。" },
        { "osc-msg-invalid-timebase", "不合理的单格时间:{time}" },
        { "osc-msg-too-long-timebase", "单格时间太长:{time}" },
        { "osc-msg-too-short-timebase", "单格时间太短:{time}" },
        { "osc-msg-invalid-rate", "不合理的采样频率{frequency}" },
        { "osc-msg-too-low-rate", "{samplingFrequency}Hz 太低，此时示波器不灵敏。" },
        { "osc-msg-too-high-rate", "{samplingFrequency / 1e6}MHz 过高，示波器不支持。" },
        { "osc-msg-invalid-voltage", "不合理的电压: {voltage}" },
        { "osc-msg-too-low-voltage", "{value}V 太低以至于不能正常显示。" },
        { "osc-msg-invalid-offset", "不合理的直流偏置: {offset}." },
        { "osc-msg-sync-time-fail", "同步时间失败: {error}" },
        { "osc-msg-sync-channel-fail", "同步通道{channel}失败: {error}" },
        { "osc-msg-sync-trigger-fail", "同步触发失败: {error}" },
        { "osc-msg-catch-data-fail", "获取数据失败: {error}" },
        { "osc-msg-start-fail", "启动失败: {error}" },
        { "osc-msg-stop-fail", "未能正常停止: {error}" },
        { "supply-positive-channel", "正通道" },
        { "supply-output-voltage", "输出电压 (V)" },
        { "supply-negative-channel", "负通道" },
        { "supply-msg-invalid-value", "不合理的电压设置。" },
        { "supply-msg-too-high-vol", "{value}V 电压过高。" },
        { "supply-msg-false-pos-neg-set", "{value}V 应当为{pos-neg}值" },
        { "supply-msg-change-vol-fail", "改变电压失败: {error}" },
        { "supply-positive", "正" },
        { "supply-negative", "负" },
        { "wave-channel", "通道{ch}" },
        { "wave-type", "类型" },
        { "wave-frequency", "频率 (Hz)" },
        { "wave-amplitude", "峰值 (V)" },
        { "wave-offset", "偏置 (V)" },
        { "wave-sym", "对称 (%)" },
        { "wave-phase", "初相 (deg)" },
        { "wave-type-dc", "直流" },
        { "wave-type-sine", "正弦波" },
        { "wave-type-square", "方波" },
        { "wave-type-triangle", "三角波" },
        { "wave-type-ramp-up", "锯齿波 (向上)" },
        { "wave-type-ramp-down", "锯齿波 (向下)" },
        { "wave-type-noise", "噪声" },
        { "wave-type-pulse", "脉冲" },
        { "wave-type-trapezium", "梯形波" },
        { "wave-type-sine-power", "交流" },
        { "wave-msg-unexpected-channel", "通道{ch}不合理。" },
        { "wave-msg-invalid-frequency", "请输入正确的频率。" },
        { "wave-msg-too-high-frequency", "{value}Hz 频率过高。" },
        { "wave-msg-not-positive-frequency", "{value}Hz 应为正数。" },
        { "wave-msg-invalid-amp", "请输入正确的幅值。" },
        { "wave-msg-too-high-amp", "{value}V 幅值过高。" },
        { "wave-msg-too-low-amp", "{value}V 幅值过低。" },
        { "wave-msg-invalid-offset", "请输入正确的偏置。" },
        { "wave-msg-too-high-offset", "{value}V 偏置过高。" },
        { "wave-msg-too-low-offset", "{value}V 偏置过低。" },
        { "wave-msg-invalid-sym", "请输入正确的对称度。" },
        { "wave-msg-too-high-sym", "{value}V 对称度过高。" },
        { "wave-msg-too-low-sym", "{value}V 对称度过低。" },
        { "wave-msg-invalid-phase", "请输入正确的相位" }
    };

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    public static string Localize(string key)
    {
        if (Culture.Name == "zh-CN")
            return Chinese.TryGetValue(key, out var value) ? value : key;
        else
            return English.TryGetValue(key, out var value) ? value : key;
    }
}