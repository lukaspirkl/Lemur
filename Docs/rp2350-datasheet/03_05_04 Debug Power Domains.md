# 3.5.4 Debug Power Domains

The SW-DP and the RP-AP are in the always-on power domain. This means they are available even when the system is in its lowest-power state, with the switched core domain (which includes the processors) fully powered down.

The remainder of the debug hardware is in the switched core domain. This is the same domain as the processors and system peripherals.

Setting the CDBGPWRUPREQ bit in the SW-DP's CTRL/STAT register will force a power up of the switched core domain, making the remaining debug hardware available. This power up takes some time, as it is sequenced by the 32 kHz lowpower oscillator [\(Section 8.4\)](#page-566-0), so the CDBGPWRUPACK bit must be polled to wait for the system to power up before attempting to access any APs other than the RP-AP. See Arm's ADIv6 specification for the SW-DP's register listing.

Note that the RP-AP is accessible without asserting CDBGPWRUPREQ, as it is always powered.

## <span id="page-87-1"></span>**3.5.5. Software control of SWD pins**

The [DBGFORCE](#page-1250-0) register in SYSCFG can be used to detach the SW-DP from the external debug pads, and instead bitbang the internal SWD signals directly from software. This is intended for a debug probe running on one core being used to debug the other core. For other use cases it is generally cleaner to use the self-hosted debug access to interface with the APs directly from the system bus.

