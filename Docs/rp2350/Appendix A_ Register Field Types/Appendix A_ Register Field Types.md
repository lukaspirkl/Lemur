# <span id="page-1345-0"></span>**Appendix A: Register Field Types**

# <span id="page-1345-1"></span>**Changes from RP2040**

Register field types are unchanged.

# <span id="page-1345-2"></span>**Standard types**

# <span id="page-1345-3"></span>**RW:**

- Read/Write
- Read operation returns the register value
- Write operation updates the register value

# <span id="page-1345-4"></span>**RO:**

- Read-only
- Read operation returns the register value
- Write operations are ignored

# <span id="page-1345-5"></span>**WO:**

- Write-only
- Read operation returns 0
- Write operation updates the register value

# <span id="page-1345-6"></span>**Clear types**

# <span id="page-1345-7"></span>**SC:**

- Self-Clearing
- Writing a 1 to a bit in an SC field will trigger an event, once the event is triggered the bit clears automatically
- Writing a 0 to a bit in an SC field does nothing

## <span id="page-1345-8"></span>**WC:**

- Write-Clear
- Writing a 1 to a bit in a WC field will write that bit to 0

Changes from RP2040 **1345**

- Writing a 0 to a bit in a WC field does nothing
- Read operation returns the register value

# <span id="page-1346-0"></span>**FIFO types**

These fields are used for reading and writing data to and from FIFOs. Accompanying registers provide FIFO control and status. There is no fixed format for the control and status registers, as they are specific to each FIFO interface.

# <span id="page-1346-1"></span>**RWF:**

- Read/Write FIFO
- Reading this field returns data from a FIFO
  - When the read is complete, the data value is removed from the FIFO
  - If the FIFO is empty, a default value will be returned; the default value is specific to each FIFO interface
- Data written to this field is pushed to a FIFO, Behaviour when the FIFO is full is specific to each FIFO interface
- Read and write operations may access different FIFOs

# <span id="page-1346-2"></span>**RF:**

- Read FIFO
- Functions the same as RWF, but read-only

# <span id="page-1346-3"></span>**WF:**

- Write FIFO
- Functions the same as RWF, but write-only

FIFO types **1346**

