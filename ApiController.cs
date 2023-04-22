using Microsoft.AspNetCore.Mvc;
using RainDropWeb.Driver;
using System.Text.Json;

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

        try {
            await Task.Run(() => { RainDrop.ConnectToDevice(serial); });
        } catch (Exception e) {
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
}