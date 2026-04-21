using System.Collections.Generic;

namespace Lemur;

public record BinaryInfo(
    string? ProgramName,
    string? ProgramVersion,
    string? ProgramUrl,
    string? BuildDate,
    string? BuildType,
    string? TargetBoard,
    string? Boot2Name,
    IReadOnlyList<PinInfo> Pins);

public record PinInfo(IReadOnlyList<int> Pins, string Label);
