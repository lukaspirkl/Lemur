# 4.3.1 List of Registers

A small number of registers are located on the same bus endpoint as boot RAM:

#### **Write Once Bits**

These are flags which once set, can only be cleared by a system reset. They are used in the implementation of certain bootrom security features.

#### **Boot Locks**

These function the same as the SIO spinlocks ([Section 3.1.4](#page-41-0)), however they are normally reserved for bootrom purposes ([Section 5.4.4](#page-379-0)).

These registers start from an offset of 0x800 above the boot RAM base address of 0x400e0000 (defined as [BOOTRAM\\_BASE](#page-31-1) in the SDK).

*Table 434. List of BOOTRAM registers*

<span id="page-339-2"></span>

| Offset | Name          | Info                                                                                                                                             |
|--------|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| 0x800  | WRITE_ONCE0   | This registers always ORs writes into its current contents. Once a<br>bit is set, it can only be cleared by a reset.                             |
| 0x804  | WRITE_ONCE1   | This registers always ORs writes into its current contents. Once a<br>bit is set, it can only be cleared by a reset.                             |
| 0x808  | BOOTLOCK_STAT | Bootlock status register. 1=unclaimed, 0=claimed. These locks<br>function identically to the SIO spinlocks, but are reserved for<br>bootrom use. |

4.3. Boot RAM **339**

| Offset | Name      | Info                                                                                                                         |  |
|--------|-----------|------------------------------------------------------------------------------------------------------------------------------|--|
| 0x80c  | BOOTLOCK0 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x810  | BOOTLOCK1 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x814  | BOOTLOCK2 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x818  | BOOTLOCK3 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x81c  | BOOTLOCK4 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x820  | BOOTLOCK5 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x824  | BOOTLOCK6 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |
| 0x828  | BOOTLOCK7 | Read to claim and check. Write to unclaim. The value returned on<br>successful claim is 1 << n, and on failed claim is zero. |  |

# <span id="page-340-1"></span>**[BOOTRAM:](#page-339-2) WRITE\_ONCE0, WRITE\_ONCE1 Registers**

**Offsets**: 0x800, 0x804

*Table 435. WRITE\_ONCE0, WRITE\_ONCE1 Registers*

| Bits | Description                                                                                                                |  | Reset      |
|------|----------------------------------------------------------------------------------------------------------------------------|--|------------|
| 31:0 | This registers always ORs writes into its current contents. Once a bit is set, it<br>RW<br>can only be cleared by a reset. |  | 0x00000000 |

## <span id="page-340-2"></span>**[BOOTRAM:](#page-339-2) BOOTLOCK\_STAT Register**

**Offset**: 0x808

*Table 436. BOOTLOCK\_STAT Register*

| Bits | Description                                                                                                                                   | Type | Reset |
|------|-----------------------------------------------------------------------------------------------------------------------------------------------|------|-------|
| 31:8 | Reserved.                                                                                                                                     | -    | -     |
| 7:0  | Bootlock status register. 1=unclaimed, 0=claimed. These locks function<br>identically to the SIO spinlocks, but are reserved for bootrom use. |      | 0xff  |

# <span id="page-340-3"></span>**[BOOTRAM:](#page-339-2) BOOTLOCK0, BOOTLOCK1, …, BOOTLOCK6, BOOTLOCK7 Registers**

**Offsets**: 0x80c, 0x810, …, 0x824, 0x828

*Table 437. BOOTLOCK0, BOOTLOCK1, …, BOOTLOCK6, BOOTLOCK7 Registers*

| Bits | Description                                                                                                                        |  | Reset      |
|------|------------------------------------------------------------------------------------------------------------------------------------|--|------------|
| 31:0 | Read to claim and check. Write to unclaim. The value returned on successful<br>RW<br>claim is 1 << n, and on failed claim is zero. |  | 0x00000000 |

