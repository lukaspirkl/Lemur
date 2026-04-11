# 13.5 Page Locks

The OTP protection hardware logically segments OTP into 64 *pages* (0 through 63), each 128 bytes in size, or equivalently 64 OTP rows.

Each page has a set of lock registers which determine read and write access for that page from Secure and Non-secure code. The lock registers are preloaded from OTP at reset, and can then be advanced (i.e. made less permissive) by software. Lock registers themselves are always world-readable.

Pages 61 through 63 are not so neatly described by a single set of lock registers. These pages store lock initialisation metadata. For more details, see [Section 13.5.4](#page-1273-1). This section describes the more common case of a page protected by a set of page locks.

