var executionLines = File.ReadAllLines("execution.log");
var testLines = File.ReadAllLines("test.log");

int executionIndex = 0;
int testIndex = 0;
string pc = "";
while(true)
{
    if (testLines[testIndex].StartsWith("PC: "))
    {
        pc = testLines[testIndex].Substring(4);
        testIndex++;

        // no save operation - skip
        if (testLines[testIndex].StartsWith("PC: "))
        {
            continue;
        }

        bool found = false;
        while(true)
        {
            if (executionLines[executionIndex].StartsWith($"core   0: {pc} ", StringComparison.OrdinalIgnoreCase))
            {
                // current PC found - go to next line to get the data
                found = true;
                executionIndex++;
                break;
            }
            else
            {
                executionIndex++;
            }
        }

        while(!testLines[testIndex].StartsWith("PC: "))
        {
            if (!found)
            {
                Console.WriteLine($"PC not found - testIndex:{testIndex} executionIndex:{executionIndex}");
                return;
            }
            
            if (!executionLines[executionIndex].Contains(testLines[testIndex], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Difference found - testIndex:{testIndex} executionIndex:{executionIndex}");
                return;
            }
            else
            {
                Console.WriteLine($"Correct - {testLines[testIndex]}");
            }

            testIndex++;
        }

        Console.WriteLine($"PC {pc} correct");
    }
    else
    {
        testIndex++;
    }
}