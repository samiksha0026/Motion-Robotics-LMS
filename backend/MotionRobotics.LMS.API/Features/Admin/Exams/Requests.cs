using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Exams;

public record GetAllExamsQuery(int? RoboticsLevelId, bool? IsActive) : IQuery<ExamListDto>;
public record GetExamByIdQuery(int ExamId) : IQuery<ExamDetailDto?>;
public record GetExamResultsQuery(int ExamId, int? SchoolId, int? ClassId) : IQuery<ExamResultsListDto>;
public record GetExamStatisticsQuery(int ExamId) : IQuery<ExamStatisticsDto?>;
public record CreateExamCommand(ExamCreateDto Data) : ICommand<ExamDetailDto>;
public record UpdateExamCommand(int ExamId, ExamUpdateDto Data) : ICommand<ExamDetailDto?>;
public record DeleteExamCommand(int ExamId) : ICommand<bool>;
