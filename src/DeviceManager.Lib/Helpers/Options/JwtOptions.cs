﻿namespace DeviceManager.Lib.Helpers.Options;

public class JwtOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }
    public required int ValidityInMinutes { get; set; }
}