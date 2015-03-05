using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrivatePasteDotNet
{
    /// <summary>
    /// Uploads pastes to PrivatePaste.com
    /// </summary>
    public static class PrivatePasteUploader
    {
        /// <summary>
        /// Gets a list of valid formatting options.
        /// </summary>
        public static string[] Formattings { get; private set; }

        /// <summary>
        /// Gets a dictionary of valid expiration modifiers, where the modifier is the key and the duration in seconds is the value.
        /// </summary>
        public static Dictionary<string, int> ExpirationModifiers { get; private set; }

        static PrivatePasteUploader()
        {
            Formattings = new []
            {
                "No Formatting",
                "ActionScript", "AppleScript", "Apache", "Bash", "BBCode",
                "Boo", "C", "Clojure", "CSS", "C++", "C#", "Delphi", "Diff",
                "Erlang", "Fortran", "Haskell", "HTML", "INI", "IRC", "Java",
                "JavaScript", "JSP", "Lighttpd", "LUA", "Makefile", "Matlab",
                "NASM", "Nginx", "Objective C", "OCaml", "Pascal", "Perl", "PHP",
                "Python", "Rst", "Ruby", "Scheme", "Smalltalk", "Smarty", "Squid",
                "SQL", "TeX", "TCL", "Wiki", "VBNet", "VimL", "XML", "XSLT", "YAML"
            };
            ExpirationModifiers = new Dictionary<string,int>
            {
                { "s",       1 }, 
                { "m",       60 },
                { "h",       3600 },
                { "d",       24 * 3600 },
                { "w",       7 * 24 * 3600 },
                { "M",       30 * 24 * 3600 },
                { "y",       365 * 24 * 3600 },
	            { "seconds", 1 },
	            { "minutes", 60 },
	            { "hours",   3600 },
	            { "days",    24 * 3600 },
	            { "weeks",   7 * 24 * 3600 },
	            { "months",  30 * 24 * 3600 },
	            { "years",   365 * 24 * 3600 }
            };
        }

        /// <summary>
        /// Uploads a paste.
        /// </summary>
        /// <param name="content">Content of the paste.</param>
        /// <param name="formatting">Formatting of the paste. For valid formatting options, see <see cref="Formattings"/></param>
        /// <param name="lineNumbers">Determines whether line numbers should be visible in the paste or not.</param>
        /// <param name="expires">Duration of the paste. For valid modifiers, see <see cref="ExpirationModifiers"/></param>
        /// <param name="password">If other than <see langword="null"/>, the paste will be secured with the provided password.</param>
        /// <param name="subdomain"></param>
        /// <returns>A <see cref="PrivatePasteResponse"/> containing the HTTP response details.</returns>
        public static async Task<PrivatePasteResponse> CreatePaste(string content,
            string formatting = null,
            bool lineNumbers = false,
            string expires = "1 months",
            string password = null,
            string subdomain = null)
        {
            using (var handler = new HttpClientHandler { AllowAutoRedirect = false })
            using (var client = new HttpClient(handler))
            {
                var values = new List<KeyValuePair<string, string>>
		        {
			        new KeyValuePair<string, string>( "paste_content", content ),
			        new KeyValuePair<string, string>( "formatting", Formattings.Contains(formatting) ? formatting : "No Formatting" ),
			        new KeyValuePair<string, string>( "line_numbers", lineNumbers ? "on" : "off" ),
			        new KeyValuePair<string, string>( "expire", ParseDuration(expires).ToString() ),
			        new KeyValuePair<string, string>( "secure_paste", !String.IsNullOrEmpty(password) ? "on" : "off" ),
			        new KeyValuePair<string, string>( "secure_paste_key", password ?? "")
		        };
#if DEBUG
                Debug.WriteLine("Creating paste with the following options:");
                Debug.Indent();
                foreach (var pair in values)
                    Debug.WriteLine("{0} : {1}", pair.Key, pair.Value);
                Debug.Unindent();
#endif
                HttpContent httpContent = new FormUrlEncodedContent(values);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var builder = new UriBuilder("https://privatepaste.com/save");
                if (!String.IsNullOrEmpty(subdomain))
                    builder.Host = subdomain + "." + builder.Host;
#if DEBUG
                Debug.WriteLine("Sending data to:");
                Debug.Indent();
                Debug.WriteLine(builder.ToString());
                Debug.Unindent();
#endif
                using (HttpResponseMessage response = await client.PostAsync(builder.ToString(), httpContent))
                {
#if DEBUG
                    var resp = new PrivatePasteResponse(response, builder.Host);
                    Debug.WriteLine("Received HTTP response:");
                    Debug.Indent();
                    Debug.WriteLine("Status: {0} ({0:D})", resp.StatusCode);
                    Debug.WriteLine("Paste ID: {0}", resp.PasteId);
                    Debug.WriteLine("Paste URL: {0}", resp.PasteUrl);
                    Debug.Unindent();
                    return resp;
#else
                    return new PrivatePasteResponse(response, builder.Host);
#endif
                }
            }
        }

        /// <summary>
        /// Parses a string into seconds. For valid modifiers, see <see cref="ExpirationModifiers"/>.
        /// </summary>
        /// <param name="sExpiration"></param>
        /// <returns>Duration in seconds.</returns>
        /// <exception cref="ArgumentException">The expiration modifier was not found in <see cref="ExpirationModifiers"/>.</exception>
        public static int ParseDuration(string sExpiration)
        {
            int duration = 0;
            foreach (Match match in new Regex(@"\d+ \w+").Matches(sExpiration))
            {
                var splitMatch = match.ToString().Split(' ');
                int mod;
                if (!ExpirationModifiers.TryGetValue(splitMatch[1], out mod))
                    throw new ArgumentException("Unidentified expiration modifier \"" + splitMatch[1] + "\"!", "sExpiration");

                duration += Int32.Parse(splitMatch[0]) * mod;
            }
#if DEBUG
            Debug.WriteLine("Expiration string parsed");
            Debug.Indent();
            Debug.WriteLine("Original: {0}", sExpiration);
            Debug.WriteLine("Parsed: {0}", duration);
            Debug.Unindent();
#endif
            return duration;
        }
    }
}
