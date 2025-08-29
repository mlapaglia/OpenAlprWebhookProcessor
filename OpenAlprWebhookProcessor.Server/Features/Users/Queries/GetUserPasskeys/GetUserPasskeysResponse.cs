using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetUserPasskeys
{
    public record GetUserPasskeysResponse(List<PasskeyDto> Passkeys);

    public record PasskeyDto(
        int Id,
        string Name,
        DateTime RegDate,
        string AaGuid);
}
