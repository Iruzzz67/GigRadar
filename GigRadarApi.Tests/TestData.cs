using GigRadarApi.Models;

namespace GigRadarApi.Tests;

/// <summary>Pembuat entitas uji ringkas.</summary>
public static class TestData
{
    public static Event MakeEvent(
        string name = "Gig Test",
        string status = "Published",
        int createdBy = 2,
        double lat = -6.2,
        double lng = 106.8,
        DateTime? start = null,
        DateTime? end = null,
        decimal minPrice = 50_000,
        decimal maxPrice = 100_000,
        int capacity = 100)
    {
        return new Event
        {
            Name = name,
            Status = status,
            CreatedBy = createdBy,
            Latitude = lat,
            Longitude = lng,
            StartDate = start ?? new DateTime(2026, 10, 1, 19, 0, 0, DateTimeKind.Utc),
            EndDate = end ?? new DateTime(2026, 10, 1, 23, 0, 0, DateTimeKind.Utc),
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Capacity = capacity
        };
    }

    public static Ticket MakeTicket(int eventId, int userId = 10, decimal price = 75_000, string status = "Active")
        => new()
        {
            EventId = eventId,
            UserId = userId,
            Price = price,
            Status = status,
            BuyerName = "Pembeli Uji",
            BuyerEmail = "pembeli@uji.test",
            BuyerPhone = "08120000000"
        };
}
