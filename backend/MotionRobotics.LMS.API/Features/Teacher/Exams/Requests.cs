using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;

namespace MotionRobotics.LMS.API.Features.Teacher.Exams;

public record GetTeacherExamResultsQuery(string UserId, int? ClassId, int? ExamId) : IQuery<TeacherExamResultsListDto>;
public record GetTeacherExamOverviewQuery(string UserId, int ExamId) : IQuery<TeacherExamOverviewDto?>;
public record GetStudentsExamEligibilityQuery(string UserId, int ClassId, int RoboticsLevelId) : IQuery<List<StudentExamEligibilityDto>>;
