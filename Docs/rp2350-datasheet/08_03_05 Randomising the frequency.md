# 8.3.5 Randomising the frequency

Randomisation is enabled by setting the drive strength controls for the first two stages of the ROSC loop to DS0\_RANDOM and DS1\_RANDOM. An LFSR then provides the drive strength controls for those two stages which are always included in the loop regardless of the FREQ\_RANGE setting. It is recommended to randomise both stages. When the low FREQ\_RANGE is selected the randomiser will increase the frequency by up to 22% of the default. The increase will be approximately half of that if only one stage is randomised. The LFSR can be seeded by writing to the RANDOM register. This can be done at any time but will restart the randomiser.

