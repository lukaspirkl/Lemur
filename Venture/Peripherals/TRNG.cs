//using Microsoft.Extensions.Logging;

//namespace Venture.Peripherals;

//public class TRNG : Peripheral32
//{
//    private readonly ILogger<TRNG> logger;

//    private const uint RNG_IMR = 0x100; // Interrupt masking.

//    bool EHR_VALID_INT_MASK = true; // Set to 1 to mask (disable) this interrupt: no interrupt will be generated.See RNG_ISR for an explanation on this interrupt.
//    bool AUTOCORR_ERR_INT_MASK = true; //  Set to 1 to mask (disable) this interrupt: no interrupt will be generated.See RNG_ISR for an explanation on this interrupt.
//    bool CRNGT_ERR_INT_MASK = true; // Set to 1 to mask (disable) this interrupt: no interrupt will be generated.See RNG_ISR for an explanation on this interrupt.
//    bool VN_ERR_INT_MASK = true; // Set to 1 to mask (disable) this interrupt: no interrupt will be generated.See RNG_ISR for an explanation on this interrupt.

//    private const uint RNG_ISR = 0x104; // RNG status register. If corresponding RNG_IMR bit is unmasked, an interrupt will be generated.
//    private const uint RNG_ICR = 0x108; // Interrupt/status bit clear Register.

//    private const uint TRNG_CONFIG = 0x10c; // Selecting the inverter-chain length.
//    private uint TRNG_CONFIG_value = 0;

//    private const uint TRNG_VALID = 0x110; // 192 bit collection indication.

//    private const uint EHR_DATA0 = 0x114; // RNG collected bits.
//    private const uint EHR_DATA1 = 0x118; // RNG collected bits.
//    private const uint EHR_DATA2 = 0x11c; // RNG collected bits.
//    private const uint EHR_DATA3 = 0x120; // RNG collected bits.
//    private const uint EHR_DATA4 = 0x124; // RNG collected bits.
//    private const uint EHR_DATA5 = 0x128; // RNG collected bits.

//    private const uint RND_SOURCE_ENABLE = 0x12c; // Enable signal for the random source.
//    private const uint SAMPLE_CNT1 = 0x130; // Counts clocks between sampling of random bit.
//    private const uint AUTOCORR_STATISTIC = 0x134; // Statistics about autocorrelation test activations.
//    private const uint TRNG_DEBUG_CONTROL = 0x138; // Debug register.
//    private const uint TRNG_SW_RESET = 0x140; // Generate internal SW reset within the RNG block.
//    private const uint RNG_DEBUG_EN_INPUT = 0x1b4; // Enable the RNG debug mode
//    private const uint TRNG_BUSY = 0x1b8; // RNG Busy indication.
//    private const uint RST_BITS_COUNTER = 0x1bc; // Reset the counter of collected bits in the RNG.
//    private const uint RNG_VERSION = 0x1c0; // Displays the version settings of the TRNG.
//    private const uint RNG_BIST_CNTR_0 = 0x1e0; // Collected BIST results.
//    private const uint RNG_BIST_CNTR_1 = 0x1e4; // Collected BIST results.
//    private const uint RNG_BIST_CNTR_2 = 0x1e8; // Collected BIST results.

//    public TRNG(ILogger<TRNG> logger)
//        : base(0x400f0000)
//    {
//        this.logger = logger;
//    }

//    protected override uint HandleRead(uint offset)
//    {
//        if (offset == RNG_IMR)
//        {
//            return (VN_ERR_INT_MASK ? (uint)1 : 0) << 0
//                | (CRNGT_ERR_INT_MASK ? (uint)1 : 0) << 1
//                | (AUTOCORR_ERR_INT_MASK ? (uint)1 : 0) << 2
//                | (EHR_VALID_INT_MASK ? (uint)1 : 0) << 3;
//        }
//        else if (offset == RNG_ISR)
//        {
//            return 1; // Random value is valid and there is no error 
//        }
//        else if (offset == RNG_ICR)
//        {
//            return 0;
//        }
//        else if (offset == TRNG_CONFIG)
//        {
//            return TRNG_CONFIG_value;
//        }
//        else if (offset == TRNG_VALID)
//        {
//            return 1;
//        }



//        logger.LogWarning("Reading from unhandled offset {offset}", offset.ToHex());
//        return 0;
//    }

//    protected override void HandleWrite(uint offset, uint value)
//    {
//        if (offset == RNG_IMR)
//        {
//            VN_ERR_INT_MASK = value.ExtractBits(0, 1) == 1;
//            CRNGT_ERR_INT_MASK = value.ExtractBits(1, 1) == 1;
//            AUTOCORR_ERR_INT_MASK = value.ExtractBits(2, 1) == 1;
//            EHR_VALID_INT_MASK = value.ExtractBits(3, 1) == 1;
//        }
//        else if (offset == RNG_ICR)
//        {
//            // TODO
//        }
//        else if (offset == TRNG_CONFIG)
//        {
//            TRNG_CONFIG_value = value;
//        }

//        logger.LogWarning("Writing to unhandled offset {offset} data {data}", offset.ToHex(), value.ToHex());
//    }
//}
