using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RainDropWeb.Locale;
using RainDropWeb.Protocol;

namespace RainDropWeb.Controllers;

[Route("Api")]
[ApiController]
public class ApiController : ControllerBase
{
    private static readonly RainDrop RainDrop = new();

    // This is to prevent multiple threads from reading the oscilloscope data at the same time.
    private static readonly Mutex OscilloscopeReadMutex = new();

    [Route("Language")]
    public IActionResult GetLanguage()
    {
        Response.ContentType = "application/json";
        return Ok(new { language = Localization.Culture.Name });
    }

    [Route("Language")]
    [HttpPost]
    public IActionResult SetLanguage([FromForm] string language)
    {
        Response.ContentType = "application/json";
        Localization.Culture = new CultureInfo(language);
        return Ok(new { success = true });
    }

    [Route("Info")]
    public async Task<IActionResult> GetInfo()
    {
        Response.ContentType = "application/json";
        var devices = await Task.Run(RainDrop.GetDevices);
        return Ok(devices.ToList());
    }

    [Route("Connect/{serial}")]
    [HttpPost]
    public async Task<IActionResult> ConnectToDevice([FromRoute] string serial)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() => { RainDrop.ConnectToDevice(serial); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Disconnect")]
    [HttpPost]
    public async Task<IActionResult> DisconnectFromDevice()
    {
        Response.ContentType = "application/json";

        await Task.Run(RainDrop.DisconnectFromDevice);

        return Ok(new { success = true });
    }

    [Route("Current")]
    public Task<IActionResult> GetCurrentStatus()
    {
        Response.ContentType = "application/json";

        return Task.FromResult<IActionResult>(Ok(new { success = true, data = RainDrop.GetStatus() }));
    }

    [Route("Status")]
    public async Task<IActionResult> GetDeviceStatus()
    {
        Response.ContentType = "application/json";

        var status = RainDrop.DeviceStatus.Ready;

        try
        {
            await Task.Run(() => { status = RainDrop.GetDeviceStatus(); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true, data = status.ToString() });
    }

    [Route("Oscilloscope/Channel/{channel:int}")]
    [HttpPost]
    public async Task<IActionResult> SetOscilloscopeChannel([FromRoute] int channel, [FromForm] bool enabled,
        [FromForm] float offset, [FromForm] float amplitude)
    {
        Response.ContentType = "application/json";

        if (channel is not (0 or 1))
            return Ok(new { success = false, error = "Invalid channel." });

        if (amplitude <= 0)
            return Ok(new { success = false, error = Localization.Localize("OSCILLOSCOPE_AMPLITUDE_POSITIVE") });

        try
        {
            await Task.Run(() => { RainDrop.SetOscilloscopeChannel(channel == 1, enabled, offset, amplitude); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Oscilloscope/Trigger")]
    [HttpPost]
    public async Task<IActionResult> SetOscilloscopeTrigger([FromForm] bool autoTimeout, [FromForm] int source,
        [FromForm] float level, [FromForm] int condition)
    {
        Response.ContentType = "application/json";

        if (source is not (0 or 1))
            return Ok(new { success = false, error = "Invalid trigger source." });

        if (condition is not (>= 0 and <= 2))
            return Ok(new { succcess = false, error = "Invalid trigger condition." });

        try
        {
            await Task.Run(() =>
            {
                RainDrop.SetOscilloscopeTrigger(autoTimeout,
                    source == 0
                        ? OscilloscopeTriggerSource.DetectorAnalogInCh1
                        : OscilloscopeTriggerSource.DetectorAnalogInCh2, level,
                    (OscilloscopeTriggerCondition)condition);
            });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Oscilloscope")]
    [HttpPost]
    public async Task<IActionResult> SetOscilloscope([FromForm] float frequency, [FromForm] int samples,
        [FromForm] float timeBase, [FromForm] int average)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() =>
            {
                RainDrop.SetOscilloscopeSamplingFrequency(frequency);
                RainDrop.SetOscilloscopeDataPointsCount(samples);
                RainDrop.OscilloscopeTimebase = timeBase;
                RainDrop.OscilloscopeAverage = average;
            });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Oscilloscope/Start")]
    [HttpPost]
    public async Task<IActionResult> StartOscilloscope()
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() => { RainDrop.SetOscilloscopeRunning(true); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Oscilloscope/Stop")]
    [HttpPost]
    public async Task<IActionResult> StopOscilloscope()
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() => { RainDrop.SetOscilloscopeRunning(false); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Oscilloscope/Read")]
    public async Task<IActionResult> ReadOscilloscopeChannel([FromRoute] int channel)
    {
        Response.ContentType = "application/json";

        try
        {
            return await Task.Run(() =>
            {
                if (!RainDrop.OscilloscopeRunning)
                    return Ok(new { success = false, error = "Oscilloscope is not running." });

                OscilloscopeReadMutex.WaitOne();
                try
                {
                    var retry = 16;

                    while (retry-- > 0 && RainDrop.GetDeviceStatus() != RainDrop.DeviceStatus.Done)
                        Task.Delay(10).Wait();

                    if (retry == 0)
                        return Ok(new { success = false, error = "Oscilloscope not ready." });

                    var data = RainDrop.ReadOscilloscopeData();
                    return Ok(new { success = true, data = new { a = data.A, b = data.B } });
                }
                finally
                {
                    OscilloscopeReadMutex.ReleaseMutex();
                }
            });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }
    }

    [Route("Supply")]
    [HttpPost]
    public async Task<IActionResult> SetSupply([FromForm] bool isNegativeChannel, [FromForm] float voltage,
        [FromForm] bool enabled)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() =>
            {
                // Disable supply first to save time
                if (!enabled)
                {
                    RainDrop.SetSupplyEnabled(isNegativeChannel, false);
                    RainDrop.SetSupplyVoltage(isNegativeChannel, voltage);
                }
                else
                {
                    RainDrop.SetSupplyVoltage(isNegativeChannel, voltage);
                    RainDrop.SetSupplyEnabled(isNegativeChannel, true);
                }
            });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("WaveGenerator")]
    [HttpPost]
    public async Task<IActionResult> SetWaveGeneratorProperty([FromForm] bool isChannel2,
        [FromForm] WaveGeneratorFunction function, [FromForm] float frequency, [FromForm] float offset,
        [FromForm] float amplitude, [FromForm] float symmetry, [FromForm] float phase)
    {
        Response.ContentType = "application/json";

        if ((int)function is not (>= 0 and <= 9))
            return Ok(new { success = false, error = Localization.Localize("INVALID_WAVE_FUNCTION") });

        if (frequency is not (> 0 and <= 40e6f))
            return Ok(new { success = false, error = Localization.Localize("INVALID_WAVE_FREQUENCY") });

        if (offset is not (>= -5 and <= 5))
            return Ok(new { success = false, error = Localization.Localize("INVALID_WAVE_OFFSET") });

        if (amplitude is not (>= -5 and <= 5))
            return Ok(new { success = false, error = Localization.Localize("INVALID_WAVE_AMP") });

        if (symmetry is not (>= 0 and <= 1))
            return Ok(new { success = false, error = Localization.Localize("INVALID_WAVE_SYM") });

        phase %= 360;

        try
        {
            await Task.Run(() =>
            {
                RainDrop.SetWaveGeneratorFunction(isChannel2, function);
                RainDrop.SetWaveGeneratorFrequency(isChannel2, frequency);
                RainDrop.SetWaveGeneratorOffsetAndAmplitude(isChannel2, offset, amplitude);
                RainDrop.SetWaveGeneratorSymmetry(isChannel2, symmetry);
                RainDrop.SetWaveGeneratorPhase(isChannel2, phase);
            });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("WaveGenerator/Enable")]
    [HttpPost]
    public async Task<IActionResult> SetWaveGeneratorEnabled([FromForm] bool isChannel2, [FromForm] bool enabled)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() => { RainDrop.SetWaveGeneratorEnabled(isChannel2, enabled); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }
}