using Microsoft.AspNetCore.Mvc;
using RainDropWeb.Protocol;

namespace RainDropWeb;

[Route("Api")]
[ApiController]
public class ApiController : ControllerBase
{
    private static readonly RainDrop RainDrop = new();

    // This is to prevent multiple threads from reading the oscilloscope data at the same time.
    private static readonly Mutex OscilloscopeReadMutex = new();

    private static bool _isAdjustingSupplyVoltage;

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

        return Task.FromResult<IActionResult>(Ok(new[] { RainDrop.CurrentDevice }));
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
        [FromForm] bool is25V)
    {
        Response.ContentType = "application/json";

        if (channel is not (0 or 1))
            return Ok(new { success = false, error = "Invalid channel." });

        try
        {
            await Task.Run(() =>
            {
                RainDrop.SetOscilloscopeChannelState(channel == 1, enabled);
                RainDrop.SetOscilloscopeChannelRange(channel == 1, is25V ? 25 : 5);
            });
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
    public async Task<IActionResult> SetOscilloscope([FromForm] float frequency, [FromForm] int samples)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() =>
            {
                RainDrop.SetOscilloscopeSamplingFrequency(frequency);
                RainDrop.SetOscilloscopeDataPointsCount(samples);
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
    public async Task<IActionResult> SetSupplyEnabled([FromForm] bool isNegativeChannel, [FromForm] bool enable)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() => { RainDrop.SetSupplyEnabled(isNegativeChannel, enable); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Supply/Voltage")]
    [HttpPost]
    public async Task<IActionResult> SetSupplyVoltage([FromForm] bool isNegativeChannel, [FromForm] float voltage)
    {
        Response.ContentType = "application/json";

        if (_isAdjustingSupplyVoltage)
            return Ok(new { success = false, error = "Another adjustment is in progress." });

        _isAdjustingSupplyVoltage = true;

        try
        {
            await Task.Run(() => { RainDrop.SetSupplyVoltage(isNegativeChannel, voltage); });
        }
        catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }
        finally
        {
            _isAdjustingSupplyVoltage = false;
        }

        return Ok(new { success = true });
    }
}