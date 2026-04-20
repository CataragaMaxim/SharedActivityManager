//// Notă: Trebuie să instalezi pachetul NuGet: Install-Package EPPlus
//using OfficeOpenXml;
//using SharedActivityManager.Enums;
//using SharedActivityManager.Models;

//namespace SharedActivityManager.Services.Adapters
//{
//    /// <summary>
//    /// Adapter pentru format Excel (.xlsx)
//    /// </summary>
//    public class ExcelAdapter : BaseFileAdapter
//    {
//        public override string FormatName => "Excel";
//        public override string[] SupportedExtensions => new[] { ".xlsx", ".xls" };

//        static ExcelAdapter()
//        {
//            // 🔥 CORECTAT: Setarea licenței pentru EPPlus
//            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//        }

//        public override async Task<List<Activity>> ImportAsync(string filePath)
//        {
//            var activities = new List<Activity>();

//            if (!FileExists(filePath))
//                throw new FileNotFoundException($"File not found: {filePath}");

//            using var package = new ExcelPackage(new FileInfo(filePath));
//            var worksheet = package.Workbook.Worksheets[0];
//            if (worksheet == null)
//                return activities;

//            var row = 2; // Sărim header-ul

//            while (worksheet.Cells[row, 1].Value != null)
//            {
//                try
//                {
//                    var activity = new Activity
//                    {
//                        Title = worksheet.Cells[row, 1].Text,
//                        Desc = worksheet.Cells[row, 2].Text,
//                        TypeId = Enum.TryParse<ActivityType>(worksheet.Cells[row, 3].Text, out var type) ? type : ActivityType.Other,
//                        StartDate = DateTime.TryParse(worksheet.Cells[row, 4].Text, out var date) ? date : DateTime.Today,
//                        StartTime = DateTime.TryParse(worksheet.Cells[row, 5].Text, out var time) ? time : DateTime.Now,
//                        IsCompleted = worksheet.Cells[row, 6].Text == "True",
//                        AlarmSet = worksheet.Cells[row, 7].Text == "True",
//                        IsPublic = worksheet.Cells[row, 8].Text == "True"
//                    };
//                    activities.Add(activity);
//                }
//                catch (Exception ex)
//                {
//                    System.Diagnostics.Debug.WriteLine($"Excel Import error at row {row}: {ex.Message}");
//                }
//                row++;
//            }

//            return await Task.FromResult(activities);
//        }

//        public override async Task ExportAsync(string filePath, List<Activity> activities)
//        {
//            using var package = new ExcelPackage();
//            var worksheet = package.Workbook.Worksheets.Add("Activities");

//            // Header
//            worksheet.Cells[1, 1].Value = "Title";
//            worksheet.Cells[1, 2].Value = "Description";
//            worksheet.Cells[1, 3].Value = "Type";
//            worksheet.Cells[1, 4].Value = "StartDate";
//            worksheet.Cells[1, 5].Value = "StartTime";
//            worksheet.Cells[1, 6].Value = "IsCompleted";
//            worksheet.Cells[1, 7].Value = "AlarmSet";
//            worksheet.Cells[1, 8].Value = "IsPublic";

//            // Formatare header
//            using (var range = worksheet.Cells[1, 1, 1, 8])
//            {
//                range.Style.Font.Bold = true;
//                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
//                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
//            }

//            // Date
//            for (int i = 0; i < activities.Count; i++)
//            {
//                var row = i + 2;
//                var activity = activities[i];

//                worksheet.Cells[row, 1].Value = activity.Title;
//                worksheet.Cells[row, 2].Value = activity.Desc;
//                worksheet.Cells[row, 3].Value = activity.TypeId.ToString();
//                worksheet.Cells[row, 4].Value = activity.StartDate.ToString("yyyy-MM-dd");
//                worksheet.Cells[row, 5].Value = activity.StartTime.ToString("HH:mm:ss");
//                worksheet.Cells[row, 6].Value = activity.IsCompleted;
//                worksheet.Cells[row, 7].Value = activity.AlarmSet;
//                worksheet.Cells[row, 8].Value = activity.IsPublic;
//            }

//            // Auto-fit columns
//            worksheet.Cells.AutoFitColumns();

//            await package.SaveAsAsync(new FileInfo(filePath));
//        }
//    }
//}