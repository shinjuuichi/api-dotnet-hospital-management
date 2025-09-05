using Stateless;
using WebAPI.Models.Enum;

namespace WebAPI.Services.StateMachine;

public class AppointmentStateMachine
{
    private readonly StateMachine<AppointmentStatusEnum, AppointmentTrigger> _machine;

    public AppointmentStateMachine(AppointmentStatusEnum initialState)
    {
        _machine = new StateMachine<AppointmentStatusEnum, AppointmentTrigger>(initialState);

        _machine.Configure(AppointmentStatusEnum.Pending)
            .Permit(AppointmentTrigger.Confirm, AppointmentStatusEnum.Confirmed)
            .Permit(AppointmentTrigger.Cancel, AppointmentStatusEnum.Cancelled);

        _machine.Configure(AppointmentStatusEnum.Confirmed)
            .Permit(AppointmentTrigger.Complete, AppointmentStatusEnum.Completed)
            .Permit(AppointmentTrigger.Cancel, AppointmentStatusEnum.Cancelled);

        _machine.Configure(AppointmentStatusEnum.Cancelled)
            .Ignore(AppointmentTrigger.Cancel);

        _machine.Configure(AppointmentStatusEnum.Completed)
            .Ignore(AppointmentTrigger.Complete);
    }

    public AppointmentStatusEnum State => _machine.State;

    public bool CanFire(AppointmentTrigger trigger) => _machine.CanFire(trigger);

    public AppointmentStatusEnum Fire(AppointmentTrigger trigger)
    {
        _machine.Fire(trigger);
        return _machine.State;
    }

    public AppointmentStatusEnum ChangeState(AppointmentTrigger trigger)
    {
        if (!_machine.CanFire(trigger))
            throw new InvalidOperationException($"Cannot {trigger.ToString().ToLower()} appointment in {_machine.State} status");
        
        _machine.Fire(trigger);
        return _machine.State;
    }

    public IEnumerable<AppointmentTrigger> PermittedTriggers => _machine.PermittedTriggers;
}

public enum AppointmentTrigger
{
    Confirm,
    Cancel,
    Complete
}
