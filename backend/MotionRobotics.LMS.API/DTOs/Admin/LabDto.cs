namespace MotionRobotics.LMS.API.DTOs.Admin;

public class LabPhotoDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class LabInfoDto
{
    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? LabDescription { get; set; }
    public string? LabArea { get; set; }
    public int? LabCapacity { get; set; }
    public List<LabPhotoDto> Photos { get; set; } = new();
}

public class UpdateLabInfoDto
{
    public string? LabDescription { get; set; }
    public string? LabArea { get; set; }
    public int? LabCapacity { get; set; }
}

public class SchoolLabSummaryDto
{
    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public int PhotoCount { get; set; }
    public bool HasLabInfo { get; set; }
}
