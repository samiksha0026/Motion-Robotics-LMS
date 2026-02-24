using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Helpers
{
    /// <summary>
    /// Helper class for generating certificate HTML/content
    /// </summary>
    public static class CertificateGenerator
    {
        /// <summary>
        /// Generates certificate HTML content
        /// </summary>
        public static string GenerateCertificateHtml(Certificate certificate)
        {
            var gradeLetter = GetGradeLetter(certificate.ExamScore, certificate.PassingScore);
            var percentage = certificate.PassingScore > 0
                ? Math.Round(certificate.ExamScore / certificate.PassingScore * 100 * certificate.PassingScore / 100, 1)
                : 0;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Certificate - {certificate.CertificateNumber}</title>
    <style>
        @page {{
            size: A4 landscape;
            margin: 0;
        }}
        body {{
            font-family: 'Georgia', serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            margin: 0;
            padding: 40px;
            min-height: 100vh;
            box-sizing: border-box;
        }}
        .certificate {{
            background: white;
            border: 3px solid #d4af37;
            border-radius: 10px;
            padding: 40px 60px;
            position: relative;
            box-shadow: 0 0 30px rgba(212, 175, 55, 0.3);
        }}
        .certificate::before {{
            content: '';
            position: absolute;
            top: 10px;
            left: 10px;
            right: 10px;
            bottom: 10px;
            border: 1px solid #d4af37;
            border-radius: 5px;
        }}
        .header {{
            text-align: center;
            margin-bottom: 20px;
        }}
        .logo {{
            height: 80px;
            margin-bottom: 10px;
        }}
        .company-name {{
            font-size: 28px;
            color: #1a1a2e;
            font-weight: bold;
            letter-spacing: 2px;
        }}
        .title {{
            font-size: 42px;
            color: #d4af37;
            text-align: center;
            margin: 20px 0;
            font-weight: bold;
            text-transform: uppercase;
            letter-spacing: 3px;
        }}
        .subtitle {{
            font-size: 18px;
            color: #666;
            text-align: center;
            margin-bottom: 30px;
        }}
        .recipient {{
            text-align: center;
            margin: 30px 0;
        }}
        .recipient-label {{
            font-size: 16px;
            color: #666;
        }}
        .recipient-name {{
            font-size: 36px;
            color: #1a1a2e;
            font-weight: bold;
            margin: 10px 0;
            border-bottom: 2px solid #d4af37;
            display: inline-block;
            padding: 0 30px 5px;
        }}
        .details {{
            text-align: center;
            font-size: 16px;
            color: #333;
            line-height: 1.8;
            margin: 20px 0;
        }}
        .level-badge {{
            display: inline-block;
            background: linear-gradient(135deg, #d4af37, #f4d03f);
            color: #1a1a2e;
            padding: 10px 25px;
            border-radius: 25px;
            font-weight: bold;
            font-size: 18px;
            margin: 15px 0;
        }}
        .score-section {{
            display: flex;
            justify-content: center;
            gap: 40px;
            margin: 20px 0;
        }}
        .score-item {{
            text-align: center;
        }}
        .score-value {{
            font-size: 28px;
            color: #d4af37;
            font-weight: bold;
        }}
        .score-label {{
            font-size: 12px;
            color: #666;
            text-transform: uppercase;
        }}
        .footer {{
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 40px;
            padding-top: 20px;
        }}
        .signature {{
            text-align: center;
        }}
        .signature-line {{
            width: 200px;
            border-top: 1px solid #333;
            margin-bottom: 5px;
        }}
        .signature-label {{
            font-size: 12px;
            color: #666;
        }}
        .certificate-number {{
            font-size: 12px;
            color: #999;
            text-align: right;
        }}
        .school-info {{
            text-align: center;
            margin-top: 10px;
        }}
        .school-name {{
            font-size: 14px;
            color: #666;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='certificate'>
        <div class='header'>
            {(string.IsNullOrEmpty(certificate.SchoolLogoUrl) ? "" : $"<img src='{certificate.SchoolLogoUrl}' alt='School Logo' class='logo' />")}
            <div class='company-name'>MOTION ROBOTICS</div>
        </div>
        
        <div class='title'>{certificate.Title}</div>
        <div class='subtitle'>This is to certify that</div>
        
        <div class='recipient'>
            <div class='recipient-name'>{certificate.StudentName}</div>
        </div>
        
        <div class='details'>
            has successfully completed the robotics training program
        </div>
        
        <div style='text-align: center;'>
            <div class='level-badge'>Level {certificate.LevelNumber} - {certificate.LevelName}</div>
        </div>
        
        <div class='score-section'>
            <div class='score-item'>
                <div class='score-value'>{certificate.ExamScore:F1}</div>
                <div class='score-label'>Score Obtained</div>
            </div>
            <div class='score-item'>
                <div class='score-value'>{gradeLetter}</div>
                <div class='score-label'>Grade</div>
            </div>
            <div class='score-item'>
                <div class='score-value'>{certificate.AcademicYearName}</div>
                <div class='score-label'>Academic Year</div>
            </div>
        </div>
        
        <div class='school-info'>
            <div class='school-name'>{certificate.SchoolName}</div>
        </div>
        
        <div class='footer'>
            <div class='signature'>
                <div class='signature-line'></div>
                <div class='signature-label'>Instructor Signature</div>
            </div>
            <div style='text-align: center;'>
                <div style='font-size: 14px; color: #333;'>Issued on: {certificate.IssuedDate:MMMM dd, yyyy}</div>
            </div>
            <div class='signature'>
                <div class='signature-line'></div>
                <div class='signature-label'>Director Signature</div>
            </div>
        </div>
        
        <div class='certificate-number'>
            Certificate No: {certificate.CertificateNumber}<br/>
            Verify at: motionrobotics.in/verify/{certificate.CertificateNumber}
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Gets grade letter based on score
        /// </summary>
        public static string GetGradeLetter(decimal score, decimal passingScore)
        {
            if (passingScore <= 0) return "N/A";

            var percentage = (score / passingScore) * 100;

            return percentage switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B+",
                >= 60 => "B",
                >= 50 => "C",
                >= 40 => "D",
                _ => "F"
            };
        }

        /// <summary>
        /// Generates a unique certificate number
        /// </summary>
        public static string GenerateCertificateNumber(int studentId, int levelId, int academicYearId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"MR-{timestamp}-{studentId:D4}-L{levelId}-Y{academicYearId}";
        }

        /// <summary>
        /// Gets certificate status text
        /// </summary>
        public static string GetProgressStatus(bool isCompleted, bool isApproved)
        {
            if (isApproved) return "Approved";
            if (isCompleted) return "PendingApproval";
            return "NotStarted";
        }
    }
}
