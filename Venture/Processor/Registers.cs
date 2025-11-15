namespace Venture.Processor;

public class Registers
{
    private uint[] data = new uint[32];

    public uint this[uint index]
    {
        get 
        {
            if (index == 0)
            {
                return 0;
            }

            return data[index]; 
        }
        set
        {
            if (index == 0)
            {
                return;
            }

            Console.WriteLine($"x{index} <- {value.ToHex()}");

            data[index] = value;
        }
    }

    public uint zero { get { return data[0]; } set { data[0] = value; } }
    public uint ra { get { return data[1]; } set { data[1] = value; } }
    public uint sp { get { return data[2]; } set { data[2] = value; } }
    public uint gp { get { return data[3]; } set { data[3] = value; } }
    public uint tp { get { return data[4]; } set { data[4] = value; } }
    public uint t0 { get { return data[5]; } set { data[5] = value; } }
    public uint t1 { get { return data[6]; } set { data[6] = value; } }
    public uint t2 { get { return data[7]; } set { data[7] = value; } }
    public uint fp { get { return data[8]; } set { data[8] = value; } }
    public uint s0 { get { return data[8]; } set { data[8] = value; } }
    public uint s1 { get { return data[9]; } set { data[9] = value; } }
    public uint a0 { get { return data[10]; } set { data[10] = value; } }
    public uint a1 { get { return data[11]; } set { data[11] = value; } }
    public uint a2 { get { return data[12]; } set { data[12] = value; } }
    public uint a3 { get { return data[13]; } set { data[13] = value; } }
    public uint a4 { get { return data[14]; } set { data[14] = value; } }
    public uint a5 { get { return data[15]; } set { data[15] = value; } }
    public uint a6 { get { return data[16]; } set { data[16] = value; } }
    public uint a7 { get { return data[17]; } set { data[17] = value; } }
    public uint s2 { get { return data[18]; } set { data[18] = value; } }
    public uint s3 { get { return data[19]; } set { data[19] = value; } }
    public uint s4 { get { return data[20]; } set { data[20] = value; } }
    public uint s5 { get { return data[21]; } set { data[21] = value; } }
    public uint s6 { get { return data[22]; } set { data[22] = value; } }
    public uint s7 { get { return data[23]; } set { data[23] = value; } }
    public uint s8 { get { return data[24]; } set { data[24] = value; } }
    public uint s9 { get { return data[25]; } set { data[25] = value; } }
    public uint s10 { get { return data[26]; } set { data[26] = value; } }
    public uint s11 { get { return data[27]; } set { data[27] = value; } }
    public uint t3 { get { return data[28]; } set { data[28] = value; } }
    public uint t4 { get { return data[29]; } set { data[29] = value; } }
    public uint t5 { get { return data[30]; } set { data[30] = value; } }
    public uint t6 { get { return data[31]; } set { data[31] = value; } }
}
