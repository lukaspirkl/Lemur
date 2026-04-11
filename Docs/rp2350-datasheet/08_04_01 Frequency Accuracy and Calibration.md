# 8.4.1 Frequency Accuracy and Calibration

The low power oscillator has an initial frequency accuracy of ±20%. However, it can be trimmed to ±1.5% using the TRIM field in the LPOSC register. 63 trim steps are available, each between 1% and 3% of the oscillator's initial frequency. The frequency can be trimmed down by 32 steps or up by 31 steps. See [Table 615, "low power oscillator output frequency](#page-567-5) [and trimming"](#page-567-5) and [Section 8.4.3, "List of Registers"](#page-567-2) for details.

*Table 615. low power oscillator output frequency and trimming*

<span id="page-567-5"></span>

| Parameter  | Description                 | Min      | Typ    | Max      | Units                            |
|------------|-----------------------------|----------|--------|----------|----------------------------------|
| F0.initial | initial output<br>frequency | 26.2144  | 32.768 | 39.3216  | kHz                              |
| trimSTEP   | frequency trim<br>step      | -        | 1      | 3        | % of initial output<br>frequency |
| F0.trimmed | trimmed output<br>frequency | 32.27648 | 32.768 | 33.25952 | kHz                              |

Frequency drift with temperature: ±14%.

Frequency drift with power supply voltage: ±20%.

