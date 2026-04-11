# 10.2.1 Background

The Cortex-M33 processors on RP2350 support the Armv8-M Security Extension. Hardware in the processor maintains two separate execution contexts, called the Secure and Non-secure domains. Access to important data, such as cryptographic keys, or hardware, such as the system voltage regulator, can be limited to the Secure domain. Separating execution into these domains prevents Non-secure execution from interfering with Secure execution. When this datasheet uses the (capitalised) terms **Secure** and **Non-secure**, we refer to these two Arm security domains and the associated bus attributes.

Code running in the Non-secure domain is not necessarily malicious. Consider complex protocols and stacks like USB, whose implementation is *expected* to be easily-exploited and prone to fatal crashes. Restricting such software to the Non-secure domain helps isolate critical software from the consequences of those design decisions. The RP2350 bootrom, for example, runs all of its USB code in the Non-secure domain so the USB code does not have to be considered in the design of critical parts of the bootrom, such as boot signature enforcement.

At any given moment, an Armv8-M processor implementing the Security Extension is in *either* the Secure execution state or the Non-secure execution state. Based on the current state, the processor limits the executable memory regions and the memory regions accessible via load/store instructions. All of the processor's AHB accesses are tagged according to the state that originated them, so that peripherals and the system bus fabric itself can filter transfers based on security domain, for example, using the access control lists described in [Section 10.6](#page-819-1).

An internal processor peripheral called the Security Attribution Unit (SAU) defines, from the processor's point of view, which address ranges are accessible to the Secure and Non-secure domains. The number of distinct address ranges which can be decoded by the SAU is limited, which is why system-level bus filters are provided for assigning peripherals to security domains.

The processor changes security state synchronously using special function calls between states. When an interrupt routed to the Secure domain occurs, the processor can also change security state asynchronously if in the Non-secure state, or vice versa (if enabled).

Both Cortex-M33 processors on RP2350 implement the security extension, so each processor maintains its own Secure

and Non-secure context. The Secure and Non-secure contexts on each core can communicate, for example using shared memory or the Secure/Non-secure SIO mailbox FIFOs. If the cores are used symmetrically (i.e. a shared dualcore Secure context, and a shared dual-core Non-secure context), software must synchronise the processor SAUs so that memory writable from a Non-secure context on one core is not executable in a Secure context on the other core. The DMA MPU, which supports the same region shape and count as the SAU, must also be kept synchronised with the processor SAUs.

It may be simpler to use the cores asymmetrically, implementing all Secure services on one core only. The [FORCE\\_CORE\\_NS](#page-827-0) register can make all core 1 accesses appear Non-secure on the system bus, for the purpose of security filtering implemented in the fabric and peripherals, as well as for SIO registers banked over Secure/Non-secure. However, this does not affect PPB accesses. This does not affect core 1 internally, so it can still maintain its own Secure/Non-secure context. However, system hardware will consider all core 1 accesses Non-secure.

## <span id="page-817-0"></span>**10.2.2. IDAU Address Map**

The Cortex-M33 provides an implementation-defined attribution unit (IDAU) interface, which allows system implementers such as Raspberry Pi Ltd to augment the security attribution map defined by the SAU. The RP2350 IDAU is a hardwired address decode network, with no user configuration. Its address map is as follows:

| Start (hex) | End (hex) | Contents        | IDAU Attribute                                      |
|-------------|-----------|-----------------|-----------------------------------------------------|
| 00000000    | 000042ff  | Arm boot        | Exempt                                              |
| 00004300    | 00007dff  | USB/RISC-V boot | Non-secure (instruction fetch), Exempt (load/store) |
| 00007e00    | 00007fff  | BootROM SGs     | Secure and Non-secure-Callable                      |
| 10000000    | 1fffffff  | XIP             | Non-secure                                          |
| 20000000    | 20081fff  | SRAM            | Non-secure                                          |
| 40000000    | 4fffffff  | APB             | Exempt                                              |
| 50000000    | 5fffffff  | AHB             | Exempt                                              |
| d0000000    | dfffffff  | SIO             | Exempt                                              |

**Exempt** regions are not checked by the processor against its current security state. Effectively, the processor considers these regions Secure when the processor is in the Secure state, and Non-secure when the processor is in the Nonsecure state.

Peripherals are marked Exempt because you are expected to assign them to security domains using the controls in ACCESSCTRL ([Section 10.6\)](#page-819-1), since there are not enough SAU regions to perform meaningful peripheral assignment, and since having separate Secure and Non-secure mirrors of the peripherals is an unnecessary source of programming errors.

The SIO is marked Exempt because it is internally banked over Secure and Non-secure based on the bus access's security attribute, which generally matches the processor's current security state.

As peripherals are Exempt, RP2350 forbids processor instruction fetch from peripherals, by physically disconnecting the bus. Processors fail to fetch instructions from peripherals even if the default MPU permissions are overridden to allow execute permission. Exempt regions permit both Secure and Non-secure access, and TrustZone-M forbids the combination of Non-secure-writable and Secure-executable, so this is a necessary restriction. The same consideration does not apply to the bootrom as the ROM is physically immutable.

The first part of the bootrom is Exempt, because it contains routines expected to be called by both Secure and Nonsecure software in cases where it may not be desirable for Non-secure code to elevate through a Secure Gateway. An example of this is the bootrom memcpy() implementation. Code in the Exempt ROM region is hardened against returnoriented programming (ROP) attacks using the redundancy coprocessor's stack canary instructions.

After a certain watermark, which may vary depending on ROM revision, the ROM becomes IDAU-Non-secure for the purpose of instruction fetch. If an Non-secure SAU region is placed over the bootrom (which is expected to be the case in general, to get the correct NSC attribute on the Secure Gateway region), this part of the ROM becomes nonexecutable to Secure code. Consequently, this part of the bootrom is not ROP-hardened. This part of the ROM contains the NSBOOT (including USB boot) implementation, as well as a RISC-V Armv6-M emulator which can be used to emulate most of the bootrom on RISC-V processors. This region is only implemented on the instruction-side IDAU query: this is an implementation detail which improves timing on the load/store IDAU query, and does not have security implications (given the mask ROM is inherently unwritable) other than that the tt instruction will not be aware of this region.

The final 512 bytes of the bootrom has the **Secure, Non-secure-Callable** (NSC) attribute. This means it contains entry points for Non-secure calls into Secure code. Note that for this IDAU-defined attribute to take effect, the SAU-defined attribute for this range must also be NSC or lower. The recommended configuration is a single Non-secure SAU region covering the entirety of the bootrom. The bootrom exits into user code with the SAU enabled, and SAU region 7 active and covering the entirety of the bootrom.

XIP and SRAM are Non-secure in the IDAU, as they are expected to be divided using the SAU. When the SAU and IDAU differ, if the IDAU attribute is not Exempt, the processor takes whichever is greater out of the SAU and IDAU attribute, in the order Secure > Non-secure-Callable > Non-secure.

Addresses not listed in this table are not decoded by the system AHB crossbar, and will return bus faults if accessed. In these ranges, the ROM's IDAU map is mirrored every 32 kB up to 0x0fffffff. The remaining addresses in the IDAU are Non-secure.

