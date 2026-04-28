using System;

namespace Lemur.Peripherals;

public interface ITickable
{
    void Tick(TimeSpan now);
}
