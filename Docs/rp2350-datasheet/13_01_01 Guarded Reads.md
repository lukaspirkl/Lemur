# 13.1.1 Guarded Reads

Reads through the guarded aliases differ from unguarded reads in the following ways:

- Permission failures return bus faults rather than a bit pattern of all-ones.
- Uncorrectable ECC errors return a bus fault if detected.
- Guarded reads perform an additional hardware consistency check to detect power transients. If this check fails, the read returns a bus fault.

These checks help to make the OTP fail-safe in contexts where deliberate fault injection is a possibility. For example, the RP2350 bootrom uses guarded reads to check boot configuration flags.

The data returned from a successful guarded read is the same as the data returned by a successful read from the corresponding unguarded alias.

#### **IMPORTANT**

Users relying on OTP data in a Secure context should always perform guarded reads, and it is strongly recommended to use ECC. For rows where ECC is not possible, software should take care to ensure the consistency of data across multiple overlapping reads.

