using Donclub.Application.Auth;
using Donclub.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperUser,Admin")]
public class SmsProviderSettingsController : ControllerBase
{
    private readonly ISmsProviderSettingsService _service;
    private readonly ISmsSender _smsSender;

    public SmsProviderSettingsController(ISmsProviderSettingsService service, ISmsSender smsSender)
    {
        _service = service;
        _smsSender = smsSender;
    }

    // GET: api/SmsProviderSettings
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SmsProviderSettingDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    // GET: api/SmsProviderSettings/active
    [HttpGet("active")]
    public async Task<ActionResult<SmsProviderSettingDto?>> GetActive(CancellationToken ct)
        => Ok(await _service.GetActiveAsync(ct));

    // GET: api/SmsProviderSettings/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SmsProviderSettingDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST: api/SmsProviderSettings
    [HttpPost]
    public async Task<ActionResult<SmsProviderSettingDto>> Create(CreateSmsProviderSettingRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/SmsProviderSettings/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SmsProviderSettingDto>> Update(int id, UpdateSmsProviderSettingRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    // DELETE: api/SmsProviderSettings/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    // POST: api/SmsProviderSettings/{id}/activate
    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await _service.SetActiveAsync(id, ct);
        return NoContent();
    }

    public record TestSmsRequest(string To, string Text);

    // POST: api/SmsProviderSettings/test
    [HttpPost("test")]
    public async Task<IActionResult> TestSend(TestSmsRequest request, CancellationToken ct)
    {
        await _smsSender.SendAsync(request.To, request.Text, ct);
        return Ok(new { ok = true });
    }
}
