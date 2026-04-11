# 12.9.6 Programmer's Model

The SDK provides a hardware\_watchdog driver to control the watchdog.

#### **12.9.6.1. Enabling the watchdog**

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_watchdog/watchdog.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_watchdog/watchdog.c#L42-L74) Lines 42 - 74*

```
42 // Helper function used by both watchdog_enable and watchdog_reboot
43 void _watchdog_enable(uint32_t delay_ms, bool pause_on_debug) {
44 valid_params_if(HARDWARE_WATCHDOG, delay_ms <= WATCHDOG_LOAD_BITS / (1000 *
  WATCHDOG_XFACTOR));
45 hw_clear_bits(&watchdog_hw->ctrl, WATCHDOG_CTRL_ENABLE_BITS);
46 
47 // Reset everything apart from ROSC and XOSC
48 hw_set_bits(&psm_hw->wdsel, PSM_WDSEL_BITS & ~(PSM_WDSEL_ROSC_BITS |
  PSM_WDSEL_XOSC_BITS));
49 
50 uint32_t dbg_bits = WATCHDOG_CTRL_PAUSE_DBG0_BITS |
51 WATCHDOG_CTRL_PAUSE_DBG1_BITS |
52 WATCHDOG_CTRL_PAUSE_JTAG_BITS;
53 
54 if (pause_on_debug) {
55 hw_set_bits(&watchdog_hw->ctrl, dbg_bits);
56 } else {
57 hw_clear_bits(&watchdog_hw->ctrl, dbg_bits);
58 }
59
```

```
60 if (!delay_ms) {
61 hw_set_bits(&watchdog_hw->ctrl, WATCHDOG_CTRL_TRIGGER_BITS);
62 } else {
63 load_value = delay_ms * 1000;
64 if (load_value > WATCHDOG_LOAD_BITS)
65 load_value = WATCHDOG_LOAD_BITS;
66 
67 watchdog_update();
68 
69 hw_set_bits(&watchdog_hw->ctrl, WATCHDOG_CTRL_ENABLE_BITS);
70 }
71 }
```

#### **12.9.6.2. Updating the watchdog counter**

*SDK: [https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2\\_common/hardware\\_watchdog/watchdog.c](https://github.com/raspberrypi/pico-sdk/blob/master/src/rp2_common/hardware_watchdog/watchdog.c#L24-L28) Lines 24 - 28*

```
24 static uint32_t load_value;
25 
26 void watchdog_update(void) {
27 watchdog_hw->load = load_value;
28 }
```

