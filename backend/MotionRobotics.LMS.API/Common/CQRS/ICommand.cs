using MediatR;

namespace MotionRobotics.LMS.API.Common.CQRS;

/// <summary>Marker interface for commands that return a result</summary>
public interface ICommand<TResult> : IRequest<TResult> { }

/// <summary>Marker interface for void commands</summary>
public interface ICommand : IRequest { }

/// <summary>Handler for commands returning a result</summary>
public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{ }

/// <summary>Handler for void commands</summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{ }
