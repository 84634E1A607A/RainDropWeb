using System.Globalization;

namespace RainDropWeb.Locale;

public static class Localization
{
    private static readonly Dictionary<string, string> English = new()
    {
        {"INVALID_WAVE_FUNCTION", "Invalid wave function."},
        {"INVALID_WAVE_FREQUENCY", "Invalid wave frequency."},
        {"INVALID_WAVE_OFFSET", "Invalid wave offset."},
        {"INVALID_WAVE_AMP", "Invalid wave amplitude."},
        {"INVALID_WAVE_SYM" , "Invalid wave symmetry."},
        {"FREQUENCY_OUT_OF_RANGE", "Frequency must be between 0 and 40 MHz."},
        {"SYM_UNFIT","Symmetry must be between 0 and 1." },
        {"BYTE_STR_EVEN_LENGTH", "must have an even length." },
        {"NEGATIVE_CH_VOL_ERR", "Negative channel voltage should be between -5 and 0." },
        {"POSITIVE_CH_VOL_ERR", "Positive channel voltage should be between 0 and 5." },
        {"SIZE_ERR", "Size should be 32, 64, 128, 256, 512, 1024 or 2048." },
        {"LEVEL_OUT_OF_RANGE", "Level {0} must be in range [-{1}, {1}]." },
        {"DEVICE_QUERY_ERR", "Error querying number of devices: " },
        {"DEVICE_LIST_ERR", "Error when getting devices list: " },
        {"DEVICE_ALREADY_OPEN", "A device is already open." },
        {"OSC_RANGE_ERR", "Range must be 5 or 25." },
        {"OSC_DATA_RANGE_ERR", "Data points must be 32, 64, 128, 256, 512, 1024 or 2048." },
        {"CHANNEL_UNAVAILABLE", "No channel is enabled." },
        {"DEVICE_NOT_OPEN", "No device is open." },
        {"UNEXPECTED_WRITTEN_DATA_LENGTH", "Written data length is not expected." },
        {"UNEXPECTED_RECEIVED_DATA_LENGTH", "Received data length is not expected." },
        {"SUPPLY_ADJUSTING", "Another adjustment is in progress."},
        {"OSCILLOSCOPE_AMPLITUDE_POSITIVE", "Amplitude of a channel mush be positive."}
    };

    private static readonly Dictionary<string, string> Chinese = new()
    {
        {"INVALID_WAVE_FUNCTION", "无效的波形函数。" },
        {"INVALID_WAVE_FREQUENCY", "无效的频率设定。" },
        {"INVALID_WAVE_OFFSET", "无效的直流偏置。"},
        {"INVALID_WAVE_AMP", "无效的振幅设定。"},
        {"INVALID_WAVE_SYM" , "无效的对称度设定。"},
        {"FREQUENCY_OUT_OF_RANGE", "频率必须介于0到40兆赫兹之间。"},
        {"SYM_UNFIT","对称度必须介于0到1之间。" },
        {"BYTE_STR_EVEN_LENGTH", "必须要有一个确定的长度。" },
        {"NEGATIVE_CH_VOL_ERR", "负电压必须设置在-5到0之间。" },
        {"POSITIVE_CH_VOL_ERR", "正电压必须设置在0到5之间。" },
        {"SIZE_ERR", "值的大小必须为32，64，128，256，514，1024，2048其中之一。" },
        {"LEVEL_OUT_OF_RANGE_1", "设定的电压{0}必须在[-{1}, {1}]范围内。" },
        {"DEVICE_QUERY_ERR", "查询设备数量错误：" },
        {"DEVICE_LIST_ERR", "获取设备列表时出错：" },
        {"DEVICE_OPEN_ERR", "已经打开了一个设备。" },
        {"OSC_RANGE_ERR", "极值应当设置为5或25。" },
        {"OSC_DATA_RANGE_ERR", "采样比应当设置为32，64，128，256，514，1024，2048其中之一。" },
        {"CHANNEL_UNAVAILABLE", "示波器暂无通道可用。" },
        {"DEVICE_NOT_OPEN", "没有打开任何设备。" },
        {"UNEXPECTED_WRITTEN_DATA_LENGTH", "写入数据长度不符合要求。" },
        {"UNEXPECTED_RECEIVED_DATA_LENGTH", "收到的数据长度不符合要求。" },
        {"SUPPLY_ADJUSTING", "另一个调整正在进行中。"},
    };

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    public static string Localize(string key)
    {
        if (Culture.Name == "zh-CN")
        {
            return Chinese.TryGetValue(key, out var value) ? value : key;
        }
        else
        {
            return English.TryGetValue(key, out var value) ? value : key;
        }
    }
}
