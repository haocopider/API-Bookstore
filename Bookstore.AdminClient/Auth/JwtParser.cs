using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace Bookstore.AdminClient.Auth
{
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    if (kvp.Key == "role" || kvp.Key == ClaimTypes.Role)
                    {
                        if (kvp.Value is JsonElement rolesElem && rolesElem.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var role in rolesElem.EnumerateArray())
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()!));
                        }
                    }
                    else if (kvp.Key == "sub") claims.Add(new Claim(ClaimTypes.NameIdentifier, kvp.Value.ToString()!));
                    else if (kvp.Key == "unique_name") claims.Add(new Claim(ClaimTypes.Name, kvp.Value.ToString()!));
                    else if (kvp.Key == "email") claims.Add(new Claim(ClaimTypes.Email, kvp.Value.ToString()!));

                    else
                    {
                        claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                    }
                }
            }
            return claims;
        }
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}