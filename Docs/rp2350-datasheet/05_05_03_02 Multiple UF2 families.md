# 5.5.3.2 Multiple UF2 families

It is possible to include sectors targeting different family IDs in the same UF2 file. The intention in the UF2 specification is to allow one file to be shipped for multiple different devices, but the expectation is that each device only accepts one UF2 family ID.

Similarly on RP2350, it is only supported to download a UF2 file containing multiple family IDs if *only one* of those family IDs is acceptable for download to the device according to the above rules.

