namespace TT.Domain.Robots;

/// <summary>
/// Value object representing a GPS coordinate pair.
/// Latitude must be in [-90, 90] and Longitude in [-180, 180].
///
/// Uses <c>init</c> setters and a public parameterless constructor so that
/// EF Core can materialise this as an owned entity without requiring a relational provider.
/// </summary>
public sealed record GpsCoordinates
{
    // Parameterless constructor required for EF Core owned-entity materialisation
    public GpsCoordinates() { }

    public GpsCoordinates(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";
}
