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

    [Route("Info")]
    public async Task<IActionResult> GetInfo()
    {
        Response.ContentType = "application/json";
        var devices = await Task.Run(RainDrop.GetDevices);
        return Ok(devices.ToList());
    }

    [Route("Connect/{serial}"), HttpPost]
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

    [Route("Disconnect"), HttpPost]
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

    [Route("Oscilloscope/Channel/{channel:int}"), HttpPost]
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

    [Route("Oscilloscope"), HttpPost]
    public async Task<IActionResult> SetOscilloscope([FromForm] float frequency, [FromForm] int samples)
    {
        Response.ContentType = "application/json";

        try
        {
            await Task.Run(() =>
            {
                RainDrop.SetOscilloscopeSamplingFrequency(frequency);
                RainDrop.SetOscilloscopeDataPointsCount(samples);
                
                // TODO:
                RainDrop.SetOscilloscopeTrigger(true, OscilloscopeTriggerSource.DetectorAnalogInCh1, 0,
                    OscilloscopeTriggerCondition.Edge);
            });
        } catch (Exception e)
        {
            return Ok(new { success = false, error = e.Message });
        }

        return Ok(new { success = true });
    }

    [Route("Oscilloscope/Start"), HttpPost]
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

    [Route("Oscilloscope/Stop"), HttpPost]
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
        try
        {
            return await Task.Run(() =>
            {
                if (!RainDrop.OscilloscopeRunning)
                    return Ok(new { success = false, error = "Oscilloscope is not running." });

                OscilloscopeReadMutex.WaitOne();
                try
                {
                    int retry = 16;

                    while (retry-- > 0 && RainDrop.GetOscilloscopeStatus() != RainDrop.DeviceStatus.Done)
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
}