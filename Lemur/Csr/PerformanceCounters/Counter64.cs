namespace Lemur.Csr.PerformanceCounters;

// Shared 64-bit backing store for counter pairs (mcycle/mcycleh, minstret/minstreth).
public class Counter64 { public ulong Value { get; set; } }
