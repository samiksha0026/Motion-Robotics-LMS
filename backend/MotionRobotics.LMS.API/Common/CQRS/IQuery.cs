using MediatR;

namespace MotionRobotics.LMS.API.Common.CQRS;

/// <summary>Marker interface for queries (read operations)</summary>
public interface IQuery<TResult> : IRequest<TResult> { }

/// <summary>Handler for queries</summary>
public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{ }
