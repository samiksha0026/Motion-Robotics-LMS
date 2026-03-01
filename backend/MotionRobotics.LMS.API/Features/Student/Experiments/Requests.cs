using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;

namespace MotionRobotics.LMS.API.Features.Student.Experiments;

public record GetStudentExperimentsQuery(string UserId) : IQuery<StudentExperimentsListDto?>;
public record GetStudentExperimentDetailQuery(string UserId, int ExperimentId) : IQuery<StudentExperimentDto?>;
public record SubmitStudentExperimentCommand(string UserId, int ExperimentId, ExperimentSubmissionDto Data)
    : ICommand<(bool Success, string Message, ExperimentSubmissionResponseDto? Result)>;
