using Microsoft.AspNetCore.Mvc;
using RainDropWeb.Driver;
using System.Text.Json;

namespace RainDropWeb;

[Route("Api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly Ftdi _ftdi = new();
    
    [Route("Info")]
    public async Task<IActionResult> GetInfo()
    {
        var error = _ftdi.GetNumberOfDevices(out var devicesCount);
        if (error != Ftdi.FtStatus.FtOk)
        {
            throw new Exception($"Error querying number of devices: {error}");
        }

        if (devicesCount == 0)
        {
            return Ok("No devices found");
        }
        
        var devStatus = new Ftdi.FtDeviceInfoNode[devicesCount];
        for (var i = 0; i < devicesCount; ++i) devStatus[i] = new Ftdi.FtDeviceInfoNode();
        error = _ftdi.GetDeviceList(devStatus);
        if (error != Ftdi.FtStatus.FtOk)
        {
            throw new Exception($"Error when getting devices list: {error}");
        }
        
        await JsonSerializer.SerializeAsync(HttpContext.Response.Body, devStatus);
        return Ok();
    }
}