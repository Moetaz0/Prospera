using Prospera.Domain.Exceptions;

namespace Prospera.Domain.ValueObjects;

public record DateRange
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new InvalidDateRangeException("End date cannot be before start date");

        StartDate = startDate;
        EndDate = endDate;
    }

    public int GetDurationInDays()
    {
        return (EndDate - StartDate).Days;
    }

    public int GetDurationInMonths()
    {
        return ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
    }

    public bool Contains(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public static DateRange CurrentMonth()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return new DateRange(startOfMonth, endOfMonth);
    }

    public static DateRange CurrentYear()
    {
        var now = DateTime.UtcNow;
        var startOfYear = new DateTime(now.Year, 1, 1);
        var endOfYear = new DateTime(now.Year, 12, 31);
        return new DateRange(startOfYear, endOfYear);
    }

    public static DateRange Last30Days()
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-30);
        return new DateRange(startDate, endDate);
    }

    public static DateRange Last90Days()
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-90);
        return new DateRange(startDate, endDate);
    }
}