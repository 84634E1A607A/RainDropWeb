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
        "home-available-devices": "可用设备",
        "home-click-to-connect": "单击以连接",
        "home-no-device": "无",
        "home-unsupported-device": "未知设备",
        "home-currently-connected": "当前连接的设备：",
        "home-disconnect": "断开连接",
        "home-msg-disconnected": "连接已断开。",
        "home-msg-connected": "已连接到 {device}",
        "home-msg-fail-to-connect": "无法连接到 {device}： {error}",
        "osc-device-attribute": "设备属性",
        "osc-start": "开始",
        "osc-stop": "停止",
        "osc-base": "单格时间 (s)",
        "osc-rate": "采样频率 (Hz)",
        "osc-samples": "采样数",
        "osc-range": "单格电压 (V/div)",
        "osc-offset": "直流偏置 (V)",
        "osc-trigger-attribute": "触发属性",
        "osc-trigger-source": "触发源",
        "osc-channel": "{c}通道",
        "osc-trigger-mode": "触发模式",
        "osc-triggermode-rise": "上升沿",
        "osc-triggermode-fall": "下降沿",
        "osc-triggermode-edge": "双沿",
        "osc-trigger-voltage": "触发电压 (V)",
        "osc-msg-invalid-timebase": "不合理的单格时间:{time}",
        "osc-msg-too-long-timebase": "单格时间太长:{time}",
        "osc-msg-too-short-timebase": "单格时间太短:{time}",
        "osc-msg-invalid-rate": "不合理的采样频率{frequency}",
        "osc-msg-too-low-rate": "{samplingFrequency}Hz 太低，此时示波器不灵敏。",
        "osc-msg-too-high-rate": "{samplingFrequency / 1e6}MHz 过高，示波器不支持。",
        "osc-msg-invalid-voltage": "不合理的电压: {voltage}",
        "osc-msg-too-low-voltage": "{value}V 太低以至于不能正常显示。",
        "osc-msg-invalid-offset": "不合理的直流偏置: {offset}.",
        "osc-msg-sync-time-fail": "同步时间失败: {data.error}",
        "osc-msg-sync-channel-fail": "同步通道{channel}失败: {data.error}",
        "osc-msg-sync-trigger-fail": "同步触发失败: {data.error}",
        "osc-msg-catch-data-fail": "获取数据失败: {data.error}",
        "osc-msg-start-fail": "启动失败: {data.error}",
        "osc-msg-stop-fail": "未能正常停止: {data.error}",
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
        "home-available-devices": "Available devices",
        "home-click-to-connect": "Click to try to connect to one.",
        "home-no-device": "None",
        "home-unsupported-device": "Unsupported device",
        "home-currently-connected": "Currently connected:",
        "home-disconnect": "Disconnect",
        "home-msg-disconnected": "Disconnected.",
        "home-msg-connected": "Connected to {device}",
        "home-msg-fail-to-connect": "Failed to connect to {device}: {error}",
        "osc-device-attribute": "Device attributes",
        "osc-start": "Start",
        "osc-stop": "Stop",
        "osc-base": "Base (s)",
        "osc-rate": "Rate (Hz)",
        "osc-samples": "Samples",
        "osc-range": "Range (V/div)",
        "osc-offset": "Offset(V)",
        "osc-trigger-attribute": "Trigger attributes",
        "osc-trigger-source": "Trigger source",
        "osc-channel": "Channel {c}",
        "osc-trigger-mode": "Trigger mode",
        "osc-triggermode-rise": "Rise",
        "osc-triggermode-fall": "Fall",
        "osc-triggermode-edge": "Either",
        "osc-trigger-voltage": "Trigger voltage (V)",
        "osc-msg-invalid-timebase": "Invalid timebase: {time}",
        "osc-msg-too-long-timebase": "Too long timebase: {time}",
        "osc-msg-too-short-timebase": "Too short timebase: {time}",
        "osc-msg-invalid-rate": "Invalid sampling frequency: {frequency}",
        "osc-msg-too-low-rate": "{samplingFrequency}Hz is too low and will make the oscilloscope unresponsive",
        "osc-msg-too-high-rate": "{samplingFrequency / 1e6}MHz is too high and oscilloscope doesn't support it",
        "osc-msg-invalid-voltage": "Invalid voltage: {voltage}",
        "osc-msg-too-low-voltage": "{value}V is too low to be accurately displayed",
        "osc-msg-invalid-offset": "Invalid offset: {offset}",
        "osc-msg-sync-time-fail": "Failed to sync time: {data.error}",
        "osc-msg-sync-channel-fail": "Fail to sync channel {channel}: {data.error}",
        "osc-msg-sync-trigger-fail": "Fail to sync trigger: {data.error}",
        "osc-msg-catch-data-fail": "Failed to get data: {data.error}",
        "osc-msg-start-fail": "Failed to start: {data.error}",
        "osc-msg-stop-fail": "Failed to stop: {data.error}",
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
        if (locale==="zh-CN") {
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
        setTimeout(() => {mdui.alert("Failed to get language. Is the server running properly?");}, 500);
    }
})
