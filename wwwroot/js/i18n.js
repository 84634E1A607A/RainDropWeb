class i18n {
    static Chinese  = {
        "site-title-bar": "锐智屿实验",
        "site-title": "锐智屿实验",
        "site-home": "主页",
        "site-osc": "示波器",
        "site-supply": "可调电源",
        "site-wave": "波形发生器",
        "home-available-devices": "可用设备",
        "home-click-to-connect": "单击以连接",
        "home-no-device": "无",
        "home-unsupported-device": "不知道是啥的设备",
        "home-currently-connected": "当前连接的设备：",
        "home-disconnect": "断开连接",
        "home-msg-disconnected": "连接已断开。",
        "home-msg-connected": "已连接到 {device}",
        "home-msg-fail-to-connect": "无法连接到 {device}： {error}"
    }

    static English = {
        "site-title-bar": "RainDrop Command Center",
        "site-title": "RainDrop Command Center",
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
        "home-msg-fail-to-connect": "Failed to connect to {device}: {error}"
    }

    static locale = i18n.Chinese;

    static Localize(key) {
        return this.locale[key] ?? key;
    }

    static LocalizeFile() {
        $("[i18n-key]").each(function () {
            var key = $(this).attr("i18n-key");
            $(this).html(i18n.Localize(key));
        });
    }
}
