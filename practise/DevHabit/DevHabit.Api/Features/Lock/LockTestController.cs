using DevHabit.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Features.Lock;

[ApiController]
[Route("api/locktest")]
public class LockTestController : ControllerBase
{
    private readonly PostgresAdvisoryLockService _lockService;

    public LockTestController(PostgresAdvisoryLockService lockService)
    {
        _lockService = lockService;
    }

    [HttpGet]
    public async Task<IActionResult> TestLock()
    {
        var key = "lock:test";

        var t1 = Task.Run(async () =>
        {
            Console.WriteLine("T1: Trying to acquire lock...");
            await _lockService.AcquireLockAsync(key);
            Console.WriteLine("T1: Lock acquired!");
            await Task.Delay(5000);
            await _lockService.ReleaseLockAsync(key);
            Console.WriteLine("T1: Lock released!");
        });

        var t2 = Task.Run(async () =>
        {
            await Task.Delay(1000);
            Console.WriteLine("T2: Trying to acquire lock...");
            await _lockService.AcquireLockAsync(key);
            Console.WriteLine("T2: Lock acquired!");
            await _lockService.ReleaseLockAsync(key);
            Console.WriteLine("T2: Lock released!");
        });

        await Task.WhenAll(t1, t2);

        return Ok("Lock test finished — check console output.");
    }
}
