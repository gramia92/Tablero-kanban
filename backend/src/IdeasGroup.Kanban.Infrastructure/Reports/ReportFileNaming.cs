namespace IdeasGroup.Kanban.Infrastructure.Reports;

internal static class ReportFileNaming
{
    public static string Slugify(string value) =>
        new(value.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
}
