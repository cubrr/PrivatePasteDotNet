using System;
using System.Net;
using System.Net.Http;

namespace PrivatePasteDotNet
{
    /// <summary>
    /// Contains an uploaded paste's URL, ID and HTTP status code.
    /// </summary>
    public class PrivatePasteResponse : IEquatable<PrivatePasteResponse>
    {
        /// <summary>
        /// URL of the uploaded paste.
        /// </summary>
        public string PasteUrl { get; private set; }

        /// <summary>
        /// ID of the uploaded paste.
        /// </summary>
        public string PasteId { get; private set; }

        /// <summary>
        /// A <see cref="HttpStatusCode"/> enumeration depicting the status of the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivatePasteResponse"/> class.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="host"></param>
        public PrivatePasteResponse(HttpResponseMessage response, string host)
        {
            string pasteId = response.Headers.Location.ToString();
            PasteId = pasteId.TrimStart('/');
            PasteUrl = "https://" + host + pasteId;
            StatusCode = response.StatusCode;
        }

        public bool Equals(PrivatePasteResponse other)
        {
            return other != null &&
                   this.PasteUrl == other.PasteUrl &&
                   this.PasteId == other.PasteId &&
                   this.StatusCode == other.StatusCode;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as PrivatePasteResponse);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 2039;
                hash ^= PasteUrl.GetHashCode();
                hash ^= PasteId.GetHashCode();
                hash ^= StatusCode.GetHashCode();
                return hash ^= 1901;
            }
        }
    }
}
