using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemur;

public class SignalLine
{
    private readonly Dictionary<object, LineState> m_DriveStates = new Dictionary<object, LineState>();

    public InitialLineState InitialState 
    { 
        get; 
        set 
        { 
            field = value; 
            State = Resolve(); 
        } 
    } = InitialLineState.PullDown;

    public bool State { get; private set; } = false;

    public event Action<ChangeData>? Changed;

    public void Drive(object source, TimeSpan time, LineState state)
    {
        if (state == LineState.HiZ)
        {
            m_DriveStates.Remove(source);
        }
        else
        {
            m_DriveStates[source] = state;
        }

        var newState = Resolve();
        var oldState = State;
        State = newState;

        Changed?.Invoke(new ChangeData(time, newState, oldState));
    }

    private bool Resolve()
    {
        var isDown = m_DriveStates.Values.Any(x => x == LineState.Down);
        var isUp = m_DriveStates.Values.Any(x => x == LineState.Up);
        
        if (isDown && isUp)
        {
            throw new InvalidOperationException("Signal line conflict");
        }

        // nobody drives - use initial state
        if (!isDown && !isUp)
        {
            if (InitialState == InitialLineState.PullUp)
            {
                return true;
            }

            if (InitialState == InitialLineState.PullDown)
            {
                return false;
            }

            // TODO: There is floting line during initialization - I should handle that
            Log.Logger.Error("Signal line floating");
        }

        return isUp;
    }

    public record ChangeData(TimeSpan Time, bool NewState, bool OldState);

    public enum LineState
    {
        Up,
        Down,
        HiZ,
    }

    public enum InitialLineState
    {
        PullDown,
        PullUp,
        HiZ,
    }
}
