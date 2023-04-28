class i18n {
    static Chinese = {
        "site-title-bar": "电实验",
        "site-title": "电实验",
        "site-lang-zh-CN": "中文",
        "site-lang-en": "English",
        "site-home": "主页",
        "site-osc": "示波器",
        "site-supply": "可调电源",
        "site-wave": "波形发生器",
        "site-msg-dll-error": "未正确加载设备驱动程序。请到<a href=\"https://ftdichip.com/drivers/d2xx-drivers/\">FTDI官网</a>下载并安装驱动程序 (ftdi2xx)。",
        "home-available-devices": "可用设备",
        "home-click-to-connect": "单击以连接",
        "home-no-device": "无",
        "home-unsupported-device": "未知设备",
        "home-currently-connected": "当前连接的设备：",
        "home-disconnect": "断开连接",
        "home-refresh": "刷新",
        "home-msg-disconnected": "连接已断开。",
        "home-msg-connected": "已连接到 {device}",
        "home-msg-fail-to-connect": "无法连接到 {device}： {error}",
        "osc-device-attribute": "设备属性",
        "osc-start": "开始",
        "osc-stop": "停止",
        "osc-single": "单次",
        "osc-base": "单格时间 (s)",
        "osc-rate": "采样频率 (Hz)",
        "osc-samples": "采样数",
        "osc-range": "单格电压 (V/div)",
        "osc-offset": "直流偏置 (V)",
        "osc-trigger-attribute": "触发属性",
        "osc-trigger-source": "触发源",
        "osc-channel": "{c}通道",
        "osc-trigger-mode": "触发模式",
        "osc-trigger-mode-rise": "上升沿",
        "osc-trigger-mode-fall": "下降沿",
        "osc-trigger-mode-edge": "双沿",
        "osc-trigger-voltage": "触发电压 (V)",
        "site-msg-failed-to-retrieve-status": "无法获取设备信息。请连接设备后刷新重试。",
        "osc-msg-invalid-timebase": "不合理的单格时间:{time}",
        "osc-msg-too-long-timebase": "单格时间太长:{time}",
        "osc-msg-too-short-timebase": "单格时间太短:{time}",
        "osc-msg-invalid-rate": "不合理的采样频率{frequency}",
        "osc-msg-too-low-rate": "{samplingFrequency}Hz 太低，此时示波器不灵敏。",
        "osc-msg-too-high-rate": "{samplingFrequency / 1e6}MHz 过高，示波器不支持。",
        "osc-msg-invalid-voltage": "不合理的电压: {voltage}",
        "osc-msg-too-low-voltage": "{value}V 太低以至于不能正常显示。",
        "osc-msg-invalid-offset": "不合理的直流偏置: {offset}.",
        "osc-msg-sync-time-fail": "同步时间失败: {error}",
        "osc-msg-sync-channel-fail": "同步通道{channel}失败: {error}",
        "osc-msg-sync-trigger-fail": "同步触发失败: {error}",
        "osc-msg-catch-data-fail": "获取数据失败: {error}",
        "osc-msg-start-fail": "启动失败: {error}",
        "osc-msg-stop-fail": "未能正常停止: {error}",
        "supply-positive-channel": "正通道",
        "supply-output-voltage": "输出电压 (V)",
        "supply-negative-channel": "负通道",
        "supply-msg-invalid-value": "不合理的电压设置。",
        "supply-msg-too-high-vol": "{value}V 电压过高。",
        "supply-msg-false-pos-neg-set": "{value}V 应当为{pos-neg}值",
        "supply-msg-change-vol-fail": "改变电压失败: {error}",
        "supply-positive": "正",
        "supply-negative": "负",
        "wave-channel": "通道{ch}",
        "wave-type": "类型",
        "wave-frequency": "频率 (Hz)",
        "wave-amplitude": "峰值 (V)",
        "wave-offset": "偏置 (V)",
        "wave-sym": "对称 (%)",
        "wave-phase": "初相 (deg)",
        "wave-type-dc": "直流",
        "wave-type-sine": "正弦波",
        "wave-type-square": "方波",
        "wave-type-triangle": "三角波",
        "wave-type-ramp-up": "锯齿波 (向上)",
        "wave-type-ramp-down": "锯齿波 (向下)",
        "wave-type-noise": "噪声",
        "wave-type-pulse": "脉冲",
        "wave-type-trapezium": "梯形波",
        "wave-type-sine-power": "交流",
        "wave-msg-unexpected-channel": "通道{ch}不合理。",
        "wave-msg-invalid-frequency": "请输入正确的频率。",
        "wave-msg-too-high-frequency": "{value}Hz 频率过高。",
        "wave-msg-not-positive-frequency": "{value}Hz 应为正数。",
        "wave-msg-invalid-amp": "请输入正确的幅值。",
        "wave-msg-too-high-amp": "{value}V 幅值过高。",
        "wave-msg-too-low-amp": "{value}V 幅值过低。",
        "wave-msg-invalid-offset": "请输入正确的偏置。",
        "wave-msg-too-high-offset": "{value}V 偏置过高。",
        "wave-msg-too-low-offset": "{value}V 偏置过低。",
        "wave-msg-invalid-sym": "请输入正确的对称度。",
        "wave-msg-too-high-sym": "{value}V 对称度过高。",
        "wave-msg-too-low-sym": "{value}V 对称度过低。",
        "wave-msg-invalid-phase": "请输入正确的相位",
    }

    static English = {
        "site-title-bar": "RainDrop Command Center",
        "site-title": "RainDrop Command Center",
        "site-lang-zh-CN": "中文",
        "site-lang-en": "English",
        "site-home": "Home",
        "site-osc": "Oscilloscope",
        "site-supply": "Power source",
        "site-wave": "Waveform",
        "site-msg-dll-error": "Dll load failed. Please download and install the driver (ftdi2xx) from <a href=\"https://ftdichip.com/drivers/d2xx-drivers/\">FTDI Official Website</a>.",
        "home-available-devices": "Available devices",
        "home-click-to-connect": "Click to try to connect to one.",
        "home-no-device": "None",
        "home-unsupported-device": "Unsupported device",
        "home-currently-connected": "Currently connected:",
        "home-disconnect": "Disconnect",
        "home-refresh": "Refresh",
        "home-msg-disconnected": "Disconnected.",
        "home-msg-connected": "Connected to {device}",
        "home-msg-fail-to-connect": "Failed to connect to {device}: {error}",
        "osc-device-attribute": "Device attributes",
        "osc-start": "Start",
        "osc-stop": "Stop",
        "osc-single": "Single",
        "osc-base": "Base (s)",
        "osc-rate": "Rate (Hz)",
        "osc-samples": "Samples",
        "osc-range": "Range (V/div)",
        "osc-offset": "Offset(V)",
        "osc-trigger-attribute": "Trigger attributes",
        "osc-trigger-source": "Trigger source",
        "osc-channel": "Channel {c}",
        "osc-trigger-mode": "Trigger mode",
        "osc-trigger-mode-rise": "Rise",
        "osc-trigger-mode-fall": "Fall",
        "osc-trigger-mode-edge": "Either",
        "osc-trigger-voltage": "Trigger voltage (V)",
        "site-msg-failed-to-retrieve-status": "Failed to retrieve device status. Please connect the device and refresh to try again.",
        "osc-msg-invalid-timebase": "Invalid timebase: {time}",
        "osc-msg-too-long-timebase": "Too long timebase: {time}",
        "osc-msg-too-short-timebase": "Too short timebase: {time}",
        "osc-msg-invalid-rate": "Invalid sampling frequency: {frequency}",
        "osc-msg-too-low-rate": "{samplingFrequency}Hz is too low and will make the oscilloscope unresponsive",
        "osc-msg-too-high-rate": "{samplingFrequency / 1e6}MHz is too high and oscilloscope doesn't support it",
        "osc-msg-invalid-voltage": "Invalid voltage: {voltage}",
        "osc-msg-too-low-voltage": "{value}V is too low to be accurately displayed",
        "osc-msg-invalid-offset": "Invalid offset: {offset}",
        "osc-msg-sync-time-fail": "Failed to sync time: {error}",
        "osc-msg-sync-channel-fail": "Fail to sync channel {channel}: {error}",
        "osc-msg-sync-trigger-fail": "Fail to sync trigger: {error}",
        "osc-msg-catch-data-fail": "Failed to get data: {error}",
        "osc-msg-start-fail": "Failed to start: {error}",
        "osc-msg-stop-fail": "Failed to stop: {error}",
        "supply-positive-channel": "Positive Channel",
        "supply-output-voltage": "Output Voltage (V)",
        "supply-negative-channel": "Negative Channel",
        "supply-msg-invalid-value": "Invalid value.",
        "supply-msg-too-high-vol": "{value}V is too high.",
        "supply-msg-false-pos-neg-set": "{value}V should be {pos-neg}.",
        "supply-msg-change-vol-fail": "Failed to change voltage: {error}",
        "supply-positive": "positive",
        "supply-negative": "negative",
        "wave-channel": "Channel {ch}",
        "wave-type": "Type",
        "wave-frequency": "Frequency (Hz)",
        "wave-amplitude": "Amplitude (V)",
        "wave-offset": "Offset (V)",
        "wave-sym": "Symmetry (%)",
        "wave-phase": "Phase (deg)",
        "wave-type-dc": "Direct Current",
        "wave-type-sine": "Sine",
        "wave-type-square": "Square",
        "wave-type-triangle": "Triangle",
        "wave-type-ramp-up": "Ramp Up",
        "wave-type-ramp-down": "Ramp Down",
        "wave-type-noise": "Noise",
        "wave-type-pulse": "Pulse",
        "wave-type-trapezium": "Trapezium",
        "wave-type-sine-power": "Sine Power",
        "wave-msg-unexpected-channel": "Unexpected Channel {ch}.",
        "wave-msg-invalid-frequency": "Please Enter Valid Frequency.",
        "wave-msg-too-high-frequency": "{value}Hz is too high.",
        "wave-msg-not-positive-frequency": "{value}Hz should be a positive value.",
        "wave-msg-invalid-amp": "Please Enter Correct Amplitude.",
        "wave-msg-too-high-amp": "{value}V is too high.",
        "wave-msg-too-low-amp": "{value}V is too low.",
        "wave-msg-invalid-offset": "Please Enter Correct Offset.",
        "wave-msg-too-high-offset": "{value}V is too high.",
        "wave-msg-too-low-offset": "{value}V is too low.",
        "wave-msg-invalid-sym": "Please Enter Correct sym.",
        "wave-msg-too-high-sym": "{value}V is too high.。",
        "wave-msg-too-low-sym": "{value}V is too low.",
        "wave-msg-invalid-phase": "Please Enter Correct Phase",
    }

    static locale = i18n.English;

    static Localize(key) {
        return this.locale[key] ?? key;
    }

    static LocalizeFile() {
        $("[i18n-key]").each(function () {
            const key = $(this).attr("i18n-key");
            $(this).html(i18n.Localize(key));
        });
    }

    static SetLocale(locale) {
        if (locale === "zh-CN") {
            this.locale = i18n.Chinese;
        } else {
            this.locale = i18n.English;
        }
    }

    static SetGlobalLocale(locale) {
        $.ajax({
            method: "POST",
            url: "/Api/Language",
            data: {
                language: locale
            },
            success: function (data) {
                window.location.reload();
            }
        });
    }
}

$.ajax({
    method: "GET",
    url: "/Api/Language",
    async: false,
    success: function (data) {
        i18n.SetLocale(data.language);
    },
    error: function (data) {
        i18n.SetLocale("en");
        setTimeout(() => {
            mdui.alert("Failed to get language. Is the server running properly?");
        }, 500);
    }
})
