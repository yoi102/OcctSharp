using System.Text.Json;

internal static class CapturePlanFixtures
{
    internal static int Run()
    {
        int cases = 0;
        void Accept(DesktopRegion client, DesktopRegion[] outputs, int parts)
        { if (ApplicationDesktopCapture.Plan(client, outputs).Length != parts) throw new InvalidOperationException("Wrong capture plan."); cases++; }
        void Reject(DesktopRegion client, DesktopRegion[] outputs)
        { try { ApplicationDesktopCapture.Plan(client, outputs); } catch (ArgumentException) { cases++; return; } catch (InvalidOperationException) { cases++; return; } throw new InvalidOperationException("Invalid plan accepted."); }
        DesktopRegion left = new(0, 0, 1920, 1080), right = new(1920, 0, 1920, 1080), negative = new(-1280, 0, 1280, 1024);
        Accept(new(10, 10, 100, 200), [left], 1);
        Accept(left, [left], 1);
        Accept(new(2200, 100, 1000, 800), [left, right], 1);
        Accept(new(1800, 100, 600, 800), [left, right], 2);
        Accept(new(-100, 100, 200, 600), [negative, left], 2);
        Accept(new(0, 0, 1920, 1080), [left, left], 1);
        Reject(new(1900, 0, 40, 100), [left]);
        Reject(new(10, 1070, 20, 40), [left]);
        Reject(new(10, 10, 0, 100), [left]);
        Reject(new(1900, 0, 200, 100), [left, new(2000, 0, 1920, 1080)]);
        Reject(new(1700, 0, 300, 100), [left, new(1600, 0, 1920, 1080)]);
        var target = new byte[24]; var client = new DesktopRegion(-1, -1, 3, 2);
        ApplicationDesktopCapture.CopyPart(target, client, Enumerable.Repeat((byte)1, 8).ToArray(), new(-1, -1, 1, 2));
        ApplicationDesktopCapture.CopyPart(target, client, Enumerable.Repeat((byte)2, 16).ToArray(), new(0, -1, 2, 2));
        if (!target.SequenceEqual(new byte[] { 1,1,1,1,2,2,2,2,2,2,2,2,1,1,1,1,2,2,2,2,2,2,2,2 })) throw new InvalidOperationException("Incorrect cross-output row copy.");
        cases++;
        long now = TimeSpan.TicksPerSecond * 100;
        foreach (var (source, expected) in new (long, bool)[] { (now, true), (now - TimeSpan.TicksPerMillisecond * 250, true), (now - TimeSpan.TicksPerMillisecond * 251, false), (now - TimeSpan.TicksPerSecond * 9, false), (now + 1, false), (0, false) })
        {
            if (ApplicationWindowCapture.IsFresh(source, now) != expected) throw new InvalidOperationException("Incorrect source frame freshness admission.");
            cases++;
        }
        Console.WriteLine(JsonSerializer.Serialize(new { state = "PASS", cases, scope = "Observer rectangle planning and exact row composition only; no physical mixed-DPI/RDP credit." }));
        return 0;
    }
}
