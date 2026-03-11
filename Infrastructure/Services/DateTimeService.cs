using Prospera.Application.Common.Interfaces;
using System;

namespace Prospera.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}