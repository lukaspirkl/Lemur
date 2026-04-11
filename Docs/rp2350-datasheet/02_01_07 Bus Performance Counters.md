# 2.1.7 Bus Performance Counters

Bus performance counters automatically count accesses to the main AHB5 crossbar arbiters. These counters can help diagnose high-traffic performance issues.

There are four performance counters, starting at [PERFCTR0](#page-1254-0). Each is a 24-bit saturating counter. Counter values can be read from BUSCTRL\_PERFCTRx and cleared by writing any value to BUSCTRL\_PERFCTRx. Each counter can count one of the 20 available events at a time, as selected by BUSCTRL\_PERFSELx. For more information, see [Section 12.15.4.](#page-1252-0)

