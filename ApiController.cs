using Microsoft.AspNetCore.Mvc;
using RainDropWeb.Protocol;

namespace RainDropWeb;

[Route("Api")]
[ApiController]
public class ApiController : ControllerBase
{
    private static readonly RainDrop RainDrop = new();

    [Route("Info")]
    public async Task<IActionResult> GetInfo()
    {
        Response.ContentType = "application/json";
        var devices = await Task.Run(RainDrop.GetDevices);
        return Ok(devices.ToList());
    }

    [Route("Connect/{serial}")]
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
    
    [Route("Test")]
    public Task<IActionResult> Test()
    {
        Response.ContentType = "application/json";

        RainDrop.SetOscilloscopeChannelState(false, true);
        RainDrop.SetOscilloscopeChannelRange(false, 5);
        RainDrop.SetOscilloscopeSamplingFrequency(1e6f);
        RainDrop.SetOscilloscopeTrigger(true, OscilloscopeTriggerSource.DetectorAnalogInCh1, 0,
            OscilloscopeTriggerCondition.Edge);
        RainDrop.SetOscilloscopeDataPointsCount(2048);
        RainDrop.SetOscilloscopeRunning(true);
        
        if (RainDrop.GetOscilloscopeStatus() != 2)
            return Task.FromResult<IActionResult>(Ok(new { success = false, error = "Oscilloscope not running." }));
        
        var data = RainDrop.ReadOscilloscopeData(false);
        
        return Task.FromResult<IActionResult>(Ok(new { success = true, data = data }));
    }
}