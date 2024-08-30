namespace SharpBeat.Utility {
    public struct URLComponents {
        public string Domain;
        public string Path;
        public string Query;
    }

    public static class UrlParser {
        public static bool IsUrl( string keyword ) {
            return keyword.StartsWith( "http" );
        }

        public static string TryGetDomain( string keyword ) {
            if ( !IsUrl( keyword ) ) {
                return string.Empty;
            }
            return new Uri( keyword ).Host;
        }

        public static URLComponents Tokenize( string url ) {
            var uri = new Uri( url );
            return new URLComponents() {
                Domain = uri.Host,
                Path = uri.PathAndQuery.Split( '?' )[0],
                Query = uri.PathAndQuery.Split( '?' )[1]
            };
        }

        public static string TryGetYoutubeID( string keyword ) {
            throw new NotImplementedException();
        }

        public static string TryGetSpotifyID( string keyword ) {
            throw new NotImplementedException();
        }
    }
}
