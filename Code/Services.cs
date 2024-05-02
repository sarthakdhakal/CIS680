using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization; // If you choose to use System.Text.Json
using NewDotnet.Models;
using NewDotnet.Code;
using System.Text.Json;
using NewDotnet.Context; // Adjust namespace to where your models are defined

namespace NewDotnet.Code
{
    public class Services
    {
        private readonly OODBModelContext _context; // Assuming dependency injection is used

        public Services(OODBModelContext context)
        {
            _context = context;
        }

        public static Version APICodeVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        // Simplified version for ASP.NET Core using dependency injection for IHttpContextAccessor
        public Dictionary<string, string> GetQueryStringValues(HttpContext httpContext)
        {
            return httpContext.Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString(), StringComparer.OrdinalIgnoreCase);
        }

        public byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static int TryParseWithDefault(string intString, int defaultValue)
        {
            return int.TryParse(intString, out int result) ? result : defaultValue;
        }


        internal static BearerTokenContents GetTokenDataFromUserPrincipal(ClaimsPrincipal user)
        {
            
            // Directly access claims without conversion to arrays or lists
            var starIdClaim = user.Claims.FirstOrDefault(c => c.Type == "starId");
 
            // Use helper methods to safely parse and get claim values
            var expiryClaim = user.Claims.FirstOrDefault(c => c.Type == "expiry");
            var playlistClaim = user.Claims.FirstOrDefault(c => c.Type == "playlist");

            // Create the token contents object using safe parsing for expiry and playlist
            return new BearerTokenContents
            {
                StarId = starIdClaim.Value,
                Expiry = DateTime.TryParse(expiryClaim?.Value, out DateTime expiry) ? expiry : DateTime.Now,
                GuestId = user.Claims.FirstOrDefault(c => c.Type == "guestId")?.Value ?? "",
                KeyType = user.Claims.FirstOrDefault(c => c.Type == "keyType")?.Value ?? "",
                Playlist = int.TryParse(playlistClaim?.Value, out int playlistId) ? playlistId : -1
            };
        }

        /* 
          public async Task RenumberPlaylistAsync()
          {
              var playlists = await _context.Playlists.ToListAsync();
              _context.Playlists.RemoveRange(playlists);
              await _context.SaveChangesAsync();

              int newOrder = 100;
              foreach (var pl in playlists)
              {
                  if (pl.ItemType == "x") break;
                  pl.PlaylistOrder = newOrder;
                  newOrder += 100;
              }

              // Special playlist item at the end
              playlists.Add(new Playlists { p = 100000, ItemType = "x", ItemId = 0 });
              _context.Playlists.AddRange(playlists);
              await _context.SaveChangesAsync();
          }
      }
      */
        public class DateSerializer : JsonConverter<DateTime>
        {
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("dd MMMM yyyy"));
            }

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(DateTime);
            }
        }
    }
}
